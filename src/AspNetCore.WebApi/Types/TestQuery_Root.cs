using GraphQL.Server.Authorization.AspNetCore;
using GraphQL.Types;

namespace AspNetCore.WebApi
{
    public class TestQuery_Root : ObjectGraphType
    {
        public TestQuery_Root()
        {
            Field<ListGraphType<NonNullGraphType<PackageType>>>(
                "Package",
                resolve: context =>
                {
                    return new[] { 
                        new Package { Name = "pkg_abc", GraphQLEndPoint = "/packages/pkg_abc/graphql" },
                        new Package { Name = "pkg_abcd", GraphQLEndPoint = "/packages/pkg_abcd/graphql" },
                        new Package { Name = "pkg_abcde", GraphQLEndPoint = "/packages/pkg_abcde/graphql" },
                        new Package { Name = "pkg_abcdef", GraphQLEndPoint = "/packages/pkg_abcdef/graphql" },
                        new Package { Name = "pkg_abcdefg", GraphQLEndPoint = "/packages/pkg_abcdefg/graphql" }
                    };
                }
            ).AuthorizeWith("DeveloperPolicy");
        }
    }

    public class Package
    {
        public string Name { get; set; }

        public string GraphQLEndPoint { get; set; }
    }

    public class PackageType : ObjectGraphType<Package>
    {
        public PackageType()
        {
            Field(x => x.Name).Description("The name of the Package.");
            Field(x => x.GraphQLEndPoint).Description("The graphql endpoint of the Package.");
        }
    }
}
