using GraphQL;
using HEF.Entity.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HEF.GraphQL.EntityQuery
{
    public class PredicateGraphQueryMiddlewareBuilder : IEntityGraphQueryMiddlewareBuilder
    {
        public PredicateGraphQueryMiddlewareBuilder(IEntityMapperProvider mapperProvider)
        {
            MapperProvider = mapperProvider ?? throw new ArgumentNullException(nameof(mapperProvider));
        }

        protected IEntityMapperProvider MapperProvider { get; }

        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Build<TEntity>(
            IResolveFieldContext resolveFieldContext) where TEntity : class
        {
            if (resolveFieldContext == null)
                throw new ArgumentNullException(nameof(resolveFieldContext));

            var whereArguments = resolveFieldContext.GetArgument<IDictionary<string, object>>(
                EntityGraphQueryConstants.GraphQueryArgumnet_Where_Name);

            var queryablePredicateFactory = BuildQueryablePredicateFactory<TEntity>();
            return queryablePredicateFactory.Compile();
        }

        #region Helper Functions
        protected static Expression<Func<IQueryable<TEntity>, IQueryable<TEntity>>> BuildQueryablePredicateFactory<TEntity>()
            where TEntity : class
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
