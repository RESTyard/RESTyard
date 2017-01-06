using System.Buffers;
using CarShack.Domain.Customer;
using CarShack.Hypermedia.Customers;
using CarShack.Hypermedia.EntryPoint;
using CarShack.Util.GloblaExceptionHandler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApiHypermediaExtensionsCore.WebApi;
using WebApiHypermediaExtensionsCore.WebApi.ExtensionMethods;

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

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddMvc(options =>
            {
                options.OutputFormatters.Clear();
                options.OutputFormatters.Add(new JsonOutputFormatter(
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    }, ArrayPool<char>.Shared));

                // Initializes and adds the Hypermedia Extensions
                options.AddHypermediaExtensions();
            });
            builder.AddMvcOptions(o => { o.Filters.Add(new GlobalExceptionFilter(null)); });

            // Required by Hypermedia Extensions
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Domain
            services.AddSingleton<HypermediaEntryPoint>();
            services.AddSingleton<HypermediaCustomersRoot>();
            services.AddSingleton<ICustomerRepository, CustomerRepository>();
        }
    }
}
