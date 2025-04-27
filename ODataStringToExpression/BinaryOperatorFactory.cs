using System;
using System.Linq.Expressions;
using Microsoft.OData.UriParser;

namespace ODataStringToExpression
{
    internal class BinaryOperatorFactory
    {
        private BinaryOperatorFactory()
        {
        }

        internal static BinaryOperatorFactory New() => new();

        internal BinaryExpression CreateExpression(string odataOperator, Expression left, Expression right)
        {
            switch (odataOperator)
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
                case "ne":
                    return Expression.NotEqual(left, right);
                default:
                    throw new NotImplementedException(odataOperator);
            }
        }

        internal BinaryExpression CreateExpression(BinaryOperatorKind binaryOperatorNode, Expression left, Expression right)
        {
            switch (binaryOperatorNode)
            {
                case BinaryOperatorKind.GreaterThan:
                    return Expression.GreaterThan(left, right);
                case BinaryOperatorKind.Equal:
                    return Expression.Equal(left, right);
                case BinaryOperatorKind.LessThan:
                    return Expression.LessThan(left, right);
                case BinaryOperatorKind.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(left, right);
                case BinaryOperatorKind.LessThanOrEqual:
                    return Expression.LessThanOrEqual(left, right);
                case BinaryOperatorKind.NotEqual:
                    return Expression.NotEqual(left, right);
                default:
                    throw new NotImplementedException(binaryOperatorNode.ToString());
            }
        }
    }
}