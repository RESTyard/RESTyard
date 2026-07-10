using System;

namespace RESTyard.Schema.Model;

/// <summary>
/// Assembly-level attribute emitted by the RESTyard source generator, pointing to the
/// generated action-result registry class for this assembly. Used by <c>AddHypermediaSchema()</c>
/// at runtime to merge controller-declared <c>ResultType</c> mappings into the schema when the
/// controllers live in a different assembly than the HTOs.
/// </summary>
/// <remarks>
/// This attribute is not intended to be applied manually — it is emitted by the source generator
/// when <c>[assembly: HypermediaAssembly]</c> is present, <c>Schema = true</c> (the default),
/// and at least one action endpoint declares a <c>ResultType</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public class HypermediaActionResultRegistryAttribute : Attribute
{
    /// <summary>
    /// The type of the generated action-result registry class.
    /// The registry has a static method <c>GetMappings()</c> that returns
    /// all <see cref="ActionResultMapping"/> instances for the assembly.
    /// </summary>
    public Type RegistryType { get; }

    /// <summary>
    /// Creates a new instance pointing to the generated action-result registry class.
    /// </summary>
    /// <param name="registryType">The generated registry type (e.g., <c>typeof(HypermediaActionResultRegistry_MyAssembly)</c>).</param>
    public HypermediaActionResultRegistryAttribute(Type registryType)
    {
        RegistryType = registryType;
    }
}
