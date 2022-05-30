using System.Buffers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using CarShack.Domain.Customer;
using CarShack.Hypermedia.Cars;
using CarShack.Hypermedia.Customers;
using CarShack.Hypermedia.EntryPoint;
using CarShack.Util.GlobalExceptionHandler;
using WebApi.HypermediaExtensions.WebApi.ExtensionMethods;

namespace CarShack
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            
            Configuration = builder.Build();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Location");
                }
            ); 
            app.UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddMvcCore(options =>
            {
                //options.OutputFormatters.Clear();
                //options.OutputFormatters.Add(new JsonOutputFormatter(
                //    new JsonSerializerSettings
                //    {
                //        NullValueHandling = NullValueHandling.Ignore,
                //        DefaultValueHandling = DefaultValueHandling.Ignore
                //    }, ArrayPool<char>.Shared));
            });
            builder.AddMvcOptions(o =>
            {
                o.Filters.Add(new GlobalExceptionFilter(null));
                o.EnableEndpointRouting = false;
            });

            // Initializes and adds the Hypermedia Extensions
            builder.AddHypermediaExtensions(
                services,
                new HypermediaExtensionsOptions
                {
                    ReturnDefaultRouteForUnknownHto = true
                });

            // Infrastructure
            services.AddCors();

            // Domain
            services.AddSingleton<HypermediaEntryPoint>();
            services.AddSingleton<HypermediaCustomersRoot>();
            services.AddSingleton<HypermediaCarsRoot>();
            services.AddSingleton<ICustomerRepository, CustomerRepository>();
        }
    }
}