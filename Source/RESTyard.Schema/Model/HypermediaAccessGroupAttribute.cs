using System;

namespace RESTyard.Schema.Model;

/// <summary>
/// Declares that the annotated element (entity type, action, link, or embedded entity)
/// requires the specified access groups. Used by the schema source generator to populate
/// <c>RequiredAccessGroups</c> on schema descriptions.
/// <para>
/// Access groups are purely descriptive metadata — the server still enforces authorization
/// at runtime. This attribute tells clients and tools what <em>could</em> be available
/// given sufficient permissions.
/// </para>
/// </summary>
/// <example>
/// <code>
/// [HypermediaAccessGroup("admin", "sales")]
/// public HypermediaAction? DeleteCustomer { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class HypermediaAccessGroupAttribute : Attribute
{
    /// <summary>
    /// The access group names required for this element.
    /// </summary>
    public string[] AccessGroups { get; }

    /// <summary>
    /// Declares that the annotated element requires the specified access groups.
    /// </summary>
    /// <param name="accessGroups">One or more access group names.</param>
    public HypermediaAccessGroupAttribute(params string[] accessGroups)
    {
        AccessGroups = accessGroups;
    }
}
