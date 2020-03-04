using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace HEF.GraphQL.ResourceQuery
{
    internal static class LambdaExpressionCache
    {
        private static readonly ConcurrentDictionary<string, LambdaExpression> _lambdaExpressionCache
            = new ConcurrentDictionary<string, LambdaExpression>();

        internal static Expression<TDelegate> GetLambdaExpression<TDelegate>(string expressionKey,
            Func<string, LambdaExpression> expressionFactory)
        {
            var lambdaExpr = _lambdaExpressionCache.GetOrAdd(expressionKey, expressionFactory);

            if (lambdaExpr is Expression<TDelegate> delegateExpr)
                return delegateExpr;

            throw new InvalidOperationException($"lambda expression cast to {typeof(Expression<TDelegate>)} failed");
        }
    }
}
