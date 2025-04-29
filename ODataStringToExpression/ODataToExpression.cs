using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataStringToExpression.ExpressionBuilders;
using ODataStringToExpression.Extensions;

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
                    var leftExpression = GenerateExpression(binaryOperatorNode.Left, parameterExpression);
                    var rightExpression = GenerateExpression(binaryOperatorNode.Right, parameterExpression);

                    if (leftExpression.Type.IsNumericType() && rightExpression.Type.IsNumericType())
                    {
                        var unifiedNumericTypeExpressions = NumericTypeExpressionUnifier.New().Unify(leftExpression, rightExpression);

                        leftExpression = unifiedNumericTypeExpressions.Left;
                        rightExpression = unifiedNumericTypeExpressions.Right;
                    }

                    return BinaryExpressionBuilder.New().Build(binaryOperatorNode.OperatorKind, leftExpression, rightExpression);
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
                        .WithSourceExpression(GenerateCollectionPropertyExpression(anyNode.Source, parameterExpression))
                        .Build();
                }
                case AllNode allNode:
                {
                    return AllExpressionBuilder.New()
                        .WithAnyNode(allNode)
                        .WithSourceExpression(GenerateCollectionPropertyExpression(allNode.Source, parameterExpression))
                        .Build();
                }
                case CountNode countNode:
                {
                    var sourceExpression = GenerateCollectionPropertyExpression(countNode.Source, parameterExpression);
                    return Expression.Property(sourceExpression, "Count");
                }
                default: throw new NotImplementedException(odataSingleValueNode.Kind.ToString());
            }
        }


        private Expression GenerateCollectionPropertyExpression(CollectionNode odataCollectionNode, ParameterExpression paramExpression)
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