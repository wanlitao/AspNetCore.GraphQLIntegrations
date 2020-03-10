using GraphQL.Types;

namespace HEF.GraphQL.EntityQuery
{
    public class LongComparisonExpr : ComparisonExpr<long>
    {
    }

    public class LongComparisonExpr_Type : ComparisonExpr_Type<LongGraphType>
    {
        public LongComparisonExpr_Type()
        {
            Description = "expression to compare columns of type Long. All fields are combined with logical 'AND'.";
        }
    }
}
