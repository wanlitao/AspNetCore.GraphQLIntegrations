using GraphQL.Types;
using System;

namespace HEF.GraphQL.ResourceQuery
{
    public class DateTimeComparisonExpr : ComparisonExpr<DateTime>
    {        
    }

    public class DateTimeComparisonExpr_Type : ComparisonExpr_Type<DateTimeGraphType>
    {
        public DateTimeComparisonExpr_Type()
        {
            Description = "expression to compare columns of type DateTime. All fields are combined with logical 'AND'.";
        }
    }
}
