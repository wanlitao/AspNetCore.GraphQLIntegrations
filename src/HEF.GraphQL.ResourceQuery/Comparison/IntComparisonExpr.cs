using GraphQL.Types;

namespace HEF.GraphQL.ResourceQuery
{
    public class IntComparisonExpr : ComparisonExpr<int>
    {
    }

    public class IntComparisonExpr_Type : ComparisonExpr_Type<IntGraphType>
    {
        public IntComparisonExpr_Type()
        {
            Description = "expression to compare columns of type Int. All fields are combined with logical 'AND'.";            
        }
    }
}
