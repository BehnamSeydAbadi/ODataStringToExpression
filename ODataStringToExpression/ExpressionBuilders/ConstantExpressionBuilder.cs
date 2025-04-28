using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace ODataStringToExpression.ExpressionBuilders;

public class ConstantExpressionBuilder
{
    private ConstantNode _constantNode;

    private ConstantExpressionBuilder()
    {
    }

    public static ConstantExpressionBuilder New() => new();

    public ConstantExpressionBuilder WithConstantNode(ConstantNode constantNode)
    {
        _constantNode = constantNode;
        return this;
    }

    public ConstantExpression Build()
    {
        if (_constantNode.Value is ODataEnumValue enumValue)
        {
            var enumType = GetEnumTypeFromTypeName(enumValue.TypeName);

            if (enumType is null)
            {
                throw new InvalidOperationException($"Enum type '{enumValue.TypeName}' could not be found.");
            }

            return Expression.Constant(Enum.Parse(enumType, enumValue.ToString()));
        }
        else if (_constantNode.Value is DateTimeOffset dateTimeOffset)
        {
            var dateTimeValue = Convert.ToDateTime(dateTimeOffset.ToString());
            return Expression.Constant(dateTimeValue, typeof(DateTime));
        }
        else if (_constantNode.Value is Date date)
        {
            var dateTimeValue = Convert.ToDateTime(date.ToString());
            return Expression.Constant(dateTimeValue, typeof(DateTime));
        }

        return Expression.Constant(_constantNode.Value);
    }

    private Type GetEnumTypeFromTypeName(string typeName)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
            .FirstOrDefault(type => type.IsEnum && type.FullName == typeName);
    }
}