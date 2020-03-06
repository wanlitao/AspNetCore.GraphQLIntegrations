using GraphQL.Types;

namespace HEF.GraphQL.EntityQuery
{
    public interface IEntityGraphTypeBuilder
    {
        ObjectGraphType<TEntity> Build<TEntity>() where TEntity : class;
    }
}
