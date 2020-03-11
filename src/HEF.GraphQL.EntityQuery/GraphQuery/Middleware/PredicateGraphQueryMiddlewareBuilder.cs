using GraphQL;
using HEF.Data.Query;
using HEF.Entity.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HEF.GraphQL.EntityQuery
{
    public class PredicateGraphQueryMiddlewareBuilder : IEntityGraphQueryMiddlewareBuilder
    {
        public PredicateGraphQueryMiddlewareBuilder(IEntityMapperProvider mapperProvider,
            IComparisonExpressionFactory comparisonExprFactory)
        {
            MapperProvider = mapperProvider ?? throw new ArgumentNullException(nameof(mapperProvider));

            ComparisonExprFactory = comparisonExprFactory ?? throw new ArgumentNullException(nameof(comparisonExprFactory));
        }

        protected IEntityMapperProvider MapperProvider { get; }

        protected IComparisonExpressionFactory ComparisonExprFactory { get; }

        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Build<TEntity>(
            IResolveFieldContext resolveFieldContext) where TEntity : class
        {
            if (resolveFieldContext == null)
                throw new ArgumentNullException(nameof(resolveFieldContext));

            var whereArguments = resolveFieldContext.GetArgument<IDictionary<string, object>>(
                EntityGraphQueryConstants.GraphQueryArgumnet_Where);
            var entityPredicateExpr = GetEntityPredicateExpression<TEntity>(whereArguments);

            var queryablePredicateFactory = BuildQueryablePredicateFactory(entityPredicateExpr);
            return queryablePredicateFactory.Compile();
        }

        #region Helper Functions
        protected virtual bool IsWherePredicateAnd(string keyName)
            => string.Compare(keyName, EntityGraphQueryConstants.GraphQueryArgument_Where_Predicate_And, true) == 0;

        protected virtual bool IsWherePredicateOr(string keyName)
            => string.Compare(keyName, EntityGraphQueryConstants.GraphQueryArgument_Where_Predicate_Or, true) == 0;

        protected virtual Expression GetEntitySubPredicateExpression(IEntityMapper entityMapper,
            ParameterExpression parameterExpr, object subWhereFieldValue,
            Func<Expression, Expression, BinaryExpression> predicateCombineOperation)
        {
            Expression subPredicateBodyExpr = null;

            if (subWhereFieldValue is IList<object> subWherePredicates)
            {
                foreach (var subWherePredicate in subWherePredicates)
                {
                    if (subWherePredicate is IDictionary<string, object> subWhereArguments)
                    {
                        Expression subPredicateExpr = GetEntityPredicateExpression(entityMapper, parameterExpr, subWhereArguments);

                        subPredicateBodyExpr = subPredicateBodyExpr == null ? subPredicateExpr
                            : predicateCombineOperation(subPredicateBodyExpr, subPredicateExpr);
                    }
                }
            }

            return subPredicateBodyExpr;
        }

        protected virtual Expression GetEntityPredicateExpression(IEntityMapper entityMapper,
            ParameterExpression parameterExpr, IDictionary<string, object> whereArguments)
        {
            Expression predicateBodyExpr = null;
            Expression subAndPredicateBodyExpr = null;
            Expression subOrPredicateBodyExpr = null;

            foreach (var whereField in whereArguments)
            {
                if (IsWherePredicateAnd(whereField.Key))
                {
                    subAndPredicateBodyExpr = GetEntitySubPredicateExpression(entityMapper, parameterExpr,
                        whereField.Value, Expression.AndAlso);
                    continue;
                }

                if (IsWherePredicateOr(whereField.Key))
                {
                    subOrPredicateBodyExpr = GetEntitySubPredicateExpression(entityMapper, parameterExpr,
                        whereField.Value, Expression.OrElse);
                    continue;
                }

                var whereProperty = entityMapper.Properties.Single(p => string.Compare(p.Name, whereField.Key, true) == 0);
                if (whereField.Value is IDictionary<string, object> propertyComparisons)
                {
                    if (propertyComparisons.Count > 1)
                        throw new InvalidOperationException("every comparison type should only have one comparsion operation");
                    
                    foreach (var propertyComparison in propertyComparisons)
                    {
                        var propertyComparisonExpr = ComparisonExprFactory.CreatePropertyComparisonExpression(parameterExpr,
                            whereProperty, propertyComparison.Key, propertyComparison.Value);

                        predicateBodyExpr = predicateBodyExpr == null ? propertyComparisonExpr
                            : Expression.AndAlso(predicateBodyExpr, propertyComparisonExpr);
                    }
                }
            }

            if (subAndPredicateBodyExpr != null)
                predicateBodyExpr = Expression.AndAlso(predicateBodyExpr, subAndPredicateBodyExpr);

            if (subOrPredicateBodyExpr != null)
                predicateBodyExpr = Expression.AndAlso(predicateBodyExpr, subOrPredicateBodyExpr);

            return predicateBodyExpr;
        }

        protected virtual Expression<Func<TEntity, bool>> GetEntityPredicateExpression<TEntity>(
            IDictionary<string, object> whereArguments)
            where TEntity : class
        {
            var entityMapper = MapperProvider.GetEntityMapper<TEntity>();
            var parameterExpr = Expression.Parameter(typeof(TEntity), "entity");

            var predicateBodyExpr = GetEntityPredicateExpression(entityMapper, parameterExpr, whereArguments);
            return Expression.Lambda<Func<TEntity, bool>>(predicateBodyExpr, parameterExpr);
        }

        protected static Expression<Func<IQueryable<TEntity>, IQueryable<TEntity>>> BuildQueryablePredicateFactory<TEntity>(
            Expression<Func<TEntity, bool>> entityPredicateExpr)
            where TEntity : class
        {
            var entityQueryableParameter = Expression.Parameter(typeof(IQueryable<TEntity>), "queryable");

            var queryableWhereFuncExpr = Expression.Call(QueryableMethods.Where.MakeGenericMethod(typeof(TEntity)),
                entityQueryableParameter, entityPredicateExpr);

            return Expression.Lambda<Func<IQueryable<TEntity>, IQueryable<TEntity>>>(queryableWhereFuncExpr, entityQueryableParameter);
        }
        #endregion
    }
}
