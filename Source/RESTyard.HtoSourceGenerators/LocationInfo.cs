using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Equatable snapshot of a source location (file path + spans), captured during analysis so
/// diagnostics can point at real code (GEN-13). <see cref="Location"/> itself holds the syntax
/// tree and would defeat incremental caching — this record stores only equatable values and is
/// converted back via <see cref="ToLocation"/> when the diagnostic is reported.
/// </summary>
internal readonly record struct LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);

    /// <summary>The symbol's first in-source location, or null for metadata-only symbols.</summary>
    public static LocationInfo? FromSymbol(ISymbol symbol)
        => From(symbol.Locations.FirstOrDefault(l => l.IsInSource));

    /// <summary>The location of an attribute application, or null when it has no syntax (metadata).</summary>
    public static LocationInfo? FromAttribute(AttributeData attribute)
        => From(attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation());

    public static LocationInfo? From(Location? location)
        => location is { IsInSource: true, SourceTree: { } tree }
            ? new LocationInfo(tree.FilePath, location.SourceSpan, location.GetLineSpan().Span)
            : null;
}
