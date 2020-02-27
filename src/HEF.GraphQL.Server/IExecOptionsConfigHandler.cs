using GraphQL;

namespace HEF.GraphQL.Server
{
    public interface IExecOptionsConfigHandler
    {
        void Configure(ExecutionOptions options);
    }
}
