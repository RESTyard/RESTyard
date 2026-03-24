using System.Collections.Generic;

namespace RESTyard.Schema.Model;

/// <summary>
/// Maps an action on an entity type to its result entity type.
/// Used for multi-assembly scenarios where the controller (with <c>ResultType</c>)
/// is in a different assembly than the HTO.
/// </summary>
public class ActionResultMapping
{
    /// <summary>
    /// Schema name of the entity that owns the action.
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the action.
    /// </summary>
    public string ActionName { get; set; } = string.Empty;

    /// <summary>
    /// Schema name of the result entity type.
    /// </summary>
    public string ResultName { get; set; } = string.Empty;

    /// <summary>
    /// Siren classes of the result entity type.
    /// </summary>
    public IReadOnlyList<string> ResultClasses { get; set; } = System.Array.Empty<string>();
}
