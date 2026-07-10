using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Scans controller endpoint attributes for <c>ResultType</c> declarations and
/// enriches extracted HTO action metadata with the resulting schema names/classes.
/// </summary>
internal static class ActionResultMappingExtractor
{
    /// <summary>
    /// Scans all types in the compilation for methods with [HypermediaActionEndpoint] that have ResultType set.
    /// Returns a dictionary keyed by (HtoClassName, ActionPropertyName) → (ResultSchemaName, ResultClasses).
    /// </summary>
    internal static (
        ImmutableDictionary<(string HtoClassName, string ActionPropertyName), (string ResultSchemaName, ImmutableArray<string> ResultClasses)> Mappings,
        ImmutableArray<(string ResultTypeName, string HtoClassName, string ActionPropertyName)> NotHtoWarnings,
        ImmutableArray<(string ControllerName, string MethodName, string ActionPropertyName)> Missing201Warnings)
        ExtractActionResultMappings(Compilation compilation)
    {
        var builder = ImmutableDictionary.CreateBuilder<(string, string), (string, ImmutableArray<string>)>();
        var notHtoWarnings = ImmutableArray.CreateBuilder<(string, string, string)>();
        var missing201Warnings = ImmutableArray.CreateBuilder<(string, string, string)>();

        foreach (var type in GetAllTypes(compilation))
        {
            foreach (var member in type.GetMembers().OfType<IMethodSymbol>())
            {
                foreach (var attr in member.GetAttributes())
                {
                    var attrClass = attr.AttributeClass;
                    if (attrClass == null || !attrClass.IsGenericType)
                        continue;

                    var originalDef = attrClass.OriginalDefinition.ToDisplayString();
                    if (!originalDef.StartsWith(WellKnownTypeNames.HypermediaActionEndpointAttributePrefix))
                        continue;

                    // Get the HTO type argument
                    var htoType = attrClass.TypeArguments[0] as INamedTypeSymbol;
                    if (htoType == null)
                        continue;

                    // Get the action property name (first constructor arg)
                    if (attr.ConstructorArguments.Length == 0 || attr.ConstructorArguments[0].Value is not string actionPropName)
                        continue;

                    // Get ResultType (named argument)
                    var resultTypeArg = attr.NamedArguments.FirstOrDefault(a => a.Key == "ResultType");
                    var hasResultType = resultTypeArg.Key == "ResultType" && resultTypeArg.Value.Value is INamedTypeSymbol;

                    var htoClassName = htoType.Name;

                    if (hasResultType)
                    {
                        var resultType = (INamedTypeSymbol)resultTypeArg.Value.Value!;

                        // Check if ResultType has [HypermediaObject]
                        var hasHypermediaObject = resultType.GetAttributes()
                            .Any(a => a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.HypermediaObjectAttributeFullName);

                        if (!hasHypermediaObject)
                        {
                            notHtoWarnings.Add((resultType.ToDisplayString(), htoClassName, actionPropName));
                        }
                        else
                        {
                            var resultSchemaName = HtoMetadataExtractor.DeriveSchemaName(resultType.Name);
                            var resultClasses = HtoMetadataExtractor.GetTargetClasses(resultType);
                            builder[(htoClassName, actionPropName)] = (resultSchemaName, resultClasses);
                        }
                    }
                    else
                    {
                        // No ResultType — check if method has 201-related attributes
                        if (Has201ResponseAttribute(member))
                        {
                            missing201Warnings.Add((type.Name, member.Name, actionPropName));
                        }
                    }
                }
            }
        }

        // Also scan legacy HttpMethodHypermediaAction attributes for ResultType
        foreach (var type in GetAllTypes(compilation))
        {
            foreach (var member in type.GetMembers().OfType<IMethodSymbol>())
            {
                foreach (var attr in member.GetAttributes())
                {
                    var attrClass = attr.AttributeClass;
                    if (attrClass == null)
                        continue;

                    // Check if this attribute inherits from HttpMethodHypermediaAction
                    if (!InheritsFrom(attrClass, WellKnownTypeNames.HttpMethodHypermediaActionBaseFullName))
                        continue;

                    // Get ResultType (named argument)
                    var resultTypeArg = attr.NamedArguments.FirstOrDefault(a => a.Key == "ResultType");
                    if (resultTypeArg.Key != "ResultType" || resultTypeArg.Value.Value is not INamedTypeSymbol resultType)
                        continue;

                    // Get the action type from the second constructor argument: typeof(HtoName.ActionOp)
                    if (attr.ConstructorArguments.Length < 2 || attr.ConstructorArguments[1].Value is not INamedTypeSymbol actionOpType)
                        continue;

                    var declaringType = actionOpType.ContainingType;
                    if (declaringType == null)
                        continue;

                    var htoClassName = declaringType.Name;
                    var actionName = actionOpType.Name.EndsWith("Op")
                        ? actionOpType.Name.Substring(0, actionOpType.Name.Length - 2)
                        : actionOpType.Name;

                    var hasHypermediaObject = resultType.GetAttributes()
                        .Any(a => a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.HypermediaObjectAttributeFullName);

                    if (!hasHypermediaObject)
                    {
                        notHtoWarnings.Add((resultType.ToDisplayString(), htoClassName, actionName));
                    }
                    else
                    {
                        var resultSchemaName = HtoMetadataExtractor.DeriveSchemaName(resultType.Name);
                        var resultClasses = HtoMetadataExtractor.GetTargetClasses(resultType);
                        builder[(htoClassName, actionName)] = (resultSchemaName, resultClasses);
                    }
                }
            }
        }

        return (builder.ToImmutable(), notHtoWarnings.ToImmutable(), missing201Warnings.ToImmutable());
    }

    /// <summary>
    /// Enriches HTO action metadata with ResultType information from controller endpoint attributes.
    /// </summary>
    internal static HtoMetadata EnrichActionsWithResultMappings(
        HtoMetadata metadata,
        ImmutableDictionary<(string HtoClassName, string ActionPropertyName), (string ResultSchemaName, ImmutableArray<string> ResultClasses)> resultMappings)
    {
        if (resultMappings.IsEmpty)
            return metadata;

        var enrichedActions = new List<ActionMetadata>();
        var changed = false;

        foreach (var action in metadata.Actions)
        {
            // Try to find a result mapping for this action.
            // The action's Name may differ from the property name (via [HypermediaAction(Name)]),
            // so we try both the action Name and look through all mappings for this HTO.
            if (TryFindResultMapping(metadata.ClassName, action.Name, resultMappings, out var resultSchemaName, out var resultClasses))
            {
                enrichedActions.Add(action with { ResultSchemaName = resultSchemaName, ResultClasses = new EquatableArray<string>(resultClasses) });
                changed = true;
            }
            else
            {
                enrichedActions.Add(action);
            }
        }

        return changed
            ? metadata with { Actions = new EquatableArray<ActionMetadata>(enrichedActions.ToImmutableArray()) }
            : metadata;
    }

    private static bool TryFindResultMapping(
        string htoClassName, string actionName,
        ImmutableDictionary<(string HtoClassName, string ActionPropertyName), (string ResultSchemaName, ImmutableArray<string> ResultClasses)> mappings,
        out string resultSchemaName, out ImmutableArray<string> resultClasses)
    {
        // The mapping key uses the property name on the HTO, which is the action's C# property name.
        // The action's Name might be overridden via [HypermediaAction(Name)], so also check by Name.
        foreach (var kvp in mappings)
        {
            if (kvp.Key.HtoClassName == htoClassName && kvp.Key.ActionPropertyName == actionName)
            {
                resultSchemaName = kvp.Value.ResultSchemaName;
                resultClasses = kvp.Value.ResultClasses;
                return true;
            }
        }

        resultSchemaName = "";
        resultClasses = ImmutableArray<string>.Empty;
        return false;
    }

    private static bool InheritsFrom(INamedTypeSymbol type, string baseFullName)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.ToDisplayString() == baseFullName)
                return true;
            current = current.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Checks if a method has a 201-related response attribute (ProducesResponseType, SwaggerResponse, etc.)
    /// by checking attribute name and constructor argument for value 201.
    /// </summary>
    private static bool Has201ResponseAttribute(IMethodSymbol method)
    {
        foreach (var attr in method.GetAttributes())
        {
            var name = attr.AttributeClass?.Name;
            if (name == null) continue;

            // Match ProducesResponseType, ProducesResponseTypeAttribute, SwaggerResponse, SwaggerResponseAttribute, etc.
            if (!name.Contains("ProducesResponseType") && !name.Contains("SwaggerResponse"))
                continue;

            // Check constructor arguments for integer value 201
            foreach (var arg in attr.ConstructorArguments)
            {
                if (arg.Value is int intVal && intVal == 201)
                    return true;
            }
        }

        return false;
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(Compilation compilation)
    {
        var stack = new Stack<INamespaceSymbol>();
        stack.Push(compilation.GlobalNamespace);

        while (stack.Count > 0)
        {
            var ns = stack.Pop();
            foreach (var type in ns.GetTypeMembers())
            {
                yield return type;
            }

            foreach (var childNs in ns.GetNamespaceMembers())
            {
                stack.Push(childNs);
            }
        }
    }
}
