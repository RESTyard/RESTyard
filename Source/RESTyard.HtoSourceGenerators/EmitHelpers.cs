using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Shared helpers for turning extracted metadata values into C# source literals.
/// </summary>
internal static class EmitHelpers
{
    /// <summary>
    /// Escapes a string for use inside a C# string literal, including control
    /// characters (newlines in titles/messages would otherwise break the literal).
    /// </summary>
    internal static string EscapeString(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                case '\0': sb.Append("\\0"); break;
                default:
                    if (char.IsControl(c))
                    {
                        sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        sb.Append(c);
                    }

                    break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Prefixes a name with <c>@</c> when it is a reserved C# keyword, so it can be
    /// emitted as an identifier (e.g. a property named <c>class</c>).
    /// </summary>
    internal static string EscapeIdentifier(string name)
        => SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;

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
