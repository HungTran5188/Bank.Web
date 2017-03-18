using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using System.Reflection;
using Bank.Entities;
using Bank.Cores;

namespace Bank.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddDbContext<BankContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("BankContext")));


            #region
//            Neither your repository nor your DbContext should be singletons.The correct way to register them is services.AddScoped or services.AddTransient, as a DbContext shouldn't live longer than a request and the AddScoped is exactly for this.

//AddScoped will return the same instance of a DbContext(and repository if you register it as such) for the lifetime of the scope(which in ASP.NET Core equals the lifetime of a request).


//When you use AddScope you shouldn't dispose the context yourself, because the next object that resolves your repository will have an disposed context.


//Entity Framework by default registers the context as scoped, so your repository should be either scoped(same lifetime as the context and request) or transient(each service instance gets it's own instance of the repository, but all repositories within a request still share the same context).


//Making the context singleton causes serious issues, especially with the memory(the more you work on it, the more memory the context consumes, as it has to track more records).So a DbContext should be as short - lived as possible.
            #endregion
            services.AddTransient<IAccountRepository, AccountRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
