using GraphQL;
using GraphQL.Types;
using HEF.GraphQL.ResourceQuery;
using System.Collections.Generic;

namespace AspNetCore.WebApi
{
    public class TestQuery_Root : ObjectGraphType
    {
        public TestQuery_Root()
        {
            Field<ListGraphType<NonNullGraphType<DroidType>>>(
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

                    return new[] { new Droid { Id = 1, Name = "R1-D2" }, new Droid { Id = 2, Name = "R2-D3" } };
                }
            );
        }
    }

    public class Droid
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DroidType : ObjectGraphType<Droid>
    {
        public DroidType()
        {
            Field(x => x.Id).Description("The Id of the Droid.");
            Field(x => x.Name).Description("The name of the Droid.");
        }
    }

    public class Droid_OrderBy
    {
        public OrderBy id { get; set; }

        public OrderBy name { get; set; }
    }

    public class Droid_OrderBy_Type : InputObjectGraphType
    {
        public Droid_OrderBy_Type()
        {
            Description = "ordering options when selecting data from \"Droid\"";
            Field<OrderBy_Type>("id");
            Field<OrderBy_Type>("name");
        }
    }

    public class Droid_Bool_Expr
    {
        public Droid_Bool_Expr[] _and { get; set; }

        public Droid_Bool_Expr[] _or { get; set; }

        public IntComparisonExpr id { get; set; }

        public StringComparisonExpr name { get; set; }
    }

    public class Droid_Bool_Expr_Type : InputObjectGraphType
    {
        public Droid_Bool_Expr_Type()
        {
            Description = "Boolean expression to filter rows from the table \"Droid\". All fields are combined with a logical 'AND'.";

            Field<ListGraphType<Droid_Bool_Expr_Type>>("_and");
            Field<ListGraphType<Droid_Bool_Expr_Type>>("_or");

            Field<IntComparisonExpr_Type>("id");
            Field<StringComparisonExpr_Type>("name");
        }
    }
}
