using System.Buffers;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;

using CarShack.Domain.Customer;
using CarShack.Hypermedia;
using CarShack.Util.GlobalExceptionHandler;
using Microsoft.AspNetCore.HttpOverrides;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;

namespace CarShack
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            
            Configuration = builder.Build();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
            app.UseForwardedHeaders();
            app.UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddMvcCore(
                options =>
                {
                    options.Filters.Add(new GlobalExceptionFilter(null));
                    options.EnableEndpointRouting = false;
                });

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto |
                                           ForwardedHeaders.XForwardedHost;
                options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Any, 0));
            });
            // Initializes and adds the Hypermedia Extensions
            builder.Services.AddHypermediaExtensions(o =>
            {
                o.ReturnDefaultRouteForUnknownHto = true;
            });

            // Infrastructure
            services.AddCors();

            // Domain
            services.AddSingleton<HypermediaEntrypointHto>();
            services.AddSingleton<HypermediaCustomersRootHto>();
            services.AddSingleton<HypermediaCarsRootHto>();
            services.AddSingleton<ICustomerRepository, CustomerRepository>();
        }
    }
}