using System.Collections.Immutable;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// A controller-declared <c>ResultType</c> mapping for an HTO action:
/// executing <c>HtoClassName.ActionPropertyName</c> yields an entity of schema
/// <c>ResultSchemaName</c> with the given Siren classes.
/// <c>HtoClassName</c> is namespace-qualified so same-named HTOs in different
/// namespaces cannot receive each other's mapping (GEN-05).
/// <c>HtoSchemaName</c>/<c>ActionName</c> are the schema-level keys (derived schema name and
/// effective action name including a <c>[HypermediaAction(Name)]</c> override) — these are what
/// the emitted action-result registry carries, since the runtime merge in another assembly
/// only sees schema names, not C# class/property names.
/// </summary>
internal sealed record ActionResultMapping(
    string HtoClassName,
    string ActionPropertyName,
    string HtoSchemaName,
    string ActionName,
    string ResultSchemaName,
    EquatableArray<string> ResultClasses);

/// <summary>A <c>ResultType</c> that is not marked with <c>[HypermediaObject]</c> (RY0032).</summary>
internal sealed record ResultTypeNotHtoWarning(
    string ResultTypeName,
    string HtoClassName,
    string ActionPropertyName,
    LocationInfo? Location);

/// <summary>An endpoint declaring a 201 response without a <c>ResultType</c> (RY0031).</summary>
internal sealed record Missing201Warning(
    string ControllerName,
    string MethodName,
    string ActionPropertyName,
    LocationInfo? Location);

/// <summary>
/// A single endpoint attribute application, tracked for duplicate detection (RY0033, GEN-11).
/// <c>Key</c> is the identity duplicates are grouped by (namespace-qualified HTO class name,
/// for actions suffixed with the action key); <c>DisplayName</c> is the human-readable form
/// used in the diagnostic message.
/// </summary>
internal sealed record EndpointOccurrence(
    string Key,
    string DisplayName,
    LocationInfo? Location);

/// <summary>
/// Value-equatable aggregate of all action-result information extracted from controller
/// endpoint attributes. Value equality is required so the incremental pipeline can cache
/// downstream outputs even though the underlying compilation changes on every edit (GEN-04).
/// </summary>
internal sealed record ActionResultData(
    EquatableArray<ActionResultMapping> Mappings,
    EquatableArray<ResultTypeNotHtoWarning> NotHtoWarnings,
    EquatableArray<Missing201Warning> Missing201Warnings,
    EquatableArray<EndpointOccurrence> EndpointOccurrences)
{
    internal static readonly ActionResultData Empty = new(
        new EquatableArray<ActionResultMapping>(ImmutableArray<ActionResultMapping>.Empty),
        new EquatableArray<ResultTypeNotHtoWarning>(ImmutableArray<ResultTypeNotHtoWarning>.Empty),
        new EquatableArray<Missing201Warning>(ImmutableArray<Missing201Warning>.Empty),
        new EquatableArray<EndpointOccurrence>(ImmutableArray<EndpointOccurrence>.Empty));
}
