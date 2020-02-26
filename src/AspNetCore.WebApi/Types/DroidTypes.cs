using GraphQL.Types;
using HEF.GraphQL.ResourceQuery;

namespace AspNetCore.WebApi
{
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
