using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData.UriParser;

namespace ODataStringToExpression.ExpressionBuilders;

public class ContainExpressionBuilder
{
    private Expression _leftExpression;
    private CollectionConstantNode _collectionConstantNode;

    private ContainExpressionBuilder()
    {
    }

    public static ContainExpressionBuilder New() => new();

    public ContainExpressionBuilder WithLeftExpression(Expression leftExpression)
    {
        _leftExpression = leftExpression;
        return this;
    }

    public ContainExpressionBuilder WithRightCollectionConstantNode(CollectionConstantNode collectionConstantNode)
    {
        _collectionConstantNode = collectionConstantNode;
        return this;
    }

    public Expression Build()
    {
        var elementType = _leftExpression.Type;

        var typedArray = Array.CreateInstance(elementType, _collectionConstantNode.Collection.Count());

        for (var index = 0; index < _collectionConstantNode.Collection.Count(); index++)
        {
            var constantNode = _collectionConstantNode.Collection[index];

            typedArray.SetValue(
                Enum.Parse(elementType, constantNode.Value.ToString()!),
                index
            );
        }

        var collectionExpression = Expression.Constant(typedArray);

        var containsMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(_leftExpression.Type);

        return Expression.Call(containsMethod, collectionExpression, _leftExpression);
    }
}