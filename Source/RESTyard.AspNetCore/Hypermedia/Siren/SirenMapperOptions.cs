namespace RESTyard.AspNetCore.Hypermedia.Siren;

/// <summary>
/// Configuration options for the generated <c>ToSiren()</c> and <c>ToSirenEmbedded()</c> methods.
/// Can be configured via DI or passed explicitly per call.
/// </summary>
public class SirenMapperOptions
{
    /// <summary>
    /// When <c>true</c> (default), the generated <c>ToSiren()</c> automatically adds a <c>"self"</c> link
    /// by resolving the HTO's own route via the route resolver.
    /// Set to <c>false</c> if self links are managed manually via explicit <c>ILink</c> properties.
    /// </summary>
    public bool AutoSelfLink { get; set; } = true;
}
