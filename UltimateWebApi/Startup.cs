using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AspNetCoreRateLimit;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NLog;
using Repository;
using Repository.DataShaping;
using UltimateWebApi.ActionFilters;
using UltimateWebApi.Extensions;
using UltimateWebApi.Utility;

namespace UltimateWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
                        
            services.ConfigureCors();
            services.ConfigureIISIntegration();
            services.ConfigureLoggerService();
            services.ConfigureSqlContext(Configuration);
            services.ConfigureRepositoryManager();

            services.AddAutoMapper(typeof(Startup));
            
            services.Configure<ApiBehaviorOptions>(options => {
            /*
             * suppress model filter and allows to use ModelState in our controllers 
             */
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddScoped<ValidationFilterAttribute>(); // custom action argument validation filter
            services.AddScoped<ValidateCompanyExistsAttribute>(); // custom company entity checker helper
            services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>(); // custom employee entity checker helper

            services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();

            services.AddScoped<ValidateMediaTypeAttribute>();

            services.AddScoped<EmployeeLinks>();

            services.ConfigureVersioning();

            services.AddHttpContextAccessor(); // Needs for Marvin.Cache.Headers library works
            services.ConfigureResponseCaching(); // + for additional for this we need to register caching middleware.
            services.ConfigureHttpCacheHeaders(); // Implements validation for cache headers

            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();
            services.ConfigureRateLimitingOptions();
            services.AddHttpContextAccessor();

            services.AddControllers(configure =>
            {
                configure.RespectBrowserAcceptHeader = true;
                configure.ReturnHttpNotAcceptable = true; // allows us to get 406 Not Acceptable status if the server does not support media type of request.
                configure.CacheProfiles.Add("120SecondsDuration", new CacheProfile { Duration = 120 }); // Sets duration for all caches with specify name
            }).AddNewtonsoftJson()
              .AddXmlDataContractSerializerFormatters()
              .AddCustomCSVFormatter();
            
            services.AddCustomMediaTypes();

            services.AddAuthentication();
            services.ConfigureIdentity();
            services.ConfigureJWT(Configuration);
            services.AddScoped<IAuthenticationManager, AuthenticationManager>();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "UltimateWebApi", 
                    Version = "v1",
                    Description = "CompanyEmployees API",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact()
                    {
                        Name = "Wolodymyr",
                        Email = "wolodymyr.d.07@gmail.com",
                        Url = new Uri("https://twitter.com/...")
                    },
                    License = new OpenApiLicense()
                    {
                        Name = "CompanyEmployees API LICX",
                        Url = new Uri("https://example.com/license"),
                    }
                });
                c.SwaggerDoc("v2", new OpenApiInfo {Title = "UltimateWebApi", Version = "v2"});
                
                // With main properties modified and this three below lines of code we implemented more description for any action which have triple slash comment
                // Main project => properties => add 1591 suppress warning and generate the xml file in build section.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                
                // Above two methods = Swagger Configuration for using JWT
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Description = "Place to add JWT with Bearer",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Name = "Bearer",
                        },
                        new List<string>()
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerManager logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UltimateWebApi v1");
                    c.SwaggerEndpoint("/swagger/v2/swagger.json", "UltimateWebApi v2");
                });
            }

            app.ConfigureExceptionHandler(logger); // custom exception middleware

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors("CorsPolicy");

            
            // Will forward proxy headers to the current request. This will help us during application deployment.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            app.UseResponseCaching(); // Need to be provided above a routing middleware.
            app.UseHttpCacheHeaders(); // Implements validation for cache headers
            
            app.UseIpRateLimiting(); // Need for rate limiting work. From AspNetCoreRateLimit library. And provides 429 Too Many Requests status.
            
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}