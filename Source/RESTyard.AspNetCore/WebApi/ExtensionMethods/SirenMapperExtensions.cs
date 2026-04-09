using System;
using Microsoft.Extensions.DependencyInjection;
using RESTyard.AspNetCore.Hypermedia.Siren;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

public static class SirenMapperExtensions
{
    /// <summary>
    /// Configures <see cref="SirenMapperOptions"/> for the generated <c>ToSiren()</c> methods.
    /// Optional — if not called, <see cref="SirenMapperOptions.Default"/> is used.
    /// </summary>
    public static IServiceCollection ConfigureSirenMapper(
        this IServiceCollection services,
        Action<SirenMapperOptions> configure)
    {
        var options = new SirenMapperOptions();
        configure(options);
        services.AddSingleton(options);
        return services;
    }
}
