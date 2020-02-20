using GraphQL.Types;

namespace AspNetCore.WebApi
{
    public class TestQuery : ObjectGraphType
    {
        public TestQuery()
        {
            Field<ListGraphType<NonNullGraphType<DroidType>>>(
              "Droid",
              arguments: new QueryArguments(
                  //new QueryArgument<ListGraphType<NonNullGraphType<Droid_Select_Column>>> { Name = "distinct_on" },
                  new QueryArgument<IntGraphType> { Name = "limit" },
                  new QueryArgument<IntGraphType> { Name = "offset" },
                  new QueryArgument<ListGraphType<NonNullGraphType<Droid_Order_By>>> { Name = "order_by" },
                  new QueryArgument<Droid_Bool_Expr> { Name = "where" }
              ),
              resolve: context => new[] { new Droid { Id = 1, Name = "R1-D2" }, new Droid { Id = 2, Name = "R2-D3" } }
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

    public class Droid_Select_Column : EnumerationGraphType
    {
        public Droid_Select_Column()
        {
            Description = "select columns of table \"Droid\"";
            AddValue("id", "column name", 0);
            AddValue("name", "column name", 1);
        }
    }

    public class Droid_Order_By : InputObjectGraphType
    {
        public Droid_Order_By()
        {
            Description = "ordering options when selecting data from \"Droid\"";
            Field<Order_By>("id");
            Field<Order_By>("name");
        }
    }

    public class Droid_Bool_Expr : InputObjectGraphType
    {
        public Droid_Bool_Expr()
        {
            Description = "Boolean expression to filter rows from the table \"Droid\". All fields are combined with a logical 'AND'.";

            Field<ListGraphType<Droid_Bool_Expr>>("_and");
            Field<ListGraphType<Droid_Bool_Expr>>("_or");

            Field<Int_Comparison_Expr>("id");
            Field<String_Comparison_Expr>("name");
        }
    }

    public class Order_By : EnumerationGraphType
    {
        public Order_By()
        {
            Description = "column ordering options";
            AddValue("asc", "in the ascending order", 0);
            AddValue("desc", "in the descending order", 1);
        }
    }

    public class Int_Comparison_Expr : InputObjectGraphType
    {
        public Int_Comparison_Expr()
        {
            Description = "expression to compare columns of type Int. All fields are combined with logical 'AND'.";
            Field<IntGraphType>("_eq");
            Field<IntGraphType>("_gt");
            Field<IntGraphType>("_gte");
            Field<IntGraphType>("_lt");
            Field<IntGraphType>("_lte");
            Field<BooleanGraphType>("_is_null");
            Field<ListGraphType<IntGraphType>>("_in");
            Field<IntGraphType>("_neq");
            Field<ListGraphType<IntGraphType>>("_nin");
        }
    }

    public class String_Comparison_Expr : InputObjectGraphType
    {
        public String_Comparison_Expr()
        {
            Description = "expression to compare columns of type String. All fields are combined with logical 'AND'.";
            Field<StringGraphType>("_eq");
            Field<StringGraphType>("_gt");
            Field<StringGraphType>("_gte");
            Field<StringGraphType>("_lt");
            Field<StringGraphType>("_lte");
            Field<BooleanGraphType>("_is_null");
            Field<ListGraphType<StringGraphType>>("_in");
            Field<StringGraphType>("_neq");
            Field<ListGraphType<StringGraphType>>("_nin");

            Field<StringGraphType>("_prelike");
            Field<StringGraphType>("_like");
            Field<StringGraphType>("_suflike");
        }
    }
}
