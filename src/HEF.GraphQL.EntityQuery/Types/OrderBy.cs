using GraphQL.Types;

namespace HEF.GraphQL.EntityQuery
{
    public enum OrderBy
    {
        asc = 1,
        desc = 2
    }

    public class OrderBy_Type : EnumerationGraphType
    {
        public OrderBy_Type()
        {
            Description = "column ordering options";
            AddValue("asc", "in the ascending order", 1);
            AddValue("desc", "in the descending order", 2);
        }
    }
}
