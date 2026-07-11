using System;

namespace RESTyard.AspNetCore.Hypermedia.Siren;

/// <summary>
/// How the generated Siren mapper reacts when a link's runtime media types
/// (set via <c>WithAvailableMediaTypes</c>) contain a media type that is not
/// declared with <c>[HypermediaMediaType]</c> on the link property.
/// Only checked when the property declares media types — links without the
/// attribute are never validated.
/// </summary>
public enum MediaTypeMismatchBehavior
{
    /// <summary>Do nothing; runtime media types are emitted as-is.</summary>
    Ignore,

    /// <summary>
    /// Emit a warning via <see cref="SirenMapperOptions.MediaTypeMismatchWarningHandler"/>
    /// and continue. Default.
    /// </summary>
    Warn,

    /// <summary>Throw an <see cref="InvalidOperationException"/>. Useful in integration tests.</summary>
    Throw,
}

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

    /// <summary>
    /// Reaction when a link's runtime media types (builder, <c>WithAvailableMediaTypes</c>)
    /// contain a media type not declared via <c>[HypermediaMediaType]</c> on the link property.
    /// Defaults to <see cref="MediaTypeMismatchBehavior.Warn"/>.
    /// Links without a <c>[HypermediaMediaType]</c> attribute are never validated.
    /// </summary>
    public MediaTypeMismatchBehavior MediaTypeMismatch { get; set; } = MediaTypeMismatchBehavior.Warn;

    /// <summary>
    /// Sink for <see cref="MediaTypeMismatchBehavior.Warn"/> messages.
    /// Defaults to <see cref="System.Diagnostics.Trace.TraceWarning(string)"/>;
    /// replace with e.g. <c>msg => logger.LogWarning(msg)</c> to route into your logging.
    /// </summary>
    public Action<string> MediaTypeMismatchWarningHandler { get; set; } =
        message => System.Diagnostics.Trace.TraceWarning(message);
}
