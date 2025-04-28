using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataStringToExpression.Extensions;

namespace ODataStringToExpression.ExpressionBuilders;

public class AllExpressionBuilder
{
    private AllNode _allNode;
    private Expression _sourceExpression;

    private AllExpressionBuilder()
    {
    }

    public static AllExpressionBuilder New() => new();

    public AllExpressionBuilder WithAnyNode(AllNode anyNode)
    {
        _allNode = anyNode;
        return this;
    }

    public AllExpressionBuilder WithSourceExpression(Expression sourceExpression)
    {
        _sourceExpression = sourceExpression;
        return this;
    }

    public Expression Build()
    {
        var typeFullName = _allNode.CurrentRangeVariable.TypeReference.FullName();
        var parameterExpression = Expression.Parameter(
            AppDomain.CurrentDomain.GetType(typeFullName),
            _allNode.CurrentRangeVariable.Name
        );

        var bodyExpression = new ODataToExpression().GenerateExpression(_allNode.Body, parameterExpression);

        var anyMethod = typeof(Enumerable)
            .GetMethods().First(m => m.Name == "All" && m.GetParameters().Length == 2)
            .MakeGenericMethod(parameterExpression.Type);

        var lambdaExpression = Expression.Lambda(bodyExpression, parameterExpression);

        return Expression.Call(
            method: anyMethod, arg0: _sourceExpression, arg1: lambdaExpression
        );
    }
}