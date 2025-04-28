using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataStringToExpression.ExpressionBuilders;

namespace ODataStringToExpression
{
    public class ODataToExpression<TEntityType> where TEntityType : class
    {
        private readonly ParameterExpression _paramExpression = Expression.Parameter(typeof(TEntityType), "tet");

        public Func<TEntityType, bool> Convert(string query)
        {
            var oDataQueryOptions = ODataQueryOptionsBuilder<TEntityType>.New().WithQuery(query).Build();

            var odataSingleValueNode = GetOdataSingleValueNode(oDataQueryOptions);

            var expression = GenerateExpression(odataSingleValueNode);

            return Expression.Lambda<Func<TEntityType, bool>>(expression, _paramExpression).Compile();
        }

        private Expression GenerateExpression(SingleValueNode odataSingleValueNode)
        {
            switch (odataSingleValueNode)
            {
                case BinaryOperatorNode binaryOperatorNode:
                {
                    return BinaryExpressionBuilder.New().Build(
                        binaryOperatorNode.OperatorKind,
                        GenerateExpression(binaryOperatorNode.Left),
                        GenerateExpression(binaryOperatorNode.Right));
                }
                case SingleValuePropertyAccessNode singleValuePropertyAccessNode:
                {
                    return Expression.Property(_paramExpression, singleValuePropertyAccessNode.Property.Name);
                }
                case ConstantNode constantNode:
                {
                    return ConstantExpressionBuilder.New().WithConstantNode(constantNode).Build();
                }
                case ConvertNode convertNode:
                {
                    return GenerateExpression(convertNode.Source);
                }
                case InNode inNode:
                {
                    return ContainExpressionBuilder.New()
                        .WithLeftExpression(GenerateExpression(inNode.Left))
                        .WithRightCollectionConstantNode(inNode.Right as CollectionConstantNode)
                        .Build();
                }
                case UnaryOperatorNode unaryOperatorNode:
                {
                    var operandExpression = GenerateExpression(unaryOperatorNode.Operand);
                    return Expression.Not(operandExpression);
                }
                default: throw new NotImplementedException(odataSingleValueNode.Kind.ToString());
            }
        }

        private SingleValueNode GetOdataSingleValueNode(ODataQueryOptions<TEntityType> oDataQueryOptions)
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntityType<TEntityType>();
            var edmModel = modelBuilder.GetEdmModel();

            var entityType = edmModel.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == typeof(TEntityType).Name);

            var dictionary = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(oDataQueryOptions.RawValues.Filter) is false)
                dictionary.Add("$filter", oDataQueryOptions.RawValues.Filter);

            if (string.IsNullOrEmpty(oDataQueryOptions.RawValues.OrderBy) is false)
                dictionary.Add("$orderby", oDataQueryOptions.RawValues.OrderBy);

            if (string.IsNullOrEmpty(oDataQueryOptions.RawValues.Top) is false)
                dictionary.Add("$top", oDataQueryOptions.RawValues.Top);

            if (string.IsNullOrEmpty(oDataQueryOptions.RawValues.Skip) is false)
                dictionary.Add("$skip", oDataQueryOptions.RawValues.Skip);


            var parser = new ODataQueryOptionParser(
                model: edmModel, targetEdmType: entityType,
                targetNavigationSource: null, queryOptions: dictionary
            );

            return parser.ParseFilter().Expression;
        }
    }
}