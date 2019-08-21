using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Devzhou.Data.Entity;
using Devzhou.Data.Repository;
using Devzhou.IdentityServer.Config;
using Devzhou.IdentityServer.Profile;
using Devzhou.IdentityServer.Validator;
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
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            #region Configuration IdentityServer
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                // Add customer profile service
                .AddProfileService<CustomerProfileService>()
                // Add customer password validator
                .AddResourceOwnerValidator<CustomerPasswordValidator>()
                .AddConfigurationStore(options =>
                {
                    options.ConnectionString = Configuration["MongoDB"];
                    options.Database = "IdentityServer4";
                })
                .AddOperationalStore(options =>
                {
                    options.ConnectionString = Configuration["MongoDB"];
                    options.Database = "IdentityServer4";
                });

            #endregion

            services.AddMvc();

            #region Configuration DI

            var builder = new ContainerBuilder();
            builder.Populate(services);
            var dataAccess = Assembly.Load("Devzhou.Data");
            builder.RegisterAssemblyTypes(dataAccess)
                .Where(t => t.Name.EndsWith("Repository"))
                .WithParameters(new NamedParameter[]
                {
                    new NamedParameter("connectionString", Configuration["MongoDB"]),
                    new NamedParameter("databaseName", "IdentityServer4")
                });
         
            var container = builder.Build();
            return new AutofacServiceProvider(container);

            #endregion
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

                var userRepository = serviceScope.ServiceProvider.GetService<UserRepository>();
                var users = userRepository.FindAll();
                if (!users.Any())
                {
                    var testUser = new User
                    {
                        UserName = "test",
                        Password = "password",
                        Email = "test@example.com"
                    };
                    await userRepository.InsertAsync(testUser);
                }
            }
        }
    }
}