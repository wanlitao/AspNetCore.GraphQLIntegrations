using GraphQL.Server.Internal;
using HEF.GraphQL.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GraphQL.Server
{
    public static class GraphQLBuilderExtensions
    {
        public static IGraphQLBuilder AddExecOptionsConfigHandler<TExecOptionsConfigHandler>(this IGraphQLBuilder builder)
            where TExecOptionsConfigHandler : class, IExecOptionsConfigHandler
        {
            builder.Services.Replace(ServiceDescriptor.Transient(typeof(IGraphQLExecuter<>), typeof(ExecOptionsConfigGraphQLExecuter<>)));

            builder.Services.AddSingleton<IExecOptionsConfigHandler, TExecOptionsConfigHandler>();

            return builder;
        }
    }
}
