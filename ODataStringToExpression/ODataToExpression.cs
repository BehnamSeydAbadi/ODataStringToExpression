using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace ODataStringToExpression
{
    public class ODataToExpression<T>
    {
        private readonly ParameterExpression _paramExpression;

        public ODataToExpression()
        {
            _paramExpression = Expression.Parameter(typeof(T), "p");
        }

        public Func<T, bool> Convert(string query)
        {
            var expression = GenerateExpression(query);

            return Expression.Lambda<Func<T, bool>>(expression, _paramExpression).Compile();
        }

        private Expression GenerateExpression(string query)
        {
            var expressions = new Dictionary<Guid, Expression>();

            if (query.Contains("(") && query.Contains(")"))
            {
                var matches = Regex.Matches(query, RegularExpressions.ForBetweenParentheses);

                foreach (Match match in matches)
                {
                    var areParanthesesForInOperation = match.Value.Contains(',');
                    if (areParanthesesForInOperation) continue;

                    var expression = GenerateExpression(match.Groups[0].Value);

                    var key = Guid.NewGuid();
                    expressions.Add(key, expression);

                    query = query.Replace($"({match.Value})", key.ToString()).Trim();
                }
            }

            if (query.Contains("and"))
            {
                var andSubQueries = query.Split("and");

                foreach (var andSubQuery in andSubQueries)
                {
                    if (andSubQuery.Contains("or"))
                    {
                        var orSubQueries = andSubQuery.Split("or");

                        var orExpressions = new List<Expression>();

                        foreach (var orSubQuery in orSubQueries)
                        {
                            if (Guid.TryParse(orSubQuery, out var key))
                            {
                                orExpressions.Add(expressions[key]);
                                expressions.Remove(key);
                            }
                            else
                                orExpressions.Add(GetExpression(orSubQuery));
                        }

                        var expression = GetMergedExpressions(orExpressions, LogicalOperator.OR);

                        expressions.Add(Guid.NewGuid(), expression);

                        continue;
                    }

                    if (Guid.TryParse(andSubQuery, out _) is false)
                        expressions.Add(Guid.NewGuid(), GetExpression(andSubQuery));
                }

                return GetMergedExpressions(expressions.Values.ToList(), LogicalOperator.AND);
            }

            if (query.Contains("or"))
            {
                var orSubQueries = query.Split("or");

                foreach (var orSubQuery in orSubQueries)
                {
                    if (orSubQuery.Contains("and"))
                    {
                        var andSubQueries = orSubQuery.Split("and");

                        var andExpressions = new List<Expression>();

                        foreach (var andSubQuery in andSubQueries)
                        {
                            if (Guid.TryParse(andSubQuery, out var key))
                            {
                                andExpressions.Add(expressions[key]);
                                expressions.Remove(key);
                            }
                            else
                                andExpressions.Add(GetExpression(andSubQuery));
                        }

                        var expression = GetMergedExpressions(andExpressions, LogicalOperator.AND);

                        expressions.Add(Guid.NewGuid(), expression);

                        continue;
                    }

                    expressions.Add(Guid.NewGuid(), GetExpression(orSubQuery));
                }

                return GetMergedExpressions(expressions.Values.ToList(), LogicalOperator.OR);
            }

            return GetExpression(query);
        }

        private Expression GetMergedExpressions(List<Expression> subExpressions, LogicalOperator @operator)
        {
            if (@operator == LogicalOperator.AND)
            {
                var andExpression = Expression.And(subExpressions[0], subExpressions[1]);

                if (subExpressions.Count > 2)
                    for (int i = 2; i < subExpressions.Count; i++)
                        andExpression = Expression.And(andExpression, subExpressions[i]);

                return andExpression;
            }
            else
            {
                var orExpression = Expression.Or(subExpressions[0], subExpressions[1]);

                if (subExpressions.Count > 2)
                    for (int i = 2; i < subExpressions.Count; i++)
                        orExpression = Expression.Or(orExpression, subExpressions[i]);

                return orExpression;
            }
        }

        private Expression GetExpression(string query)
        {
            var (left, @operator, right) = GetElementsOfQuery(query);

            if (@operator.IsBinaryExpression())
            {
                return CreateBinaryExpression(
                       left.Trim(), @operator.Trim(), right.Trim());
            }
            else if (@operator.IsMethodCallExpression())
            {
                return CreateMethodCallExpression(
                       left.Trim(), @operator.Trim(), right.Trim());
            }
            else
                throw new NotImplementedException();
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

        private BinaryExpression CreateBinaryExpression(
                string left, string @operator, string right)
        {
            var property = typeof(T).GetProperty(left);

            var propertyExpression = Expression.Property(_paramExpression, left);

            var rightExpression = GetConstantExpression(right, property.PropertyType);

            return BinaryOperatorFactory.GetInstance(@operator)
                   .CreateExpression(propertyExpression, rightExpression);
        }

        private MethodCallExpression CreateMethodCallExpression(
                string left, string @operator, string right)
        {
            var property = typeof(T).GetProperty(left);

            var propertyType = property.PropertyType;

            var leftExpression = Expression.Property(_paramExpression, left);

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
            var arrayValues = Regex.Match(right, RegularExpressions.ForBetweenParentheses)
                .Value.Split(",");

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