using System;
using System.Linq;
using Json.Schema;

namespace RESTyard.AspNetCore.Test.JsonSchema;

public static class JsonSchemaExtensions
{
    /// <summary>
    /// Resolves the schema for a property by finding either inline or referenced schema
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="rootSchema"></param>
    /// <returns>The resolved schema</returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="Exception"></exception>
    public static Json.Schema.JsonSchema? ResolveSchema(
        this Json.Schema.JsonSchema schema,
        Json.Schema.JsonSchema rootSchema)
    {
        // no type format or ref
        if (schema.Keywords == null) return null;

        var refKeyword = schema.Keywords.OfType<RefKeyword>().FirstOrDefault();

        // no ref, all needs to be here already
        if (refKeyword == null)
        {
            return schema;
        }

        // it is a $ref need to find it

        var refString = refKeyword.Reference.ToString();
        if (!refString.StartsWith("#"))
        {
            throw new NotImplementedException("Only local schemas are resolved");
        }

        // Remove the "#/" prefix and split by '/' to find the path
        // e.g., "#/$defs/Uri" -> ["$defs", "Uri"]
        var path = refString.TrimStart('#').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (path.Length < 2)
        {
            throw new Exception($"path too short: {refString}");
        }

        var section = path[0]; // "$defs" or "definitions"
        var key = path[1]; // The type, e.g. "uri"

        // 3. Look in modern "$defs" older "definitions" (Draft 6/7) not handled
        if (section != "$defs")
        {
            throw new Exception($"Could not resolve def for: {refString}, root schema has no $defs. Old drafts use 'definitions', not supported.");
        }

        if (rootSchema.Keywords == null)
        {
            throw new Exception($"Could not resolve def for: {refString}, root schema has no keywords");
        }

        var defs = rootSchema.Keywords.OfType<DefsKeyword>().FirstOrDefault();
        if (defs == null)
        {
            throw new Exception($"Could not resolve def for: {refString}, root schema has no keyword: defs");
        }

        if (defs.Definitions.TryGetValue(key, out var target))
        {
            return target.ResolveSchema(rootSchema); // Recurse
        }

        throw new Exception($"Could not resolve schema for {key}");
    }
}