using System;
using System.Linq;
using System.Reflection;

namespace RESTyard.AspNetCore.Hypermedia.Attributes;

/// <summary>
/// Provides auto-discovery of assemblies marked with <see cref="HypermediaAssemblyAttribute"/>.
/// Use this as a convention-based alternative to manually specifying assemblies
/// in <c>ControllerAndHypermediaAssemblies</c>.
/// </summary>
/// <example>
/// <code>
/// builder.Services.AddHypermediaExtensions(o =>
/// {
///     o.ControllerAndHypermediaAssemblies = HypermediaAssemblyDiscovery.GetAssemblies();
/// });
/// </code>
/// </example>
public static class HypermediaAssemblyDiscovery
{
    /// <summary>
    /// Scans all currently loaded assemblies for <see cref="HypermediaAssemblyAttribute"/>
    /// and returns them as an array.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method inspects <see cref="AppDomain.CurrentDomain"/> for loaded assemblies.
    /// Assemblies that have not been loaded yet (e.g., due to lazy loading) will not be found.
    /// In practice this is not an issue for typical ASP.NET Core applications because assemblies
    /// containing HTOs and controllers are always loaded before DI setup — they are referenced
    /// by the application's controller/HTO types.
    /// </para>
    /// <para>
    /// Each assembly that participates in RESTyard should be marked with
    /// <c>[assembly: HypermediaAssembly]</c>. This includes assemblies containing HTOs,
    /// controllers, or both. The attribute also gates source generation — without it,
    /// the RESTyard source generator emits no code for that assembly.
    /// </para>
    /// </remarks>
    /// <returns>
    /// An array of assemblies that have <see cref="HypermediaAssemblyAttribute"/> applied.
    /// Returns an empty array if no assemblies are found.
    /// </returns>
    public static Assembly[] GetAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetCustomAttribute<HypermediaAssemblyAttribute>() != null)
            .ToArray();
    }
}
