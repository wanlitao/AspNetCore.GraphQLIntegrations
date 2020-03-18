using GraphQL;
using HEF.Data.Query;
using HEF.Entity.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HEF.GraphQL.EntityQuery
{
    internal static class ComparisonExpressionBuilder
    {
        internal static Func<IPropertyMap, ConstantExpression> BuildComparisonValueGetter(object comparisonValue)
        {
            return (property) =>
            {
                if (comparisonValue == null)
                    throw new ArgumentNullException(nameof(comparisonValue));

                return Expression.Constant(comparisonValue, property.PropertyInfo.PropertyType);
            };
        }

        internal static Func<IPropertyMap, ConstantExpression> BuildEnumerableComparisonValueGetter(object comparisonValue)
        {
            return (property) =>
            {
                if (comparisonValue == null)
                    throw new ArgumentNullException(nameof(comparisonValue));

                var enumerablePropertyType = typeof(IEnumerable<>).MakeGenericType(property.PropertyInfo.PropertyType);
                var enumerableComparisonValue = comparisonValue.GetPropertyValue(enumerablePropertyType);

                return Expression.Constant(enumerableComparisonValue, enumerablePropertyType);
            };
        }

        internal static Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue,
            Func<Expression, Expression, Expression> propertyComparisonOperation)
        {
            return BuildPropertyComparisonExpression(entityParameter, property,
                BuildComparisonValueGetter(comparisonValue), propertyComparisonOperation);
        }

        internal static Expression BuildPropertyComparisonExpression(
            ParameterExpression entityParameter, IPropertyMap property,
            Func<IPropertyMap, ConstantExpression> comparisonValueGetter,
            Func<Expression, Expression, Expression> propertyComparisonOperation)
        {
            if (entityParameter == null)
                throw new ArgumentNullException(nameof(entityParameter));

            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var propertyExpr = Expression.Property(entityParameter, property.PropertyInfo);
            var comparisonValueExpr = comparisonValueGetter(property);

            return propertyComparisonOperation(propertyExpr, comparisonValueExpr);
        }
    }

    public class EqualComparisonExpressionBuilder : IComparisonExpressionBuilder
    {
        public string ComparisonType => "_eq";

        public Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue)
        {
            return ComparisonExpressionBuilder.BuildPropertyComparisonExpression(entityParameter,
                property, comparisonValue, Expression.Equal);
        }
    }

    public class GreaterThanComparisonExpressionBuilder : IComparisonExpressionBuilder
    {
        public string ComparisonType => "_gt";

        public Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue)
        {
            return ComparisonExpressionBuilder.BuildPropertyComparisonExpression(entityParameter,
                property, comparisonValue, Expression.GreaterThan);
        }
    }

    public class GreaterThanOrEqualComparisonExpressionBuilder : IComparisonExpressionBuilder
    {
        public string ComparisonType => "_gte";

        public Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue)
        {
            return ComparisonExpressionBuilder.BuildPropertyComparisonExpression(entityParameter,
                property, comparisonValue, Expression.GreaterThanOrEqual);
        }
    }

    public class LessThanComparisonExpressionBuilder : IComparisonExpressionBuilder
    {
        public string ComparisonType => "_lt";

        public Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue)
        {
            return ComparisonExpressionBuilder.BuildPropertyComparisonExpression(entityParameter,
                property, comparisonValue, Expression.LessThan);
        }
    }

    public class LessThanOrEqualComparisonExpressionBuilder : IComparisonExpressionBuilder
    {
        public string ComparisonType => "_lte";

        public Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue)
        {
            return ComparisonExpressionBuilder.BuildPropertyComparisonExpression(entityParameter,
                property, comparisonValue, Expression.LessThanOrEqual);
        }
    }

    public class IsNullComparisonExpressionBuilder : IComparisonExpressionBuilder
    {
        public string ComparisonType => "_is_null";

        private static Func<Expression, Expression, Expression> GetNullComparisonOperation(object comparisonValue)
        {
            if (true.Equals(comparisonValue))
                return Expression.Equal;

            return Expression.NotEqual;
        }

        public Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue)
        {
            return ComparisonExpressionBuilder.BuildPropertyComparisonExpression(entityParameter,
                    property, (property) => Expression.Constant(null), GetNullComparisonOperation(comparisonValue));
        }
    }

    public class NotEqualComparisonExpressionBuilder : IComparisonExpressionBuilder
    {
        public string ComparisonType => "_neq";

        public Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue)
        {
            return ComparisonExpressionBuilder.BuildPropertyComparisonExpression(entityParameter,
                property, comparisonValue, Expression.NotEqual);
        }
    }

    public class InComparisonExpressionBuilder : IComparisonExpressionBuilder
    {
        public string ComparisonType => "_in";

        public Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue)
        {
            var enumerableContainsMethod = EnumerableMethods.Contains.MakeGenericMethod(property.PropertyInfo.PropertyType);

            return ComparisonExpressionBuilder.BuildPropertyComparisonExpression(
                entityParameter, property, ComparisonExpressionBuilder.BuildEnumerableComparisonValueGetter(comparisonValue),
                (propertyExpr, comparisonValueExpr) => Expression.Call(enumerableContainsMethod, comparisonValueExpr, propertyExpr));
        }
    }

    public class NotInComparisonExpressionBuilder : IComparisonExpressionBuilder
    {
        public string ComparisonType => "_nin";

        public Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue)
        {
            var enumerableContainsMethod = EnumerableMethods.Contains.MakeGenericMethod(property.PropertyInfo.PropertyType);

            return ComparisonExpressionBuilder.BuildPropertyComparisonExpression(
                entityParameter, property, ComparisonExpressionBuilder.BuildEnumerableComparisonValueGetter(comparisonValue),
                (propertyExpr, comparisonValueExpr) => Expression.Not(Expression.Call(enumerableContainsMethod, comparisonValueExpr, propertyExpr)));
        }
    }

    public class PrefixLikeComparisonExpressionBuilder : IComparisonExpressionBuilder
    {
        public string ComparisonType => "_prelike";

        private static readonly MethodInfo _stringStartsWithMethod
            = typeof(string).GetTypeInfo()
                .GetDeclaredMethods(nameof(string.StartsWith))
                .Single(m =>
                {
                    var parameters = m.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
                });

        public Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue)
        {
            return ComparisonExpressionBuilder.BuildPropertyComparisonExpression(
                entityParameter, property, comparisonValue,
                (propertyExpr, comparisonValueExpr) => Expression.Call(propertyExpr, _stringStartsWithMethod, comparisonValueExpr));
        }
    }

    public class LikeComparisonExpressionBuilder : IComparisonExpressionBuilder
    {
        public string ComparisonType => "_like";

        private static readonly MethodInfo _stringContainsMethod
            = typeof(string).GetTypeInfo()
                .GetDeclaredMethods(nameof(string.Contains))
                .Single(m =>
                {
                    var parameters = m.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
                });

        public Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue)
        {
            return ComparisonExpressionBuilder.BuildPropertyComparisonExpression(
                entityParameter, property, comparisonValue,
                (propertyExpr, comparisonValueExpr) => Expression.Call(propertyExpr, _stringContainsMethod, comparisonValueExpr));
        }
    }

    public class SuffixLikeComparisonExpressionBuilder : IComparisonExpressionBuilder
    {
        public string ComparisonType => "_suflike";

        private static readonly MethodInfo _stringEndsWithMethod
            = typeof(string).GetTypeInfo()
                .GetDeclaredMethods(nameof(string.EndsWith))
                .Single(m =>
                {
                    var parameters = m.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
                });

        public Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue)
        {
            return ComparisonExpressionBuilder.BuildPropertyComparisonExpression(
                entityParameter, property, comparisonValue,
                (propertyExpr, comparisonValueExpr) => Expression.Call(propertyExpr, _stringEndsWithMethod, comparisonValueExpr));
        }
    }
}
