using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using MicrosoftAspNetODataRoutingODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace ODataStringToExpression.ExpressionBuilders;

public class ODataQueryOptionsBuilder<TEntity> where TEntity : class
{
    private string _query;

    private ODataQueryOptionsBuilder()
    {
    }

    public static ODataQueryOptionsBuilder<TEntity> New() => new();

    public ODataQueryOptionsBuilder<TEntity> WithQuery(string query)
    {
        _query = query;
        return this;
    }


    public ODataQueryOptions<TEntity> Build()
    {
        var serviceProvider = CreateServiceProvider();

        var routeBuilder = new RouteBuilder(new ApplicationBuilder(serviceProvider));
        routeBuilder.EnableDependencyInjection();

        var edmModel = CreateEdmModel(serviceProvider);

        var httpRequest = CreateHttpRequest(_query, serviceProvider);

        var oDataQueryContext = new ODataQueryContext(
            edmModel, typeof(TEntity), new MicrosoftAspNetODataRoutingODataPath()
        );

        return new ODataQueryOptions<TEntity>(oDataQueryContext, httpRequest);
    }


    private HttpRequest CreateHttpRequest(string queryString, ServiceProvider serviceProvider)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider;
        var request = httpContext.Request;
        request.Method = "GET";
        request.QueryString = new QueryString(queryString);
        return request;
    }

    private IEdmModel CreateEdmModel(ServiceProvider serviceProvider)
    {
        var builder = new ODataConventionModelBuilder(serviceProvider);
        builder.EntitySet<TEntity>(typeof(TEntity).Name);
        var edmModel = builder.GetEdmModel();
        return edmModel;
    }

    private ServiceProvider CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddOData();
        serviceCollection.AddODataQueryFilter();
        serviceCollection.AddTransient<ODataUriResolver>();
        serviceCollection.AddTransient<ODataQueryValidator>();
        serviceCollection.AddTransient<TopQueryValidator>();
        serviceCollection.AddTransient<FilterQueryValidator>();
        serviceCollection.AddTransient<SkipQueryValidator>();
        serviceCollection.AddTransient<OrderByQueryValidator>();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        return serviceProvider;
    }
}