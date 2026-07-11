using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Json.Schema.Generation;
using Json.Schema.Generation.Intents;

namespace RESTyard.Schema.SchemaGeneration;

/// <summary>
/// An <see cref="ISchemaRefiner"/> that derives the JSON Schema <c>required</c> keyword
/// from C# nullability: every non-nullable property is required.
/// Matches C# semantics — a <c>string</c> property must be present, a <c>string?</c> may be absent.
/// </summary>
/// <remarks>
/// Properties whose nullability is unknown (no nullable reference type information,
/// e.g. <c>#nullable disable</c> contexts) are treated as optional — <c>required</c> is only
/// derived where the compiler recorded an explicit annotation.
/// Merges with an existing <c>required</c> list (e.g. from C# <c>required</c> members).
/// </remarks>
public class RequiredFromNonNullableRefiner : ISchemaRefiner
{
    private readonly SchemaGeneratorConfiguration config;

    public RequiredFromNonNullableRefiner(SchemaGeneratorConfiguration config)
    {
        this.config = config;
    }

    /// <inheritdoc />
    public bool ShouldRun(SchemaGenerationContextBase context)
        => context.Intents.Any(i => i is PropertiesIntent);

    /// <inheritdoc />
    public void Run(SchemaGenerationContextBase context)
    {
        var propertiesIntent = context.Intents.OfType<PropertiesIntent>().First();
        var generatedNames = propertiesIntent.Properties.Keys;

        var requiredNames = new List<string>();
        foreach (var property in context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0 || !property.CanRead)
            {
                continue;
            }

            var name = ResolveName(property);

            // Only claim properties the generator actually put into the schema
            // (skips ignored properties and guards against name-resolution drift).
            if (!generatedNames.Contains(name) || IsNullable(property))
            {
                continue;
            }

            requiredNames.Add(name);
        }

        if (requiredNames.Count == 0)
        {
            return;
        }

        var existing = context.Intents.OfType<RequiredIntent>().FirstOrDefault();
        if (existing != null)
        {
            foreach (var name in requiredNames.Where(n => !existing.RequiredProperties.Contains(n)))
            {
                existing.RequiredProperties.Add(name);
            }
        }
        else
        {
            context.Intents.Add(new RequiredIntent(requiredNames));
        }
    }

    private string ResolveName(PropertyInfo property)
        => property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
           ?? config.PropertyNameResolver?.Invoke(property)
           ?? property.Name;

    private static bool IsNullable(PropertyInfo property)
    {
        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
        {
            return true;
        }

        if (property.PropertyType.IsValueType)
        {
            return false;
        }

        return IsNullableReference(property);
    }

#if NET6_0_OR_GREATER
    private static bool IsNullableReference(PropertyInfo property)
    {
        // NullabilityInfoContext is not thread-safe — use a fresh instance per call
        var state = new NullabilityInfoContext().Create(property).ReadState;

        // Unknown (oblivious context) counts as optional — see class remarks
        return state != NullabilityState.NotNull;
    }
#else
    // netstandard2.0 has no NullabilityInfoContext — read the compiler-embedded
    // nullability metadata directly. Flag values: 1 = not annotated (non-nullable),
    // 2 = annotated (nullable), 0 = oblivious.
    private static bool IsNullableReference(PropertyInfo property)
    {
        foreach (var attribute in property.CustomAttributes)
        {
            if (attribute.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute")
            {
                return ReadNullableFlag(attribute) != 1;
            }
        }

        // No per-property attribute — the flag is lifted to the enclosing NullableContext
        for (var type = property.DeclaringType; type != null; type = type.DeclaringType)
        {
            foreach (var attribute in type.CustomAttributes)
            {
                if (attribute.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute")
                {
                    return (byte)attribute.ConstructorArguments[0].Value! != 1;
                }
            }
        }

        // Oblivious — counts as optional, see class remarks
        return true;
    }

    private static byte ReadNullableFlag(CustomAttributeData attribute)
    {
        var argument = attribute.ConstructorArguments[0];

        // byte[] form: first element describes the top-level type
        if (argument.Value is IReadOnlyList<CustomAttributeTypedArgument> flags && flags.Count > 0)
        {
            return (byte)flags[0].Value!;
        }

        return argument.Value is byte flag ? flag : (byte)0;
    }
#endif
}
