using GraphQL.Types;
using HEF.Entity.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HEF.GraphQL.EntityQuery
{
    internal static class GraphTypeBuilder
    {
        #region ObjectGraphAssignProperty
        internal static Expression CreateObjectGraphAssignPropertyExpression(
            ParameterExpression graphTypeVariableExpr,
            Type graphType, string propertyName, Func<object> propertyValueGetter)
        {
            var graphTypeProperty = graphType.GetProperty(propertyName);
            if (graphTypeProperty == null)
                throw new ArgumentException($"not found property '{propertyName}' from the {graphType}");

            var graphTypePropertyExpr = Expression.Property(graphTypeVariableExpr, graphTypeProperty);
            var graphTypePropertyValueExpr = Expression.Constant(propertyValueGetter.Invoke(), graphTypeProperty.PropertyType);

            return Expression.Assign(graphTypePropertyExpr, graphTypePropertyValueExpr);
        }
        #endregion

        #region ObjectGraphFieldByGraph
        private static readonly MethodInfo _objectGraphFieldByGenericGraphMethod
            = typeof(ComplexGraphType<object>).GetTypeInfo()
                .GetDeclaredMethods(nameof(ObjectGraphType.Field))
                .Single(mi => mi.IsGenericMethod && mi.ReturnType == typeof(FieldType));

        private static IEnumerable<Expression> BuildObjectGraphFieldByGraphMethodParamExpressions(
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

        internal static Expression CreateObjectGraphFieldByGraphExpression(
            ParameterExpression graphTypeVariableExpr,
            Type propertyGraphType, IPropertyMap property)
        {
            var objectGraphFieldByGraphMethod = _objectGraphFieldByGenericGraphMethod.MakeGenericMethod(propertyGraphType);
            var objectGraphFieldByGraphMethodParamExprs = BuildObjectGraphFieldByGraphMethodParamExpressions(objectGraphFieldByGraphMethod, property);

            return Expression.Call(graphTypeVariableExpr, objectGraphFieldByGraphMethod, objectGraphFieldByGraphMethodParamExprs);
        }
        #endregion
    }
}