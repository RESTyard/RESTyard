using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace RESTyard.Schema.SchemaGeneration;

/// <summary>
/// Post-processes a schema generated with <see cref="ComplexTypeDefinitionRefiner"/> so that every complex
/// type is described once in <c>$defs</c> and referenced everywhere else.
/// </summary>
/// <remarks>
/// JsonSchema.Net always inlines nullable reference-type members (<c>type: ["object", "null"]</c>) instead of
/// using <c>$ref</c>, and the refiner's <c>$id</c> marker is repeated at every use site. This rewrites:
/// <list type="bullet">
/// <item>an inline copy of a nullable type to <c>oneOf: [{ "$ref": ... }, { "type": "null" }]</c>,
/// an inline copy of a non-nullable type to <c>$ref</c>;</item>
/// <item>types that are only used as nullable are moved into <c>$defs</c>;</item>
/// <item>use-site <c>$id</c>s are removed, so each <c>$id</c> occurs once (in <c>$defs</c>).</item>
/// </list>
/// Member annotations (e.g. <c>title</c>, <c>description</c>, <c>deprecated</c>) that differ from the
/// definition are kept next to the reference.
/// </remarks>
internal static class DefinitionReferenceNormalizer
{
    private const string DefsPrefix = "#/$defs/";

    // Keywords describing the type itself — they live in the definition, not at the use site.
    private static readonly HashSet<string> StructuralKeywords = new()
    {
        "$id", "$ref", "$defs", "type", "properties", "required", "additionalProperties", "items",
    };

    // Keywords whose values are instances, not subschemas — never descended into.
    private static readonly HashSet<string> InstanceKeywords = new()
    {
        "const", "default", "enum", "examples",
    };

    public static void Normalize(JsonObject root)
    {
        var state = new State(root);

        VisitChildren(root, state, skipDefs: true);
        while (state.PendingDefinitions.Count > 0)
        {
            VisitChildren(state.PendingDefinitions.Dequeue(), state, skipDefs: false);
        }

        if (state.Defs.Count > 0 && !root.ContainsKey("$defs"))
        {
            root["$defs"] = state.Defs;
        }
    }

    private static void VisitChildren(JsonObject schema, State state, bool skipDefs)
    {
        foreach (var keyword in schema.ToList())
        {
            var key = keyword.Key;
            if (InstanceKeywords.Contains(key) || (skipDefs && key == "$defs"))
            {
                continue;
            }

            switch (keyword.Value)
            {
                case JsonObject child:
                    var rewritten = Rewrite(child, state);
                    if (!ReferenceEquals(rewritten, child))
                    {
                        schema[key] = rewritten;
                    }
                    break;
                case JsonArray array:
                    for (var i = 0; i < array.Count; i++)
                    {
                        if (array[i] is JsonObject element)
                        {
                            var rewrittenElement = Rewrite(element, state);
                            if (!ReferenceEquals(rewrittenElement, element))
                            {
                                array[i] = rewrittenElement;
                            }
                        }
                    }
                    break;
            }
        }
    }

    private static JsonObject Rewrite(JsonObject schema, State state)
    {
        var id = GetId(schema);
        if (id is null)
        {
            VisitChildren(schema, state, skipDefs: false);
            return schema;
        }

        if (schema.ContainsKey("$ref"))
        {
            schema.Remove("$id");
            return schema;
        }

        var reference = id == state.RootId ? "#" : DefsPrefix + state.GetOrAddDefinition(id, schema);
        return BuildReference(schema, reference, state.FindDefinition(reference));
    }

    private static JsonObject BuildReference(JsonObject inlineCopy, string reference, JsonObject? definition)
    {
        var result = IsNullable(inlineCopy)
            ? new JsonObject
            {
                ["oneOf"] = new JsonArray(
                    new JsonObject { ["$ref"] = reference },
                    new JsonObject { ["type"] = "null" }),
            }
            : new JsonObject { ["$ref"] = reference };

        foreach (var keyword in inlineCopy)
        {
            if (StructuralKeywords.Contains(keyword.Key))
            {
                continue;
            }

            if (definition is not null && definition.TryGetPropertyValue(keyword.Key, out var definitionValue)
                                       && JsonNode.DeepEquals(definitionValue, keyword.Value))
            {
                continue;
            }

            result[keyword.Key] = keyword.Value?.DeepClone();
        }

        return result;
    }

    private static bool IsNullable(JsonObject schema)
        => schema["type"] is JsonArray types && types.Any(t => t?.GetValue<string>() == "null");

    private static string? GetId(JsonObject schema)
        => schema["$id"] is JsonValue id && id.TryGetValue<string>(out var value) ? value : null;

    private sealed class State
    {
        private readonly Dictionary<string, string> defKeyById;

        public State(JsonObject root)
        {
            RootId = GetId(root);
            Defs = root["$defs"] as JsonObject ?? new JsonObject();
            defKeyById = Defs
                .Where(d => d.Value is JsonObject)
                .Select(d => (Key: d.Key, Id: GetId((JsonObject)d.Value!)))
                .Where(d => d.Id is not null)
                .GroupBy(d => d.Id!)
                .ToDictionary(g => g.Key, g => g.First().Key);

            foreach (var definition in Defs.Select(d => d.Value).OfType<JsonObject>())
            {
                PendingDefinitions.Enqueue(definition);
            }
        }

        public string? RootId { get; }
        public JsonObject Defs { get; }
        public Queue<JsonObject> PendingDefinitions { get; } = new();

        public string GetOrAddDefinition(string id, JsonObject inlineCopy)
        {
            if (defKeyById.TryGetValue(id, out var existingKey))
            {
                return existingKey;
            }

            var key = UniqueKey(DefinitionKeyFromId(id));
            var definition = (JsonObject)inlineCopy.DeepClone();
            RemoveNullType(definition);
            Defs[key] = definition;
            defKeyById[id] = key;
            PendingDefinitions.Enqueue(definition);
            return key;
        }

        public JsonObject? FindDefinition(string reference)
            => reference.StartsWith(DefsPrefix, StringComparison.Ordinal)
                ? Defs[reference.Substring(DefsPrefix.Length)] as JsonObject
                : null;

        private string UniqueKey(string baseKey)
        {
            var key = baseKey;
            for (var i = 2; Defs.ContainsKey(key); i++)
            {
                key = baseKey + i;
            }
            return key;
        }

        // "type:My.Namespace.Outer+Country" → "country"; generic arity and arguments are dropped
        // (the display name is taken from $id at render time).
        private static string DefinitionKeyFromId(string id)
        {
            const string typePrefix = "type:";
            var name = id.StartsWith(typePrefix, StringComparison.Ordinal) ? id.Substring(typePrefix.Length) : id;
            var genericStart = name.IndexOf('`');
            if (genericStart >= 0)
            {
                name = name.Substring(0, genericStart);
            }
            name = name.Substring(name.LastIndexOfAny(['.', '+']) + 1);
            return name.Length > 0 ? char.ToLowerInvariant(name[0]) + name.Substring(1) : "definition";
        }

        private static void RemoveNullType(JsonObject definition)
        {
            if (definition["type"] is not JsonArray types)
            {
                return;
            }

            var remaining = types.Select(t => t!.GetValue<string>()).Where(t => t != "null").ToList();
            definition["type"] = remaining.Count == 1
                ? JsonValue.Create(remaining[0])
                : new JsonArray(remaining.Select(t => (JsonNode?)JsonValue.Create(t)).ToArray());
        }
    }
}
