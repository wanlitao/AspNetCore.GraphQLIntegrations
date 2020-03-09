namespace HEF.GraphQL.EntityQuery
{
    public interface IEntityGraphQueryBuilderFactory
    {
        IEntityGraphQueryBuilder<TEntity> Create<TEntity>() where TEntity : class;
    }
}
