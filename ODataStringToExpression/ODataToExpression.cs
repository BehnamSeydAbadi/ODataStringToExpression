using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace ODataStringToExpression
{
    public class ODataToExpression
    {
        public Func<T, bool> Convert<T>(string query)
        {
            var paramExpression = Expression.Parameter(typeof(T), "p");

            var expressions = new List<Expression>();

            foreach (var subQuery in query.Split(new[] { "and", "or" }, StringSplitOptions.None))
            {
                var (left, @operator, right) = GetElementsOfQuery(subQuery);

                if (@operator.IsBinaryExpression())
                {
                    var expression = CreateBinaryExpression<T>(
                    left.Trim(), @operator.Trim(), right.Trim(), paramExpression);

                    expressions.Add(expression);
                }
                else if (@operator.IsMethodCallExpression())
                {
                    var expression = CreateMethodCallExpression<T>(
                    left.Trim(), @operator.Trim(), right.Trim(), paramExpression);

                    expressions.Add(expression);
                }
                else
                    throw new NotImplementedException();
            }

            if (expressions.Count > 1)
            {
                if (query.Contains("and"))
                {
                    var andExpression = Expression.And(expressions[0], expressions[1]);

                    if (expressions.Count > 2)
                        for (int i = 2; i < expressions.Count; i++)
                            andExpression = Expression.And(andExpression, expressions[i]);

                    return Expression.Lambda<Func<T, bool>>(andExpression, paramExpression).Compile();
                }
                else
                {
                    var andExpression = Expression.Or(expressions[0], expressions[1]);

                    if (expressions.Count > 2)
                        for (int i = 2; i < expressions.Count; i++)
                            andExpression = Expression.Or(andExpression, expressions[i]);

                    return Expression.Lambda<Func<T, bool>>(andExpression, paramExpression).Compile();
                }
            }

            return Expression.Lambda<Func<T, bool>>(expressions[0], paramExpression).Compile();
        }

        private (string Left, string Operator, string Right) GetElementsOfQuery(string query)
        {
            string pattern;

            if (Regex.IsMatch(query, RegularExpressions.ForInOperator))
                pattern = RegularExpressions.ForInOperator;
            else if (Regex.IsMatch(query, RegularExpressions.ForDateTimeAtRight))
                pattern = RegularExpressions.ForDateTimeAtRight;
            else if (Regex.IsMatch(query, RegularExpressions.ForNumericAtRight))
                pattern = RegularExpressions.ForNumericAtRight;
            else
                throw new NotImplementedException();

            var tokens = Regex.Match(query.Trim(), pattern).Groups;
            return (tokens[1].Value, tokens[2].Value, tokens[3].Value);
        }

        private BinaryExpression CreateBinaryExpression<T>(
                string left, string @operator, string right, ParameterExpression paramExpression)
        {
            var property = typeof(T).GetProperty(left);

            var propertyExpression = Expression.Property(paramExpression, left);

            var rightExpression = GetConstantExpression(right, property.PropertyType);

            return BinaryOperatorFactory.GetInstance(@operator)
                   .CreateExpression(propertyExpression, rightExpression);
        }

        private MethodCallExpression CreateMethodCallExpression<T>(
                string left, string @operator, string right, ParameterExpression paramExpression)
        {
            var property = typeof(T).GetProperty(left);

            var propertyType = property.PropertyType;

            var leftExpression = Expression.Property(paramExpression, left);

            var rightExpression = GetListInitExpression(right, propertyType);

            var method = typeof(List<>).MakeGenericType(propertyType)
                .GetMethod("Contains", new[] { propertyType });

            return Expression.Call(
                   instance: rightExpression,
                   method,
                   arguments: leftExpression);
        }

        private ConstantExpression GetConstantExpression(string right, Type propertyType)
        {
            if (propertyType.IsEnum)
            {
                var @enum = Enum.Parse(propertyType, right);
                return Expression.Constant(@enum);
            }
            else
                return Expression.Constant(System.Convert.ChangeType(right, propertyType));
        }

        private ListInitExpression GetListInitExpression(string right, Type propertyType)
        {
            var arrayValues = Regex.Match(right, RegularExpressions.ForElementsInArray)
                .Groups[1].Value.Split(',').Select(v => v.Trim());

            var constantExpressions = new List<ConstantExpression>();

            foreach (var value in arrayValues)
            {
                var @enum = Enum.Parse(propertyType, value);
                constantExpressions.Add(Expression.Constant(@enum));
            }

            var typeOfGenericList = typeof(List<>).MakeGenericType(propertyType);

            return Expression.ListInit(Expression.New(typeOfGenericList), constantExpressions);
        }
    }
}