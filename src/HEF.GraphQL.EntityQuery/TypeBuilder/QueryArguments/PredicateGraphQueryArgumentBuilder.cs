using GraphQL.Types;
using HEF.Entity.Mapper;
using HEF.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HEF.GraphQL.EntityQuery
{
    public class PredicateGraphQueryArgumentBuilder : IEntityGraphQueryArgumentBuilder
    {
        public PredicateGraphQueryArgumentBuilder(IEntityMapperProvider mapperProvider)
        {
            MapperProvider = mapperProvider ?? throw new ArgumentNullException(nameof(mapperProvider));
        }

        protected IEntityMapperProvider MapperProvider { get; }

        public QueryArgument Build<TEntity>() where TEntity : class
        {
            var entityMapper = MapperProvider.GetEntityMapper<TEntity>();

            var entityPredicateGraphTypeFactory = LambdaExpressionCache.GetLambdaExpression<Func<InputObjectGraphType>>(
                $"{GetEntityPredicateTypeName<TEntity>()}_Factory",
                (key) => BuildEntityPredicateGraphTypeFactory<TEntity>(entityMapper.Properties.ToArray()));

            var entityPredicateType = entityPredicateGraphTypeFactory.Compile().Invoke();
            return new QueryArgument(entityPredicateType) { Name = EntityGraphQueryConstants.GraphQueryArgumnet_Where_Name };
        }

        #region Helper Functions
        protected virtual string GetEntityPredicateTypeName<TEntity>() where TEntity : class
            => $"{typeof(TEntity).Name}_Predicate_Type";

        protected virtual string GetEntityPredicateTypeDescription<TEntity>() where TEntity : class
            => $"Boolean expression to filter rows from the resource {typeof(TEntity).Name}. All fields are combined with a logical 'AND'.";

        private static readonly MethodInfo _objectGraphFieldByGraphExtensionMethod
            = typeof(ComplexGraphTypeExtensions).GetTypeInfo()
                .GetDeclaredMethods(nameof(ObjectGraphType.Field))
                .Single();

        protected static IEnumerable<Expression> BuildObjectGraphFieldByGraphExtMethodParamExpressions(
            MethodInfo objectGraphFieldByGraphExtMethod, params Expression[] paramWithoutDefaultValueExprs)
        {
            foreach (var parameterExpr in paramWithoutDefaultValueExprs)
            {
                yield return parameterExpr;
            }

            var objectGraphFieldByGraphExtMethodParameters = objectGraphFieldByGraphExtMethod.GetParameters();

            foreach (var methodParameter in objectGraphFieldByGraphExtMethodParameters)
            {
                if (methodParameter.HasDefaultValue)
                {
                    yield return Expression.Constant(methodParameter.DefaultValue, methodParameter.ParameterType);
                }
            }
        }

        protected Expression<Func<InputObjectGraphType>> BuildEntityPredicateGraphTypeFactory<TEntity>(params IPropertyMap[] properties)
            where TEntity : class
        {
            var entityPredicateGraphType = typeof(InputObjectGraphType);

            // collect the body
            var bodyExprs = new List<Expression>();

            // var entityPredicateType = new InputObjectGraphType();
            var entityPredicateTypeVariableExpr = Expression.Variable(entityPredicateGraphType, "entityPredicateType");
            var newEntityPredicateGraphTypeExpr = Expression.New(entityPredicateGraphType);
            var assignEntityPredicateTypeVariableExpr = Expression.Assign(entityPredicateTypeVariableExpr, newEntityPredicateGraphTypeExpr);
            bodyExprs.Add(assignEntityPredicateTypeVariableExpr);

            // entityPredicateType.Name = $"{EntityName}_Predicate_Type";
            var assignEntityPredicateTypeNameExpr = GraphTypeBuilder.CreateObjectGraphAssignPropertyExpression(
                entityPredicateTypeVariableExpr, entityPredicateGraphType, nameof(InputObjectGraphType.Name), () => GetEntityPredicateTypeName<TEntity>());
            bodyExprs.Add(assignEntityPredicateTypeNameExpr);

            // entityPredicateType.Description = $"Boolean expression to filter rows from the resource {EntityName}. All fields are combined with a logical 'AND'.";
            var assignEntityPredicateTypeDescriptionExpr = GraphTypeBuilder.CreateObjectGraphAssignPropertyExpression(
                entityPredicateTypeVariableExpr, entityPredicateGraphType,
                nameof(InputObjectGraphType.Description), () => GetEntityPredicateTypeDescription<TEntity>());
            bodyExprs.Add(assignEntityPredicateTypeDescriptionExpr);

            // entityPredicateType.Field("_and", new ListGraphType(entityPredicateType));
            var newListEntityPredicateTypeExpr = Expression.New(typeof(ListGraphType).GetConstructors()[0], entityPredicateTypeVariableExpr);
            var objectGraphFieldByGraphExtMethodParamExprs = BuildObjectGraphFieldByGraphExtMethodParamExpressions(_objectGraphFieldByGraphExtensionMethod,
                entityPredicateTypeVariableExpr, Expression.Constant("_and"), newListEntityPredicateTypeExpr);
            var predicateAndFieldTypeExpr = Expression.Call(_objectGraphFieldByGraphExtensionMethod, objectGraphFieldByGraphExtMethodParamExprs);
            bodyExprs.Add(predicateAndFieldTypeExpr);

            // entityPredicateType.Field("_or", new ListGraphType(entityPredicateType));            
            objectGraphFieldByGraphExtMethodParamExprs = BuildObjectGraphFieldByGraphExtMethodParamExpressions(_objectGraphFieldByGraphExtensionMethod,
                entityPredicateTypeVariableExpr, Expression.Constant("_or"), newListEntityPredicateTypeExpr);
            var predicateOrFieldTypeExpr = Expression.Call(_objectGraphFieldByGraphExtensionMethod, objectGraphFieldByGraphExtMethodParamExprs);
            bodyExprs.Add(predicateOrFieldTypeExpr);

            if (properties.IsNotEmpty())
            {
                foreach (var property in properties)
                {
                    // entityPredicateType.Field<ComparisonExpr_Type>(property.Name)
                    var propertyComparisonExprGraphType = property.PropertyInfo.PropertyType.GetComparisonExprGraphType();

                    var predicateComparisonFieldTypeExpr = GraphTypeBuilder.CreateObjectGraphFieldByGraphExpression(
                        entityPredicateTypeVariableExpr, propertyComparisonExprGraphType, property);
                    bodyExprs.Add(predicateComparisonFieldTypeExpr);
                }
            }

            // code: return (InputObjectGraphType)entityPredicateType;
            var castResultExpr = Expression.Convert(entityPredicateTypeVariableExpr, entityPredicateGraphType);
            bodyExprs.Add(castResultExpr);

            var factoryBodyExpr = Expression.Block(
                entityPredicateGraphType, /* return type */
                new[] { entityPredicateTypeVariableExpr } /* local variables */,
                bodyExprs /* body expressions */);

            return Expression.Lambda<Func<InputObjectGraphType>>(factoryBodyExpr);
        }
        #endregion
    }
}
