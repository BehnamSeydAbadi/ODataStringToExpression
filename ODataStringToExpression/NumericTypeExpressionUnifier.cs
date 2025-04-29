using System;
using System.Linq.Expressions;

namespace ODataStringToExpression;

public class NumericTypeExpressionUnifier
{
    private NumericTypeExpressionUnifier()
    {
    }

    public static NumericTypeExpressionUnifier New() => new();

    public UnifiedNumericTypeExpressions Unify(Expression left, Expression right)
    {
        if (left.Type == right.Type) return new UnifiedNumericTypeExpressions(left, right);

        var promotedType = PromoteNumericType(left.Type, right.Type);

        return new UnifiedNumericTypeExpressions(
            Expression.Convert(left, promotedType), Expression.Convert(right, promotedType)
        );
    }

    private Type PromoteNumericType(Type leftType, Type rightType)
    {
        var typePrecedence = new[]
        {
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(ulong),
            typeof(long),
            typeof(uint),
            typeof(int),
            typeof(ushort),
            typeof(short),
            typeof(byte),
            typeof(sbyte)
        };

        foreach (var type in typePrecedence)
        {
            if (type == leftType || type == rightType)
            {
                #region Special case: ulong + long = decimal to prevent overflow

                var isUlongWithLongOperand = type == typeof(ulong) && (leftType == typeof(long) || rightType == typeof(long));

                var isLongWithUlongOperand = type == typeof(long) && (leftType == typeof(ulong) || rightType == typeof(ulong));

                var isDecimalSpecialCase = isUlongWithLongOperand || isLongWithUlongOperand;

                if (isDecimalSpecialCase) return typeof(decimal);

                #endregion


                return type;
            }
        }

        return typeof(double); // default data type
    }


    public record UnifiedNumericTypeExpressions(Expression Left, Expression Right);
}