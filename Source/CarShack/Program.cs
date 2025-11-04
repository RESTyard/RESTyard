using System;
using System.Threading.Tasks;
using CarShack.Controllers.EntryPoint;
using CarShack.Domain.Customer;
using CarShack.Hypermedia;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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

            app.UseCors(b =>
            {
                b
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Location");
            });
            app.MapControllers();
            app.UseHypermediaUI(
                new HypermediaConfig(
                    disableDeveloperControls: false,
                    onlyAllowConfiguredEntryPoints: false,
                    configuredEntryPoints:
                    [
                        new ConfiguredEntryPoint(
                            alias: "CarShack",
                            title: "CarShack",
                            entryPointUri: new Uri("http://localhost:5000/EntryPoint"))
                    ]));

            await app.RunAsync();
        }
    }
}
