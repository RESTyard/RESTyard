# Migration Guide: SirenConverter to ToSiren()

This guide covers behavioral differences and required changes when migrating from the reflection-based `SirenConverter` / `SirenHypermediaFormatter` to the source-generated `ToSiren()` extension methods.

## Required: `[assembly: HypermediaAssembly]`

The source generator only runs for assemblies marked with the assembly-level attribute. Add it once
per assembly (e.g. in `Program.cs` or a dedicated `AssemblyInfo.cs`):

```csharp
using RESTyard.AspNetCore.Hypermedia.Attributes;

[assembly: HypermediaAssembly]
```

**Action required:** Add this attribute to **every** assembly that contains HTOs **and** every
assembly that contains controllers — even a controller assembly with no HTOs of its own. Without it:

- The generator emits nothing — no `GetSchema()`, no Properties POCO, no `ToSiren()` mappers — even
  with the NuGet package referenced.
- `HypermediaAssemblyDiscovery.GetAssemblies()` won't find the assembly.
- Controller-only assemblies must still carry it so the generator can read controller attributes
  (e.g. `ResultType` on `[HypermediaActionEndpoint]`).

To enable `ToSiren()` generation (not just schema), set `Siren = true`:

```csharp
[assembly: HypermediaAssembly(Siren = true)]
```

## New Build Diagnostics

Turning on source generation can surface **new build diagnostics** on HTOs that compiled fine under
the reflection-based formatter:

| Diagnostic | Severity | Cause | Fix |
|---|---|---|---|
| `RY0020` | Warning | `IEmbeddedEntity<T>` property missing `[Relations]` | Add `[Relations(["rel"])]` to the property |
| `RY0021` | Warning | `ILink<T>` property missing `[Relations]` | Add `[Relations(["rel"])]` to the property |
| `RY0024` | Error | Two HTOs derive the same schema name (e.g. `HypermediaCustomerHto` + `CustomerHto` → both `Customer`) | Apply `[HypermediaSchemaName]` to one of them |
| `RY0030` | Warning | `Siren = true` combined with `Schema = false` | Remove `Schema = false` (Siren needs the Properties POCO) |
| `RY0032` | Warning | `ResultType` on an action endpoint is not a `[HypermediaObject]` | Remove `ResultType`, or suppress if the result is intentionally non-hypermedia |

**Action required:** `ILink`/`IEmbeddedEntity` properties without `[Relations]` are excluded from
the schema (RY0020/RY0021 warn) — add `[Relations]` to include them. A schema-name collision
(RY0024) fails the build; disambiguate with `[HypermediaSchemaName("...")]`
(`RESTyard.Schema.Model`) on one of the colliding HTOs.

## Assembly Discovery

Manual assembly lists can be replaced with auto-discovery (relies on the `[HypermediaAssembly]`
attribute above):

```csharp
builder.Services.AddHypermediaExtensions(o =>
{
    o.ControllerAndHypermediaAssemblies = HypermediaAssemblyDiscovery.GetAssemblies();
});
```

**Action required:** Optional. If you keep an explicit assembly list, no change is needed — but every
listed assembly still requires `[assembly: HypermediaAssembly]` for generation to run.

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

## Serializer Configuration

**`SirenConverter` (old):** Serialization is handled internally by the converter using Newtonsoft.Json. The user has no direct control over how the Siren JSON is produced.

**`ToSiren()` (new):** Returns a `SirenEntity<T>` POCO. The user serializes it with `System.Text.Json` (or any serializer) and is in full control of the `JsonSerializerOptions`.

**Recommended `JsonSerializerOptions`:**

```csharp
var options = new JsonSerializerOptions
{
    // Omit null structural properties (class, title, type on links/actions)
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

    // Enum serialization — see "Enum Serialization" section below
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
};
```

## Enum Serialization

**`SirenConverter` (old):** Enums in HTO properties are serialized using `[EnumMember(Value = "...")]` attribute values via `EnumHelper.GetEnumMemberValue()`. For example, `[EnumMember(Value = "red")] Red` serializes as `"red"`.

**`ToSiren()` (new):** Enum serialization is controlled by the consumer's `JsonSerializerOptions`. The `[EnumMember]` attribute is **not supported** by `System.Text.Json` in .NET 8. Without configuration, enums serialize as numeric values (e.g. `0`, `1`).

**Action required:**
- Add `JsonStringEnumConverter` to your serializer options. Use `JsonNamingPolicy.CamelCase` for simple cases where `[EnumMember]` values match the camelCase member name.
- For custom `[EnumMember]` values that don't match camelCase (e.g. `[EnumMember(Value = "some-custom-value")]`), replace `[EnumMember]` with System.Text.Json attributes:
  - .NET 9+: Use `[JsonStringEnumMemberName("some-custom-value")]`
  - .NET 8: Use a custom `JsonConverter` or a third-party package
- **This also applies to enums in action prefilled/default values** — they go through the same serializer.

## Null Property Handling

**`SirenConverter` (old):** Null property values are included in the output as `"propertyName": null` (controlled by `HypermediaConverterConfiguration.WriteNullProperties`, default `true`).

**`ToSiren()` (new):** Null handling is controlled by the consumer's `JsonSerializerOptions.DefaultIgnoreCondition`. With `WhenWritingNull`, null properties are omitted from the JSON output.

**Action required:** If clients depend on null properties being present in the JSON, do NOT set `DefaultIgnoreCondition = WhenWritingNull`, or set it only at the serializer level and not on the properties POCO. Note that `WhenWritingNull` applies globally — it also omits null Siren structural properties (`class`, `title`, etc.), which is typically desirable.

## Non-Nullable Actions Must Always Render (new behavior)

Previously, a null or unavailable (`CanExecute() == false`) action property was always silently
omitted from the Siren output — even when the property was declared non-nullable. Links and
embedded entities already threw `InvalidOperationException` for null.

**New:** in `ToSiren()`, a non-nullable action property is **mandatory**: it is always on the
wire, matching `isMandatory: true` in the generated schema. Declaring the property nullable is
the explicit way to say "this action may be absent".

- **Non-nullable action property is null** → `InvalidOperationException` at render time,
  same as links and embedded entities.
- **Non-nullable action with `CanExecute() == false`** → `InvalidOperationException` at render
  time. A mandatory action must always be available.
- **Nullable action property is null** → silently omitted (unchanged).
- **Nullable action with `CanExecute() == false`** → silently omitted (unchanged); `CanExecute`
  remains the mechanism for conditional availability on nullable actions.

Note: in `#nullable disable` contexts there are no nullability annotations, so **every**
link, action, and embedded-entity property counts as mandatory — a null value throws.
Enable nullable reference types and annotate optional members with `?`.

**Action required:** if an action can be hidden — null property or a `canExecute` delegate that
can return false — declare the property nullable. Keep non-nullable only for actions that are
always available.

### Contract-First Migration

The contract-first templates generate **non-nullable** action properties by default, whose `*Op`
constructors require a `canExecute` delegate. Under the rule above this means: such an action's
delegate must always return `true` once the project adopts `Siren = true` — a conditionally
available action declared non-nullable throws at render time.

To express "this operation may be absent", set `mandatory="false"` on the `<Operation>` element.
The attribute defaults to `true` (mirroring the existing `mandatory` attribute on `<Property>`
and `<Link>`), so existing contract files validate unchanged and produce identical code:

```xml
<Operation name="MarkAsFavorite" method="Post" mandatory="false" />
```

The server templates (`server/csharp/v4` and `v5`) then generate a **nullable** action property
and a nullable constructor argument — pass `null` (or let its `canExecute` delegate return
`false`) to omit the action from the response. The client templates already model every action
as optional, so nothing changes on the consuming side.

## Schema: `required` Derived from Non-Nullability (new behavior)

Previously, generated JSON Schemas (entity properties and action parameters) never contained the
`required` keyword — every field looked optional to schema consumers.

**New:** `JsonSchemaFactory` emits `required` for every **non-nullable** property, matching C# semantics:
`string Name` is required, `string? Nickname` is optional. This applies to entity `propertiesSchema`,
action `parameterSchema`, and nested types in `$defs`. The C# `required` keyword continues to work
and is merged with the derived list.

Notes:

- Nullable reference annotations from the HTO are now preserved in the generated properties POCO
  (previously `string?` degraded to `string`), so the derived `required` reflects your HTO declarations.
- Properties without nullability information (`#nullable disable` contexts) are treated as optional —
  `required` is only derived where the compiler recorded an annotation.
- **Opt-out:** `new JsonSchemaFactory(deriveRequiredFromNonNullable: false)` restores the old output.
- **Action required:** none for servers. Schema consumers (client generators, validators) that assumed
  "everything optional" will now see accurate `required` lists — regenerate clients after upgrading.

## Schema: External Links Now Included (new behavior)

Previously, `ExternalLink` properties were silently absent from the generated schema.

**New:** `ExternalLink` properties with `[Relations]` appear in the entity's `links` array marked
`isExternal: true`, **without** `targetName`/`targetClasses` (there is no target entity type in the
API). The expected media type(s) can be declared with
`[HypermediaMediaType("application/pdf", "text/html")]` on the property and are emitted as
`mediaTypes` (a string array).
`ExternalLink` properties **without** `[Relations]` now trigger the RY0021 warning (previously silent).

**Action required:** none for servers — the Siren wire format is unchanged (external links were always
rendered there). Schema consumers should treat links with `isExternal: true` as external
(dereferencing yields a non-Siren resource).

## Link Media Types: Schema `mediaTypes` and Siren `type` Fallback (new behavior)

Every schema link now carries `mediaTypes: string[]` — the declared `[HypermediaMediaType]` values,
or `["application/vnd.siren+json"]` when the attribute is absent. Entity links always target Siren
resources, so the default is accurate there; for external links to non-Siren resources, declare the
attribute.

The generated `ToSiren()` mapper emits the link `type` with this precedence:

1. Runtime media types set on the reference (`WithAvailableMediaType(s)`) — unchanged, always win.
2. Declared `[HypermediaMediaType]` values (comma-joined, as before for multiple types).
3. Neither → `type` omitted (unchanged). Plain navigation links stay type-less on the wire —
   Siren is the baseline, and the schema states the default once via `mediaTypes`.

**Behavior change:** only case 2 is new — links with a declared `[HypermediaMediaType]` now carry
a `type` even when the runtime sets none. The legacy reflection-based `SirenConverter` is
unchanged — it only emits runtime media types.

**Mismatch validation:** when a property declares media types and the runtime reference returns one
outside the declared list, the mapper warns by default (`SirenMapperOptions.MediaTypeMismatch`,
`Warn`/`Throw`/`Ignore`; warning sink `MediaTypeMismatchWarningHandler`, default
`Trace.TraceWarning`). Links without the attribute are never validated.

**Action required:** none for servers. To silence mismatch warnings, either fix the declaration or
set `MediaTypeMismatch = MediaTypeMismatchBehavior.Ignore`.

## Schema Names: `[HypermediaSchemaName]` and Collision Errors (new behavior)

Schema names are derived by stripping the `Hypermedia` prefix and `Hto` suffix from the class name.
Two HTOs deriving the same name previously produced a silently broken schema (cross-references
pointed at the wrong entity); this is now build error **RY0024**. Disambiguate with the new
`[HypermediaSchemaName("...")]` attribute (`RESTyard.Schema.Model`) — the override applies to the
entity name and every cross-reference (`targetName`, `resultName`) consistently.

**Action required:** only if your assembly contains colliding class names (e.g.
`HypermediaCustomerHto` and `CustomerHto`).

## Record HTOs Now Generate (new behavior)

`record` HTOs were previously ignored by the source generator without any diagnostic (the runtime
`SirenConverter` handled them fine). They now generate schema and Siren mappers like class HTOs;
positional properties become data properties. Note: records cannot derive from the obsolete
`HypermediaObject` base class — implement `IHypermediaObject` directly.

**Action required:** none — existing record HTOs start producing schema entries on rebuild.

## Embedded Entity Collection Detection Fixed (new behavior)

Detection now matches the runtime `SirenConverter`: arrays of `IEmbeddedEntity<THto>` and types
implementing `IEnumerable<IEmbeddedEntity<THto>>` are embedded collections. Two changes:

- `IEmbeddedEntity<THto>[]` **array properties** previously leaked into the data-properties POCO
  as data; they are now embedded entity collections (and RY0020 warns when `[Relations]` is missing).
- Non-collection generics over an embedded entity (`Func<IEmbeddedEntity<T>>`,
  `Dictionary<IEmbeddedEntity<T>, X>`) previously counted as embedded collections; they are now
  ordinary data properties.

**Action required:** none in typical code bases; only affects the exotic property shapes above.

## Controller Usage Pattern

**`SirenConverter` (old):** Controllers return HTO objects directly; the formatter converts them automatically and sets the content type:

```csharp
return Ok(myHto);
```

**`ToSiren()` (new):** Two approaches — convenience or manual.

### Convenience: `OkSiren()` (recommended)

A generated controller extension per HTO that resolves services from DI, calls `ToSiren()`, and sets the `application/vnd.siren+json` content type:

```csharp
[ApiController]
public class MyController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var hto = new MyHto();
        return this.OkSiren(hto);
    }
}
```

### Manual: constructor injection

Inject `IHypermediaRouteResolver` and `IQueryStringBuilder` via constructor, call `ToSiren()` directly. Use `[Produces]` to set the content type:

```csharp
[Produces("application/vnd.siren+json")]
[ApiController]
public class MyController(
    IHypermediaRouteResolver resolver,
    IQueryStringBuilder queryStringBuilder) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var hto = new MyHto();
        return Ok(hto.ToSiren(resolver, queryStringBuilder));
    }
}
```

The manual approach gives full control over serialization and response handling. `SirenMapperOptions` can be injected as an additional parameter if needed (falls back to `SirenMapperOptions.Default` when omitted).

## Action Parameter Deserialization (System.Text.Json)

Hypermedia action parameters are now deserialized with **System.Text.Json**. The Newtonsoft-based custom body binder has been removed and the request body for an action is a **plain JSON object** instead of the legacy Siren array-wrapper `[{ "TypeName": { … } }]`.

### Removed: the custom body binder

`HypermediaParameterFromBodyBinderProvider` / `HypermediaParameterFromBodyBinder` no longer exist. Non-file action parameter bodies bind through the standard framework body path (the System.Text.Json input formatter for controllers, native body binding for minimal APIs). No RESTyard-specific registration is required for action bodies.

### Changed: `[HypermediaActionParameterFromBody]` → `[FromBody]`

`HypermediaActionParameterFromBodyAttribute` is now an obsolete alias for `[FromBody]`.

```csharp
// Before
public Task<ActionResult> MarkAsFavorite([HypermediaActionParameterFromBody] MarkAsFavoriteParameters p) { … }

// After
public Task<ActionResult> MarkAsFavorite([FromBody] MarkAsFavoriteParameters p) { … }
```

Existing code keeps compiling (with an obsolete warning); update it to `[FromBody]` at your convenience. The code generator now emits `[FromBody]`.

### Custom JSON converters: one place to register them

Register custom `JsonConverter`s via **`ConfigureHttpJsonOptions`**. This single source applies uniformly to:

- minimal-API action bodies (native),
- the hypermedia **file-upload form binder** and the `JsonDeserializer` (they resolve `Http.Json.JsonOptions` from request services), and
- controller `[FromBody]` action bodies (RESTyard bridges the converters into `Mvc.JsonOptions`).

```csharp
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new MyCustomConverter());
});
```

### Behavioral change: enums sent as strings

The old Newtonsoft-based body binder parsed enum **names** out of the box. System.Text.Json does not — an action parameter with an enum property whose clients send the enum's string name (e.g. `"PropertyName": "Age"`) will fail deserialization unless you register `JsonStringEnumConverter`:

```csharp
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
```

This is the most common parity gap when moving off the Newtonsoft body binder. Review your action/query parameter types for enum properties.

### Client: legacy parameter serializers are obsolete

The array-wrapper client serializers and their builder methods are obsolete. Switch to the plain-object equivalents:

| Obsolete | Use instead |
|---|---|
| `SingleNewtonsoftJsonObjectParameterSerializer` | `NewtonsoftJsonObjectParameterSerializer` |
| `SingleSystemTextJsonObjectParameterSerializer` | `SystemTextJsonObjectParameterSerializer` |
| `WithSingleNewtonsoftJsonObjectParameterSerializer()` | `WithNewtonsoftJsonObjectParameterSerializer()` |
| `WithSingleSystemTextJsonObjectParameterSerializer()` | `WithSystemTextJsonObjectParameterSerializer()` |

Clients that keep emitting the array-wrapper format will no longer be unwrapped on the action body path — send the plain object.
