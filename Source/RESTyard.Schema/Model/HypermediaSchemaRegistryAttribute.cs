using System;

namespace RESTyard.Schema.Model;

/// <summary>
/// Assembly-level attribute emitted by the RESTyard source generator, pointing to the
/// generated schema registry class for this assembly. Used by <c>AddHypermediaSchema()</c>
/// at runtime to discover and aggregate schema registries from all loaded assemblies.
/// </summary>
/// <remarks>
/// This attribute is not intended to be applied manually — it is emitted by the source generator
/// when <c>[assembly: HypermediaAssembly]</c> is present and <c>Schema = true</c> (the default).
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public class HypermediaSchemaRegistryAttribute : Attribute
{
    /// <summary>
    /// The type of the generated schema registry class.
    /// The registry has a static method <c>GetSchemas(IJsonSchemaFactory)</c> that returns
    /// all <c>EntityTypeSchema</c> instances for the assembly.
    /// </summary>
    public Type RegistryType { get; }

    /// <summary>
    /// Creates a new instance pointing to the generated schema registry class.
    /// </summary>
    /// <param name="registryType">The generated registry type (e.g., <c>typeof(HypermediaSchemaRegistry_MyAssembly)</c>).</param>
    public HypermediaSchemaRegistryAttribute(Type registryType)
    {
        RegistryType = registryType;
    }
}
