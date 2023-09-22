using System;
using System.Linq.Expressions;

namespace ODataStringToExpression
{
    internal class BinaryOperatorFactory
    {
        private readonly string _odataOperator;

        internal static BinaryOperatorFactory GetInstance(string odataOperator) => new BinaryOperatorFactory(odataOperator);
        private BinaryOperatorFactory(string odataOperator) => _odataOperator = odataOperator;

        internal BinaryExpression CreateExpression(Expression left, Expression right)
        {
            switch (_odataOperator)
            {
                case "gt":
                    return Expression.GreaterThan(left, right);
                case "eq":
                    return Expression.Equal(left, right);
                case "lt":
                    return Expression.LessThan(left, right);
                case "ge":
                    return Expression.GreaterThanOrEqual(left, right);
                case "le":
                    return Expression.LessThanOrEqual(left, right);
                default:
                    throw new NotImplementedException(_odataOperator);
            }
        }
    }
}
