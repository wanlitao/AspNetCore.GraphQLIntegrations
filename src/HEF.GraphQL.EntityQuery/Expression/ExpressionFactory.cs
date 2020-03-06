using HEF.Entity.Mapper;
using System.Linq.Expressions;

namespace HEF.GraphQL.EntityQuery
{
    internal static class ExpressionFactory
    {
        internal static LambdaExpression BuildEntityPropertyExpression<TEntity>(IPropertyMap property) where TEntity : class
        {
            var parameterExpr = Expression.Parameter(typeof(TEntity), "entity");
            var propertyExpr = Expression.Property(parameterExpr, property.PropertyInfo);

            var delegateType = Expression.GetFuncType(typeof(TEntity), property.PropertyInfo.PropertyType);
            return Expression.Lambda(delegateType, propertyExpr, parameterExpr);
        }
    }
}
