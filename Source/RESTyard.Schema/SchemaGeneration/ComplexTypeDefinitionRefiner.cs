using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Json.Schema.Generation;
using Json.Schema.Generation.Intents;

namespace RESTyard.Schema.SchemaGeneration;

/// <summary>
/// An <see cref="ISchemaRefiner"/> that forces complex object types to be extracted
/// into <c>$defs</c> with <c>$ref</c> references, instead of being inlined.
/// Uses the fully qualified CLR type name as the definition key to avoid collisions.
/// </summary>
/// <remarks>
/// Skips: primitives, enums, strings, arrays, and generic collection types
/// (List, IList, ICollection, IEnumerable, IReadOnlyList, IReadOnlyCollection, Dictionary).
/// </remarks>
public class ComplexTypeDefinitionRefiner : ISchemaRefiner
{
    private static readonly HashSet<Type> SkippedGenericDefinitions = new()
    {
        typeof(List<>),
        typeof(IList<>),
        typeof(ICollection<>),
        typeof(IEnumerable<>),
        typeof(IReadOnlyList<>),
        typeof(IReadOnlyCollection<>),
        typeof(Dictionary<,>),
        typeof(IDictionary<,>),
        typeof(IReadOnlyDictionary<,>),
        typeof(HashSet<>),
        typeof(ISet<>),
#if NET6_0_OR_GREATER
        typeof(IReadOnlySet<>),
#endif
        typeof(Nullable<>),
    };

    private static readonly HashSet<Type> SkippedValueLikeTypes = new()
    {
        typeof(string),
        typeof(decimal),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(Guid),
        typeof(Uri),
#if NET6_0_OR_GREATER
        typeof(DateOnly),
        typeof(TimeOnly),
#endif
    };

    /// <inheritdoc />
    public bool ShouldRun(SchemaGenerationContextBase context)
    {
        var type = context.Type;

        return type switch
        {
            _ when type.IsPrimitive => false,
            _ when type.IsEnum => false,
            _ when type.IsArray => false,
            _ when SkippedValueLikeTypes.Contains(type) => false,
            _ when type.IsGenericType && SkippedGenericDefinitions.Contains(type.GetGenericTypeDefinition()) => false,
            _ when typeof(IEnumerable).IsAssignableFrom(type) => false,
            _ when context.Intents.Any(i => i is IdIntent) => false,
            _ => true,
        };
    }

    /// <inheritdoc />
    public void Run(SchemaGenerationContextBase context)
    {
        var typeName = context.Type.FullName ?? context.Type.Name;
        context.Intents.Add(new IdIntent(new Uri($"type:{typeName}", UriKind.RelativeOrAbsolute)));
    }
}
