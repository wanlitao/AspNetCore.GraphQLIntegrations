﻿using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRoutingGraphQL<TSchema>(this IApplicationBuilder builder,
            string startPath, string endPath = "/graphql")
            where TSchema : ISchema
            => builder.UseRoutingGraphQL<TSchema>(new PathString(startPath), new PathString(endPath), null);        

        public static IApplicationBuilder UseRoutingGraphQL<TSchema>(this IApplicationBuilder builder,
            PathString startPath, PathString endPath,
            Action<HttpContext> contextSettings)
            where TSchema : ISchema
        {
            var routingPredicate = BuildRoutingPredicate(startPath, endPath);

            return builder.UseMapWhenGraphQL<TSchema>(routingPredicate, context => context.Request.Path, contextSettings);
        }

        public static IApplicationBuilder UseMapWhenGraphQL<TSchema>(this IApplicationBuilder builder,
            Func<HttpContext, bool> predicate,
            Func<HttpContext, PathString> pathGetter,
            Action<HttpContext> contextSettings)
            where TSchema : ISchema
        {
            return builder.MapWhen(predicate, app =>
                {
                    app.Use(next =>
                    {
                        return context =>
                        {
                            var path = pathGetter(context);

                            contextSettings?.Invoke(context);

                            var graphqlDelegate = app.New().UseGraphQL<TSchema>(path).Build();
                            return graphqlDelegate(context);
                        };
                    });
                });
        }

        private static Func<HttpContext, bool> BuildRoutingPredicate(PathString startPath, PathString endPath)
        {
            return (context) =>
            {
                return !context.WebSockets.IsWebSocketRequest
                 && context.Request.Path.StartsWithSegments(startPath, out PathString remainingPath)
                 && remainingPath.EndsWithSegments(endPath);
            };
        }
    }
}
