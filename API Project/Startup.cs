using API_Project.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;            // JWT Authentication
using Microsoft.IdentityModel.Tokens;                           // JWT Tokens
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Http;
using API_Project.Services.UserService;
using API_Project.Services.EmailService;
using Serilog;
using API_Project.Models;
using API_Project.Services.Newsletter;

namespace API_Project
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Load settings from appsettings.json
            services.Configure<EmailConfig>(Configuration.GetSection("EmailConfiguration"));                                    // Load the Email SMTP settings from appsettings.json
            services.Configure<SendInBlueConfig>(Configuration.GetSection("SendInBlueConfiguartion"));
            //services.AddOptions();                                                                                          // IOptions<T> is needed to access the EmailConfiguration via dependancy injection in the classes.


            // Entity Database Context. Connection string in appsettings.json. DataContext in Data/DataContext.cs
            services.AddDbContext<DataContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));   

            services.AddSwaggerGen(c =>                                                                                     // Swagger Config
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API_Project", Version = "v1" });
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme                                                 // So swagger can add bearer tokens to requests
                {
                    Description = "Please add the authorization token. Example: \'bearer 14a156fg...\'",                    // Adds "Authorize" button description
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.OperationFilter<SecurityRequirementsOperationFilter>();                                                   // Install Swashbuckle.AspNetCore.Filters
            });


            services.AddAutoMapper(typeof(Startup));                                                // Automapper. For use with DTOs
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthRepository, AuthRepository>();                                  // Interface IAuthRepository maps to AuthRepository.cs . If we ever wanted to change it to AuthRepoistory2 just make the change here.
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<INewsletter, Newsletter>();


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(OptionsBuilderConfigurationExtensions =>
                {
                    OptionsBuilderConfigurationExtensions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
            // To Access our session object outside of a Controller you need to inject HttpContext.Session
            // This code here enables the accessor, we then inject it into the constructor in CharacterService.cs
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddControllers();
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Use Swagger in Development
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API_Project");
                    c.RoutePrefix = "";                                                 // Set Swagger as homepage
                });
            }

            if (env.IsProduction())
            {
                // Use Swagger in Production
                // app.UseSwaggerAuthorized();          // Secure api documentation page with username and password. Have to add additional stuff. https://medium.com/@niteshsinghal85/securing-swagger-in-production-92d0a045a5
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API_Project");
                    c.RoutePrefix = "";                                                 // Set Swagger as homepage
                });
            }

            app.UseHttpsRedirection();

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

            app.UseAuthentication();                // Must be added above UseAuthorization() !

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
