using System.Linq;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Shared helpers for turning extracted metadata values into C# source literals.
/// </summary>
internal static class EmitHelpers
{
    internal static string EscapeString(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    /// <summary>Formats items as <c>"a", "b"</c> for use inside an array initializer.</summary>
    internal static string QuotedList(EquatableArray<string> items)
        => string.Join(", ", items.Select(i => $"\"{EscapeString(i)}\""));

    /// <summary>Formats items as <c>"a", "b"</c> for use inside an array initializer.</summary>
    internal static string QuotedList(System.Collections.Immutable.ImmutableArray<string> items)
        => string.Join(", ", items.Select(i => $"\"{EscapeString(i)}\""));

    /// <summary>
    /// Formats items as a <c>string[]</c> expression:
    /// <c>new[] { "a", "b" }</c>, or <c>System.Array.Empty&lt;string&gt;()</c> when empty.
    /// </summary>
    internal static string StringArrayLiteral(EquatableArray<string> items)
        => items.Length == 0
            ? "System.Array.Empty<string>()"
            : $"new[] {{ {QuotedList(items)} }}";
}
