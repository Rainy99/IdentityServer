using System.Linq;
using System.Threading.Tasks;
using Devzhou.IdentityServer.Config;
using IdentityServer4.MongoDB.DbContexts;
using IdentityServer4.MongoDB.Interfaces;
using IdentityServer4.MongoDB.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Devzhou.IdentityServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddTestUsers(InMemoryConfiguration.Users().ToList())
                .AddConfigurationStore(options =>
                {
                    options.ConnectionString = Configuration["MongoDB"];
                    options.Database = "IdentityServer";
                })
                .AddOperationalStore(options =>
                {
                    options.ConnectionString = Configuration["MongoDB"];
                    options.Database = "IdentityServer";
                });


            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,IApplicationLifetime applicationLifetime)
        {
            InitializeDatabase(app).Wait();
            app.UseIdentityServer();
            app.UseIdentityServerMongoDBTokenCleanup(applicationLifetime);
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
        
        /// <summary>
        /// InitializeDatabase
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <returns></returns>
        private async Task InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<IConfigurationDbContext>();
                if (!context.Clients.Any())
                {
                    foreach (var client in InMemoryConfiguration.Clients())
                    {
                        await context.AddClient(client.ToEntity());
                    }
                }
                if (!context.ApiResources.Any())
                {
                    foreach (var resource in InMemoryConfiguration.ApiResources())
                    {
                        await context.AddApiResource(resource.ToEntity());
                    }
                }
                if (!context.IdentityResources.Any())
                {
                    foreach (var identity in InMemoryConfiguration.GetIdentityResources())
                    {
                        await context.AddIdentityResource(identity.ToEntity());
                    }
                }

            }
        }
    }
}