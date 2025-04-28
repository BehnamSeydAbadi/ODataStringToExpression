using System;
using System.Linq.Expressions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataStringToExpression.Extensions;

namespace ODataStringToExpression.ExpressionBuilders;

public class ConstantExpressionBuilder
{
    private ConstantNode _constantNode;
    private CollectionConstantNode _collectionConstantNode;

    private ConstantExpressionBuilder()
    {
    }

    public static ConstantExpressionBuilder New() => new();

    public ConstantExpressionBuilder WithConstantNode(ConstantNode constantNode)
    {
        _constantNode = constantNode;
        return this;
    }

    public ConstantExpressionBuilder WithCollectionConstantNode(CollectionConstantNode collectionConstantNode)
    {
        _collectionConstantNode = collectionConstantNode;
        return this;
    }

    public ConstantExpression Build()
    {
        if (_constantNode is not null && _collectionConstantNode is not null)
            throw new InvalidOperationException("ConstantNode and CollectionConstantNode cannot both be set.");

        if (_constantNode is not null)
        {
            return BuildSingleConstantExpression();
        }
        else if (_collectionConstantNode is not null)
        {
            return BuildCollectionConstantExpression();
        }
        else
            throw new InvalidOperationException("Either ConstantNode or CollectionConstantNode must be set.");
    }

    private ConstantExpression BuildSingleConstantExpression()
    {
        if (_constantNode.Value is ODataEnumValue enumValue)
        {
            var enumType = AppDomain.CurrentDomain.GetType(enumValue.TypeName);

            if (enumType is null)
            {
                throw new InvalidOperationException($"Enum type '{enumValue.TypeName}' could not be found.");
            }

            return Expression.Constant(Enum.Parse(enumType, enumValue.ToString()));
        }
        else if (_constantNode.Value is DateTimeOffset dateTimeOffset)
        {
            var dateTimeValue = dateTimeOffset.UtcDateTime;
            return Expression.Constant(dateTimeValue, typeof(DateTime));
        }
        else if (_constantNode.Value is Date date)
        {
            var dateTimeValue = Convert.ToDateTime(date.ToString());
            return Expression.Constant(dateTimeValue, typeof(DateTime));
        }

        return Expression.Constant(_constantNode.Value);
    }

    private ConstantExpression BuildCollectionConstantExpression()
    {
        // return Expression.Constant()
        throw new NotImplementedException();
    }
}