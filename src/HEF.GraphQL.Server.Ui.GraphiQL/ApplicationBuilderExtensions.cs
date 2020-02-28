using GraphQL.Server.Ui.GraphiQL;
using Microsoft.AspNetCore.Http;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRoutingGraphiQLServer(this IApplicationBuilder builder,
            string startPath, string endPath = "/graphiql")
            => builder.UseRoutingGraphiQLServer(new PathString(startPath), new PathString(endPath));

        public static IApplicationBuilder UseRoutingGraphiQLServer(this IApplicationBuilder builder,
            PathString startPath, PathString endPath)
        {
            var routingPredicate = BuildRoutingPredicate(startPath, endPath);
            var graphiqlOptionsGetter = BuildRoutingGraphiQLOptionsGetter(endPath);

            return builder.UseMapWhenGraphiQLServer(routingPredicate, graphiqlOptionsGetter);
        }

        public static IApplicationBuilder UseMapWhenGraphiQLServer(this IApplicationBuilder builder,
            Func<HttpContext, bool> predicate,
            Func<HttpContext, GraphiQLOptions> graphiqlOptionsGetter)
        {
            return builder.MapWhen(predicate, app =>
            {
                app.Use(next =>
                {
                    return context =>
                    {
                        var graphiqlOptions = graphiqlOptionsGetter(context);

                        var graphiqlDelegate = app.New().UseGraphiQLServer(graphiqlOptions).Build();
                        return graphiqlDelegate(context);
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

        private static Func<HttpContext, GraphiQLOptions> BuildRoutingGraphiQLOptionsGetter(PathString endPath)
        {
            return (context) =>
            {
                return new GraphiQLOptions
                {
                    Path = context.Request.Path,
                    GraphQLEndPoint = context.Request.Path.EndReplace(endPath, new PathString("/graphql"))
                };
            };
        }
    }
}
