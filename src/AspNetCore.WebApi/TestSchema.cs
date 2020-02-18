using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AspNetCore.WebApi
{
    public class TestSchema : Schema
    {
        public TestSchema(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Query = serviceProvider.GetRequiredService<TestQuery>();
        }
    }
}
