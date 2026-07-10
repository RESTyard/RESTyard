using System.Linq;
using Microsoft.CodeAnalysis;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// The <c>[assembly: HypermediaAssembly]</c> configuration controlling what the generator emits.
/// </summary>
internal readonly record struct AssemblyConfig(bool Schema, bool Siren)
{
    /// <summary>
    /// Reads the <c>[HypermediaAssembly]</c> attribute from the compilation.
    /// Returns null when the attribute is absent (no generation at all).
    /// </summary>
    internal static AssemblyConfig? FromCompilation(Compilation compilation)
    {
        var attr = compilation.Assembly.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.HypermediaAssemblyAttributeFullName);

        if (attr == null)
        {
            return null;
        }

        var schema = true;
        var siren = false;

        foreach (var named in attr.NamedArguments)
        {
            if (named.Key == "Schema" && named.Value.Value is bool s)
            {
                schema = s;
            }
            else if (named.Key == "Siren" && named.Value.Value is bool si)
            {
                siren = si;
            }
        }

        return new AssemblyConfig(schema, siren);
    }

    /// <summary>
    /// Applies the "Siren requires Schema" rule: <c>Siren = true</c> forces <c>Schema = true</c>.
    /// Returns the effective config plus whether RY0030 should be reported for the override.
    /// </summary>
    internal (AssemblyConfig Effective, bool WarnSirenRequiresSchema) Normalize()
        => Siren && !Schema
            ? (this with { Schema = true }, true)
            : (this, false);
}
