using System.Linq;
using Contracts;
using Entities;
using LoggerService;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using UltimateWebApi.Controllers;
using UltimateWebApi.Csv;

namespace UltimateWebApi.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services) => 
            services.AddCors(options => 
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyOrigin();
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                }));
        
        public static void ConfigureIISIntegration(this IServiceCollection services) =>
            services.Configure<IISOptions>(options =>
            {
                // Currently do not need
            });

        public static void ConfigureLoggerService(this IServiceCollection services) =>
            services.AddScoped<ILoggerManager, LoggerManager>();

        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>
            services.AddDbContext<RepositoryContext>(options => options.UseSqlServer(configuration.GetConnectionString("SqlConnection"),
                builder => builder.MigrationsAssembly("UltimateWebApi"))); // Saves the migrations at the main project.

        public static void ConfigureRepositoryManager(this IServiceCollection services) =>
            services.AddScoped<IRepositoryManager, RepositoryManager>(); // Scoped creating on every request

        public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder) =>
            builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));

        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                var newtonSoftJsonOutputFormatter =
                    config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();
                if (newtonSoftJsonOutputFormatter != null)
                {
                    newtonSoftJsonOutputFormatter
                        .SupportedMediaTypes
                        .Add("application/vnd.codemaze.hateoas+json");
                    newtonSoftJsonOutputFormatter
                        .SupportedMediaTypes
                        .Add("application/vnd.codemaze.apiroot+json");
                }

                var xmlOutputFormatter =
                    config.OutputFormatters.OfType<XmlDataContractSerializerOutputFormatter>()?.FirstOrDefault();
                if (xmlOutputFormatter != null)
                {
                    xmlOutputFormatter
                        .SupportedMediaTypes
                        .Add("application/vnd.codemaze.hateoas+xml");
                    xmlOutputFormatter
                        .SupportedMediaTypes
                        .Add("application/vnd.codemaze.apiroot+xml");
                }
            });
        }
        
        public static void ConfigureVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true; // Adds the API version to the response header.
                options.AssumeDefaultVersionWhenUnspecified = true; // It specifies the default API version if the client doesn’t send one.
                options.DefaultApiVersion = new ApiVersion(1, 0); // sets the default version count.
                options.ApiVersionReader = new HeaderApiVersionReader("api-version"); // Or QueryApiVersionReader for query string versioning
                
                // Registers versions for our controllers, that give us to not implement the ApiVersion attribute.
                options.Conventions.Controller<CompaniesController>().HasApiVersion(new ApiVersion(1,0));
                options.Conventions.Controller<CompaniesV2Controller>().HasDeprecatedApiVersion(new ApiVersion(2, 0));
            });
        }

        public static void ConfigureResponseCaching(this IServiceCollection services) =>
            services.AddResponseCaching();

        public static void ConfigureHttpCacheHeaders(this IServiceCollection services) =>
            services.AddHttpCacheHeaders(expirationOpt => // Configure cache parameters for all controllers
            {
                expirationOpt.MaxAge = 65;
                expirationOpt.CacheLocation = CacheLocation.Private; // holds an age header of cache lives
            },
            validationOpt =>
            {
                validationOpt.MustRevalidate = true;
            });
    }
}