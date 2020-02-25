using GraphQL;
using GraphQL.Execution;
using GraphQL.Server;
using GraphQL.Server.Internal;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace AspNetCore.WebApi
{
    public class GraphQLExecuter<TSchema> : DefaultGraphQLExecuter<TSchema>, IGraphQLExecuter<TSchema>
        where TSchema : ISchema
    {
        public GraphQLExecuter(
            TSchema schema,
            IDocumentExecuter documentExecuter,
            IOptions<GraphQLOptions> options,
            IEnumerable<IDocumentExecutionListener> listeners,
            IEnumerable<IValidationRule> validationRules)
            : base(schema, documentExecuter, options, listeners, validationRules)
        {

        }
    }
}
