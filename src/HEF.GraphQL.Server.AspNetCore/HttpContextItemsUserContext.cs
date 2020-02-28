using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace HEF.GraphQL.Server.AspNetCore
{
    public class HttpContextItemsUserContext : Dictionary<string, object>
    {
        public HttpContextItemsUserContext(HttpContext httpContext)
        {
            Context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

            FillHttpContextItems(httpContext);
        }

        protected HttpContext Context { get; }

        protected void FillHttpContextItems(HttpContext httpContext)
        {
            foreach (var item in httpContext.Items)
            {
                if (item.Key is string keyStr)
                {
                    Add(keyStr, item.Value);
                }
            }
        }
    }
}
