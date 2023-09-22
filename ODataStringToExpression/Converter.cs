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
            var value = splittedQuery[2];

            //p =>
            var paramExpression = Expression.Parameter(typeof(T), "p");

            //p => p.Price
            var propertyExpression = Expression.Property(paramExpression, propertyName);

            var valueExpression = Expression.Constant(decimal.Parse(value));

            var greaterThanExpression = Expression.GreaterThan(propertyExpression, valueExpression);

            return Expression.Lambda<Func<T, bool>>(greaterThanExpression, paramExpression);
        }
    }
}


