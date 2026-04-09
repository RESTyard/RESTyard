namespace RESTyard.AspNetCore.Hypermedia.Siren;

/// <summary>
/// Configuration options for the generated <c>ToSiren()</c> and <c>ToSirenEmbedded()</c> methods.
/// Can be configured via DI or passed explicitly per call.
/// </summary>
public class SirenMapperOptions
{
    /// <summary>
    /// Default options instance. Used when no options are passed to <c>ToSiren()</c>
    /// and no <c>SirenMapperOptions</c> is registered in DI.
    /// </summary>
    public static readonly SirenMapperOptions Default = new();

    /// <summary>
    /// When <c>true</c> (default), the generated <c>ToSiren()</c> automatically adds a <c>"self"</c> link
    /// by resolving the HTO's own route via the route resolver.
    /// <para>
    /// <b>Duplicate prevention:</b> If the HTO already has an explicit <c>ILink</c> property with
    /// <c>[Relations(["self"])]</c> (case-insensitive), the auto self link is suppressed at compile time
    /// regardless of this setting — the explicit link takes precedence.
    /// </para>
    /// <para>
    /// Set to <c>false</c> to disable auto self links globally for HTOs that do not have explicit self links.
    /// </para>
    /// </summary>
    public bool AutoSelfLink { get; set; } = true;
}
