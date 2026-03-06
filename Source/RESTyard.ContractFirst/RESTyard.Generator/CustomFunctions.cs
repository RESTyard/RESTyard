using System;
using Scriban.Runtime;

namespace RESTyard.Generator;

internal class CustomFunctions : ScriptObject
{
    public static string Uncapitalize(string s) => s.Length == 0 ? string.Empty : char.ToLowerInvariant(s[0]) + s[1..];
    
    public static string Capitalize(string s) => s.Length == 0 ? string.Empty : char.ToUpperInvariant(s[0]) + s[1..];

    public static void Warning(string message) => Console.WriteLine($"[WARNING] {message}");
}