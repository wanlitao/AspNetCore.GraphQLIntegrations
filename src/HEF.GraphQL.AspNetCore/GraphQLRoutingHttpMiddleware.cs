using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HEF.GraphQL.AspNetCore
{
    public class GraphQLRoutingHttpMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly PathString _prefixPath;
        private readonly PathString _graphqlPath;

        public GraphQLRoutingHttpMiddleware(RequestDelegate next, PathString prefixPath, PathString graphqlPath)
        {
            _next = next;

            _prefixPath = prefixPath;
            _graphqlPath = graphqlPath;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest
                || !context.Request.Path.StartsWithSegments(_prefixPath, out PathString remainingPath)
                || !remainingPath.EndsWithSegments(_graphqlPath, out PathString remainingPath2))
            {
                await _next(context);
                return;
            }

            var packageName = remainingPath2.Value.Trim('/');
        }
    }
}
