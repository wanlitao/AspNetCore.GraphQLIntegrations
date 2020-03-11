using HEF.Entity.Mapper;
using HEF.Util;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HEF.GraphQL.EntityQuery
{
    public class ComparisonExpressionFactory : IComparisonExpressionFactory
    {
        public ComparisonExpressionFactory(IEnumerable<IComparisonExpressionBuilder> comparisonExpressionBuilders)
        {
            ComparisonExpressionBuilders = comparisonExpressionBuilders
                ?? throw new ArgumentNullException(nameof(comparisonExpressionBuilders));
        }

        protected IEnumerable<IComparisonExpressionBuilder> ComparisonExpressionBuilders { get; }

        public Expression CreatePropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, string comparisonType, object comparisonValue)
        {
            if (comparisonType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(comparisonType));

            foreach (var comparisonExpressionBuilder in ComparisonExpressionBuilders)
            {
                if (comparisonExpressionBuilder.IsComparisonType(comparisonType))
                {
                    return comparisonExpressionBuilder.BuildPropertyComparisonExpression(entityParameter,
                        property, comparisonValue);
                }
            }

            throw new NotSupportedException($"build expression of target comparison '{comparisonType}' not supported");
        }
    }
}
