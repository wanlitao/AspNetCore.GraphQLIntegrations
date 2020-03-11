using HEF.Entity.Mapper;
using System.Linq.Expressions;

namespace HEF.GraphQL.EntityQuery
{
    public interface IComparisonExpressionBuilder
    {
        string ComparisonType { get; }

        bool IsComparisonType(string comparisonType)
            => string.Compare(ComparisonType, comparisonType, true) == 0;

        Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue);
    }
}
