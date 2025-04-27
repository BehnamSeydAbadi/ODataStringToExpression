using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace ODataStringToExpression
{
    public class ODataToExpressionUsingMicrosoftOdataCore<TEntityType> where TEntityType : class
    {
        private readonly ParameterExpression _paramExpression = Expression.Parameter(typeof(TEntityType), "tet");

        public Func<TEntityType, bool> Convert(string query)
        {
            var oDataQueryOptions = ODataQueryOptionsBuilder<TEntityType>.New().WithQuery(query).Build();

            var odataSingleValueNode = GetOdataSingleValueNode(oDataQueryOptions);

            var expression = GenerateExpression(odataSingleValueNode);

            return Expression.Lambda<Func<TEntityType, bool>>(expression, _paramExpression).Compile();
        }

        private Expression GenerateExpression(SingleValueNode odataSingleValueNode)
        {
            if (odataSingleValueNode is BinaryOperatorNode binaryOperatorNode)
            {
                Expression binaryExpression;

                if (binaryOperatorNode.OperatorKind is BinaryOperatorKind.Equal)
                {
                    binaryExpression = Expression.Equal(
                        GenerateExpression(binaryOperatorNode.Left),
                        GenerateExpression(binaryOperatorNode.Right));
                }
                else
                {
                    throw new NotImplementedException();
                }

                return binaryExpression;
            }
            else if (odataSingleValueNode is SingleValuePropertyAccessNode singleValuePropertyAccessNode)
            {
                return Expression.Property(_paramExpression, singleValuePropertyAccessNode.Property.Name);
            }
            else if (odataSingleValueNode is ConstantNode constant)
            {
                return Expression.Constant(constant.Value);
            }
            else
            {
                throw new NotImplementedException(odataSingleValueNode.Kind.ToString());
            }
        }

        private SingleValueNode GetOdataSingleValueNode(ODataQueryOptions<TEntityType> oDataQueryOptions)
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntityType<TEntityType>();
            var edmModel = modelBuilder.GetEdmModel();

            var entityType = edmModel.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == typeof(TEntityType).Name);

            var dictionary = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(oDataQueryOptions.RawValues.Filter) is false)
                dictionary.Add("$filter", oDataQueryOptions.RawValues.Filter);

            if (string.IsNullOrEmpty(oDataQueryOptions.RawValues.OrderBy) is false)
                dictionary.Add("$orderby", oDataQueryOptions.RawValues.OrderBy);

            if (string.IsNullOrEmpty(oDataQueryOptions.RawValues.Top) is false)
                dictionary.Add("$top", oDataQueryOptions.RawValues.Top);

            if (string.IsNullOrEmpty(oDataQueryOptions.RawValues.Skip) is false)
                dictionary.Add("$skip", oDataQueryOptions.RawValues.Skip);


            var parser = new ODataQueryOptionParser(
                model: edmModel, targetEdmType: entityType,
                targetNavigationSource: null, queryOptions: dictionary
            );

            return parser.ParseFilter().Expression;
        }
    }
}