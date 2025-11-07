using System;
using System.Threading.Tasks;
using CarShack.Controllers.EntryPoint;
using CarShack.Domain.Customer;
using CarShack.Hypermedia;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
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
            builder.Services.Configure<HypermediaUiConfig>(builder.Configuration.GetSection(nameof(HypermediaUiConfig)));
            builder.Services.AddHypermediaExtensions(o =>
            {
                o.ReturnDefaultRouteForUnknownHto = true;
                o.ControllerAndHypermediaAssemblies = [typeof(EntryPointController).Assembly];
            });
            builder.Services.AddLogging();

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
                    app.Services.GetRequiredService<ILogger<Program>>().LogError(e, context.Request.Path);
                    throw;
                }
            });
            app.UseCors(b =>
            {
                b
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(HeaderNames.Location);
            });
            app.MapControllers();
            app.UseHypermediaUI("swagger/hui");
            app.UseHypermediaUI(
                "swagger2/hui",
                new HypermediaUiConfig()
                {
                    DisableDeveloperControls = true,
                    OnlyAllowConfiguredEntryPoints = true,
                    ConfiguredEntryPoints =
                    [
                        new ConfiguredEntryPoint()
                        {
                            Alias = "CarShack2",
                            Title = "You've had CarShack, yes, but what about second CarShack?",
                            EntryPointUri = new Uri("http://localhost:5000/EntryPoint"),
                        },
                    ],
                });
            app.MapGet("crash", () =>
            {
                throw new Exception("BOOM");
                return "Hi";
            });

            await app.RunAsync();
        }
    }
}
