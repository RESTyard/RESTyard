using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RESTyard.Schema.Model;
using RESTyard.Schema.SchemaGeneration;

namespace RESTyard.Schema;

/// <summary>
/// Extension methods for registering RESTyard hypermedia schema services in a DI container.
/// </summary>
public static class HypermediaSchemaExtensions
{
    /// <summary>
    /// Registers <see cref="HypermediaSchemaOptions"/> and a singleton <see cref="HypermediaApiSchema"/>
    /// built by auto-discovering per-assembly schema registries.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Requires <see cref="IJsonSchemaFactory"/> to be registered in the service collection
    /// (e.g., via <c>AddHypermediaExtensions()</c> in ASP.NET Core, or manually for tooling).
    /// </para>
    /// <para>
    /// Registry discovery scans all loaded assemblies for
    /// <see cref="HypermediaSchemaRegistryAttribute"/>, which is emitted by the source generator
    /// when <c>[assembly: HypermediaAssembly]</c> is present.
    /// If no registries are found, a warning is logged.
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for schema metadata (title, description, version, etc.).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHypermediaSchema(
        this IServiceCollection services,
        Action<HypermediaSchemaOptions>? configure = null)
    {
        var options = new HypermediaSchemaOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton(sp =>
        {
            var factory = sp.GetRequiredService<IJsonSchemaFactory>();
            var logger = sp.GetService<ILoggerFactory>()?.CreateLogger(typeof(HypermediaSchemaBuilder));
            return HypermediaSchemaBuilder.Build(factory, options, logger);
        });

        return services;
    }
}
