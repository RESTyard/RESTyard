using System;

namespace RESTyard.AspNetCore.Hypermedia.Attributes;

/// <summary>
/// Marks an assembly as participating in RESTyard hypermedia.
/// This attribute serves three purposes:
/// <list type="number">
///   <item><description>
///     <b>Assembly discovery:</b> <see cref="HypermediaAssemblyDiscovery.GetAssemblies"/>
///     scans loaded assemblies for this attribute, providing a convention-based alternative
///     to manually listing assemblies in <c>ControllerAndHypermediaAssemblies</c>.
///   </description></item>
///   <item><description>
///     <b>Source generation gate:</b> The RESTyard source generator only emits code
///     (schema methods, properties POCOs, registries) for assemblies with this attribute.
///     Without it, no code is generated even if the generator NuGet is referenced.
///   </description></item>
///   <item><description>
///     <b>Feature configuration:</b> The <see cref="Schema"/> and <see cref="Siren"/>
///     properties control which source generation features are active.
///   </description></item>
/// </list>
/// </summary>
/// <example>
/// <code>
/// // Enable schema generation (default)
/// [assembly: HypermediaAssembly]
///
/// // Enable schema + ToSiren() mappers
/// [assembly: HypermediaAssembly(Siren = true)]
///
/// // Discovery only, no generation (safety hatch)
/// [assembly: HypermediaAssembly(Schema = false)]
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public class HypermediaAssemblyAttribute : Attribute
{
    /// <summary>
    /// Controls whether the source generator emits schema-related code:
    /// <c>GetSchema()</c> methods, properties POCO classes, and the per-assembly schema registry.
    /// Default is <c>true</c>. Set to <c>false</c> to disable schema generation while
    /// keeping the assembly discoverable via <see cref="HypermediaAssemblyDiscovery.GetAssemblies"/>.
    /// </summary>
    /// <remarks>
    /// If <see cref="Siren"/> is <c>true</c> and <c>Schema</c> is explicitly set to <c>false</c>,
    /// the generator emits a diagnostic warning and forces <c>Schema</c> to <c>true</c>,
    /// because the Siren mappers depend on the generated properties POCOs.
    /// </remarks>
    public bool Schema { get; set; } = true;

    /// <summary>
    /// Controls whether the source generator emits Siren format-specific code:
    /// <c>ToSiren()</c> extension methods and Siren POCO types.
    /// Default is <c>false</c>. Set to <c>true</c> to opt in to generated Siren mappers,
    /// replacing the reflection-based <c>SirenConverter</c>.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, <see cref="Schema"/> is implicitly forced to <c>true</c>
    /// because the Siren mappers depend on the generated properties POCOs.
    /// </remarks>
    public bool Siren { get; set; } = false;
}
