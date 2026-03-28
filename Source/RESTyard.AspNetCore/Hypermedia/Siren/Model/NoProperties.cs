namespace RESTyard.AspNetCore.Hypermedia.Siren.Model;

/// <summary>
/// Marker type indicating a Siren entity has no data properties.
/// Used as the type parameter for <see cref="SirenEntity{TProperties}"/> and
/// <see cref="SirenEmbeddedEntity{TProperties}"/> when the HTO has no properties to serialize.
/// </summary>
public sealed class NoProperties;
