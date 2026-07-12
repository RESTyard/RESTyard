using System;

namespace RESTyard.Schema.Model;

/// <summary>
/// Overrides the schema name of an HTO class in the generated
/// <see cref="EntityTypeSchema"/>. Without this attribute the name is derived from the
/// class name by stripping the <c>Hypermedia</c> prefix and <c>Hto</c> suffix
/// (<c>HypermediaCustomerHto</c> → <c>Customer</c>).
/// <para>
/// Use it to resolve derived-name collisions (<c>HypermediaCustomerHto</c> and
/// <c>CustomerHto</c> both derive <c>Customer</c> — reported as an error by the schema
/// generator) or to pick a shorter/custom name for documentation and diagrams.
/// The name is the identifier used in schema cross-references
/// (<c>targetName</c>, <c>resultName</c>).
/// </para>
/// </summary>
/// <example>
/// <code>
/// [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
/// [HypermediaSchemaName("CrmCustomer")]
/// public class HypermediaCustomerHto : IHypermediaObject { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class HypermediaSchemaNameAttribute : Attribute
{
    /// <summary>
    /// The schema name to use for this HTO.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Overrides the schema name of the HTO class.
    /// </summary>
    /// <param name="name">The schema name to use (must be non-empty).</param>
    public HypermediaSchemaNameAttribute(string name)
    {
        Name = name;
    }
}
