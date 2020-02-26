using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace AspNetCore.WebApi
{
    public class TestGraphUserContext : Dictionary<string, object>
    {
        public TestGraphUserContext(HttpContext httpContext)
        {
            Context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

            FillHttpContextItems(httpContext);
        }

        protected HttpContext Context { get; }

        protected void FillHttpContextItems(HttpContext httpContext)
        {
            foreach(var item in httpContext.Items)
            {
                if (item.Key is string keyStr)
                {
                    Add(keyStr, item.Value);
                }
            }
        }
    }
}
