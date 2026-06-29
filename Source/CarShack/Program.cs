using System.Threading.Tasks;
using CarShack.Controllers.EntryPoint;
using CarShack.Domain.Customer;
using CarShack.Hypermedia;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.Schema;

[assembly: HypermediaAssembly(Siren = true)]

namespace CarShack
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            // Action parameter bodies are deserialized with System.Text.Json. Register custom converters
            // here once: RESTyard bridges these into the MVC JsonOptions, so they apply uniformly to
            // controller [FromBody] action bodies, the file-upload form binder, and minimal-API bodies.
            // JsonStringEnumConverter lets enum-valued parameters be sent as their string names.
            builder.Services.ConfigureHttpJsonOptions(o =>
            {
                o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

            builder.Services.AddHypermediaExtensions(o =>
            {
                o.ReturnDefaultRouteForUnknownHto = true;
                o.ControllerAndHypermediaAssemblies = [typeof(EntryPointController).Assembly];
            });

            builder.Services.AddHypermediaSchema(o =>
            {
                o.Title = "CarShack API";
                o.Description = "RESTyard demo API for managing cars and customers";
                o.ApiVersion = "1.2.3";
            });

            builder.Services.AddCors();

            builder.Services
                .AddSingleton<HypermediaEntrypointHto>()
                .AddSingleton<HypermediaCustomersRootHto>()
                .AddSingleton<HypermediaCarsRootHto>()
                .AddSingleton<ICustomerRepository, CustomerRepository>();

            var app = builder.Build();

            if (app.GenerateSchemaIfRequested(args))
            {
                return;
            }

            app.UseCors(builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Location");
            });
            app.MapControllers();
            app.MapHypermediaSchema();
            app.MapHypermediaSchemaAccessGroups();

            await app.RunAsync();
        }
    }
}
