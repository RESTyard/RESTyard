using System;
using System.Threading.Tasks;
using CarShack.Controllers.EntryPoint;
using CarShack.Domain.Customer;
using CarShack.Hypermedia;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RESTyard.AspNetCore.HypermediaUI;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;

namespace CarShack
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddControllers();
            builder.Services.Configure<HypermediaConfig>(builder.Configuration.GetSection(nameof(HypermediaConfig)));
            builder.Services.AddHypermediaExtensions(o =>
            {
                o.ReturnDefaultRouteForUnknownHto = true;
                o.ControllerAndHypermediaAssemblies = [typeof(EntryPointController).Assembly];
            });

            builder.Services.AddCors();

            builder.Services
                .AddSingleton<HypermediaEntrypointHto>()
                .AddSingleton<HypermediaCustomersRootHto>()
                .AddSingleton<HypermediaCarsRootHto>()
                .AddSingleton<ICustomerRepository, CustomerRepository>();

            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception e)
                {
                    app.Services.GetRequiredService<ILogger>().LogError(e, context.Request.Path);
                    throw;
                }
            });
            app.UseCors(b =>
            {
                b
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Location");
            });
            app.MapControllers();
            app.UseHypermediaUI(sp =>
                sp.GetService<IOptions<HypermediaConfig>>()?.Value);

            await app.RunAsync();
        }
    }
}
