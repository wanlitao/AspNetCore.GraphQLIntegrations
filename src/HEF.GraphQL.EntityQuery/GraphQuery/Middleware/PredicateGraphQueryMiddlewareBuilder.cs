using GraphQL;
using HEF.Entity.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HEF.GraphQL.EntityQuery
{
    public class PredicateGraphQueryMiddlewareBuilder : IEntityGraphQueryMiddlewareBuilder
    {
        public PredicateGraphQueryMiddlewareBuilder(IEntityMapperProvider mapperProvider)
        {
            MapperProvider = mapperProvider ?? throw new ArgumentNullException(nameof(mapperProvider));
        }

        protected IEntityMapperProvider MapperProvider { get; }

        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Build<TEntity>(
            IResolveFieldContext resolveFieldContext) where TEntity : class
        {
            if (resolveFieldContext == null)
                throw new ArgumentNullException(nameof(resolveFieldContext));

            var whereArguments = resolveFieldContext.GetArgument<IDictionary<string, object>>(
                EntityGraphQueryConstants.GraphQueryArgumnet_Where);

            var entityPredicateExpr = GetEntityPredicateExpression<TEntity>(whereArguments);

            return queryable =>
            {
                return queryable;
            };
        }

        #region Helper Functions
        protected virtual bool IsWherePredicateAnd(string keyName)
            => string.Compare(keyName, EntityGraphQueryConstants.GraphQueryArgument_Where_Predicate_And, true) == 0;

        protected virtual bool IsWherePredicateOr(string keyName)
            => string.Compare(keyName, EntityGraphQueryConstants.GraphQueryArgument_Where_Predicate_Or, true) == 0;

        protected virtual Expression<Func<TEntity, bool>> GetEntityPredicateExpression<TEntity>(
            IDictionary<string, object> whereArguments)
            where TEntity : class
        {
            var entityMapper = MapperProvider.GetEntityMapper<TEntity>();

            Expression predicateBodyExpr = null;
            Expression subAndPredicateBodyExpr = null;
            Expression subOrPredicateBodyExpr = null;

            var parameterExpr = Expression.Parameter(typeof(TEntity), "entity");
            foreach (var whereField in whereArguments)
            {
                if (IsWherePredicateAnd(whereField.Key))
                {
                    if (whereField.Value is IList<object> subAndWherePredicates)
                    {
                        foreach(var subAndWherePredicate in subAndWherePredicates)
                        {
                            if (subAndWherePredicate is IDictionary<string, object> subAndWhereArguments)
                            {
                                Expression subAndPredicateExpr = GetEntityPredicateExpression<TEntity>(subAndWhereArguments);

                                subAndPredicateBodyExpr = (subAndPredicateBodyExpr == null) ? subAndPredicateExpr
                                    : Expression.AndAlso(subAndPredicateBodyExpr, subAndPredicateExpr);
                            }
                        }
                    }
                    continue;
                }
                
                if (IsWherePredicateOr(whereField.Key))
                {
                    if (whereField.Value is IList<object> subOrWherePredicates)
                    {
                        foreach (var subOrWherePredicate in subOrWherePredicates)
                        {
                            if (subOrWherePredicate is IDictionary<string, object> subOrWhereArguments)
                            {
                                Expression subOrPredicateExpr = GetEntityPredicateExpression<TEntity>(subOrWhereArguments);

                                subOrPredicateBodyExpr = (subOrPredicateBodyExpr == null) ? subOrPredicateExpr
                                    : Expression.OrElse(subOrPredicateBodyExpr, subOrPredicateExpr);
                            }
                        }
                    }
                    continue;
                }
                
                var whereProperty = entityMapper.Properties.Single(p => string.Compare(p.Name, whereField.Key, true) == 0);
                if (whereField.Value is IDictionary<string, object> propertyComparisonExprs)
                {
                    if (propertyComparisonExprs.Count > 1)
                        throw new InvalidOperationException("every comparison type should only have one comparsion operation");

                    var wherePropertyExpr = Expression.Property(parameterExpr, whereProperty.PropertyInfo);
                    foreach (var propertyComparisonExpr in propertyComparisonExprs)
                    {
                        var comparisonValueExpr = Expression.Constant(propertyComparisonExpr.Value, whereProperty.PropertyInfo.PropertyType);
                        var propertyPredicateExpr = propertyComparisonExpr.Key switch
                        {
                            "_eq" => Expression.Equal(wherePropertyExpr, comparisonValueExpr),
                            "_gt" => Expression.GreaterThan(wherePropertyExpr, comparisonValueExpr),
                            "_gte" => Expression.GreaterThanOrEqual(wherePropertyExpr, comparisonValueExpr),
                            "_lt" => Expression.LessThan(wherePropertyExpr, comparisonValueExpr),
                            "_lte" => Expression.LessThanOrEqual(wherePropertyExpr, comparisonValueExpr),
                            "_is_null" => Expression.Equal(wherePropertyExpr, comparisonValueExpr),
                            "_neq" => Expression.NotEqual(wherePropertyExpr, comparisonValueExpr),
                            "_prelike" => Expression.Equal(wherePropertyExpr, comparisonValueExpr),
                            "_like" => Expression.Equal(wherePropertyExpr, comparisonValueExpr),
                            "_suflike" => Expression.Equal(wherePropertyExpr, comparisonValueExpr),
                            _ => throw new NotSupportedException($"target comparison '{propertyComparisonExpr.Key}' not supported")
                        };

                        predicateBodyExpr = (predicateBodyExpr == null) ? propertyPredicateExpr
                            : Expression.AndAlso(predicateBodyExpr, propertyPredicateExpr);
                    }
                }
            }

            if (subAndPredicateBodyExpr != null)
                predicateBodyExpr = Expression.AndAlso(predicateBodyExpr, subAndPredicateBodyExpr);

            if (subOrPredicateBodyExpr != null)
                predicateBodyExpr = Expression.AndAlso(predicateBodyExpr, subOrPredicateBodyExpr);

            return Expression.Lambda<Func<TEntity, bool>>(predicateBodyExpr, parameterExpr);
        }

        protected static Expression<Func<IQueryable<TEntity>, IQueryable<TEntity>>> BuildQueryablePredicateFactory<TEntity>()
            where TEntity : class
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
