using GraphQL.Types;

namespace HEF.GraphQL.EntityQuery
{
    public class ComparisonExpr<TStruct> where TStruct : struct
    {
        public TStruct? _eq { get; set; }

        public TStruct? _gt { get; set; }

        public TStruct? _gte { get; set; }

        public TStruct? _lt { get; set; }

        public TStruct? _lte { get; set; }

        public bool? _is_null { get; set; }

        public TStruct[] _in { get; set; }

        public TStruct? _neq { get; set; }

        public TStruct[] _nin { get; set; }
    }

    public abstract class ComparisonExpr_Type<TGraphType> : InputObjectGraphType
        where TGraphType : ScalarGraphType
    {
        public ComparisonExpr_Type()
        {
            Field<TGraphType>("_eq");
            Field<TGraphType>("_gt");
            Field<TGraphType>("_gte");
            Field<TGraphType>("_lt");
            Field<TGraphType>("_lte");
            Field<BooleanGraphType>("_is_null");
            Field<ListGraphType<NonNullGraphType<TGraphType>>>("_in");
            Field<TGraphType>("_neq");
            Field<ListGraphType<NonNullGraphType<TGraphType>>>("_nin");
        }
    }
}
