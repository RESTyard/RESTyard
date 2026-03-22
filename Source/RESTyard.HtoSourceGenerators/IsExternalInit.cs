// Polyfill for record types on netstandard2.0.
// Records require IsExternalInit which is only available in .NET 5+.
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit;
}
