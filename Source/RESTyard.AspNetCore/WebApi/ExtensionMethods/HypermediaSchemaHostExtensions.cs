using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RESTyard.Schema.Generation;
using RESTyard.Schema.Model;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Convenience extension methods for schema generation on <see cref="IHost"/>.
/// </summary>
public static class HypermediaSchemaHostExtensions
{
    /// <summary>
    /// If <c>--generate-schema</c> is present in <paramref name="args"/>, generates schema
    /// artifacts and returns <c>true</c> (caller should exit). Otherwise returns <c>false</c>.
    /// </summary>
    /// <remarks>
    /// Resolves <see cref="HypermediaApiSchema"/> from DI (registered via <c>AddHypermediaSchema()</c>)
    /// and delegates to <see cref="HypermediaSchemaGenerator.GenerateIfRequested"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var app = builder.Build();
    /// if (app.GenerateSchemaIfRequested(args)) return;
    /// app.Run();
    /// </code>
    /// </example>
    /// <param name="host">The host whose DI container contains the schema.</param>
    /// <param name="args">Command-line arguments to parse.</param>
    /// <returns><c>true</c> if schema generation was performed; <c>false</c> otherwise.</returns>
    public static bool GenerateSchemaIfRequested(this IHost host, string[] args)
    {
        var schema = host.Services.GetRequiredService<HypermediaApiSchema>();
        return HypermediaSchemaGenerator.GenerateIfRequested(schema, args);
    }
}
