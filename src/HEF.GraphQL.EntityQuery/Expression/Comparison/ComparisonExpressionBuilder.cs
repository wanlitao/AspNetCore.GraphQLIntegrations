using HEF.Entity.Mapper;
using System.Linq.Expressions;
using System;
using System.Reflection;
using System.Linq;

namespace HEF.GraphQL.EntityQuery
{
    internal static class ComparisonExpressionBuilder
    {
        private static Func<IPropertyMap, ConstantExpression> BuildComparisonValueGetter(object comparisonValue)
        {
            return (property) =>
            {
                if (comparisonValue == null)
                    throw new ArgumentNullException(nameof(comparisonValue));

                return Expression.Constant(comparisonValue, property.PropertyInfo.PropertyType);
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

        public Expression BuildPropertyComparisonExpression(ParameterExpression entityParameter,
            IPropertyMap property, object comparisonValue)
        {
            return ComparisonExpressionBuilder.BuildPropertyComparisonExpression(entityParameter,
                property, (property) => Expression.Constant(null), Expression.Equal);
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
