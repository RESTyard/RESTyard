# RESTyard.HtoSourceGenerators — Review Findings

Code review of `Source/RESTyard.HtoSourceGenerators` (mainly `HtoSchemaGenerator.cs`, 2745 lines)
against [HypermediaSchema-Design.md](HypermediaSchema-Design.md) and [HypermediaSchema-Plan.md](HypermediaSchema-Plan.md).
Review date: 2026-07-10. Referenced from **Phase 6B** in the plan.

Line numbers refer to `HtoSchemaGenerator.cs` at the time of review and will drift.
Since REF-01 the code is split: analysis in `HtoMetadataExtractor` / `ActionResultMappingExtractor`,
emission in `SchemaEmitter` / `PropertiesPocoEmitter` / `SirenEmitter` / `SirenHelperEmitter` /
`RegistryEmitter`, diagnostics in `GeneratorDiagnostics`, pipeline wiring in `HtoSchemaGenerator`.

## Overview

| ID     | Finding                                                                                                                   | Type          | Size | Risk   | Worth fixing            |
|--------|---------------------------------------------------------------------------------------------------------------------------|---------------|------|--------|-------------------------|
| GEN-01 | Multi-assembly action-result feature dead end-to-end                                                                      | Bug           | M    | High   | ✅ Done                  |
| GEN-02 | `ResultType` enrichment breaks with `[HypermediaAction(Name = ...)]`                                                      | Bug           | S    | High   | ✅ Done                  |
| GEN-03 | RY0031/RY0032 diagnostics duplicated once per HTO; wrong gating                                                           | Bug           | S    | Medium | ✅ Done                  |
| GEN-04 | Incrementality defeated by compilation-wide controller scan                                                               | Perf bug      | M–L  | High   | ✅ Done                  |
| GEN-05 | Same HTO class name in two namespaces crashes generator (hint names)                                                      | Bug           | S    | Medium | ✅ Done                  |
| GEN-06 | Silent schema-name collisions; `[HypermediaSchemaName]` not implemented                                                   | Gap           | M    | Medium | Yes                     |
| GEN-07 | Generated code can fail to compile (escaping, culture, identifiers)                                                       | Bug           | M    | High   | ✅ Done                  |
| GEN-08 | `record` HTOs silently ignored                                                                                            | Gap           | S    | Medium | Yes (or diagnostic)     |
| GEN-09 | Embedded-collection detection too loose and too tight (arrays leak)                                                       | Bug           | S–M  | Medium | Yes                     |
| GEN-10 | Null mandatory action silently omitted; links/embedded throw                                                              | Inconsist.    | S    | Low    | Yes — decide + document |
| GEN-11 | No diagnostic for zero/multiple endpoints per HTO/action (design says)                                                    | Gap           | M    | Medium | Later                   |
| GEN-12 | Diagnostic severity vs. wording mismatch (RY0020/21/30)                                                                   | Inconsist.    | S    | Low    | ✅ Done                  |
| GEN-13 | All diagnostics use `Location.None`                                                                                       | DX gap        | M    | Low    | ✅ Done                  |
| GEN-14 | Startup validation of dangling `TargetName` refs not implemented                                                          | Gap           | M    | Medium | Later                   |
| GEN-15 | Minor issues (201 named args, embedded dup-relations, name sanitizing)                                                    | Nits          | S    | Low    | ✅ Done                  |
| GEN-16 | `required` never emitted in properties/parameter schemas                                                                  | Gap           | M    | Medium | ✅ Done                  |
| GEN-17 | Inherited actions on derived HTOs lose `resultName`/`resultClasses`                                                       | Bug           | S    | Medium | ✅ Done                  |
| GEN-18 | `ExternalLink` properties silently absent; `mediaType` never emitted                                                      | Gap           | M    | Medium | ✅ Done                  |
| REF-01 | Split 2745-line god class into extractor + emitters + pipeline                                                            | Refactoring   | L    | —      | ✅ Done                  |
| REF-02 | Replace indentation-string emission with a `CodeWriter`                                                                   | Refactoring   | M    | —      | ✅ Done                  |
| REF-03 | `GenerateSirenHelper` emits fully static text via `AppendLine` calls                                                      | Refactoring   | S    | —      | ✅ Done                  |
| REF-04 | Unify six duplicated base-type property walks into one classification                                                     | Refactoring   | M    | —      | ✅ Done                  |
| REF-05 | Deduplicate assembly-config normalization (`Siren`→`Schema` rule)                                                         | Refactoring   | S    | —      | ✅ Done                  |
| REF-06 | Add `.WithTrackingName()` + cacheability tests                                                                            | Test gap      | S–M  | —      | ✅ Done                  |
| DOC-01 | Review docs (migration-guide.md, HypermediaApiSchema.md, SourceGenerator.md, readme.md) incorporate changes from done issues | Documentation | S  | —      | yes                     |

Size: S ≈ hours, M ≈ a day, L ≈ multiple days. Risk = impact of leaving it unfixed.

## Suggested fix order

1. ✅ **REF-01 + REF-02 + REF-03** — restructure first; every later fix lands in a smaller, testable unit.
2. ✅ **GEN-04 + REF-06** — incrementality fix with its regression guard.
3. ✅ **GEN-01, GEN-02, GEN-03, GEN-17** — the action-result feature cluster (fix or cut together).
4. ✅ **GEN-05, GEN-07** — generation robustness (crash + invalid code).
5. **GEN-06, GEN-08, GEN-09, GEN-10, ✅ GEN-12, ✅ GEN-13, ✅ GEN-16, ✅ GEN-18** — behavior gaps and DX
   (GEN-16/18 unblock schema-driven client generation; GEN-12/13 pulled forward and done).
6. **GEN-11, GEN-14, ✅ GEN-15, ✅ REF-04, ✅ REF-05** — opportunistic / later (GEN-15, REF-04/05 pulled forward and done).
7. **DOC-01** — documentation update.

## Bugs and gaps

### ✅ GEN-01 — Multi-assembly action-result feature dead end-to-end

The `ResultType` merge across assemblies (plan Step 2.13, multi-assembly support) is broken twice,
independently:

1. **Registry not emitted where needed.** In a controller-only assembly (controllers here, HTOs elsewhere —
   the exact scenario the feature exists for), the registry output block returns early on
   `allHtos.IsEmpty` (line ~326) *before* the `HypermediaActionResultRegistry` `AddSource` call (line ~344).
   The registry is only generated in assemblies that also contain HTOs — where it is not needed.
2. **Nothing consumes it.** The generated `HypermediaActionResultRegistry_<Assembly>` gets no discovery
   attribute (the schema registry emits `[assembly: HypermediaSchemaRegistryAttribute]`; this one emits
   nothing), and `HypermediaSchemaBuilder` contains no code that finds it or merges `ActionResultMapping`s
   into `ActionDescription`s. The plan describes this merge in `ComposeSchema()`; it does not exist.

**Fix:** emit the registry before the `IsEmpty` check, add a discovery attribute
(e.g. `HypermediaActionResultRegistryAttribute`), and implement the merge in
`HypermediaSchemaBuilder.ComposeSchema()`. Add a multi-assembly integration test.
Alternatively: explicitly cut multi-assembly `ResultType` support and delete the dead registry emission.

**Done:** the registry is now emitted before the `allHtos.IsEmpty` gate, so controller-only
assemblies (with `[assembly: HypermediaAssembly]`) get it. It carries a new discovery attribute
`[assembly: HypermediaActionResultRegistryAttribute(typeof(...))]` (mirroring the schema registry
attribute), and its mappings now use **schema-level keys** — derived entity name plus the effective
action name with any `[HypermediaAction(Name)]` override resolved from the referenced HTO's
metadata — since the runtime merge never sees C# class/property names.
`HypermediaSchemaBuilder.Build()` discovers the registries (`DiscoverActionResultMappings`) and a
new `ComposeSchema` overload fills `ResultName`/`ResultClasses` on matching actions (existing
values from compile-time enrichment win; unmatched mappings log a warning). Covered by a
two-compilation generator test (controller-only assembly referencing an HTO assembly) and
`ComposeSchema` merge tests. Known limitation: the runtime merge matches entities by exact schema
name, so inherited actions on *derived* HTOs in another assembly are not enriched (the schema
model has no inheritance information); single-assembly inheritance is handled at compile time
(GEN-17).

### ✅ GEN-02 — `ResultType` enrichment breaks with `[HypermediaAction(Name = ...)]`

The result-mapping dictionary is keyed by the action **property** name (the controller attribute's
constructor argument), but `EnrichActionsWithResultMappings` looks up with `action.Name` (line ~2647) —
the overridden name when `[HypermediaAction(Name = ...)]` is set. `ActionMetadata.PropertyName` exists but
is not used for the lookup. The comment at line ~2668 acknowledges the ambiguity without resolving it.
Result: `ResultName`/`ResultClasses` silently missing for renamed actions.

**Fix:** look up by `action.PropertyName` (optionally fall back to `action.Name`). Add a test with a
renamed action + `ResultType`.

**Done:** `TryFindResultMapping` now takes the full `ActionMetadata` and probes keys in order:
`(HtoClassName, PropertyName)`, `(DeclaringClassName, PropertyName)` (GEN-17), then the same two
with `action.Name` as fallback (legacy attributes key by the Op-type-derived name). Regression
test: renamed action (`[HypermediaAction(Name = "startQuery")]` + endpoint naming the property)
gets `ResultName` populated.

### ✅ GEN-03 — RY0031/RY0032 diagnostics duplicated per HTO; wrong gating

The warning loops (lines ~201–216) run inside the **per-HTO** `RegisterSourceOutput`:

- An assembly with 20 HTOs reports each warning 20 times.
- They run *before* the `config == null` check, so assemblies **without** `[HypermediaAssembly]` still get
  schema diagnostics — contradicting "no attribute → emit nothing".
- In a controller-only assembly (zero HTOs) the per-HTO block never fires, so the warnings never appear —
  exactly where they matter (see GEN-01).

**Fix:** register a dedicated diagnostics output keyed on `actionResultMappings` (+ assembly config) alone.

**Done:** RY0031/RY0032 (and RY0030, which had the same per-HTO duplication) moved to a dedicated
compilation-level output keyed on `actionResultMappings.Combine(assemblyConfig)`: reported once,
gated on `[HypermediaAssembly]` presence (RY0031/32 additionally on `Schema = true`), and firing
even in assemblies with zero HTOs. Tests: single occurrence with multiple HTOs, nothing without
the assembly attribute, RY0031 in a controller-only assembly.

### ✅ GEN-04 — Incrementality defeated by compilation-wide controller scan

`actionResultMappings` comes from `context.CompilationProvider.Select(...)` (line ~189) and is `Combine`d
into every HTO output. Consequences:

- The compilation changes on every keystroke; the result tuple contains `ImmutableDictionary` /
  `ImmutableArray` (reference equality only) → **every HTO regenerates on every edit**. The careful
  `EquatableArray` work in the metadata records is nullified by this one node.
- `GetAllTypes` walks `compilation.GlobalNamespace` — the **merged** namespace including all referenced
  assemblies (BCL included) — a large scan per edit, and it is enumerated **twice**
  (lines ~2458 and ~2519).
- `GetAllTypes` never recurses into nested types → controllers declared as nested classes are missed.

**Fix:** use `ForAttributeWithMetadataName` for `HypermediaActionEndpointAttribute` (generic attributes
are supported via the `` `1 `` metadata name); restrict the legacy `HttpMethodHypermediaAction` scan to
`compilation.Assembly.GlobalNamespace` (source assembly only); make the provider output an
`EquatableArray` of records; recurse into nested types. See REF-06 for the regression guard.

**Done:** modern `[HypermediaActionEndpoint<THto>]` attributes are now matched per method via
`ForAttributeWithMetadataName`; the legacy `HttpMethodHypermediaAction` scan is restricted to the
source assembly and recurses into nested types; both feed equatable records
(`ActionResultData` / `ActionResultMapping` in `ActionResultData.cs`) merged deterministically in
`ActionResultMappingExtractor.Merge` (legacy wins on duplicate keys, preserving the old
single-dictionary behavior; mappings sorted for stable registry output).
`IncrementalCacheabilityTests` now asserts the output nodes stay cached, plus a behavioral test for
a `ResultType` endpoint on a nested controller. Note: legacy-scan restriction means `ResultType` on
legacy attributes in *referenced* assemblies is no longer picked up — intentional, per this finding.

### ✅ GEN-05 — Same HTO class name in two namespaces crashes the generator

Hint names are `$"{metadata.ClassName}Schema.g.cs"` (line ~284, same pattern for Properties and
SirenExtensions). Two HTOs with the same class name in different namespaces produce a duplicate
`AddSource` hint name → `ArgumentException`, generation fails for the whole assembly.

Related: the result-mapping dictionary keys on bare `htoType.Name`, so same-named HTOs in different
namespaces can receive each other's `ResultType`.

**Fix:** include the namespace (dot-sanitized) or a stable hash in hint names; key result mappings by
fully qualified name.

**Done:** Hint names for the per-HTO artifacts are namespace-qualified via a new
`HtoMetadata.FullClassName` (e.g. `MyApp.HypermediaFooHtoSchema.g.cs`). Result mappings are keyed
by the namespace-qualified HTO class name on both sides: `ActionResultMapping.HtoClassName` and
`ActionMetadata.DeclaringClassName` now carry the qualified name
(`HtoMetadataExtractor.GetNamespaceQualifiedName` is the shared key format), and the enrichment
lookup passes `metadata.FullClassName`. Tests cover generation succeeding with two same-named HTOs
and `ResultType` not leaking across namespaces. Note: derived schema names can still collide
("Customer" from both) — that is GEN-06, unchanged here.

### GEN-06 — Silent schema-name collisions; `[HypermediaSchemaName]` missing

`DeriveSchemaName` strips the `Hypermedia` prefix and `Hto` suffix — `HypermediaCustomerHto`,
`CustomerHto`, and `Customer` all map to `"Customer"` with no diagnostic. Cross-references
(`LinkDescription.TargetName`, `ResultName`) then silently point at the wrong entity. Multi-assembly
setups make collisions more likely.

The design doc specifies `[HypermediaSchemaName("Name")]` as the override; **it does not exist anywhere
in the codebase** (no attribute type, no generator support).

**Fix:** implement `[HypermediaSchemaName]`; emit a diagnostic on duplicate derived schema names within
an assembly (cross-assembly collisions can only be caught at compose time — log there).
Alternatively update the design doc if the attribute is deliberately dropped.

### ✅ GEN-07 — Generated code can fail to compile

Several emission paths produce invalid C# without any diagnostic:

- **Control characters:** `EscapeString` (line ~2743) only handles `\` and `"`. A multi-line
  `[Obsolete]` message or a title containing a literal newline breaks the emitted string literal.
  Escape `\n`, `\r`, `\t`, `\0` (or use `SymbolDisplay.FormatLiteral`).
- **Culture-sensitive formatting:** `FormatTypedConstant` falls through to `constant.Value.ToString()`
  (line ~2654). On a de-DE build machine a forwarded `[SomeAttr(1.5)]` emits `1,5`. Chars are emitted
  unquoted, longs without the `L` suffix. Use invariant culture / `SymbolDisplay.FormatPrimitive`.
- **Invalid identifiers:** `[HypermediaProperty(Name = "full-name")]` is applied *structurally* as the
  POCO property name → `public string full-name` does not compile. C# keyword names (`class`) are not
  `@`-escaped. Validate with `SyntaxFacts.IsValidIdentifier`, `@`-escape keywords, emit a diagnostic
  for invalid names.
- **Type-name clash:** if the user already has a `{ClassName}Properties` (or `...Siren`,
  `...SirenEmbedded`, `SirenHelper`) type in that namespace, generation collides with no diagnostic.

**Done:** All four paths fixed:

- `EmitHelpers.EscapeString` escapes `\n`, `\r`, `\t`, `\0` and all other control characters
  (`\uXXXX`); round-trip tested with a newline/tab title.
- `FormatTypedConstant` formats with `CultureInfo.InvariantCulture` and emits type suffixes
  (`1.5d`, `2.5f`, `5L`, `UL`, `U`), quoted/escaped char literals, `float.NaN`-style specials,
  and parenthesized enum casts (`(E)(-1)` — `(E)-1` would parse as subtraction).
- New `EmitHelpers.EscapeIdentifier` `@`-escapes keyword names at every identifier emission site
  (POCO property declarations and all `hto.X` member accesses in the Siren mapper — a C# property
  declared `@class` also needs this). Non-identifier `[HypermediaProperty(Name)]` overrides are
  rejected at extraction (`SyntaxFacts.IsValidIdentifier`), reported as **RY0022** (warning), and
  fall back to the C# property name.
- Type-name collisions are detected: `{ClassName}Properties`/`{ClassName}SirenExtensions` against
  source types in the HTO's namespace (during extraction), the global-namespace `SirenHelper` via a
  dedicated compilation provider. Each reports **RY0023** (error) naming the colliding type, and the
  colliding artifact is skipped so the user sees the real cause instead of CS0101 on generated code
  (a Properties collision also skips the Siren mapper, which would otherwise bind the user's type).

### GEN-08 — `record` HTOs silently ignored

The syntax predicate is `node is ClassDeclarationSyntax` (line ~183); `RecordDeclarationSyntax` does not
match, so a `record` HTO with `[HypermediaObject]` produces no schema and no Siren mapper, silently.
Either support records (include `RecordDeclarationSyntax`) or emit a diagnostic saying records are
unsupported. Same question applies to the runtime `SirenConverter` — behavior should match.

### GEN-09 — Embedded-collection detection too loose and too tight

`GetCollectionEmbeddedEntityTarget` (line ~748) accepts **any** generic type whose first type argument is
an embedded entity — `Func<IEmbeddedEntity<T>>` or `Dictionary<IEmbeddedEntity<T>, X>` falsely count as
collections. Meanwhile `IEmbeddedEntity<T>[]` (an `IArrayTypeSymbol`) is not detected at all, so an array
of embedded entities **leaks into the properties POCO as a data property**.

**Fix:** require the type to implement `IEnumerable<IEmbeddedEntity<T>>`; handle `IArrayTypeSymbol`.
The doc comment already claims the stricter behavior — make the code match it.

### GEN-10 — Inconsistent null handling for mandatory members

A null non-nullable link or embedded entity throws `InvalidOperationException` at runtime; a null
non-nullable **action** is silently omitted. The if/else in `EmitActionResolution` had two identical
branches — that dead duplication was removed during REF-01/REF-02 (now a single branch in
`SirenEmitter.EmitActionResolution`), but the policy decision below is still open.

**Fix:** pick one policy (probably: mandatory action null → throw, matching links/embedded), simplify the
emitter, and document the behavior in the migration guide. Also document that in `#nullable disable`
contexts everything counts as mandatory (`NullableAnnotation != Annotated`).

### GEN-11 — No zero/multiple-endpoint diagnostics

Design doc constraint: "If the generator finds zero or multiple endpoints for the same HTO/action, it
should emit a diagnostic error." Not implemented — multiple `[HypermediaActionEndpoint]` attributes for
the same action last-win silently in the mapping dictionary; missing endpoints are not detected at all.

### ✅ GEN-12 — Diagnostic severity vs. wording mismatch

RY0020/RY0021 messages say "it **will be ignored** in the schema" and doc comments call them warnings,
but both are declared `DiagnosticSeverity.Error`. RY0030 is `Error` while plan Step 2.9 says "diagnostic
warning" — and *error + force Schema=true* is contradictory (the build fails, so the forcing never
matters). Decide per diagnostic: error with error wording, or warning as planned.

**Done:** Policy documented in `GeneratorDiagnostics`: diagnostics whose message says the generator
recovered ("will be ignored", "has been forced to true") are **warnings**; errors are reserved for
cases where generation or the subsequent compilation cannot proceed correctly (RY0023).
RY0020, RY0021, and RY0030 changed from Error to Warning — matching their wording and plan Step 2.9.

### ✅ GEN-13 — All diagnostics use `Location.None`

No squiggles, no click-to-navigate in the IDE. Capture the property/attribute location into the metadata
records — as file path + `TextSpan` (both equatable) rather than `Location` itself, to keep incremental
caching correct.

**Done:** New equatable `LocationInfo` record (file path + `TextSpan` + `LinePositionSpan`, converted
back via `Location.Create` at report time) threaded through all diagnostic paths:

- RY0020/RY0021: missing-relations lists changed from property names to `PropertyRef` (name + location).
- RY0022: location of the `[HypermediaProperty]` attribute (fallback: the property).
- RY0023: location of the *colliding user type* (the message says "rename the existing type") — the
  per-HTO collision flags became `LocationInfo?` on `HtoMetadata`; the `SirenHelper` provider now
  yields the colliding type's location instead of a bool.
- RY0030: the `[assembly: HypermediaAssembly]` attribute (captured in `AssemblyConfig`).
- RY0031/RY0032: the endpoint attribute application (both modern and legacy extraction).
- RY0040/RY0041: the duplicate link/embedded property (`Location` added to `LinkMetadata` /
  `EmbeddedEntityMetadata`).

Incremental caching stays intact — all captured values are equatable; outputs re-run only when the
declaring file actually changes (which shifts spans). Regression-tested: the diagnostic's `SourceSpan`
must cover the offending property identifier.

### GEN-14 — Startup validation of dangling `TargetName` references missing

Design doc ("Runtime Opt-In" section): the aggregated schema should validate that all `TargetName` /
`ResultName` references resolve to an existing `EntityTypeSchema.Name`, log warnings for dangling refs,
with an `AllowUnresolvedReferences` option producing placeholder entries. `ComposeSchema()` implements
none of this. Becomes more important once GEN-06 collisions are possible.

### ✅ GEN-15 — Minor issues (collect opportunistically)

- `Has201ResponseAttribute` only checks constructor args — misses `[ProducesResponseType(StatusCode = 201)]`
  named-argument form.
- Duplicate-relations detection (RY0040) exists for links but not for embedded entities
  (intentionally allowed at runtime per Step 6.4, but a compile-time hint may still help).
- `SanitizeAssemblyName` maps `My.App` and `My_App` to the same registry class name — theoretical
  global-namespace clash between two assemblies.
- Orphaned doc comment: `SanitizeAssemblyName`'s `<summary>` (line ~2440) is immediately followed by a
  second `<summary>` belonging to `ExtractActionResultMappings`.
- Verify `[HypermediaObject]` positional constructor arguments (if any exist) — only named arguments
  `Title`/`Classes` are read.
- XML doc `<inheritdoc/>` on HTO properties is copied verbatim to the POCO where it resolves to nothing.

**Done:** All six items resolved:

- `Has201ResponseAttribute` now also matches the `StatusCode = 201` named-argument form
  (enum values like `HttpStatusCode.Created` already matched — their boxed value is the underlying int).
- New **RY0041** (Info) hints at identical `[Relations]` on two embedded entity properties — Info, not
  Warning, because duplicates are valid Siren and intentionally allowed at runtime (unlike RY0040 links).
- `SanitizeAssemblyName` collisions assessed — no code change: the registry types live in *different
  assemblies* and are discovered via assembly attributes, never referenced by name across assemblies,
  so equal sanitized type names cannot clash. Documented on the method.
- Orphaned doc comment: already fixed by the REF-01 split (`SanitizeAssemblyName` carries its own
  `<summary>` in `HtoSchemaGenerator`; `ExtractActionResultMappings` no longer exists).
- `[HypermediaObject]` verified: the attribute has no constructor parameters at all (`Title`/`Classes`
  are settable properties only), so reading named arguments is complete.
- `<inheritdoc/>` is no longer copied verbatim to the POCO: it is resolved against the overridden
  property's doc where possible, otherwise dropped.

### ✅ GEN-16 — `required` never emitted in properties/parameter schemas

Reported externally by a client-generator design review (2026-07-10).
`JsonSchemaFactory` (RESTyard.Schema, not the source generator) does not handle `[Required]` — the
commented-out `DataAnnotationsSupport.AddDataAnnotations()` at the top of the file documents this
deliberately. Verified: zero `"required"` occurrences in the CarShack sample schema. Nullability
(`type: [x, null]`) is the only optionality signal, so schema consumers (client generators, doc
mappers) must treat every field as optional. Fix candidates: enable the JsonSchema.Net
DataAnnotations add-on, or derive `required` from non-nullable properties (matches C# semantics
better, but changes meaning for consumers — decide and document).

**Done (option: derive from non-nullability):** new `RequiredFromNonNullableRefiner`
(`ISchemaRefiner`) adds every non-nullable property to `required`, cross-checked against the
`PropertiesIntent` keys ([`JsonPropertyName`] respected) and merged with an existing `required`
list from the C# `required` keyword. Nullability read via `NullabilityInfoContext` on net6+ and a
manual `NullableAttribute`/`NullableContextAttribute` reader on netstandard2.0; unknown/oblivious
counts as optional. On by default; opt-out via
`new JsonSchemaFactory(deriveRequiredFromNonNullable: false)`. Prerequisite fixed along the way:
the generated properties POCO was dropping nullable reference annotations
(`string?` HTO property emitted as `string`) — the extractor now uses
`SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier`, so nullability (and thus
`required`) reflects the HTO declarations. Documented as a behavior change in the migration guide.

### ✅ GEN-17 — Inherited actions on derived HTOs lose `resultName`/`resultClasses`

Reported externally by a client-generator design review (2026-07-10).
Verified in the CarShack sample: `Car.UpdateInspection` has `resultName: "Car"`, but the inherited
copies on `DerivedCar` and `NextLevelDerivedCar` have none. Cause: `ExtractActionResultMappings`
keys mappings by `(HtoClassName, ActionPropertyName)` taken from the action-endpoint attribute,
which names the *base* HTO. `EnrichActionsWithResultMappings` (~line 2632) looks up with the
*derived* class name → never matches for inherited actions. Fix: resolve the mapping against the
declaring type of the action property (walk base types during lookup), or key by the declaring
HTO. Related to GEN-01/GEN-02 (same enrichment path) — fix together.

**Done:** `ActionMetadata` gained `DeclaringClassName` (`ContainingType.Name` of the action
property — the base HTO for inherited actions), and the mapping lookup tries it after the HTO's
own class name (see GEN-02 for the full key order). Regression test covers a two-level
inheritance chain — base, derived, and next-level derived all get `resultName`/`resultClasses`.
Multi-assembly caveat: see the GEN-01 known limitation.

### ✅ GEN-18 — `ExternalLink` properties silently absent from the schema; `mediaType` never emitted

Reported externally by a client-generator design review (2026-07-10).
`ExternalLink` implements only the non-generic `ILink` (`Link.cs`), but `ExtractLinks` /
`GetLinkTargetType` match only `ILink<THto>` — external/download links are dropped from the schema
without any diagnostic (they also bypass RY0021, which likewise checks `ILink<T>` only).
Additionally `LinkDescription.MediaType` exists in the model but the generator never populates it,
so `mediaType` is dead in practice. Consequence: a schema-driven client cannot know an entity
exposes a download/external link at all. Fix: emit `ExternalLink` properties as links without
`targetName`/`targetClasses` (schema model already allows null), add a way to declare the expected
media type (attribute on the property → `mediaType`), and include them in the missing-`[Relations]`
diagnostic.

**Done:** the classifier now also matches the non-generic `ILink` (implemented by `ExternalLink`),
so external links land in the links bucket: with `[Relations]` they are emitted as
`LinkDescription` with `isExternal: true` and without `targetName`/`targetClasses`
(`LinkDescription.TargetName` is now nullable and omitted from JSON when null),
without `[Relations]` they trigger RY0021. New
`[HypermediaMediaType(params string[])]` attribute (`RESTyard.Schema.Model`) populates
`mediaTypes: string[]` on any link; links without the attribute default to
`["application/vnd.siren+json"]` (constant `SchemaMediaTypes.Siren`).
Doc mappers handle target-less links
(Markdown renders "*external*" with the media types; Mermaid draws the edge to a shared
`_external["External"]` node). Documented in HypermediaApiSchema.md, SourceGenerator.md, and the
migration guide.

Follow-up (media-type dual-source): runtime media types come from the reference builder
(`WithAvailableMediaType(s)`), which the generator cannot see, so declared and runtime values could
drift. Resolved by unifying in the generated Siren mapper: link `type` precedence is runtime →
declared `[HypermediaMediaType]` → omitted (plain navigation links stay type-less on the wire —
Siren is the baseline; only the schema states the default via `mediaTypes`). Optional mismatch
validation via
`SirenMapperOptions.MediaTypeMismatch` (`Warn` default with pluggable
`MediaTypeMismatchWarningHandler`, `Throw` for tests, `Ignore`) flags runtime media types outside
the declared list; links without the attribute are never validated. The legacy `SirenConverter`
is unchanged (runtime media types only); the parity tests compare link `type` leniently and assert
the documented divergence instead.

## Refactorings (structure, testability, debuggability)

Overlaps with plan Step 8.7 (Cleanup) — Phase 6B supersedes/concretizes that step for the generator.

### ✅ REF-01 — Split the god class

`HtoSchemaGenerator` mixes three separable concerns in 2745 lines. All members are already `static`,
so the split is mostly mechanical:

| Concern                    | Extract to                                                           | Win |
|----------------------------|----------------------------------------------------------------------|-----|
| Symbol → metadata analysis | `HtoMetadataExtractor`, `ActionResultMappingExtractor`               | Test extraction against a `Compilation` without asserting on generated text |
| Code emission              | `SchemaEmitter`, `PropertiesPocoEmitter`, `SirenEmitter`, `RegistryEmitter` | Pure `HtoMetadata → string` functions; unit-test with hand-built metadata, no Roslyn |
| Diagnostics                | `Diagnostics` descriptor class + dedicated reporting step            | One place to check ID/severity consistency (GEN-12) |
| Pipeline wiring            | Slim `HtoSchemaGenerator.Initialize`                                 | Incremental graph readable at a glance |

Debugging payoff: any bad output can be reproduced by feeding the captured `HtoMetadata` straight into
the emitter in a test.

### ✅ REF-02 — `CodeWriter` instead of indentation strings

Hundreds of `sb.Append("                    ")` calls make the emitters hard to read and fragile.
`System.CodeDom.Compiler.IndentedTextWriter` is available on netstandard2.0; a thin `CodeWriter` with
`using (w.Block())` scopes shrinks the emitters substantially and makes the generated shape visible in
the generator source.

### ✅ REF-03 — `GenerateSirenHelper` as a constant

`GenerateSirenHelper` (lines ~2201–2384) emits ~180 lines of **completely static** text via `AppendLine`
calls — zero interpolation. Make it a single verbatim string constant (or embedded resource). Same for
most of `EmitOkSirenExtension`. Cheapest readability win in the file.

### ✅ REF-04 — One property walk + classification pass

`ExtractProperties`, `ExtractLinks`, `ExtractActions`, `ExtractEmbeddedEntities`,
`FindEmbeddedEntityPropertiesWithoutRelations`, and `FindLinkPropertiesWithoutRelations` each repeat the
same base-type walk with the same accessibility/static/indexer/`seen` filtering — six copies.

Replace with one `EnumerateInstanceProperties()` iterator plus a single classification pass that buckets
each property once (Data / Link / Action / Embedded / MissingRelations). One walk instead of six, and it
forces explicit precedence when a property matches two categories (e.g. an `ILink<T>` property that also
carries `[HypermediaAction]` — currently the outcome depends on which extractor claims it).

**Done:** `HtoMetadataExtractor.ClassifyProperties` walks once via `EnumerateInstanceProperties()` and
buckets each property via `Classify()` with explicit precedence link > embedded entity > action > data.

### ✅ REF-05 — Deduplicate assembly-config normalization

The `siren && !schema → schema = true` rule appears in both `RegisterSourceOutput` blocks (lines ~228 and
~321) and only one reports RY0030. Extract an `AssemblyConfig` record with a `Normalize()` returning the
effective config plus whether to warn.

**Done:** `AssemblyConfig` record struct with `FromCompilation()` and `Normalize()`; RY0030 has
since moved to the compilation-level diagnostics output (GEN-03) and is reported once.

### ✅ REF-06 — Tracking names + cacheability tests

Add `.WithTrackingName()` to pipeline stages and write tests asserting
`IncrementalStepRunReason.Cached` on an unchanged re-run via `GeneratorDriver`. Given GEN-04 there is
currently no regression guard for incrementality; this is the standard way to get one. Do together with
or immediately after GEN-04.

**Done:** all pipeline stages carry tracking names (`TrackingNames`), and
`IncrementalCacheabilityTests` asserts Cached/Unchanged for the HTO-metadata, assembly-config,
assembly-name, and action-result-mapping stages on a re-run with an unrelated source change.
With GEN-04 fixed, the tests also assert via `TrackedOutputSteps` that no `RegisterSourceOutput`
block re-runs — the end-to-end incrementality guarantee.
