using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Extracts <c>ResultType</c> declarations from controller endpoint attributes and
/// enriches extracted HTO action metadata with the resulting schema names/classes.
/// Modern <c>[HypermediaActionEndpoint&lt;THto&gt;]</c> attributes are matched incrementally via
/// <c>ForAttributeWithMetadataName</c>.
/// </summary>
internal static class ActionResultMappingExtractor
{
    /// <summary>
    /// Extracts action-result data from the <c>[HypermediaActionEndpoint&lt;THto&gt;]</c> attributes
    /// on a single method (the <c>ForAttributeWithMetadataName</c> transform).
    /// </summary>
    internal static ActionResultData ExtractFromEndpointAttributes(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not IMethodSymbol method)
        {
            return ActionResultData.Empty;
        }

        var mappings = ImmutableArray.CreateBuilder<ActionResultMapping>();
        var notHtoWarnings = ImmutableArray.CreateBuilder<ResultTypeNotHtoWarning>();
        var missing201Warnings = ImmutableArray.CreateBuilder<Missing201Warning>();
        var occurrences = ImmutableArray.CreateBuilder<EndpointOccurrence>();

        foreach (var attr in context.Attributes)
        {
            var attrClass = attr.AttributeClass;
            if (attrClass == null || !attrClass.IsGenericType)
                continue;

            // Get the HTO type argument
            var htoType = attrClass.TypeArguments[0] as INamedTypeSymbol;
            if (htoType == null)
                continue;

            // Get the action property name (first constructor arg)
            if (attr.ConstructorArguments.Length == 0 || attr.ConstructorArguments[0].Value is not string actionPropName)
                continue;

            // Track every attribute application for duplicate-endpoint detection (RY0033),
            // regardless of whether it declares a ResultType.
            occurrences.Add(new EndpointOccurrence(
                $"action:{HtoMetadataExtractor.GetNamespaceQualifiedName(htoType)}.{actionPropName}",
                $"{htoType.Name}.{actionPropName}",
                LocationInfo.FromAttribute(attr)));

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
                    notHtoWarnings.Add(new ResultTypeNotHtoWarning(
                        resultType.ToDisplayString(), htoClassName, actionPropName,
                        LocationInfo.FromAttribute(attr)));
                }
                else
                {
                    var resultSchemaName = HtoMetadataExtractor.GetSchemaName(resultType);
                    var resultClasses = HtoMetadataExtractor.GetTargetClasses(resultType);
                    mappings.Add(new ActionResultMapping(
                        HtoMetadataExtractor.GetNamespaceQualifiedName(htoType),
                        actionPropName,
                        HtoMetadataExtractor.GetSchemaName(htoType),
                        ResolveActionName(htoType, actionPropName),
                        resultSchemaName,
                        new EquatableArray<string>(resultClasses)));
                }
            }
            else
            {
                // No ResultType — check if method has 201-related attributes
                if (Has201ResponseAttribute(method))
                {
                    missing201Warnings.Add(new Missing201Warning(
                        method.ContainingType.Name, method.Name, actionPropName,
                        LocationInfo.FromAttribute(attr)));
                }
            }
        }

        return new ActionResultData(
            new EquatableArray<ActionResultMapping>(mappings.ToImmutable()),
            new EquatableArray<ResultTypeNotHtoWarning>(notHtoWarnings.ToImmutable()),
            new EquatableArray<Missing201Warning>(missing201Warnings.ToImmutable()),
            new EquatableArray<EndpointOccurrence>(occurrences.ToImmutable()));
    }

    /// <summary>
    /// Extracts endpoint occurrences from the <c>[HypermediaObjectEndpoint&lt;THto&gt;]</c> attributes
    /// on a single method, for duplicate-endpoint detection (RY0033).
    /// </summary>
    internal static EquatableArray<EndpointOccurrence> ExtractObjectEndpointOccurrences(
        GeneratorAttributeSyntaxContext context)
    {
        var occurrences = ImmutableArray.CreateBuilder<EndpointOccurrence>();

        foreach (var attr in context.Attributes)
        {
            var attrClass = attr.AttributeClass;
            if (attrClass == null || !attrClass.IsGenericType)
                continue;

            if (attrClass.TypeArguments[0] is not INamedTypeSymbol htoType)
                continue;

            occurrences.Add(new EndpointOccurrence(
                $"object:{HtoMetadataExtractor.GetNamespaceQualifiedName(htoType)}",
                htoType.Name,
                LocationInfo.FromAttribute(attr)));
        }

        return new EquatableArray<EndpointOccurrence>(occurrences.ToImmutable());
    }

    /// <summary>
    /// Groups endpoint occurrences by identity and returns, for every HTO/action with more than
    /// one endpoint, all occurrences after the first (in file-position order) — each of those is
    /// reported as RY0033. Deterministic so incremental re-runs report identical diagnostics.
    /// </summary>
    internal static ImmutableArray<EndpointOccurrence> FindDuplicateEndpoints(
        IEnumerable<EndpointOccurrence> occurrences)
    {
        return occurrences
            .GroupBy(o => o.Key, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .SelectMany(g => g
                .OrderBy(o => o.Location?.FilePath ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(o => o.Location?.TextSpan.Start ?? int.MaxValue)
                .Skip(1))
            .ToImmutableArray();
    }

    /// <summary>
    /// Merges the per-method endpoint fragments into one deterministic <see cref="ActionResultData"/>.
    /// Duplicate (HTO, action) keys are last-wins; mappings are sorted for stable output ordering.
    /// </summary>
    internal static ActionResultData Merge(ImmutableArray<ActionResultData> endpointFragments)
    {
        var mappingsByKey = new Dictionary<(string HtoClassName, string ActionPropertyName), ActionResultMapping>();
        var notHtoWarnings = ImmutableArray.CreateBuilder<ResultTypeNotHtoWarning>();
        var missing201Warnings = ImmutableArray.CreateBuilder<Missing201Warning>();
        var occurrences = ImmutableArray.CreateBuilder<EndpointOccurrence>();

        foreach (var fragment in endpointFragments)
        {
            foreach (var mapping in fragment.Mappings)
                mappingsByKey[(mapping.HtoClassName, mapping.ActionPropertyName)] = mapping;
            notHtoWarnings.AddRange(fragment.NotHtoWarnings);
            missing201Warnings.AddRange(fragment.Missing201Warnings);
            occurrences.AddRange(fragment.EndpointOccurrences);
        }

        var orderedMappings = mappingsByKey.Values
            .OrderBy(m => m.HtoClassName, StringComparer.Ordinal)
            .ThenBy(m => m.ActionPropertyName, StringComparer.Ordinal)
            .ToImmutableArray();

        return new ActionResultData(
            new EquatableArray<ActionResultMapping>(orderedMappings),
            new EquatableArray<ResultTypeNotHtoWarning>(notHtoWarnings.ToImmutable()),
            new EquatableArray<Missing201Warning>(missing201Warnings.ToImmutable()),
            new EquatableArray<EndpointOccurrence>(occurrences.ToImmutable()));
    }

    /// <summary>
    /// Enriches HTO action metadata with ResultType information from controller endpoint attributes.
    /// </summary>
    internal static HtoMetadata EnrichActionsWithResultMappings(
        HtoMetadata metadata,
        EquatableArray<ActionResultMapping> resultMappings)
    {
        if (resultMappings.IsEmpty)
            return metadata;

        var enrichedActions = new List<ActionMetadata>();
        var changed = false;

        foreach (var action in metadata.Actions)
        {
            if (TryFindResultMapping(metadata.FullClassName, action, resultMappings, out var mapping))
            {
                enrichedActions.Add(action with { ResultSchemaName = mapping.ResultSchemaName, ResultClasses = mapping.ResultClasses });
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
        string htoClassName, ActionMetadata action,
        EquatableArray<ActionResultMapping> mappings,
        out ActionResultMapping mapping)
    {
        // Class keys are namespace-qualified (GEN-05). Modern endpoint attributes key mappings by
        // the action's C# property name; the attribute may name a base HTO for inherited actions,
        // so the declaring class is tried as well.
        return TryFindByKey(htoClassName, action.PropertyName, mappings, out mapping)
               || TryFindByKey(action.DeclaringClassName, action.PropertyName, mappings, out mapping);
    }

    private static bool TryFindByKey(
        string htoClassName, string actionKey,
        EquatableArray<ActionResultMapping> mappings,
        out ActionResultMapping mapping)
    {
        foreach (var candidate in mappings)
        {
            if (candidate.HtoClassName == htoClassName && candidate.ActionPropertyName == actionKey)
            {
                mapping = candidate;
                return true;
            }
        }

        mapping = null!;
        return false;
    }

    /// <summary>
    /// Resolves the effective schema action name for a property on an HTO: the
    /// <c>[HypermediaAction(Name)]</c> override when present, else the property name.
    /// Walks base types since the property may be inherited.
    /// </summary>
    private static string ResolveActionName(INamedTypeSymbol htoType, string actionPropertyName)
    {
        for (var current = htoType;
             current != null && current.SpecialType != SpecialType.System_Object;
             current = current.BaseType)
        {
            var property = current.GetMembers(actionPropertyName).OfType<IPropertySymbol>().FirstOrDefault();
            if (property == null)
            {
                continue;
            }

            var actionAttr = property.GetAttributes().FirstOrDefault(a =>
                a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.HypermediaActionAttributeFullName);

            return (actionAttr != null ? HtoMetadataExtractor.GetNamedArgumentString(actionAttr, "Name") : null)
                   ?? actionPropertyName;
        }

        return actionPropertyName;
    }

    /// <summary>
    /// Checks if a method has a 201-related response attribute (ProducesResponseType, SwaggerResponse, etc.)
    /// by checking attribute name plus constructor and named arguments for value 201.
    /// Enum arguments (e.g. <c>HttpStatusCode.Created</c>) match too — their boxed value is the underlying int.
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

            foreach (var arg in attr.ConstructorArguments)
            {
                if (arg.Value is int intVal && intVal == 201)
                    return true;
            }

            // Named-argument form: [ProducesResponseType(..., StatusCode = 201)]
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "StatusCode" && namedArg.Value.Value is int namedVal && namedVal == 201)
                    return true;
            }
        }

        return false;
    }
}
