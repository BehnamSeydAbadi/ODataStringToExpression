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
    public class ODataToExpression
    {
        public Func<TEntityType, bool> Convert<TEntityType>(
            string query, ParameterExpression? parameterExpression = null
        ) where TEntityType : class
        {
            var paramExpression = parameterExpression ??
                                  Expression.Parameter(typeof(TEntityType), $"default_{Guid.NewGuid()}");

            var oDataQueryOptions = ODataQueryOptionsBuilder<TEntityType>.New().WithQuery(query).Build();

            var odataSingleValueNode = GetOdataSingleValueNode<TEntityType>(oDataQueryOptions);

            var expression = GenerateExpression(odataSingleValueNode, paramExpression);

            return Expression.Lambda<Func<TEntityType, bool>>(expression, paramExpression).Compile();
        }

        public Expression GenerateExpression(SingleValueNode odataSingleValueNode,
            ParameterExpression parameterExpression)
        {
            switch (odataSingleValueNode)
            {
                case BinaryOperatorNode binaryOperatorNode:
                {
                    return BinaryExpressionBuilder.New().Build(
                        binaryOperatorNode.OperatorKind,
                        GenerateExpression(binaryOperatorNode.Left, parameterExpression),
                        GenerateExpression(binaryOperatorNode.Right, parameterExpression)
                    );
                }
                case SingleValuePropertyAccessNode singleValuePropertyAccessNode:
                {
                    return Expression.Property(parameterExpression, singleValuePropertyAccessNode.Property.Name);
                }
                case ConstantNode constantNode:
                {
                    return ConstantExpressionBuilder.New().WithConstantNode(constantNode).Build();
                }
                case ConvertNode convertNode:
                {
                    return GenerateExpression(convertNode.Source, parameterExpression);
                }
                case InNode inNode:
                {
                    var leftExpression = GenerateExpression(inNode.Left, parameterExpression);

                    return ContainExpressionBuilder.New()
                        .WithLeftExpression(leftExpression)
                        .WithRightCollectionConstantNode((inNode.Right as CollectionConstantNode)!)
                        .Build();
                }
                case UnaryOperatorNode unaryOperatorNode:
                {
                    var operandExpression = GenerateExpression(unaryOperatorNode.Operand, parameterExpression);
                    return Expression.Not(operandExpression);
                }
                case AnyNode anyNode:
                {
                    return AnyExpressionBuilder.New()
                        .WithAnyNode(anyNode)
                        .WithSourceExpression(GenerateExpression(anyNode.Source, parameterExpression))
                        .Build();
                }
                case AllNode allNode:
                {
                    return AllExpressionBuilder.New()
                        .WithAnyNode(allNode)
                        .WithSourceExpression(GenerateExpression(allNode.Source, parameterExpression))
                        .Build();
                }
                default: throw new NotImplementedException(odataSingleValueNode.Kind.ToString());
            }
        }

        public Expression GenerateExpression(CollectionNode odataCollectionNode, ParameterExpression paramExpression)
        {
            switch (odataCollectionNode)
            {
                case CollectionNavigationNode collectionNavigationNode:
                {
                    return Expression.Property(paramExpression, collectionNavigationNode.NavigationProperty.Name);
                }
                case CollectionPropertyAccessNode collectionPropertyAccessNode:
                {
                    return Expression.Property(paramExpression, collectionPropertyAccessNode.Property.Name);
                }
                case CollectionConstantNode collectionConstantNode:
                {
                    return ConstantExpressionBuilder.New()
                        .WithCollectionConstantNode(collectionConstantNode)
                        .Build();
                }
                default: throw new NotImplementedException(odataCollectionNode.Kind.ToString());
            }
        }


        private SingleValueNode GetOdataSingleValueNode<TEntityType>(
            ODataQueryOptions oDataQueryOptions
        ) where TEntityType : class
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntityType<TEntityType>();
            var edmModel = modelBuilder.GetEdmModel();

            var entityType = edmModel.SchemaElements.OfType<IEdmEntityType>()
                .First(e => e.Name == typeof(TEntityType).Name);

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