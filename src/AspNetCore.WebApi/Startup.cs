using GraphQL.Server;
using GraphQL.Server.Internal;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Server.Ui.Playground;
using GraphQL.Types;
using HEF.GraphQL.AspNetCore;
using HEF.GraphQL.ResourceQuery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspNetCore.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly IConfigurationSection _appSettings;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _appSettings = Configuration.GetSection("appSettings");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<OrderBy_Type>();
            services.AddSingleton<IntComparisonExpr_Type>();
            services.AddSingleton<StringComparisonExpr_Type>();

            services.AddSingleton<DroidType>();
            services.AddSingleton<Droid_OrderBy_Type>();
            services.AddSingleton<Droid_Bool_Expr_Type>();

            services.AddSingleton<TestQuery_Root>();
            services.AddSingleton<ISchema, TestSchema>();

            services.AddGraphQL(options =>
            {
                options.EnableMetrics = false;
                options.ExposeExceptions = true;
            });

            services.AddTransient(typeof(IGraphQLExecuter<>), typeof(GraphQLExecuter<>));

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.MapWhen(IsPackageGraphQLRequestPath, app =>
            {
                app.Use(async (context, next) =>
                {
                    context.Request.Path.EndsWithSegments(new PathString("/graphql"), out PathString remainingPath);

                    var graphqlMiddleware = new GraphQLHttpMiddleware<ISchema>((context) => next.Invoke(), context.Request.Path, (settings) => { });
                    await graphqlMiddleware.InvokeAsync(context);
                });
            });

            app.MapWhen(IsPackageGraphiQLRequestPath, app =>
            {
                app.Use(async (context, next) =>
                {
                    context.Request.Path.EndsWithSegments(new PathString("/graphiql"), out PathString remainingPath);

                    var graphiqlOptions = new GraphiQLOptions { Path = context.Request.Path, GraphQLEndPoint = remainingPath + "/graphql" };

                    var graphiqlMiddleware = new GraphiQLMiddleware((context) => next.Invoke(), graphiqlOptions);
                    await graphiqlMiddleware.Invoke(context);
                });
            });

            app.MapWhen(IsPackageGraphQLPlaygroundRequestPath, app =>
            {
                app.Use(async (context, next) =>
                {
                    context.Request.Path.EndsWithSegments(new PathString("/ui/playground"), out PathString remainingPath);

                    var graphqlPlaygroundOptions = new GraphQLPlaygroundOptions { Path = context.Request.Path, GraphQLEndPoint = remainingPath + "/graphql" };

                    var graphqlPlaygroundMiddleware = new PlaygroundMiddleware((context) => next.Invoke(), graphqlPlaygroundOptions);
                    await graphqlPlaygroundMiddleware.Invoke(context);
                });
            });
        }

        private bool IsPackageGraphQLRequestPath(HttpContext context)
        {
            return !context.WebSockets.IsWebSocketRequest
                && context.Request.Path.StartsWithSegments(new PathString("/packages"), out PathString remainingPath)
                && remainingPath.EndsWithSegments(new PathString("/graphql"));
        }

        private bool IsPackageGraphiQLRequestPath(HttpContext context)
        {
            return !context.WebSockets.IsWebSocketRequest
                && context.Request.Path.StartsWithSegments(new PathString("/packages"), out PathString remainingPath)
                && remainingPath.EndsWithSegments(new PathString("/graphiql"));
        }

        private bool IsPackageGraphQLPlaygroundRequestPath(HttpContext context)
        {
            return !context.WebSockets.IsWebSocketRequest
                && context.Request.Path.StartsWithSegments(new PathString("/packages"), out PathString remainingPath)
                && remainingPath.EndsWithSegments(new PathString("/ui/playground"));
        }
    }
}
