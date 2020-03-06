using GraphQL.Types;
using HEF.Entity.Mapper;
using HEF.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HEF.GraphQL.EntityQuery
{
    public class OrderByGraphQueryArgumentBuilder : IEntityGraphQueryArgumentBuilder
    {
        public OrderByGraphQueryArgumentBuilder(IEntityMapperProvider mapperProvider)
        {
            MapperProvider = mapperProvider ?? throw new ArgumentNullException(nameof(mapperProvider));
        }

        protected IEntityMapperProvider MapperProvider { get; }

        public QueryArgument Build<TEntity>() where TEntity : class
        {
            var entityMapper = MapperProvider.GetEntityMapper<TEntity>();

            var entityOrderByGraphTypeFactory = LambdaExpressionCache.GetLambdaExpression<Func<InputObjectGraphType>>(
                $"{GetEntityOrderByTypeName<TEntity>()}_Factory",
                (key) => BuildEntityOrderByGraphTypeFactory<TEntity>(entityMapper.Properties.ToArray()));

            var entityOrderByType = entityOrderByGraphTypeFactory.Compile().Invoke();
            return new QueryArgument(new ListGraphType(new NonNullGraphType(entityOrderByType)))
            {
                Name = EntityGraphQueryConstants.GraphQueryArgumnet_OrderBy_Name
            };
        }

        #region Helper Functions
        protected virtual string GetEntityOrderByTypeName<TEntity>() where TEntity : class
            => $"{typeof(TEntity).Name}_OrderBy_Type";

        protected virtual string GetEntityOrderByTypeDescription<TEntity>() where TEntity : class
            => $"ordering options when selecting data from {typeof(TEntity).Name}";

        protected Expression<Func<InputObjectGraphType>> BuildEntityOrderByGraphTypeFactory<TEntity>(params IPropertyMap[] properties)
            where TEntity : class
        {
            var entityOrderByGraphType = typeof(InputObjectGraphType);

            // collect the body
            var bodyExprs = new List<Expression>();

            // var entityOrderByType = new InputObjectGraphType();
            var entityOrderByTypeVariableExpr = Expression.Variable(entityOrderByGraphType, "entityOrderByType");
            var newEntityOrderByGraphTypeExpr = Expression.New(entityOrderByGraphType);
            var assignEntityOrderByTypeVariableExpr = Expression.Assign(entityOrderByTypeVariableExpr, newEntityOrderByGraphTypeExpr);
            bodyExprs.Add(assignEntityOrderByTypeVariableExpr);

            // entityOrderByType.Name = $"{EntityName}_OrderBy_Type";
            var assignEntityOrderByTypeNameExpr = GraphTypeBuilder.CreateObjectGraphAssignPropertyExpression(
                entityOrderByTypeVariableExpr, entityOrderByGraphType, nameof(InputObjectGraphType.Name), () => GetEntityOrderByTypeName<TEntity>());
            bodyExprs.Add(assignEntityOrderByTypeNameExpr);

            // entityOrderByType.Description = $"ordering options when selecting data from {EntityName}";
            var assignEntityOrderByTypeDescriptionExpr = GraphTypeBuilder.CreateObjectGraphAssignPropertyExpression(
                entityOrderByTypeVariableExpr, entityOrderByGraphType,
                nameof(InputObjectGraphType.Description), () => GetEntityOrderByTypeDescription<TEntity>());
            bodyExprs.Add(assignEntityOrderByTypeDescriptionExpr);

            if (properties.IsNotEmpty())
            {
                foreach (var property in properties)
                {
                    // entityOrderByType.Field<OrderBy_Type>(property.Name)
                    var fieldTypeExpr = GraphTypeBuilder.CreateObjectGraphFieldByGraphExpression(
                        entityOrderByTypeVariableExpr, typeof(OrderBy_Type), property);
                    bodyExprs.Add(fieldTypeExpr);
                }
            }

            // code: return (InputObjectGraphType)entityOrderByType;
            var castResultExpr = Expression.Convert(entityOrderByTypeVariableExpr, entityOrderByGraphType);
            bodyExprs.Add(castResultExpr);

            var factoryBodyExpr = Expression.Block(
                entityOrderByGraphType, /* return type */
                new[] { entityOrderByTypeVariableExpr } /* local variables */,
                bodyExprs /* body expressions */);

            return Expression.Lambda<Func<InputObjectGraphType>>(factoryBodyExpr);
        }
        #endregion
    }
}
