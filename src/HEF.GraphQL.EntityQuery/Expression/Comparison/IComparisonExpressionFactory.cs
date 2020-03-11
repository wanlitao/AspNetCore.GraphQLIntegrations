using HEF.Entity.Mapper;
using System.Linq.Expressions;

namespace HEF.GraphQL.EntityQuery
{
    public interface IComparisonExpressionFactory
    {
        public Expression CreatePropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, string comparisonType, object comparisonValue);
    }
}
