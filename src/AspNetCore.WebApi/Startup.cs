using GraphQL.Server;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Server.Ui.Playground;
using GraphQL.Types;
using HEF.GraphQL.Server.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

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
            services.AddSingleton<PackageType>();
            services.AddSingleton<TestQuery_Root>();
            services.AddSingleton<ISchema, TestSchema>();
            
            services.AddMySqlConnection(Configuration.GetConnectionString("DefaultConnection"))
                .AddEntityMapperProvider(typeof(DbEntityMapper<>))
                .AddSqlBuilder()
                .AddMySqlFormatter()
                .AddExpressionToMySql()
                .AddEntityQueryable();

            services.AddGraphQL(options =>
            {
                options.EnableMetrics = false;
                options.ExposeExceptions = true;
            })
            .AddSystemTextJson(configureSerializerSettings: options => options.AddDateTimeFormating("yyyy-MM-dd HH:mm:ss"))
            .AddUserContextBuilder((ctx) => new HttpContextUserContext(ctx))
            .AddExecOptionsConfigHandler<PackageSchemaExecOptionsConfigHandler>()
            .AddEntityGraphQuery();
            //.AddGraphQLAuthorization(options => options.AddPolicy("DeveloperPolicy", policy => policy.RequireClaim("DeveloperId")));

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddIdentityServerAuthentication(options =>
            //    {
            //        options.Authority = _appSettings["IdentityServer_Address"];
            //        options.RequireHttpsMetadata = false;

            //        options.ApiName = "CDCApi_Developer";
            //    });

            //services.Configure<IISServerOptions>(options =>
            //{
            //    options.AllowSynchronousIO = true;
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseAuthentication();

            app.UseGraphQL<ISchema>("/packages/graphql");
            app.UseGraphiQLServer(new GraphiQLOptions { Path = "/packages/graphiql", GraphQLEndPoint = "/packages/graphql" });
            app.UseGraphQLPlayground(new GraphQLPlaygroundOptions { Path = "/packages/ui/playground", GraphQLEndPoint = "/packages/graphql" });

            app.UseRoutingGraphQL<ISchema>("/packages", "/graphql", (context) =>
            {
                context.Request.Path.EndsWithSegments(new PathString("/graphql"), out PathString remainingPath);
                remainingPath.StartsWithSegments(new PathString("/packages"), out PathString remainingPath2);

                var packageName = remainingPath2.Value.Trim('/');
                context.Items.Add("package", packageName);
            });
            app.UseRoutingGraphiQLServer("/packages");
            app.UseRoutingGraphQLPlayground("/packages");
        }
    }
}
