using HEF.GraphQL.EntityQuery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GraphQL.Server
{
    public static class GraphQLBuilderExtensions
    {
        public static IGraphQLBuilder AddEntityGraphQuery(this IGraphQLBuilder builder)
        {
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
