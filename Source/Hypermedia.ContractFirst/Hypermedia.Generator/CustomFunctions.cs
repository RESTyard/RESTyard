using Scriban.Runtime;

namespace Hypermedia.Generator;

internal class CustomFunctions : ScriptObject
{
    public static string Uncapitalize(string s) => s.Length == 0 ? string.Empty : char.ToLower(s[0]) + s[1..];
}