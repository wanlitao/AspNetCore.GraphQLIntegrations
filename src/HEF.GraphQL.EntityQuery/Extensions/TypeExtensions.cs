using System;
using System.Collections.Generic;

namespace HEF.GraphQL.EntityQuery
{
    internal static class TypeExtensions
    {
        private static readonly IDictionary<Type, Type> _comparisonExprGraphTypeMapping = new Dictionary<Type, Type>();

        static TypeExtensions()
        {
            InitComparisonExprGraphMapping();
        }

        private static void InitComparisonExprGraphMapping()
        {
            _comparisonExprGraphTypeMapping.Add(typeof(int), typeof(IntComparisonExpr_Type));
            _comparisonExprGraphTypeMapping.Add(typeof(long), typeof(LongComparisonExpr_Type));
            _comparisonExprGraphTypeMapping.Add(typeof(decimal), typeof(DecimalComparisonExpr_Type));
            _comparisonExprGraphTypeMapping.Add(typeof(DateTime), typeof(DateTimeComparisonExpr_Type));
            _comparisonExprGraphTypeMapping.Add(typeof(string), typeof(StringComparisonExpr_Type));
        }

        internal static Type GetComparisonExprGraphType(this Type type)
        {
            if (_comparisonExprGraphTypeMapping.TryGetValue(type, out Type comparisonExprType))
            {
                return comparisonExprType;
            }
            
            throw new NotSupportedException($"The comparison of '{type}' is not supported");
        }
    }
}
