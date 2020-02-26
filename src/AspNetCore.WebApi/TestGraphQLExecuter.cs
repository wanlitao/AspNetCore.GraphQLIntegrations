using GraphQL;
using GraphQL.Execution;
using GraphQL.Server;
using GraphQL.Server.Internal;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AspNetCore.WebApi
{
    public class TestGraphQLExecuter<TSchema> : DefaultGraphQLExecuter<TSchema>, IGraphQLExecuter<TSchema>
        where TSchema : ISchema
    {
        public TestGraphQLExecuter(
            TSchema schema,
            IDocumentExecuter documentExecuter,
            IOptions<GraphQLOptions> options,
            IEnumerable<IDocumentExecutionListener> listeners,
            IEnumerable<IValidationRule> validationRules)
            : base(schema, documentExecuter, options, listeners, validationRules)
        { }

        protected override ExecutionOptions GetOptions(string operationName, string query, Inputs variables, IDictionary<string, object> context, CancellationToken cancellationToken)
        {
            var options = base.GetOptions(operationName, query, variables, context, cancellationToken);

            var packageName = GetContextPackageName(context);
            if (string.IsNullOrWhiteSpace(packageName))
                return options;

            var packageSchema = GetPackageSchema(packageName);
            options.Schema = packageSchema;

            return options;
        }

        private string GetContextPackageName(IDictionary<string, object> context)
        {
            if (context == null)
                return string.Empty;

            if (context.TryGetValue("package", out object value))
            {
                return value.ToString();
            }

            return string.Empty;
        }

        private ISchema GetPackageSchema(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                throw new ArgumentNullException(nameof(packageName));            

            var root = new ObjectGraphType { Name = $"{packageName}_Query_Root", Description = $"query root for package: {packageName}" };
            root.Field<ListGraphType<NonNullGraphType<DroidType>>>(
                "Droid",
                arguments: new QueryArguments(
                    new QueryArgument<IntGraphType> { Name = "limit" },
                    new QueryArgument<IntGraphType> { Name = "offset" },
                    new QueryArgument<ListGraphType<NonNullGraphType<Droid_OrderBy_Type>>> { Name = "order_by" },
                    new QueryArgument<Droid_Bool_Expr_Type> { Name = "where" }
                ),
                resolve: context => 
                {
                    var limit = context.GetArgument<int>("limit");
                    var offset = context.GetArgument<int>("offset");
                    var orderBy = context.GetArgument<IList<Droid_OrderBy>>("order_by");
                    var where = context.GetArgument<Droid_Bool_Expr>("where");

                    return new[] { new Droid { Id = 1, Name = $"{packageName}-R1-D2" }, new Droid { Id = 2, Name = $"{packageName}-R2-D3" } };
                });

            return new Schema { Query = root };
        }
    }
}
