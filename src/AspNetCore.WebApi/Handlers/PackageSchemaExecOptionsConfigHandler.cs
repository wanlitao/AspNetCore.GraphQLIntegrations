using GraphQL;
using GraphQL.Types;
using HEF.GraphQL.ResourceQuery;
using HEF.GraphQL.Server;
using System;
using System.Collections.Generic;

namespace AspNetCore.WebApi
{
    public class PackageSchemaExecOptionsConfigHandler : IExecOptionsConfigHandler
    {
        public PackageSchemaExecOptionsConfigHandler(IEntityGraphTypeBuilder entityGraphTypeBuilder)
        {
            EntityGraphTypeBuilder = entityGraphTypeBuilder ?? throw new ArgumentNullException(nameof(entityGraphTypeBuilder));
        }

        protected IEntityGraphTypeBuilder EntityGraphTypeBuilder { get; }

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

            var droidOrderByType = new InputObjectGraphType() { Name = "Droid_OrderBy_Type" };
            droidOrderByType.Description = $"ordering options when selecting data from {nameof(Droid)}";
            droidOrderByType.Field<OrderBy_Type>("id");
            droidOrderByType.Field<OrderBy_Type>("name");

            var droidBoolExprType = new InputObjectGraphType() { Name = "Droid_Bool_Expr_Type" };
            droidBoolExprType.Description = $"Boolean expression to filter rows from the table {nameof(Droid)}. All fields are combined with a logical 'AND'.";
            droidBoolExprType.Field("_and", new ListGraphType(droidBoolExprType));
            droidBoolExprType.Field("_or", new ListGraphType(droidBoolExprType));
            droidBoolExprType.Field(typeof(IntComparisonExpr_Type), "id");
            droidBoolExprType.Field(typeof(StringComparisonExpr_Type), "name");

            var root = new ObjectGraphType { Name = $"{packageName}_Query_Root", Description = $"query root for package: {packageName}" };
            root.Field(
                "Droid",
                new ListGraphType(new NonNullGraphType(droidType)),
                arguments: new QueryArguments(
                    new QueryArgument<IntGraphType> { Name = "limit" },
                    new QueryArgument<IntGraphType> { Name = "offset" },
                    new QueryArgument(new ListGraphType(new NonNullGraphType(droidOrderByType))) { Name = "order_by" },
                    new QueryArgument(droidBoolExprType) { Name = "where" }
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
