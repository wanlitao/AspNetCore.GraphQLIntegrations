using GraphQL.Server;
using GraphQL.Types;
using HEF.GraphQL.ResourceQuery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

            app.UseGraphQL<ISchema>();

            app.UseGraphiQLServer();
            app.UseGraphQLPlayground();
        }
    }
}
