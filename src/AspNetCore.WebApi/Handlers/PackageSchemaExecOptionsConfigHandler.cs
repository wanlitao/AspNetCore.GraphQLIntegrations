using GraphQL;
using GraphQL.Types;
using HEF.GraphQL.ResourceQuery;
using HEF.GraphQL.Server;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace AspNetCore.WebApi
{
    public class PackageSchemaExecOptionsConfigHandler : IExecOptionsConfigHandler
    {
        public PackageSchemaExecOptionsConfigHandler(IEntityGraphTypeBuilder entityGraphTypeBuilder,
            IEntityGraphQueryArgumentsBuilder entityGraphQueryArgumentsBuilder)
        {
            EntityGraphTypeBuilder = entityGraphTypeBuilder ?? throw new ArgumentNullException(nameof(entityGraphTypeBuilder));
            EntityGraphQueryArgumentsBuilder = entityGraphQueryArgumentsBuilder
                ?? throw new ArgumentNullException(nameof(entityGraphQueryArgumentsBuilder));
        }

        protected IEntityGraphTypeBuilder EntityGraphTypeBuilder { get; }

        protected IEntityGraphQueryArgumentsBuilder EntityGraphQueryArgumentsBuilder { get; }

        public void Configure(ExecutionOptions options)
        {
            var packageName = GetContextPackageName(options.UserContext);
            if (string.IsNullOrWhiteSpace(packageName))
                return;

            var packageSchema = GetPackageSchema(packageName);
            options.Schema = packageSchema;
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

            var droidType = EntityGraphTypeBuilder.Build<Droid>();

            var root = new ObjectGraphType { Name = $"{packageName}_Query_Root", Description = $"query root for package: {packageName}" };
            root.Field(
                "Droid",
                new ListGraphType(new NonNullGraphType(droidType)),
                arguments: EntityGraphQueryArgumentsBuilder.Build<Droid>(),
                resolve: context =>
                {
                    var limit = context.GetArgument<int?>("limit");
                    var offset = context.GetArgument<int?>("offset");
                    var orderBy = context.GetArgument<IList<IDictionary<string, object>>>("order_by");
                    var where = context.GetArgument<object>("where");

                    return new[] { new Droid { Id = 1, Name = $"{packageName}-R1-D2" }, new Droid { Id = 2, Name = $"{packageName}-R2-D3" } };
                });

            return new Schema { Query = root };
        }
    }
}
