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
    public class EntityGraphQuery<TEntity> where TEntity : class
    {
        public EntityGraphQuery(IResolveFieldContext resolveFieldContext,
            IEntityMapperProvider mapperProvider)
        {
            ResolveContext = resolveFieldContext ?? throw new ArgumentNullException(nameof(resolveFieldContext));
            MapperProvider = mapperProvider ?? throw new ArgumentNullException(nameof(mapperProvider));

            Limit = ResolveContext.GetArgument<int?>(EntityGraphQueryConstants.GraphQueryArgumnet_Limit_Name);
            Offset = ResolveContext.GetArgument<int?>(EntityGraphQueryConstants.GraphQueryArgumnet_Offset_Name);
            OrderBys = ResolveContext.GetArgument<IList<IDictionary<string, object>>>(EntityGraphQueryConstants.GraphQueryArgumnet_OrderBy_Name);
            Wheres = ResolveContext.GetArgument<IDictionary<string, object>>(EntityGraphQueryConstants.GraphQueryArgumnet_Where_Name);
        }

        protected IResolveFieldContext ResolveContext { get; }

        protected IDictionary<string, object> ResolveArguments => ResolveContext.Arguments;

        protected IEntityMapperProvider MapperProvider { get; }

        public int? Limit { get; }

        public int? Offset { get; }

        public IList<IDictionary<string, object>> OrderBys { get; }

        public IDictionary<string, object> Wheres { get; }

        #region Helper Functions
        protected static OrderBy ConvertToOrderBy(int orderByValue)
        {
            if (Enum.IsDefined(typeof(OrderBy), orderByValue))
            {
                return (OrderBy)Enum.ToObject(typeof(OrderBy), orderByValue);
            }

            throw new ArgumentException("invalid OrderBy value");
        }
        #endregion

        #region Limit
        protected Func<IQueryable<TEntity>, IQueryable<TEntity>> BuildQueryableLimitFunction()
        {
            return queryable =>
            {
                if (Limit.HasValue)
                {
                    queryable = queryable.Take(Limit.Value);
                }

                return queryable;
            };
        }
        #endregion

        #region Offset
        protected Func<IQueryable<TEntity>, IQueryable<TEntity>> BuildQueryableOffsetFunction()
        {
            return queryable =>
            {
                if (Offset.HasValue)
                {
                    queryable = queryable.Skip(Offset.Value);
                }

                return queryable;
            };
        }
        #endregion

        #region OrderBy
        protected Expression<Func<IQueryable<TEntity>, IQueryable<TEntity>>> BuildQueryableOrderByFactory(
            params (IPropertyMap, OrderBy)[] orderByProperties)
        {
            var entityQueryableType = typeof(IQueryable<TEntity>);
            var entityQueryableParameter = Expression.Parameter(entityQueryableType, "queryable");

            var bodyExprs = new List<Expression>();

            var index = 0;
            foreach (var orderByProperty in orderByProperties)
            {
                var orderByPropertyType = orderByProperty.Item1.PropertyInfo.PropertyType;
                var entityPropertyExpr = ExpressionFactory.BuildEntityPropertyExpression<TEntity>(orderByProperty.Item1);
                var checkOrderAscExpr = Expression.Equal(Expression.Constant(orderByProperty.Item2, typeof(OrderBy)), Expression.Constant(OrderBy.asc));
                if (index++ == 0)
                {
                    // queryable = item == OrderBy.asc ? queryable.OrderBy(x => x.PropertyName) : queryable.OrderByDescending(x => x.PropertyName)
                    var orderByFuncExpr = Expression.Call(QueryableMethods.OrderBy.MakeGenericMethod(typeof(TEntity), orderByPropertyType),
                        entityQueryableParameter, entityPropertyExpr);
                    var orderByDescFuncExpr = Expression.Call(QueryableMethods.OrderByDescending.MakeGenericMethod(typeof(TEntity), orderByPropertyType),
                        entityQueryableParameter, entityPropertyExpr);

                    var orderByConditionExpr = Expression.Condition(checkOrderAscExpr, orderByFuncExpr, orderByDescFuncExpr);
                    var assignQueryableExpr = Expression.Assign(entityQueryableParameter, orderByConditionExpr);
                    bodyExprs.Add(assignQueryableExpr);
                }
                else
                {
                    // queryable = item == OrderBy.asc ? queryable.ThenBy(x => x.PropertyName) : queryable.ThenByDescending(x => x.PropertyName)
                    var thenByFuncExpr = Expression.Call(QueryableMethods.ThenBy.MakeGenericMethod(typeof(TEntity), orderByPropertyType),
                        entityQueryableParameter, entityPropertyExpr);
                    var thenByDescFuncExpr = Expression.Call(QueryableMethods.ThenByDescending.MakeGenericMethod(typeof(TEntity), orderByPropertyType),
                        entityQueryableParameter, entityPropertyExpr);

                    var thenByConditionExpr = Expression.Condition(checkOrderAscExpr, thenByFuncExpr, thenByDescFuncExpr);
                    var assignQueryableExpr = Expression.Assign(entityQueryableParameter, thenByConditionExpr);
                    bodyExprs.Add(assignQueryableExpr);
                }
            }

            // code: return (IQueryable<TEntity>)queryable;
            var castResultExpr = Expression.Convert(entityQueryableParameter, entityQueryableType);
            bodyExprs.Add(castResultExpr);

            var factoryBodyExpr = Expression.Block(
                entityQueryableType, /* return type */                
                bodyExprs /* body expressions */);

            return Expression.Lambda<Func<IQueryable<TEntity>, IQueryable<TEntity>>>(factoryBodyExpr, entityQueryableParameter);            
        }

        protected Func<IQueryable<TEntity>, IQueryable<TEntity>> BuildQueryableOrderByFunction()
        {
            var entityMapper = MapperProvider.GetEntityMapper<TEntity>();
            var orderByProperties = new List<(IPropertyMap, OrderBy)>();
            foreach (var orderByItem in OrderBys)
            {
                foreach (var orderByField in orderByItem)
                {
                    var propertyName = orderByField.Key;
                    var property = entityMapper.Properties.Single(p => string.Compare(p.Name, propertyName, true) == 0);
                    var propertyOrderBy = ConvertToOrderBy(orderByField.Value.ParseInt());

                    orderByProperties.Add((property, propertyOrderBy));
                }
            }

            var queryableOrderByFactory = BuildQueryableOrderByFactory(orderByProperties.ToArray());
            return queryableOrderByFactory.Compile();
        }
        #endregion

        #region Where
        protected Func<IQueryable<TEntity>, IQueryable<TEntity>> BuildQueryableWhereFunction()
        {
            return queryable =>
            {
                return queryable;
            };
        }
        #endregion
    }
}
