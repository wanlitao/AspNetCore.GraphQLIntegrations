using GraphQL.Types;
using GraphQL.Utilities;
using HEF.GraphQL.EntityQuery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace GraphQL.Server
{
    public static class GraphQLBuilderExtensions
    {
        public static IGraphQLBuilder AddEntityGraphQuery(this IGraphQLBuilder builder)
        {
            GraphTypeTypeRegistry.Register<DateTime, DateTimeGraphType>(); //fix DateTime GraphType mapping
            builder.Services.TryAddSingleton<IEntityGraphTypeBuilder, EntityGraphTypeBuilder>();

            builder.AddEntityGraphQueryTypes();

            builder.AddEntityGraphQueryArguments();

            builder.AddEntityGraphQueryResolver();

            return builder;
        }

        public static IGraphQLBuilder AddEntityGraphQueryTypes(this IGraphQLBuilder builder)
        {
            builder.Services.TryAddSingleton<OrderBy_Type>();
            builder.Services.TryAddSingleton<IntComparisonExpr_Type>();
            builder.Services.TryAddSingleton<LongComparisonExpr_Type>();
            builder.Services.TryAddSingleton<StringComparisonExpr_Type>();
            builder.Services.TryAddSingleton<DecimalComparisonExpr_Type>();
            builder.Services.TryAddSingleton<DateTimeComparisonExpr_Type>();

            return builder;
        }

        public static IGraphQLBuilder AddEntityGraphQueryArguments(this IGraphQLBuilder builder)
        {
            builder.Services.AddSingleton<IEntityGraphQueryArgumentBuilder, LimitGraphQueryArgumentBuilder>();
            builder.Services.AddSingleton<IEntityGraphQueryArgumentBuilder, OffsetGraphQueryArgumentBuilder>();
            builder.Services.AddSingleton<IEntityGraphQueryArgumentBuilder, OrderByGraphQueryArgumentBuilder>();
            builder.Services.AddSingleton<IEntityGraphQueryArgumentBuilder, PredicateGraphQueryArgumentBuilder>();

            builder.Services.TryAddSingleton<IEntityGraphQueryArgumentsBuilder, EntityGraphQueryArgumentsBuilder>();

            return builder;
        }

        public static IGraphQLBuilder AddEntityGraphQueryResolver(this IGraphQLBuilder builder)
        {
            #region ComparisonExpression
            builder.Services.AddSingleton<IComparisonExpressionBuilder, EqualComparisonExpressionBuilder>();
            builder.Services.AddSingleton<IComparisonExpressionBuilder, GreaterThanComparisonExpressionBuilder>();
            builder.Services.AddSingleton<IComparisonExpressionBuilder, GreaterThanOrEqualComparisonExpressionBuilder>();
            builder.Services.AddSingleton<IComparisonExpressionBuilder, LessThanComparisonExpressionBuilder>();
            builder.Services.AddSingleton<IComparisonExpressionBuilder, LessThanOrEqualComparisonExpressionBuilder>();
            builder.Services.AddSingleton<IComparisonExpressionBuilder, IsNullComparisonExpressionBuilder>();
            builder.Services.AddSingleton<IComparisonExpressionBuilder, InComparisonExpressionBuilder>();
            builder.Services.AddSingleton<IComparisonExpressionBuilder, NotEqualComparisonExpressionBuilder>();
            builder.Services.AddSingleton<IComparisonExpressionBuilder, NotInComparisonExpressionBuilder>();
            builder.Services.AddSingleton<IComparisonExpressionBuilder, PrefixLikeComparisonExpressionBuilder>();
            builder.Services.AddSingleton<IComparisonExpressionBuilder, LikeComparisonExpressionBuilder>();
            builder.Services.AddSingleton<IComparisonExpressionBuilder, SuffixLikeComparisonExpressionBuilder>();

            builder.Services.TryAddSingleton<IComparisonExpressionFactory, ComparisonExpressionFactory>();
            #endregion

            builder.Services.AddSingleton<IEntityGraphQueryMiddlewareBuilder, LimitGraphQueryMiddlewareBuilder>();
            builder.Services.AddSingleton<IEntityGraphQueryMiddlewareBuilder, OffsetGraphQueryMiddlewareBuilder>();
            builder.Services.AddSingleton<IEntityGraphQueryMiddlewareBuilder, OrderByGraphQueryMiddlewareBuilder>();
            builder.Services.AddSingleton<IEntityGraphQueryMiddlewareBuilder, PredicateGraphQueryMiddlewareBuilder>();

            builder.Services.TryAddScoped<IEntityGraphQueryBuilderFactory, EntityGraphQueryBuilderFactory>();
            builder.Services.TryAddScoped<IEntityGraphQueryResolver, EntityGraphQueryResolver>();

            return builder;
        }
    }
}
