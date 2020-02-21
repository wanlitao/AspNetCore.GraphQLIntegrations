using GraphQL.Types;

namespace HEF.GraphQL.ResourceQuery
{
    public class IntComparisonExpr
    {
        public int? _eq { get; set; }

        public int? _gt { get; set; }

        public int? _gte { get; set; }

        public int? _lt { get; set; }

        public int? _lte { get; set; }

        public bool? _is_null { get; set; }

        public int[] _in { get; set; }

        public int? _neq { get; set; }

        public int[] _nin { get; set; }
    }

    public class IntComparisonExpr_Type : InputObjectGraphType
    {
        public IntComparisonExpr_Type()
        {
            Description = "expression to compare columns of type Int. All fields are combined with logical 'AND'.";
            Field<IntGraphType>("_eq");
            Field<IntGraphType>("_gt");
            Field<IntGraphType>("_gte");
            Field<IntGraphType>("_lt");
            Field<IntGraphType>("_lte");
            Field<BooleanGraphType>("_is_null");
            Field<ListGraphType<NonNullGraphType<IntGraphType>>>("_in");
            Field<IntGraphType>("_neq");
            Field<ListGraphType<NonNullGraphType<IntGraphType>>>("_nin");
        }
    }
}
