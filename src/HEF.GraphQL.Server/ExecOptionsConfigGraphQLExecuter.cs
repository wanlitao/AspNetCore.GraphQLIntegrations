using GraphQL;
using GraphQL.Execution;
using GraphQL.Server;
using GraphQL.Server.Internal;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading;

namespace HEF.GraphQL.Server
{
    public class ExecOptionsConfigGraphQLExecuter<TSchema> : DefaultGraphQLExecuter<TSchema>
        where TSchema : ISchema
    {
        private readonly IEnumerable<IExecOptionsConfigHandler> _configureHandlers;

        public ExecOptionsConfigGraphQLExecuter(
            TSchema schema,
            IDocumentExecuter documentExecuter,
            IOptions<GraphQLOptions> options,
            IEnumerable<IDocumentExecutionListener> listeners,
            IEnumerable<IValidationRule> validationRules,
            IEnumerable<IExecOptionsConfigHandler> configureHandlers)
            : base(schema, documentExecuter, options, listeners, validationRules)
        {
            _configureHandlers = configureHandlers;
        }

        protected override ExecutionOptions GetOptions(string operationName, string query, Inputs variables, IDictionary<string, object> context, CancellationToken cancellationToken)
        {
            var options = base.GetOptions(operationName, query, variables, context, cancellationToken);

            foreach(var configHandler in _configureHandlers)
            {
                configHandler.Configure(options);
            }

            return options;
        }
    }
}
