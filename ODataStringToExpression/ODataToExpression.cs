using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ODataStringToExpression
{
    public class ODataToExpression
    {
        public Func<T, bool> Convert<T>(string query)
        {
            var paramExpression = Expression.Parameter(typeof(T), "p");

            var expressions = new List<BinaryExpression>();

            foreach (var subQuery in query.Split(new[] { "and", "or" }, StringSplitOptions.None))
            {
                var tokens = subQuery.Trim().Split(' ');

                var left = tokens[0];
                var @operator = tokens[1];
                var right = tokens[2];

                var binaryExpression = CreateBinaryExpression<T>(left, @operator, right, paramExpression);

                expressions.Add(binaryExpression);
            }

            if (expressions.Count > 1)
            {
                if (query.Contains("and"))
                    return Expression.Lambda<Func<T, bool>>(Expression.And(expressions[0], expressions[1]), paramExpression).Compile();
                else
                    return Expression.Lambda<Func<T, bool>>(Expression.Or(expressions[0], expressions[1]), paramExpression).Compile();
            }

            return Expression.Lambda<Func<T, bool>>(expressions[0], paramExpression).Compile();
        }

        private BinaryExpression CreateBinaryExpression<T>(
                string left, string @operator, string right, ParameterExpression paramExpression)
        {
            var property = typeof(T).GetProperty(left);

            var propertyExpression = Expression.Property(paramExpression, left);
            var valueExpression = GetValueExpression(right, property.PropertyType);

            return BinaryOperatorFactory.GetInstance(@operator)
                   .CreateExpression(propertyExpression, valueExpression);
        }

        private ConstantExpression GetValueExpression(string right, Type propertyType)
        {
            if (propertyType.IsEnum)
            {
                var @enum = Enum.Parse(propertyType, right);
                return Expression.Constant(@enum);
            }
            else
                return Expression.Constant(System.Convert.ChangeType(right, propertyType));
        }
    }
}