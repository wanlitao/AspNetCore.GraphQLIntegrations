using GraphQL.Resolvers;
using System;
using System.Threading.Tasks;

namespace GraphQL.Types
{
    public static class ComplexGraphTypeExtensions
    {
        public static void Field(
            this IComplexGraphType obj,
            string name,
            IGraphType type,
            string description = null,
            QueryArguments arguments = null,
            Func<IResolveFieldContext, object> resolve = null)
        {
            var field = new FieldType
            {
                Name = name,
                Description = description,
                Arguments = arguments,
                ResolvedType = type,
                Resolver = resolve != null ? new FuncFieldResolver<object>(resolve) : null
            };
            obj.AddField(field);
        }

        public static void FieldAsync(
            this IObjectGraphType obj,
            string name,
            IGraphType type,
            string description = null,
            QueryArguments arguments = null,
            Func<IResolveFieldContext, Task<object>> resolve = null)
        {
            var field = new FieldType
            {
                Name = name,
                Description = description,
                Arguments = arguments,
                ResolvedType = type,
                Resolver = resolve != null ? new AsyncFieldResolver<object>(resolve) : null
            };
            obj.AddField(field);
        }
    }
}
