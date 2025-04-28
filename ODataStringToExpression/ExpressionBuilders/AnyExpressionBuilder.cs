using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataStringToExpression.Extensions;

namespace ODataStringToExpression.ExpressionBuilders;

public class AnyExpressionBuilder
{
    private AnyNode _anyNode;
    private Expression _sourceExpression;

    private AnyExpressionBuilder()
    {
    }

    public static AnyExpressionBuilder New() => new();

    public AnyExpressionBuilder WithAnyNode(AnyNode anyNode)
    {
        _anyNode = anyNode;
        return this;
    }

    public AnyExpressionBuilder WithSourceExpression(Expression sourceExpression)
    {
        _sourceExpression = sourceExpression;
        return this;
    }

    public Expression Build()
    {
        var typeFullName = _anyNode.CurrentRangeVariable.TypeReference.FullName();
        var parameterExpression = Expression.Parameter(
            AppDomain.CurrentDomain.GetType(typeFullName),
            _anyNode.CurrentRangeVariable.Name
        );

        var bodyExpression = new ODataToExpression().GenerateExpression(_anyNode.Body, parameterExpression);

        var anyMethod = typeof(Enumerable)
            .GetMethods().First(m => m.Name == "Any" && m.GetParameters().Length == 2)
            .MakeGenericMethod(parameterExpression.Type);

        var lambdaExpression = Expression.Lambda(bodyExpression, parameterExpression);

        return Expression.Call(
            method: anyMethod, arg0: _sourceExpression, arg1: lambdaExpression
        );
    }
}