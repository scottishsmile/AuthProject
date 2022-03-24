using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Services;
using Serilog;
using Microsoft.Extensions.Options;
using Website.Models;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;              //React Client App
using Microsoft.AspNetCore.SpaServices.Extensions;                          //React Client App
using System.IO;

namespace Website
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Session Storage Access for JWT Tokens in browser
            services.AddMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });

            services.AddControllersWithViews();

            // React Client App
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.AddCors();                     // CORS allowed for login form...



            // Appsettings.json
            // Allow appsettings.json configs
            services.AddOptions();
            services.Configure<OurSite>(Configuration.GetSection("OurSite"));



            // Use IHttpClientFactory as a central place for all Http clients
            // Named Http Client called OurAPI
            services.AddHttpClient("OurAPI", client =>
            {
                client.BaseAddress = new Uri(Configuration.GetSection("OurSite:Api").Value);      // The start of the API address, we will just add on stuff after the last slash '/'. Appsettings.json "OurSite"
            });

            services.AddScoped<IHttpClientService, HttpClientService>();             // Interface IHttpClientService maps to HttpClientService. If we ever wanted to change it to IHttpClientService2 just make the change here.
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Enable use of default/static files so we can use javascript and root > js > site.js
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSpaStaticFiles(new StaticFileOptions { RequestPath = "/ClientApp/build" });                // React Client

            // Logging. Log all requests.
            app.UseSerilogRequestLogging();

            // CORS allowed for login form...
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                                                    //.WithOrigins("https://localhost:44351")); // Allow only this origin can also have multiple origins separated with comma
                .AllowCredentials()); // allow credentials

            app.UseRouting();
            app.UseAuthorization();

            // Session Storage Access for JWT Tokens in browser
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=React}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "Brains",
                    pattern: "{controller=Brains}/{action=SignIn}/{id?}");
                
            });


            // React Client App
            app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = Path.Join(env.ContentRootPath, "ClientApp");

                    if (env.IsDevelopment())
                    {
                        spa.UseReactDevelopmentServer(npmScript: "start");
                    }
                });
        }
    }
}
