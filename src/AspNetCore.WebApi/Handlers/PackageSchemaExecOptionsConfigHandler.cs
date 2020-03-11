using GraphQL;
using GraphQL.Types;
using HEF.GraphQL.EntityQuery;
using HEF.GraphQL.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.WebApi
{
    public class PackageSchemaExecOptionsConfigHandler : IExecOptionsConfigHandler
    {
        public PackageSchemaExecOptionsConfigHandler(IEntityGraphTypeBuilder entityGraphTypeBuilder,
            IEntityGraphQueryArgumentsBuilder entityGraphQueryArgumentsBuilder,           
            IEntityGraphQueryResolver entityGraphQueryResolver)
        {
            EntityGraphTypeBuilder = entityGraphTypeBuilder ?? throw new ArgumentNullException(nameof(entityGraphTypeBuilder));
            EntityGraphQueryArgumentsBuilder = entityGraphQueryArgumentsBuilder
                ?? throw new ArgumentNullException(nameof(entityGraphQueryArgumentsBuilder));
            EntityGraphQueryResolver = entityGraphQueryResolver ?? throw new ArgumentNullException(nameof(entityGraphQueryResolver));
        }

        protected IEntityGraphTypeBuilder EntityGraphTypeBuilder { get; }

        protected IEntityGraphQueryArgumentsBuilder EntityGraphQueryArgumentsBuilder { get; }

        protected IEntityGraphQueryResolver EntityGraphQueryResolver { get; }

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

            var customerType = EntityGraphTypeBuilder.Build<Customer>();

            var root = new ObjectGraphType { Name = $"{packageName}_Query_Root", Description = $"query root for package: {packageName}" };            
            root.Field(
                "Customer",
                new ListGraphType(new NonNullGraphType(customerType)),
                arguments: EntityGraphQueryArgumentsBuilder.Build<Customer>(),
                resolve: context =>
                {
                    var queryable = EntityGraphQueryResolver.Resolve<Customer>(context);
                    
                    return queryable.ToList();
                });

            return new Schema { Query = root };
        }
    }
}
