# Migration Guide: SirenConverter to ToSiren()

This guide covers behavioral differences and required changes when migrating from the reflection-based `SirenConverter` / `SirenHypermediaFormatter` to the source-generated `ToSiren()` extension methods.

## New Namespaces

When using `ToSiren()` directly in controllers or consuming the Siren POCO types, add these namespaces:

```csharp
using RESTyard.AspNetCore.Hypermedia.Siren.Model;  // SirenEntity<T>, SirenLink, SirenAction, etc.
using RESTyard.AspNetCore.Hypermedia.Siren;         // SirenMapperOptions
```

## Property Name Casing

**`SirenConverter` (old):** Entity property names are always PascalCase (the C# property name, or `[HypermediaProperty(Name)]` override). The user has no control over casing — it's hardcoded in the Newtonsoft-based serialization.

**`ToSiren()` (new):** Siren structural properties (`class`, `rel`, `href`, `title`, etc.) are always lowercase via `[JsonPropertyName]` on the Siren POCOs. Entity data properties (the `TProperties` POCO) use C# property names as-is. **The user controls casing via `JsonSerializerOptions.PropertyNamingPolicy`** at the ASP.NET Core serializer level.

**Action required:** If your clients depend on PascalCase property names, ensure your serializer does NOT have `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`. If you want camelCase (common for web APIs), set the naming policy — this is now under your control.

## Auto Self Link

**`SirenConverter` (old):** Self links are only present if the HTO has an explicit `ILink<TSelf>` property with `[Relations(["self"])]`, initialized via `Link.To(this)`. No automatic self link.

**`ToSiren()` (new):** A self link is **automatically added** by resolving the HTO's own route via `resolver.ObjectToRoute(hto)`. This is controlled by `SirenMapperOptions.AutoSelfLink` (default: `true`).

**Duplicate prevention:** If the HTO has an explicit `ILink` property with `[Relations(["self"])]` (case-insensitive match), the auto self link is **suppressed at compile time** — the explicit link takes precedence. No duplicate self links will be produced in this case, regardless of the `AutoSelfLink` setting.

**Action required:**
- If your HTOs already have explicit self link properties: **no action needed** — the generator detects them and suppresses the auto self link.
- If your HTOs do NOT have self links today and clients don't expect them, set `AutoSelfLink = false` to preserve the existing behavior.
- If you want the new auto-self-link behavior (recommended), no action needed — it's the default.

## NoProperties Marker Type

**`SirenConverter` (old):** HTOs without data properties produce `"properties": {}` (empty object).

**`ToSiren()` (new):** HTOs without data properties use `SirenEntity<NoProperties>`. The `Properties` field is `null`, which may be omitted from JSON depending on serializer settings.

**Action required:** If clients depend on `"properties": {}` being present, ensure your serializer includes null properties (`DefaultIgnoreCondition` does not exclude nulls).
