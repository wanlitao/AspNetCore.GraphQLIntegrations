using GraphQL.Builders;
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
    public class EntityGraphTypeBuilder : IEntityGraphTypeBuilder
    {
        public EntityGraphTypeBuilder(IEntityMapperProvider mapperProvider)
        {
            MapperProvider = mapperProvider ?? throw new ArgumentNullException(nameof(mapperProvider));
        }

        protected IEntityMapperProvider MapperProvider { get; }

        public ObjectGraphType<TEntity> Build<TEntity>() where TEntity : class
        {
            var entityMapper = MapperProvider.GetEntityMapper<TEntity>();

            var entityGraphTypeFactory = LambdaExpressionCache.GetLambdaExpression<Func<ObjectGraphType<TEntity>>>(
                $"{GetEntityGraphTypeName<TEntity>()}_Factory",
                (key) => BuildEntityGraphTypeFactory<TEntity>(entityMapper.Properties.ToArray()));

            return entityGraphTypeFactory.Compile().Invoke();
        }

        #region Helper Functions
        protected virtual string GetEntityGraphTypeName<TEntity>() where TEntity : class
            => $"{typeof(TEntity).Name}_Type";

        protected virtual string GetEntityGraphTypeFieldDescription<TEntity>(IPropertyMap property) where TEntity : class
            => $"The {property.Name} of the {typeof(TEntity).Name}.";

        protected static MethodInfo GetObjectGraphFieldByPropertyExpressionMethod<TEntity>() where TEntity : class
            => typeof(ComplexGraphType<TEntity>).GetTypeInfo()
                .GetDeclaredMethods(nameof(ObjectGraphType.Field))
                .Single(mi =>
                {
                    var parameters = mi.GetParameters();
                    return parameters.Length == 3 && typeof(LambdaExpression).IsAssignableFrom(parameters[0].ParameterType);
                });

        protected static MethodInfo GetFieldBuilderDescriptionMethod<TEntity>(Type propertyType) where TEntity : class
            => typeof(FieldBuilder<,>).MakeGenericType(typeof(TEntity), propertyType).GetTypeInfo()
                .GetDeclaredMethods(nameof(FieldBuilder<object, object>.Description))
                .Single();

        protected static IEnumerable<Expression> BuildObjectGraphFieldByPropertyMethodParamExpressions<TEntity>(
            MethodInfo objectGraphFieldByPropertyMethod, IPropertyMap property)
            where TEntity : class
        {
            var objectGraphFieldByPropertyMethodParameters = objectGraphFieldByPropertyMethod.GetParameters();

            foreach(var methodParameter in objectGraphFieldByPropertyMethodParameters)
            {
                if (typeof(LambdaExpression).IsAssignableFrom(methodParameter.ParameterType))
                {
                    yield return ExpressionFactory.BuildEntityPropertyExpression<TEntity>(property);
                    continue;
                }                
                yield return Expression.Constant(methodParameter.DefaultValue, methodParameter.ParameterType);                
            }
        }

        protected Expression<Func<ObjectGraphType<TEntity>>> BuildEntityGraphTypeFactory<TEntity>(params IPropertyMap[] properties)
            where TEntity : class
        {
            var entityGraphType = typeof(ObjectGraphType<TEntity>);

            // collect the body
            var bodyExprs = new List<Expression>();

            // var entityGraphType = new ObjectGraphType<TEntity>();
            var entityGraphTypeVariableExpr = Expression.Variable(entityGraphType, "entityGraphType");
            var newEntityGraphTypeExpr = Expression.New(entityGraphType);
            var assignEntityGraphTypeVariableExpr = Expression.Assign(entityGraphTypeVariableExpr, newEntityGraphTypeExpr);
            bodyExprs.Add(assignEntityGraphTypeVariableExpr);

            // entityGraphType.Name = $"{EntityName}_Type";
            var assignEntityGraphTypeNameExpr = GraphTypeBuilder.CreateObjectGraphAssignPropertyExpression(
                entityGraphTypeVariableExpr, entityGraphType, nameof(ObjectGraphType.Name), () => GetEntityGraphTypeName<TEntity>());
            bodyExprs.Add(assignEntityGraphTypeNameExpr);

            if (properties.IsNotEmpty())
            {
                foreach (var property in properties)
                {
                    var propertyType = property.PropertyInfo.PropertyType;

                    // entityGraphType.Field(entity => entity.Property)
                    var objectGraphFieldByPropertyMethod = GetObjectGraphFieldByPropertyExpressionMethod<TEntity>().MakeGenericMethod(propertyType);
                    var objectGraphFieldByPropertyMethodParamExprs = 
                        BuildObjectGraphFieldByPropertyMethodParamExpressions<TEntity>(objectGraphFieldByPropertyMethod, property);
                    var fieldBuilderExpr = Expression.Call(entityGraphTypeVariableExpr, objectGraphFieldByPropertyMethod,
                        objectGraphFieldByPropertyMethodParamExprs);

                    // ObjectGraphType.Field(entity => entity.Property).Description("The PropertyName of the EntityName.")
                    var fieldBuilderDescriptionMethod = GetFieldBuilderDescriptionMethod<TEntity>(propertyType);
                    var fieldDescriptionExpr = Expression.Constant(GetEntityGraphTypeFieldDescription<TEntity>(property));
                    fieldBuilderExpr = Expression.Call(fieldBuilderExpr, fieldBuilderDescriptionMethod, fieldDescriptionExpr);

                    bodyExprs.Add(fieldBuilderExpr);
                }
            }

            // code: return (ObjectGraphType<TEntity>)entityGraphType;
            var castResultExpr = Expression.Convert(entityGraphTypeVariableExpr, entityGraphType);
            bodyExprs.Add(castResultExpr);

            var factoryBodyExpr = Expression.Block(
                entityGraphType, /* return type */
                new[] { entityGraphTypeVariableExpr } /* local variables */,
                bodyExprs /* body expressions */);
            
            return Expression.Lambda<Func<ObjectGraphType<TEntity>>>(factoryBodyExpr);
        }
        #endregion
    }
}