using GraphQL.Server.Ui.Playground;
using Microsoft.AspNetCore.Http;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRoutingGraphQLPlayground(this IApplicationBuilder builder,
            string startPath, string endPath = "/ui/playground")
            => builder.UseRoutingGraphQLPlayground(new PathString(startPath), new PathString(endPath));

        public static IApplicationBuilder UseRoutingGraphQLPlayground(this IApplicationBuilder builder,
            PathString startPath, PathString endPath)
        {
            var routingPredicate = BuildRoutingPredicate(startPath, endPath);
            var graphqlPlaygroundOptionsGetter = BuildRoutingGraphQLPlaygroundOptionsGetter(endPath);

            return builder.UseMapWhenGraphQLPlayground(routingPredicate, graphqlPlaygroundOptionsGetter);
        }

        public static IApplicationBuilder UseMapWhenGraphQLPlayground(this IApplicationBuilder builder,
            Func<HttpContext, bool> predicate,
            Func<HttpContext, GraphQLPlaygroundOptions> graphqlPlaygroundOptionsGetter)
        {
            return builder.MapWhen(predicate, app =>
            {
                app.Use(next =>
                {
                    return context =>
                    {
                        var graphqlPlaygroundOptions = graphqlPlaygroundOptionsGetter(context);

                        var graphqlPlaygroundDelegate = app.New().UseGraphQLPlayground(graphqlPlaygroundOptions).Build();
                        return graphqlPlaygroundDelegate(context);
                    };
                });
            });
        }

        private static Func<HttpContext, bool> BuildRoutingPredicate(PathString startPath, PathString endPath)
        {
            return (context) =>
            {
                return !context.WebSockets.IsWebSocketRequest
                 && context.Request.Path.IsRouting(startPath, endPath);
            };
        }

        private static Func<HttpContext, GraphQLPlaygroundOptions> BuildRoutingGraphQLPlaygroundOptionsGetter(PathString endPath)
        {
            return (context) =>
            {
                return new GraphQLPlaygroundOptions
                {
                    Path = context.Request.Path,
                    GraphQLEndPoint = context.Request.Path.EndReplace(endPath, new PathString("/graphql"))
                };
            };
        }
    }
}
