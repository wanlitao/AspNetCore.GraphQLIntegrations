using HEF.Entity.Mapper;

namespace AspNetCore.WebApi
{
    public class DbEntityMapper<TEntity> : AutoEntityMapper<TEntity>
        where TEntity : class
    {
        public DbEntityMapper()
        {
            DeleteFlag("IsDel");
            AutoMap();
        }
    }
}
