using GraphQL;
using HEF.Data.Query;
using HEF.Entity.Mapper;
using HEF.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HEF.GraphQL.EntityQuery
{
    public class OrderByGraphQueryMiddlewareBuilder : IEntityGraphQueryMiddlewareBuilder
    {
        public OrderByGraphQueryMiddlewareBuilder(IEntityMapperProvider mapperProvider)
        {
            MapperProvider = mapperProvider ?? throw new ArgumentNullException(nameof(mapperProvider));
        }

        protected IEntityMapperProvider MapperProvider { get; }

        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Build<TEntity>(
            IResolveFieldContext resolveFieldContext) where TEntity : class
        {
            if (resolveFieldContext == null)
                throw new ArgumentNullException(nameof(resolveFieldContext));

            var orderByArguments = resolveFieldContext.GetArgument<IList<IDictionary<string, object>>>(
                EntityGraphQueryConstants.GraphQueryArgumnet_OrderBy);
            if (orderByArguments.IsEmpty())
                return queryable => queryable;

            var orderByProperties = GetOrderByProperties<TEntity>(orderByArguments);

            var queryableOrderByFactory = BuildQueryableOrderByFactory<TEntity>(orderByProperties.ToArray());
            return queryableOrderByFactory.Compile();
        }

        #region Helper Functions
        protected static OrderBy ConvertToOrderBy(int orderByValue)
        {
            if (Enum.IsDefined(typeof(OrderBy), orderByValue))
            {
                return (OrderBy)Enum.ToObject(typeof(OrderBy), orderByValue);
            }

            throw new ArgumentException("invalid OrderBy value");
        }

        protected virtual IEnumerable<(IPropertyMap, OrderBy)> GetOrderByProperties<TEntity>(
            IList<IDictionary<string, object>> orderByArguments) where TEntity : class
        {
            var entityMapper = MapperProvider.GetEntityMapper<TEntity>();
           
            foreach (var orderByItem in orderByArguments)
            {
                foreach (var orderByField in orderByItem)
                {                    
                    var property = entityMapper.Properties.Single(p => string.Compare(p.Name, orderByField.Key, true) == 0);
                    var propertyOrderBy = ConvertToOrderBy(orderByField.Value.ParseInt());

                    yield return (property, propertyOrderBy);
                }
            }
        }

        protected static Expression<Func<IQueryable<TEntity>, IQueryable<TEntity>>> BuildQueryableOrderByFactory<TEntity>(
            params (IPropertyMap, OrderBy)[] orderByProperties) where TEntity : class
        {
            var entityQueryableType = typeof(IQueryable<TEntity>);
            var entityQueryableParameter = Expression.Parameter(entityQueryableType, "queryable");

            var bodyExprs = new List<Expression>();

            var index = 0;
            var entityOrderedQueryableVariable = Expression.Variable(typeof(IOrderedQueryable<TEntity>), "orderedQueryable");
            foreach (var orderByProperty in orderByProperties)
            {
                var orderByPropertyType = orderByProperty.Item1.PropertyInfo.PropertyType;
                var entityOrderByPropertyExpr = ExpressionFactory.BuildEntityPropertyExpression<TEntity>(orderByProperty.Item1);
                var isOrderAsc = orderByProperty.Item2 == OrderBy.asc;
                if (index++ == 0)
                {
                    // orderedQueryable = item == OrderBy.asc ? queryable.OrderBy(x => x.PropertyName) : queryable.OrderByDescending(x => x.PropertyName)
                    var orderByFuncExpr = Expression.Call(QueryableMethods.OrderBy.MakeGenericMethod(typeof(TEntity), orderByPropertyType),
                        entityQueryableParameter, entityOrderByPropertyExpr);
                    var orderByDescFuncExpr = Expression.Call(QueryableMethods.OrderByDescending.MakeGenericMethod(typeof(TEntity), orderByPropertyType),
                        entityQueryableParameter, entityOrderByPropertyExpr);

                    var assignOrderedQueryableExpr = Expression.Assign(entityOrderedQueryableVariable, isOrderAsc ? orderByFuncExpr : orderByDescFuncExpr);
                    bodyExprs.Add(assignOrderedQueryableExpr);
                }
                else
                {
                    // orderedQueryable = item == OrderBy.asc ? orderedQueryable.ThenBy(x => x.PropertyName) : orderedQueryable.ThenByDescending(x => x.PropertyName)
                    var thenByFuncExpr = Expression.Call(QueryableMethods.ThenBy.MakeGenericMethod(typeof(TEntity), orderByPropertyType),
                        entityOrderedQueryableVariable, entityOrderByPropertyExpr);
                    var thenByDescFuncExpr = Expression.Call(QueryableMethods.ThenByDescending.MakeGenericMethod(typeof(TEntity), orderByPropertyType),
                        entityOrderedQueryableVariable, entityOrderByPropertyExpr);
                    
                    var assignOrderedQueryableExpr = Expression.Assign(entityOrderedQueryableVariable, isOrderAsc ? thenByFuncExpr : thenByDescFuncExpr);
                    bodyExprs.Add(assignOrderedQueryableExpr);
                }
            }

            // code: return (IQueryable<TEntity>)orderedQueryable;
            var castResultExpr = Expression.Convert(entityOrderedQueryableVariable, entityQueryableType);
            bodyExprs.Add(castResultExpr);

            var factoryBodyExpr = Expression.Block(
                entityQueryableType, /* return type */
                new[] { entityOrderedQueryableVariable } /* local variables */,
                bodyExprs /* body expressions */);

            return Expression.Lambda<Func<IQueryable<TEntity>, IQueryable<TEntity>>>(factoryBodyExpr, entityQueryableParameter);
        }
        #endregion
    }
}
