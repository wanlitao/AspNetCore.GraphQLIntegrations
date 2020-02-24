using GraphQL.Types;

namespace HEF.GraphQL.ResourceQuery
{
    public class DecimalComparisonExpr : ComparisonExpr<decimal>
    {
    }

    public class DecimalComparisonExpr_Type : ComparisonExpr_Type<DecimalGraphType>
    {
        public DecimalComparisonExpr_Type()
        {
            Description = "expression to compare columns of type Decimal. All fields are combined with logical 'AND'.";            
        }
    }
}
