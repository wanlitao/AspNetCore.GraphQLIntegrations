using GraphQL.Types;
using HEF.Entity.Mapper;
using HEF.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HEF.GraphQL.ResourceQuery
{
    public class EntityGraphQueryArgumentsBuilder : IEntityGraphQueryArgumentsBuilder
    {
        protected static readonly QueryArgument LimitQueryArgument = new QueryArgument<IntGraphType> { Name = "limit" };
        protected static readonly QueryArgument OffsetQueryArgument = new QueryArgument<IntGraphType> { Name = "offset" };

        public EntityGraphQueryArgumentsBuilder(IEntityMapperProvider mapperProvider)
        {
            MapperProvider = mapperProvider ?? throw new ArgumentNullException(nameof(mapperProvider));
        }

        protected IEntityMapperProvider MapperProvider { get; }

        public QueryArgument BuildOrderBy<TEntity>() where TEntity : class
        {
            var entityMapper = MapperProvider.GetEntityMapper<TEntity>();

            var entityOrderByGraphTypeFactory = LambdaExpressionCache.GetLambdaExpression<Func<InputObjectGraphType>>(
                $"{GetEntityOrderByTypeName<TEntity>()}_Factory",
                (key) => BuildEntityOrderByGraphTypeFactory<TEntity>(entityMapper.Properties.ToArray()));

            var entityOrderByType = entityOrderByGraphTypeFactory.Compile().Invoke();
            return new QueryArgument(new ListGraphType(new NonNullGraphType(entityOrderByType))) { Name = "order_by" };
        }

        public QueryArgument BuildPredicate<TEntity>() where TEntity : class
        {
            var entityMapper = MapperProvider.GetEntityMapper<TEntity>();

            var entityPredicateGraphTypeFactory = LambdaExpressionCache.GetLambdaExpression<Func<InputObjectGraphType>>(
                $"{GetEntityPredicateTypeName<TEntity>()}_Factory",
                (key) => BuildEntityPredicateGraphTypeFactory<TEntity>(entityMapper.Properties.ToArray()));

            var entityPredicateType = entityPredicateGraphTypeFactory.Compile().Invoke();
            return new QueryArgument(entityPredicateType) { Name = "where" };
        }

        public QueryArguments Build<TEntity>() where TEntity : class
        {
            return new QueryArguments(
                    LimitQueryArgument,
                    OffsetQueryArgument,
                    BuildOrderBy<TEntity>(),
                    BuildPredicate<TEntity>()
                );
        }

        #region Helper Functions
        protected static Expression CreateObjectGraphAssignPropertyExpression(
            ParameterExpression graphTypeVariableExpr,
            Type graphType, string propertyName, Func<object> propertyValueGetter)
        {
            var graphTypeProperty = graphType.GetProperty(propertyName);
            if (graphTypeProperty == null)
                throw new ArgumentException($"not found property '{propertyName}' from the {graphType.Name}");

            var graphTypePropertyExpr = Expression.Property(graphTypeVariableExpr, graphTypeProperty);
            var graphTypePropertyValueExpr = Expression.Constant(propertyValueGetter.Invoke(), graphTypeProperty.PropertyType);

            return Expression.Assign(graphTypePropertyExpr, graphTypePropertyValueExpr);
        }

        protected static Expression CreateObjectGraphFieldByGraphExpression(
            ParameterExpression graphTypeVariableExpr,
            Type propertyGraphType, IPropertyMap property)
        {
            var objectGraphFieldByGraphMethod = _objectGraphFieldByGenericGraphMethod.MakeGenericMethod(propertyGraphType);
            var objectGraphFieldByGraphMethodParamExprs = BuildObjectGraphFieldByGraphMethodParamExpressions(objectGraphFieldByGraphMethod, property);

            return Expression.Call(graphTypeVariableExpr, objectGraphFieldByGraphMethod, objectGraphFieldByGraphMethodParamExprs);
        }
        #endregion

        #region OrderBy
        protected virtual string GetEntityOrderByTypeName<TEntity>() where TEntity : class
            => $"{typeof(TEntity).Name}_OrderBy_Type";

        protected virtual string GetEntityOrderByTypeDescription<TEntity>() where TEntity : class
            => $"ordering options when selecting data from {typeof(TEntity).Name}";

        private static readonly MethodInfo _objectGraphFieldByGenericGraphMethod
            = typeof(ComplexGraphType<object>).GetTypeInfo()
                .GetDeclaredMethods(nameof(ObjectGraphType.Field))
                .Single(mi => mi.IsGenericMethod && mi.ReturnType == typeof(FieldType));

        protected static IEnumerable<Expression> BuildObjectGraphFieldByGraphMethodParamExpressions(
            MethodInfo objectGraphFieldByGraphMethod, IPropertyMap property)
        {
            var objectGraphFieldByGraphMethodParameters = objectGraphFieldByGraphMethod.GetParameters();

            foreach (var methodParameter in objectGraphFieldByGraphMethodParameters)
            {
                if (methodParameter.Position == 0 && !methodParameter.HasDefaultValue)
                {
                    yield return Expression.Constant(property.Name);
                    continue;
                }
                yield return Expression.Constant(methodParameter.DefaultValue, methodParameter.ParameterType);
            }
        }

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
            var assignEntityOrderByTypeNameExpr = CreateObjectGraphAssignPropertyExpression(entityOrderByTypeVariableExpr,
                entityOrderByGraphType, nameof(InputObjectGraphType.Name), () => GetEntityOrderByTypeName<TEntity>());
            bodyExprs.Add(assignEntityOrderByTypeNameExpr);

            // entityOrderByType.Description = $"ordering options when selecting data from {EntityName}";
            var assignEntityOrderByTypeDescriptionExpr = CreateObjectGraphAssignPropertyExpression(entityOrderByTypeVariableExpr,
                entityOrderByGraphType, nameof(InputObjectGraphType.Description), () => GetEntityOrderByTypeDescription<TEntity>());
            bodyExprs.Add(assignEntityOrderByTypeDescriptionExpr);

            if (properties.IsNotEmpty())
            {
                foreach (var property in properties)
                {
                    // entityOrderByType.Field<OrderBy_Type>(property.Name)
                    var fieldTypeExpr = CreateObjectGraphFieldByGraphExpression(entityOrderByTypeVariableExpr, typeof(OrderBy_Type), property);
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

        #region Predicate
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
            foreach(var parameterExpr in paramWithoutDefaultValueExprs)
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
            var assignEntityPredicateTypeNameExpr = CreateObjectGraphAssignPropertyExpression(entityPredicateTypeVariableExpr,
                entityPredicateGraphType, nameof(InputObjectGraphType.Name), () => GetEntityPredicateTypeName<TEntity>());
            bodyExprs.Add(assignEntityPredicateTypeNameExpr);

            // entityPredicateType.Description = $"Boolean expression to filter rows from the resource {EntityName}. All fields are combined with a logical 'AND'.";
            var assignEntityPredicateTypeDescriptionExpr = CreateObjectGraphAssignPropertyExpression(entityPredicateTypeVariableExpr,
                entityPredicateGraphType, nameof(InputObjectGraphType.Description), () => GetEntityPredicateTypeDescription<TEntity>());
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

                    var predicateComparisonFieldTypeExpr = CreateObjectGraphFieldByGraphExpression(
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