using System;
using System.Linq.Expressions;

namespace ODataStringToExpression
{
    public static class Converter
    {
        public static Expression<Func<T, bool>> ToExpression<T>(this string query)
        {
            var splittedQuery = query.Split(' ');

            var propertyName = splittedQuery[0];
            var @operator = splittedQuery[1];
            var value = splittedQuery[2];

            var paramExpression = Expression.Parameter(typeof(T), "p");

            var propertyExpression = Expression.Property(paramExpression, propertyName);

            var valueExpression = Expression.Constant(decimal.Parse(value));


            BinaryExpression binaryExpression = null;

            if (@operator == "gt")
            {
                binaryExpression = Expression.GreaterThan(propertyExpression, valueExpression);
            }
            else if (@operator == "eq")
            {
                binaryExpression = Expression.Equal(propertyExpression, valueExpression);
            }
            else if (@operator == "lt")
            {
                binaryExpression = Expression.LessThan(propertyExpression, valueExpression);
            }
            else if (@operator == "ge")
            {
                binaryExpression = Expression.GreaterThanOrEqual(propertyExpression, valueExpression);
            }
            else if (@operator == "le")
            {
                binaryExpression = Expression.LessThanOrEqual(propertyExpression, valueExpression);
            }

            return Expression.Lambda<Func<T, bool>>(binaryExpression, paramExpression);
        }
    }
}


