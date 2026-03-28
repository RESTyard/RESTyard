# Hypermedia Schema — Implementation Plan

Companion to [HypermediaSchema-Design.md](HypermediaSchema-Design.md) (the spec).

## Working Guidelines

- **Keep the spec updated.** As implementation progresses, update `HypermediaSchema-Design.md` with any design decisions, edge cases, behavior clarifications, or spec changes discovered during implementation. The spec is the single source of truth and will later be used to create user documentation — treat it as living memory.
- **Small steps.** Each step below is scoped to be implementable and testable independently.
- **Test as you go.** Every step includes verification. Don't batch testing to the end.
- **No magic strings in the generator.** The source generator cannot reference `RESTyard.Schema` (dependency loading issues in the compiler host). All emitted type names, property names, and namespace strings must go through `SchemaTypeNames` constants in the generator project. When a new schema type or property is emitted, add the corresponding constant first.

## Testing Strategy

### Source Generator Tests

Use Verify (snapshot testing), consistent with the existing `RESTyard.AspNetCore.Analyzers.Tests` and `RESTyard.Generator.Test` projects. For each generator feature:

- Define a minimal HTO input as a source string
- Run the generator via `CSharpGeneratorDriver`
- Snapshot-verify the generated `ToSiren()` and `GetSchema()` output

A `GeneratorTestHelper` compiles HTO/controller source, runs the generator, and returns a deserialized `HypermediaApiSchema` for direct assertions:

```csharp
var schema = GeneratorTestHelper.GetSchema(typeof(HypermediaCustomerHto), typeof(CustomerController));

schema.EntityTypes.Should().HaveCount(1);
var customer = schema.EntityTypes[0];
customer.Name.Should().Be("Customer");
customer.Actions.Should().ContainSingle(a => a.Name == "MarkAsFavorite");
customer.Links.Should().ContainSingle(l => l.Relations.Contains("bestFriend"));
```

Snapshot tests catch unexpected changes in the generated code; assertion-based tests verify specific schema behaviors and are easier to write for targeted cases.

Cover at minimum:
- Simple HTO with properties only
- HTO with links (mandatory, optional)
- HTO with actions (parameterless, with parameters, file upload, with CanExecute)
- HTO with embedded entities (single, collection)
- Nested/complex property types
- `[Title]`/`[Description]` attribute harvesting
- XML doc comment harvesting (title, description)
- Attribute-based title/description overrides
- Nullable properties and links
- Controller scanning for action endpoint matching
- Per-assembly registry generation
- Multi-assembly scenario
- Generated properties POCO: correct property names, `[HypermediaProperty]` name applied structurally
- Generated properties POCO: `[FormatterIgnoreHypermediaProperty]` properties omitted
- Generated properties POCO: non-RESTyard attributes forwarded verbatim (e.g., `[JsonConverter]`, `[JsonPropertyName]`, custom attributes)
- Generated properties POCO: RESTyard-specific attributes NOT forwarded (`[Key]`, `[Relations]`, etc.)
- `SirenEntity<TProperties>` return type from `ToSiren()`

### Schema Model Tests

- Round-trip serialization: `HypermediaApiSchema` → JSON → deserialize → assert equality
- Schema endpoint integration test: start CarShack via `WebApplicationFactory`, call `/hypermedia-schema`, verify the returned JSON matches expected structure
- Validate that the schema JSON is stable (snapshot test) — breaking changes in the schema format should be caught
- Mermaid mapper output: snapshot tests for both diagram types against known schemas
- Mermaid mapper options: tests for `IncludeProperties`/`IncludeActions` toggle behavior
- Markdown mapper output: snapshot tests for full documentation against known schemas
- Markdown mapper options: tests for `IncludeTableOfContents`/`IncludeDiagram` toggle behavior
- Markdown mapper BFS ordering: tests with cyclic entity graphs to verify cycle safety

### Parity Tests

During migration, compare the JSON output of the existing `SirenConverter` against `ToSiren()` for every HTO in CarShack. These tests ensure the new generated mappers produce identical Siren output to the reflection-based formatter.

## Phases

### Phase 1: ✅  Foundation — Schema Model Library

**Goal:** Ship the schema model as a standalone NuGet package that external tools can consume.

#### Step 1.1: ✅ Create `RESTyard.Schema` project and test project
- New `netstandard2.0` class library project
- New `RESTyard.Schema.Test` xunit test project
- Add both to `RESTyard.sln`
- Set up `Directory.Build.props` integration, NuGet metadata

#### Step 1.2: ✅ Implement schema model classes
- `HypermediaApiSchema`, `EntityTypeSchema`, `LinkDescription`, `ActionDescription`, `EmbeddedEntityDescription`
- Include JSON serialization attributes (`System.Text.Json`)
- Unit tests: round-trip serialization (model → JSON → deserialize → assert equality)

#### Step 1.3: ✅  Implement Mermaid mapper
- `schema.ToApiMap()` and `schema.ToClassDiagram()`
- Unit tests with snapshot verification against hand-crafted schema inputs

#### Step 1.4: ✅ Add `MermaidMapperOptions` to class diagram
- `MermaidMapperOptions` with `IncludeProperties` (default true) and `IncludeActions` (default true)
- Update `ToClassDiagram()` signature to accept optional `MermaidMapperOptions`
- When `IncludeProperties = false`, omit property lines from class boxes
- When `IncludeActions = false`, omit action lines from class boxes
- Update existing tests, add tests for options combinations

#### Step 1.5: ✅ Implement Markdown documentation mapper
- `schema.ToDocumentation(MarkdownMapperOptions?)` in `RESTyard.Schema`
- `MarkdownMapperOptions` with `IncludeTableOfContents` (default true) and `IncludeDiagram` (default true)
- **Header section**: API title, description, version, external docs URL
- **Table of Contents**: anchor links to each entity section (opt-out via options)
- **API Map**: embedded Mermaid entity graph via `schema.ToApiMap()` (opt-out via options)
- **Entity sections** ordered by BFS from entry point (cycle-safe via visited set):
  - Title/description, Siren classes
  - Properties table (from `PropertiesSchema` JSON Schema — same `type`-only parsing as Mermaid)
  - Links table with cross-links to target entity sections, `*(optional)*` suffix on non-mandatory
  - Actions table with parameter sub-tables, `Returns: [Target](#anchor)` for actions with `ResultName`
  - Embedded entities table with cross-links, collection indicator
  - `**[Deprecated]**` badge + message on deprecated elements
  - Self links included
- Snapshot tests against hand-crafted schema inputs
- Tests for options (TOC on/off, diagram on/off)
- Tests for cycle handling in BFS ordering

### Phase 2: Source Generator — Project Setup and Schema Generation

**Goal:** Set up the generator project and generate `GetSchema()` methods that produce `EntityTypeSchema` per HTO.

#### Step 2.1: ✅ Create `RESTyard.HtoSourceGenerators` project and test project
- New `netstandard2.0` class library with `<IsRoslynComponent>true</IsRoslynComponent>`
- New `RESTyard.HtoSourceGenerators.Test` xunit + Verify test project
- Implement `IIncrementalGenerator` skeleton
- Set up `CSharpGeneratorDriver`-based test infrastructure:
  - `GeneratorTestHelper` class that compiles HTO source strings via `CSharpCompilation`, runs the generator via `CSharpGeneratorDriver`, and returns the `GeneratorDriverRunResult` for snapshot verification and assertions
  - Include `MetadataReference`s to corlib, `RESTyard.AspNetCore` (for HTO attributes/base types), and other required assemblies
- Create `TestHtoSources` static class with reusable HTO/controller source strings (similar pattern to `TestSchemaFactory` in `RESTyard.Schema.Test`):
  - `SimpleHto` — minimal HTO with properties only
  - `HtoWithLinks` — mandatory and optional links
  - `HtoWithActions` — parameterless, with parameters, file upload
  - `HtoWithEmbedded` — single and collection embedded entities
  - `FullHto` — combines all features (properties, links, actions, embedded)
  - Additional sources added as later steps require them (attributes, XML docs, deprecation, etc.)
  - Each source is a `const string` that can be reused across snapshot tests and assertion-based tests in Steps 2.2–2.9

#### Step 2.2: ✅ HTO discovery and basic metadata
- Find all `IHypermediaObject` types in compilation
- Extract `[HypermediaObject(Title, Classes)]`
- Emit `GetSchema()` returning `EntityTypeSchema` with `Name`, `Classes`, `Title`
- Verify tests: snapshot + assertion for a minimal HTO

#### Step 2.3: ✅ Property analysis → runtime JSON Schema via `IJsonSchemaFactory`
- **Approach change:** Do NOT duplicate JSON Schema type mapping in the source generator. Instead, emit code that calls `IJsonSchemaFactory.Generate(typeof(T))` at runtime. This reuses the existing `JsonSchemaFactory` (custom temporal generators, attribute handlers, user extensions) and guarantees parity with the action parameter schema endpoint.
- Remove the compile-time `JsonSchemaBuilder` from the source generator project
- Source generator extracts property metadata at compile time: property names (respecting `[HypermediaProperty(Name)]`), property CLR types, exclusion of `[FormatterIgnoreHypermediaProperty]` / `[Relations]` / `[HypermediaAction]` properties
- Generated `GetSchema()` method receives `IJsonSchemaFactory` (via parameter) and calls it at runtime to build `PropertiesSchema`
- Verify tests: HTO with various property types, attribute-based exclusion and renaming, generated code compiles and calls factory

#### Step 2.4: ✅ Link analysis
- Scan `ILink<T>` properties with `[Relations]`
- Populate `LinkDescription` (relations, target name/classes, mandatory from nullability)
- Verify tests: HTO with mandatory/optional links

#### Step 2.5: ✅ Action analysis
- Scan `HypermediaAction` / `HypermediaAction<T>` / `FileUploadHypermediaAction` properties
- Populate `ActionDescription` (name, title, parameter schema, file upload flag, mandatory from nullability)
- Verify tests: parameterless, with parameters, file upload, nullable actions

#### Step 2.6: ✅ Embedded entity analysis
- Scan `IEmbeddedEntity<THto>` properties and `List<IEmbeddedEntity<THto>>` collections with `[Relations]`
- Populate `EmbeddedEntityDescription` (relations, target name/classes, isCollection, isMandatory)
- Verify tests: single embedded, collection, different target, empty, exclusion from properties schema

#### Step 2.7: ✅ Title and description harvesting
- Primary source: `[Title("...")]` and `[Description("...")]` from `JsonSchema.Net.Generation` (already in the dependency tree, used on `IHypermediaActionParameter` types)
- Fallback: XML doc `<summary>` → title, `<remarks>` → description
- Attributes take precedence over XML docs when both are present
- Apply to entity types, properties, actions, links
- Verify tests: HTO with attributes, with XML docs, with both (attribute wins)

#### Step 2.7.1a: ✅ Emit properties POCO class per HTO
- Emit a properties POCO class per HTO (e.g., `HypermediaCustomerHtoProperties`) containing only data properties — same filtering rules as Step 5.1 (exclude `[FormatterIgnoreHypermediaProperty]`, links, actions, keys, embedded entities)
- Apply `[HypermediaProperty(Name = "x")]` structurally: use `x` as the C# property name on the POCO
- Forward all non-RESTyard attributes from the HTO property verbatim (serializer attributes, `[Title]`, `[Description]`, 3rd-party)
- Do NOT forward RESTyard-specific attributes: `[Key]`, `[Relations]`, `[HypermediaAction]`, `[HypermediaProperty]`, `[FormatterIgnoreHypermediaProperty]`
- Copy XML doc comments from HTO properties to the generated POCO properties verbatim (do NOT convert to `[Title]`/`[Description]` attributes — avoids pulling `JsonSchema.Net.Generation` dependency into the source generator)
- Emit the POCO **alongside** existing per-property schema generation — do not change `GetSchema()` yet
- This POCO is intended to be **reused in Phase 5** (Step 5.1) for `ToSiren()` emission — same type serves both schema generation and Siren property mapping
- Verify tests: generated POCO compiles, attribute forwarding correct (3rd-party forwarded, RESTyard-specific excluded), `[HypermediaProperty(Name)]` applied structurally, XML docs copied, `[FormatterIgnoreHypermediaProperty]` properties omitted

#### Step 2.7.1b: ✅ Use properties POCO for schema generation
- Replace the per-property `schemaFactory.Generate(typeof(string))` + `SchemaHelper.BuildPropertiesSchema()` pattern with a single `schemaFactory.Generate(typeof(HypermediaCustomerHtoProperties))` call in generated `GetSchema()` methods
- Remove `SchemaHelper.BuildPropertiesSchema` (no longer needed)
- Update existing tests: generated source assertions for the new pattern, `RunGeneratorAndGetSchema` assertions for property schema parity
- Verify: schema output matches previous output for all existing test cases

#### Step 2.8: ✅ Deprecation support
- Read `[Obsolete("message")]` → `IsDeprecated`, `DeprecationMessage`
- Apply to entity types, actions, links, embedded entities
- Verify tests

#### Step 2.8.1: ✅ `[Obsolete]` → JSON Schema `deprecated` via `IAttributeHandler`
- Add `ObsoleteAttributeHandler : IAttributeHandler<ObsoleteAttribute>` to `JsonSchemaFactory.cs` (same pattern as existing `DisplayNameAttributeHandler` and `DescriptionAttributeHandler`)
- Handler emits `deprecated: true` into the JSON Schema for any type or property annotated with `[Obsolete]`
- Register the handler in `JsonSchemaFactory` constructor via `AttributeHandler.AddHandler()`
- This covers both action parameter properties and entity properties (after Step 2.7.1, `[Obsolete]` on HTO properties is forwarded to the generated POCO and picked up by the handler automatically)
- Verify tests: action parameter type with `[Obsolete]` property, entity HTO with `[Obsolete]` property — both produce `deprecated: true` in the JSON Schema
- **Documentation required:** The built-in attribute handlers (`[Obsolete]` → `deprecated`, `[DisplayName]` → `title`, `[Description]` → `description`) MUST be documented for users — these are non-obvious behaviors that affect the generated JSON Schema

#### Step 2.9: ✅ `[HypermediaAssembly]` attribute, opt-in gating, and assembly discovery
- Define `HypermediaAssemblyAttribute` in `RESTyard.AspNetCore.Hypermedia.Attributes` — `[AttributeUsage(AttributeTargets.Assembly)]` with `bool Schema` (default `true`) and `bool Siren` (default `false`)
- Add `HypermediaAssemblyDiscovery.GetAssemblies()` static helper in `RESTyard.AspNetCore` — scans `AppDomain.CurrentDomain.GetAssemblies()` for `[HypermediaAssembly]`, returns `Assembly[]`. Must have thorough XML doc: purpose, how it discovers assemblies, the loaded-assembly caveat, relationship to `[HypermediaAssembly]`, usage example with `ControllerAndHypermediaAssemblies`
- Update the source generator to check for `[assembly: HypermediaAssembly]` — if absent, emit nothing
- When `Schema = true` (default), emit `GetSchema()`, Properties POCO, schema registry
- When `Schema = false`, emit nothing (safety hatch) — but assembly is still discoverable via `HypermediaAssemblyDiscovery`
- When `Siren = true` with `Schema = false`, emit diagnostic warning and force `Schema = true` (Siren needs Properties POCO)
- Read `Siren` property — store for Phase 5 (ToSiren emission); for now, just check and skip
- Verify tests: source without attribute → no generated output; source with attribute → schema output as before; source with `Schema = false` → no generated output; source with `Siren = true, Schema = false` → diagnostic warning + schema generated
- Add `[assembly: HypermediaAssembly]` to CarShack and verify it still compiles and tests pass
- **Documentation required:** The `[assembly: HypermediaAssembly]` attribute MUST be prominently documented:
  - Purpose: marks assemblies for RESTyard discovery and source generation
  - Without it: generator silently does nothing, assembly not auto-discovered
  - `Schema`/`Siren` properties and their defaults
  - `HypermediaAssemblyDiscovery.GetAssemblies()` as alternative to manual assembly lists
  - The `Schema = false` safety hatch for disabling generation without removing discovery

#### Step 2.9.1: ✅ Schema registry generation
- Emit per-assembly `HypermediaSchemaRegistry_<AssemblyName>` class with static `GetSchemas(IJsonSchemaFactory)` collecting all `GetSchema()` results
- Emit `[assembly: HypermediaSchemaRegistryAttribute(typeof(Registry))]` attribute for discovery
- Define `HypermediaSchemaRegistryAttribute` in `RESTyard.AspNetCore` (so it's available at runtime for scanning)
- The registry method calls each HTO's `GetSchema()` — passing `IJsonSchemaFactory` where needed, parameterless where not
- Verify tests: snapshot the generated registry for a multi-HTO source, verify attribute is emitted

#### Step 2.9.2: ✅ `HypermediaSchemaOptions` and DI integration
- Define `HypermediaSchemaOptions` class in `RESTyard.AspNetCore`: `Title`, `Description`, `ApiVersion`, `EntryPointName`, `ExternalDocsUrl` — all nullable with sensible defaults (assembly name for title, assembly version for ApiVersion, auto-detect entry point from entity with Siren class `"EntryPoint"`)
- Define `AddHypermediaSchema(Action<HypermediaSchemaOptions>?)` as a **separate** extension method on `IServiceCollection`, decoupled from `AddHypermediaExtensions`
- `AddHypermediaSchema()` auto-discovers per-assembly registries by scanning all loaded assemblies (`AppDomain.CurrentDomain.GetAssemblies()`) for `[HypermediaSchemaRegistryAttribute]` — does NOT read from `HypermediaExtensionsOptions`
- Aggregate discovered registries + `HypermediaSchemaOptions` into a singleton `HypermediaApiSchema` available via DI
- Reference `RESTyard.Schema` from `RESTyard.AspNetCore` (already added as project reference)
- Add `HypermediaSchemaBuilder.Build(IServiceProvider, HypermediaSchemaOptions? options = null)` as standalone helper for programmatic use (tests, custom tooling)
- `AddHypermediaSchema()` logs a warning if zero registries are found — catches "forgot the attribute" and "attribute present but `Schema = false`" cases
- Test: `AddHypermediaSchema()` with no registries logs warning

#### Step 2.9.3: ✅ CLI schema generation
- **Core logic in `RESTyard.Schema`:** Add `HypermediaSchemaGenerator` static class with:
  - `Generate(HypermediaApiSchema schema, string outputPath, SchemaOutputFormats formats, SchemaGeneratorOptions? options)` — writes requested output files using `RESTyard.Schema` mappers (JSON via `ToJson()`, `ToApiMap()`, `ToClassDiagram()`, `ToDocumentation()`)
  - `GenerateIfRequested(HypermediaApiSchema schema, string[] args)` → `bool` — parses CLI args, calls `Generate()`, returns `true` if `--generate-schema` was present
  - `SchemaOutputFormats` flags enum: `Json`, `MermaidMap`, `MermaidClass`, `Markdown`, `All`
  - `SchemaGeneratorOptions` for mapper pass-through: `MermaidIncludeProperties`, `MermaidIncludeActions`, `MarkdownIncludeToc`, `MarkdownIncludeDiagram`
- **Convenience extension in `RESTyard.AspNetCore`:** Add `GenerateSchemaIfRequested(this IHost host, string[] args)` → `bool` that resolves `HypermediaApiSchema` from DI and delegates to `HypermediaSchemaGenerator.GenerateIfRequested()`
- CLI args: `--generate-schema` (trigger), `--schema-output <path>` (default: `./generated-schema`), `--schema-artifacts <formats>` (default: all), `--mermaid-include-properties`, `--mermaid-include-actions`, `--markdown-include-toc`, `--markdown-include-diagram`
- Schema format selection: `--schema-artifacts json` produces only `schema.json`, `--schema-artifacts mermaid-map,markdown` produces only those two
- Simple unit tests in `RESTyard.Schema.Test`: test `HypermediaSchemaGenerator` with a hand-built schema — verify arg parsing, file output, format selection, return value. No CarShack — end-to-end testing deferred to Step 2.9.4.
- Usage patterns (for future user documentation):
  ```csharp
  // ASP.NET Core (convenience):
  if (app.GenerateSchemaIfRequested(args)) return;
  app.Run();

  // Tooling (no ASP.NET Core):
  var schema = HypermediaSchemaBuilder.Build(factory, options);
  HypermediaSchemaGenerator.Generate(schema, "./output", SchemaOutputFormats.All);
  ```

#### Step 2.9.4: ✅ CarShack integration and end-to-end verification
- Add `[assembly: HypermediaAssembly]` to CarShack
- Add `AddHypermediaSchema()` to CarShack `Program.cs`
- Add `GenerateSchemaIfRequested()` to CarShack `Program.cs`
- Extend CarShack readme with examle CLI usage (generate all)
- Verify: CarShack compiles with generated registry listing all its entity types
- Verify: resolve `HypermediaApiSchema` from CarShack DI, confirm it contains all entity types with correct metadata from `SchemaOptions`
- Verify: `dotnet run -- --generate-schema --schema-output ./test-output` produces all four output files (JSON, mermaid-map, mermaid-class, markdown), process exits with code 0
- Acceptance: CarShack `Program.cs` has `AddHypermediaSchema()` + one CLI line, schema JSON contains configured title/description

#### Step 2.9.5: ✅ Document the HypermediaApiSchema for users
- Write user-facing documentation for the schema model in Docs/HypermediaSchema/
- **Schema overview**: what the schema describes (type-level metadata, not runtime URLs), how it complements Siren responses
- **Top-level `HypermediaApiSchema`**: explain each field — `SchemaVersion` (format versioning), `ApiVersion` (user's API version), `EntryPointName` (navigation start), `Definitions` (shared JSON Schema types referenced via `$ref`)
- **`EntityTypeSchema`**: `Name` (identifier for cross-references, derived from class name or `[HypermediaSchemaName]`), `Classes` (Siren wire-format matching), `PropertiesSchema` (JSON Schema as `JsonDocument` — type, required, descriptions), relationship to the Siren `properties` bag
- **`LinkDescription`**: `Relations` (Siren rel array), `TargetName`/`TargetClasses` (cross-reference to another entity type), `IsMandatory` (nullability-derived — always present vs. conditional), `MediaType` (default `application/vnd.siren+json`, verify at runtime)
- **`ActionDescription`**: `Name`/`Title` (from `[HypermediaAction]`), `ParameterSchema` (JSON Schema for the action parameter type, null if parameterless), `IsFileUpload`, `IsMandatory`, `ContentType` (inferred), `ResultName`/`ResultClasses` (action returns a resource)
- **`EmbeddedEntityDescription`**: `Relations`, `TargetName`/`TargetClasses`, `IsCollection`, `IsMandatory`
- **Target audience**: client generator authors, documentation tool authors, AI agents consuming the schema — explain what each field is useful for and when it can be null
- **Examples**: annotated JSON snippets showing a real schema (e.g., from CarShack) with callouts explaining each section
- **CLI usage**: how to generate schema artifacts with `--generate-schema`, format selection

#### Step 2.10: ✅ Document the source generator for server developers (Docs/HypermediaSchema/)
- seperate document
- Getting started guide: how the source generator is enabled (bundled in NuGet after 2.10), what it generates (`GetSchema()`, schema registry, assembly attribute)
- Explain `[HypermediaSchemaName]` for custom entity names, when and why to use it (multi-assembly collisions, shorter names for docs/diagrams)
- What `GetSchema()` produces and how it uses `IJsonSchemaFactory` at runtime
- How to verify generation works: check for `*SirenMapper.g.cs` in build output, common troubleshooting (missing assembly reference, generator not running)
- How to use `GenerateSchemaIfRequested` in `Program.cs` — one-line setup, CLI args reference
- How to use `HypermediaSchemaBuilder.Build(IServiceProvider)` for programmatic access

#### Step 2.11: ✅ Bundle source generator into `RESTyard.AspNetCore` NuGet
- Add the source generator DLL to the `RESTyard.AspNetCore` NuGet package alongside the existing analyzers:
  ```xml
  <None Include="..\RESTyard.HtoSourceGenerators\bin\$(Configuration)\netstandard2.0\RESTyard.HtoSourceGenerators.dll"
        Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
  ```
- This enables consumers who reference only `RESTyard.AspNetCore` to get both the analyzers and the source generator automatically
- clean up carshack project reference to HtoSourceGenerators and update docs "SourceGenerator.md"
- Defer to after the source generator is feature-complete (link analysis, action analysis, etc.)

#### Step 2.12: ✅ Resolve complex property types in mappers
- **Problem**: `JsonSchema.Net` inlines nested objects as `"type": "object"` with properties expanded inline — no `$ref`, no type name. The mappers see `"type": "object"` and display `object`. This affects both:
  - **Entity properties**: e.g., `Customer.Address` shows as `object` instead of `Address`
  - **Action parameters**: e.g., `CustomerMove.Address` shows as `object` instead of `NewAddress`
- **Markdown documentation mapper**: show the type name in property/parameter tables, link to a Definitions section listing each complex type with its own property table. Cross-links both ways.
- **Mermaid class diagram (`mermaid-htos`)**: replace `object` with the type name (e.g., `Address`). Do not expand inline — name is sufficient for diagrams.
- **Mermaid API map (`mermaid-api-map`)**: no change needed (entity-level graph, doesn't show property types).
- **Approach — spike needed**: `$ref` resolution infrastructure already exists in `JsonSchemaExtensions` (`GetRef()`, `SchemaToTypeString()`, `SchemaToLinkedTypeString()`) — if `$ref` is present, the mappers already resolve it correctly. The root cause is that `JsonSchema.Net` inlines nested objects instead of extracting to `$defs`.
  - **(a) Root cause fix (preferred)**: Make `JsonSchemaFactory` (or an `ISchemaRefiner`) extract inline complex objects to `$defs` with `$ref`. Investigate whether `JsonSchema.Net.Generation` has built-in support for this, or if a post-processing refiner is needed. This solves the problem for both entity properties and action parameters with zero mapper changes — the existing `$ref` resolution handles the rest.
  - (b) Fallback: Extend schema model with property type name metadata — source generator emits CLR type names alongside JSON Schema. Adds fields to `EntityTypeSchema` and `ActionDescription`.
  - (c) Fallback: Custom `x-type-name` JSON Schema extension keyword — ugly to implement and retrieve in `JsonSchema.Net`'s strongly-typed model.
  - **Lead for (a)**: An `ISchemaRefiner` that adds `IdIntent(context.Type.FullName)` for complex types forces `JsonSchema.Net` to extract them to `$defs` with `$ref`. Use the fully qualified CLR type name (e.g., `MyApp.Models.Address`) to avoid name collisions between same-named types in different namespaces. No source generator changes needed — just register the refiner in `JsonSchemaFactory`. Watch out for: root type ending up in `$defs`, and primitives/collections being incorrectly extracted. Try this first.
  - **Self-contained schemas + Definitions as catalog (Option A)**: Each `PropertiesSchema` / `ParameterSchema` remains a valid, self-contained JSON Schema with its own local `$defs`. `HypermediaApiSchema.Definitions` is populated as a deduplicated catalog of shared types — tools use it to detect shared types and generate one class per definition. Schemas are duplicated in JSON but each is independently valid and processable. Cross-document `$ref` (Option B) was rejected because it breaks JSON Schema validity and forces custom resolution on every consumer. **User documentation must clearly explain** why definitions appear both locally in each schema and in the top-level `Definitions` — "schemas are self-contained for validation, Definitions is a deduplicated index for tooling."
  - **Deduplication to `HypermediaApiSchema.Definitions`**: After all per-entity schemas are collected (in `HypermediaSchemaBuilder.ComposeSchema()`), scan all `PropertiesSchema` and `ParameterSchema` `$defs` entries. Extract unique definitions to the top-level `Definitions` dictionary. Handle duplicates: same name + same content → deduplicate; same name + different content → disambiguate using the full type name. The local `$defs` remain intact (schemas stay self-contained). **Edge case error messages**: log clear warnings for name collisions (e.g., "Definition 'Address' found with different schemas in entities 'Customer' and 'Order' — disambiguated as 'MyApp.Models.Address' and 'MyApp.Billing.Address'"), missing `$defs` references, and any other deduplication anomalies. These should be actionable — tell the user what happened and what to do.
  - **Evaluate complexity vs. value**: Adding `$defs` extraction may make the individual JSON Schemas harder to read (lots of `$ref` indirection). Weigh whether the improved mapper output and tooling support justifies the added schema complexity. May not be worth it for simple APIs — consider making it opt-in.
  - **Fallback for (a)**: If the `ISchemaRefiner`/`IdIntent` approach is fragile (undocumented `JsonSchema.Net` behavior, breaks on library updates) or too magical (silently changes schema structure), fall back to a `JsonDocument` post-processor that we fully control: after `schemaFactory.Generate()` returns, parse the `JsonDocument`, find inline `"type": "object"` schemas, extract them to `$defs`, and rewrite properties to `$ref`. More code but zero dependency on `JsonSchema.Net` internals.
  - ✅ **Implemented approach (a)**: `ComplexTypeDefinitionRefiner` (`ISchemaRefiner`) with `IdIntent(urn:restyard:type:{FullName})` forces complex types to `$defs`. Self-contained schemas (Option A) + `Definitions` catalog populated via deduplication in `ComposeSchema()`. Mappers automatically resolve `$ref` — zero mapper changes needed. Thread-safe attribute handler registration added. Helper functions and `ToMergedJsonSchema()` deferred to when tooling needs them.
- **Helper functions for tooling**: Provide utilities to make consuming the schema easier for tool authors:
  - `HypermediaApiSchema.GetDefinition(string name)` — look up a shared type by name from the `Definitions` catalog
  - `EntityTypeSchema.GetReferencedDefinitions()` — list all definition names referenced via `$ref` in the entity's `PropertiesSchema`
  - `ActionDescription.GetReferencedDefinitions()` — same for `ParameterSchema`
  - `SchemaHelpers.ResolveProperty(JsonDocument schema, string propertyName)` — given a properties schema and a property name, return the fully resolved property schema (follow `$ref` to local `$defs` if present, otherwise return as-is)
  - `HypermediaApiSchema.ToMergedJsonSchema()` — produce a single merged JSON Schema document containing all entity types and all shared definitions in one `$defs`. Off-the-shelf JSON Schema code generators (NJsonSchema, etc.) can consume this directly and naturally deduplicate shared types without needing the two-pass approach. Less intrusive for tool authors who don't want to implement custom schema traversal.
- Tests: snapshot tests for Markdown and Mermaid output with schemas containing nested object properties.

#### Step 2.13: ✅ Populate `ActionDescription.ResultName` via `ResultType` on `HypermediaActionEndpoint`
- Extend `HypermediaActionEndpointAttribute` with an optional `Type? ResultType` property. XML doc must explain: "Indicates that this action endpoint produces a Location header pointing to an entity of the specified HTO type. Used by schema generation to populate `ActionDescription.ResultName`/`ResultClasses`."
- The source generator already scans controllers for `[HypermediaActionEndpoint]` — additionally read `ResultType` when present
- When `ResultType` is set: resolve the target HTO's schema name and Siren classes, populate `ActionDescription.ResultName` and `ResultClasses`
- When `ResultType` is null (default): no change — `ResultName`/`ResultClasses` remain null
- Emit `RY0031` diagnostic warning when a 201-related attribute is found on a controller method that has `[HypermediaActionEndpoint]` without `ResultType` set — hint that the user may want to declare the result type. Detect by attribute name string matching (no dependency on Swagger/ASP.NET Core packages):
  - `[ProducesResponseType(201)]` / `[ProducesResponseType(typeof(...), 201)]` — ASP.NET Core
  - `[SwaggerResponse(201)]` — Swashbuckle
  - `[SwaggerResponseHeader(201, ...)]` — Swashbuckle
  - Check constructor arguments for integer value `201` (the `TypedConstant` gives the resolved value regardless of whether the user wrote `201` or `StatusCodes.Status201Created`)
  - Standard diagnostic suppression mechanisms apply — users can silence via `#pragma warning disable RY0031`, `.editorconfig` (`dotnet_diagnostic.RY0031.severity = none`), or `<NoWarn>RY0031</NoWarn>` in `.csproj`. No custom silencing needed.
- This enables: "Returns: [CustomerQueryResult](#customerqueryresult)" in Markdown documentation, incoming "Referenced by" links on result entities, and the API map showing action-result edges
- **Documentation required**: Explain `ResultType` in user docs (SourceGenerator.md) — what it does, when to use it, example usage with `CreateQuery`
- **Multi-assembly support:** When HTOs and controllers are in different assemblies, the source generator processing the HTO assembly won't see `ResultType` (which lives on controller attributes in the other assembly). Solution: the generator in the controller assembly emits a separate `HypermediaActionResultRegistry_<Assembly>` containing `ActionResultMapping(EntityName, ActionName, ResultName, ResultClasses)` entries. `HypermediaSchemaBuilder.ComposeSchema()` merges these mappings into the existing `ActionDescription` entries after collecting all schema registries — same post-processing pattern as `Definitions` deduplication. The schema model (`EntityTypeSchema`, `ActionDescription`) is not changed — `ResultName`/`ResultClasses` are populated at compose time, not at generation time. `ActionResultMapping` is a simple record in `RESTyard.Schema.Model`.
- Verify tests: action with `ResultType` populates `ResultName`, Markdown shows "Returns" link, result entity shows incoming "Referenced by", multi-assembly scenario merges correctly

#### Step 2.14: ✅ Legacy attribute support for `ResultType`
- Added `ResultType` property to legacy `HttpMethodHypermediaAction` base class (same signature as on `HypermediaActionEndpointAttribute<T>`)
- Source generator scans legacy attributes via `InheritsFrom` check — extracts `ResultType` from `[Http*HypermediaAction(..., ResultType = typeof(...))]` in addition to `[HypermediaActionEndpoint<T>(..., ResultType = typeof(...))]`
- Added `Legacy_HttpMethodHypermediaAction_type_exists` guard test — fails when the legacy type is removed, reminding to clean up the generator's legacy scan code
- Legacy cleanup comment added to generator constant `HttpMethodHypermediaActionBaseFullName`
- This enables existing projects using the contract-first generator (which emits legacy attributes) to manually add `ResultType` on their controller endpoints

#### Step 2.15: ✅ Add `ResultType` to hand-written CarShack controllers
- Added `ResultType = typeof(...)` to 5 hand-written controller action endpoints: UploadCarImage → CarImageHto, UploadInsuranceScan → CarInsuranceHto, UpdateInspection → HypermediaCarHto, BuyCar → HypermediaCarHto, CreateQuery → HypermediaCustomerQueryResultHto, CreateCustomer → HypermediaCustomerHto
- Verified: schema JSON has `resultName` on 5 actions, Markdown docs show "Returns" links, all end-to-end
- Added `EmitCompilerGeneratedFiles` to CarShack csproj for debugging generated source

#### Step 2.15.1: ✅ Action result edges in API map and class diagram
- Currently the Mermaid API map only shows link and embedded entity edges between entity types
- Actions with `ResultName` (producing a Location header to another entity) represent a navigation path that is not visualized
- Explore adding dashed or differently-styled edges for action results (e.g., `CustomersRoot -. "CreateQuery" .-> CustomerQueryResult`)
- Consider: does this add clarity or clutter? For APIs with many actions returning results, the map could get busy
- If useful, implement in `MermaidMapper.ToApiMap()` — add edges for `ActionDescription.ResultName` where non-null
- Evaluate with CarShack output
- In `mermaid-htos` class diagram: actions with `ResultName` should show the return type (e.g., `CreateQuery() → CustomerQueryResult` instead of just `CreateQuery()`)

#### Step 2.13.1: Deferred → Phase 8, Step 8.4

### Phase 3: Schema Endpoint

**Goal:** Serve the schema at runtime via `/hypermedia-schema`. DI integration (singleton `HypermediaApiSchema`) is already done in Step 2.9.2.

#### Step 3.1: ✅ Schema endpoint
- `MapHypermediaSchema(Action<HypermediaSchemaEndpointOptions>? configure = null)` extension method on `IEndpointRouteBuilder` (works with both `WebApplication` and `IApplicationBuilder`)
- `HypermediaSchemaEndpointOptions`: `Route` (default `"/hypermedia-schema"`), extensible for future options (auth policy etc.)
- Returns `HypermediaApiSchema` as JSON (`application/vnd.restyard.hypermedia-schema+json`)
- Usage: `app.MapHypermediaSchema();` or `app.MapHypermediaSchema(o => o.Route = "/api/schema");`
- Integration test: CarShack → `WebApplicationFactory` → `GET /hypermedia-schema` → verify JSON structure and content type
- Add to CarShack `Program.cs`
- Document Schema endpoint usage in user docs

#### Step 3.2: ❌ ~~Refactor `ResultType` to generic type parameter~~ — REJECTED
- **Reason:** `ResultType` can legitimately be a non-HTO type. Without a meaningful generic constraint (`where TResult : IHypermediaObject`), the generic provides no compile-time safety advantage over `typeof()`. The only remaining benefit (shorter syntax) is offset by needing two attribute classes and worse readability (`<THto, TResult>` with two long type names).
- **Decision:** Keep `ResultType = typeof(...)` property syntax. RY0032 warning is sufficient for the HTO case.

### Phase 4 (Optional): Access Groups

> **Optional.** See the "Future Idea: Access Groups" section in `HypermediaSchema-Design.md` for the full design. Placed here (before ToSiren) because it's a schema concern that naturally extends Phases 2–3.

**Goal:** Allow the schema to describe which entity types, actions, links, and embedded entities require which access groups, and let clients request a filtered schema. This enables documentation tools, client generators, and AI agents to understand permission boundaries and request schemas scoped to their access level.

**Scope:** Entity types, actions, links, and embedded entities. **Not** individual properties — too granular, runtime visibility already handles this.

#### Step 4.1: ✅ `[HypermediaAccessGroup]` attribute and generator support
- Define `[HypermediaAccessGroup("group1", "group2", ...)]` attribute in `RESTyard.Schema` — accepts a `params string[]` of access group names
- Applicable to:
  - **HTO classes** — marks the entire entity type as requiring the access group
  - **Action properties** — marks individual actions as restricted
  - **Link properties** — marks individual links as restricted
  - **Embedded entity properties** — marks individual embedded entities as restricted
- Extend the source generator to read `[HypermediaAccessGroup]` from all four targets
- Emit `AccessGroups` on `EntityTypeSchema`, `ActionDescription`, `LinkDescription`, `EmbeddedEntityDescription`
- Collect all discovered access groups into `HypermediaApiSchema.DeclaredAccessGroups`
- Verify tests: HTO with grouped and ungrouped elements at all levels, `DeclaredAccessGroups` completeness

#### Step 4.2: ✅ Filtered schema endpoint — include mode
- Implement `HypermediaSchemaFilter.ForAccessGroups(schema, grantedAccessGroups)`
  - Remove elements whose `AccessGroups` are not satisfied by the granted set
  - Remove unreachable entity types
  - Strip `DeclaredAccessGroups` from filtered output
- Extend `/hypermedia-schema` endpoint to accept `?accessGroups=read,write` query parameter. should be possible to buidl a link to this with usual RESTyard mechanisms.
- Verify tests:
- Integration test: CarShack with access groups, verify filtered output for different group combinations

#### Step 4.2b: ✅ Filtered schema endpoint — exclude mode
- Implement `HypermediaSchemaFilter.ExcludeAccessGroups(schema, excludedAccessGroups)`
  - Remove elements whose `AccessGroups` intersect with the excluded set
  - Remove unreachable entity types
  - Strip `DeclaredAccessGroups` from filtered output
- Extend `/hypermedia-schema` endpoint to accept `?excludeAccessGroups=admin,sales` query parameter
- Integration test: CarShack excluding specific access groups, verify elements are removed correctly

#### Step 4.3: ✅ `ISchemaAccessGroupSanitizer` hook
- Define `ISchemaAccessGroupSanitizer` interface in `RESTyard.AspNetCore`: `SanitizeRequestedGroups(IReadOnlySet<string> requestedGroups, HttpContext httpContext)` → returns the groups the user is allowed to query
- Default behavior when no implementation registered: pass through unchanged (schema is public)
- Wire into the `/hypermedia-schema` endpoint: sanitize before calling `HypermediaSchemaFilter`
- Unit test: sanitizer removes groups, verify filtered output reflects sanitized set
- Integration test: register a role-based sanitizer in CarShack, verify non-admin can't query admin-only groups

#### Step 4.3.1: ✅ Access groups discovery endpoint
- `MapHypermediaSchemaAccessGroups(Action<HypermediaSchemaAccessGroupsOptions>? configure = null)` extension method on `IEndpointRouteBuilder`
- `HypermediaSchemaAccessGroupsOptions`: `Route` (default `"/schema/access-groups"`)
- Returns `{ "accessGroups": [...] }` with content type `application/vnd.restyard.hypermedia-schema-access-groups+json` — the `DeclaredAccessGroups` filtered through `ISchemaAccessGroupSanitizer` for the current user
- Add constant to `SchemaMediaTypes`: `HypermediaSchemaAccessGroups`
- When no sanitizer registered: returns all `DeclaredAccessGroups`
- Returns `IEndpointConventionBuilder` for chaining `.RequireAuthorization()` etc.
- Unit test: verify endpoint returns sanitized groups based on registered sanitizer
- Integration test: verify non-admin sees fewer groups than admin

#### Step 4.4: ✅ CarShack demo
- Add `[HypermediaAccessGroup]` to 1-2 hand-written HTO partials (entity-level only — action/link attributes can't be added from partial classes)
- Verify `DeclaredAccessGroups` appears in full schema and filtering works end to end

#### Step 4.5: ✅ Access group filtering and help in CLI
- Add `--access-groups <groups>` (include mode) and `--exclude-access-groups <groups>` (exclude mode) to `GenerateSchemaIfRequested`
- Reuse `HypermediaSchemaFilter.ForAccessGroups` / `ExcludeAccessGroups` — apply filter before passing schema to mappers
- Validate mutual exclusivity (error if both specified)
- Note: CLI does not use `ISchemaAccessGroupSanitizer` (no HTTP context) — the caller is trusted
- Add `--schema-help` — prints all available schema generation arguments to console and returns `true`
- Test with CarShack: generate filtered schema/diagrams for specific access group combinations

#### Step 4.6: ✅ Update mappers to render access groups

**MarkdownMapper:**
- **Header section:** List `DeclaredAccessGroups` after entry point (e.g., "**Declared Access Groups:** admin, read, write"). Omit when null.
- **Entity heading:** Show `AccessGroups` after description (e.g., "**Access Groups:** admin, sales"). Omit for public entities.
- **Actions:** Show `AccessGroups` after action heading, same format as existing **Returns:** line (e.g., "**Access Groups:** admin")
- **Links table:** Add "Access Groups" column showing groups or empty for public
- **Embedded entities table:** Add "Access Groups" column showing groups or empty for public

**MermaidMapper:**
- **API Map:** Append access group annotation to node labels for restricted entities (e.g., `Entity["Entity 🔒"]` or `Entity["Entity [admin]"]`)
- **Class Diagram:** Use `<<access: group1, group2>>` stereotype on restricted entity classes. Optionally annotate restricted actions/links in method/relationship labels.
- Keep diagrams readable — only annotate restricted elements, not public ones

**Tests:**
- Unit tests for MarkdownMapper output containing access group text
- Unit tests for MermaidMapper output containing access group annotations

#### Step 4.7: ✅ Update documentation for access groups

**Attribute & semantics:**
- `[HypermediaAccessGroup]` attribute: targets (class, property), `params string[]`, OR semantics (any match grants access)
- Relation to `[Authorize]`: descriptive only, no runtime enforcement
- `AccessGroups` on schema model types: null = public, populated = restricted
- `DeclaredAccessGroups` on `HypermediaApiSchema`: auto-collected, useful for typo detection

**Filtered schema endpoint:**
- `?accessGroups=` (include) and `?excludeAccessGroups=` (exclude) query parameters
- Mutually exclusive — specifying both returns 400
- `RemoveUnreachableEntityTypes`: entity types that become orphaned after filtering are removed

**Access groups discovery endpoint:**
- `MapHypermediaSchemaAccessGroups()` setup with `HypermediaSchemaAccessGroupsOptions` (configurable route, default `/schema/access-groups`)
- `AccessGroupsResponse` model and `SchemaMediaTypes.HypermediaSchemaAccessGroups` media type

**Sanitizer:**
- `ISchemaAccessGroupSanitizer`: purpose, default pass-through, brief example

**Link helpers:**
- `HypermediaSchema.Link()` and `HypermediaSchemaAccessGroups.Link()` — usage for discoverable links from HTOs

**CLI:**
- `--access-groups`, `--exclude-access-groups` args
- `--schema-help` usage

#### Step 4.8: ✅ Filtered schema links via `HypermediaSchema.Link(filter)`
- `HypermediaSchema.Link(HypermediaSchemaFilterParameters)` creates links to filtered schemas
- `HypermediaSchemaFilterParameters` shared between link helper and endpoint (`[AsParameters]`)
- Added `schema-customer` filtered link to CarShack entrypoint
- Integration test: entrypoint link resolves to filtered schema
- No convenience schema HTO/action endpoint built — the link helper enables users to build their own if needed

### Phase 5: Source Generator — Siren POCOs

**Goal:** Add the Siren POCO types to `RESTyard.AspNetCore`.

#### Step 5.1: ✅ Add Siren POCO types to `RESTyard.AspNetCore`
- Add Siren POCO classes as regular C# files in `RESTyard.AspNetCore/Hypermedia/Siren/Model/`: `SirenEntity`, `SirenEntity<TProperties>`, `SirenLink`, `SirenAction`, `SirenField`, `SirenSubEntity`, `SirenEmbeddedEntity`, `SirenLinkedEntity`
- **Not source-generated** — source generator assemblies run inside the Roslyn compiler host and cannot expose types to consuming projects at runtime. Since every HTO project already references `RESTyard.AspNetCore`, no extra dependency is needed. Real C# classes are easier to read, edit, and navigate in the IDE.
- Reference the official Siren JSON Schema (https://github.com/kevinswiber/siren/blob/master/siren.schema.json) to ensure the POCOs match the Siren spec. Validate property names, types, required fields, and structure against the schema. Document any intentional deviations (e.g., generic `TProperties` extension).
- `SirenEntity` has no `Properties` — only structural fields (Class, Title, Links, Actions, Entities)
- `SirenEntity<TProperties> : SirenEntity` adds `TProperties? Properties`
- `SirenEmbeddedEntity.Entity` is typed as `SirenEntity` (non-generic base)
- Use `[JsonPropertyName("class")]` on `Class` properties since `class` is a C# keyword
- Unit tests: verify JSON round-trip serialization of Siren POCOs (serialize → deserialize → assert equality)

### Phase 6: Source Generator — ToSiren() Emission

**Goal:** Generate `ToSiren()` extension methods replacing the reflection-based `SirenConverter`.

**Migration guide:** `Docs/HypermediaSchema/migration-guide.md` — tracks behavioral differences discovered during implementation. Update as new differences are found in each step.

**Key design note — property name casing:** When using `ToSiren()` directly, the user controls property name casing via `JsonSerializerOptions.PropertyNamingPolicy`. Siren structural properties (`class`, `rel`, `href`) always use lowercase via `[JsonPropertyName]`. Entity data properties use C# property names as-is — the user chooses the serializer naming policy. This differs from `SirenConverter` which always uses PascalCase.

**Key design note — auto self link:** `ToSiren()` automatically adds a `"self"` link via `resolver.ObjectToRoute(hto)` (controlled by `SirenMapperOptions.AutoSelfLink`, default `true`). This is new behavior — `SirenConverter` only includes self links from explicit `ILink<T>` properties. Documented in migration guide.

**Testing strategy — parity tests alongside snapshots:**
- All `ToSiren()` tests live in `RESTyard.HtoSourceGenerators.Test` (already references `RESTyard.AspNetCore`)
- Each test verifies both: snapshot of generated Siren JSON (Verify) AND parity with `SirenConverter` output
- `StubRouteResolver` implements `IHypermediaRouteResolver` — configured via a route mappings list (e.g., `RouteMapping.ForObject<THto>(url, method)`), no mocking library needed
- Same `StubRouteResolver` instance feeds both `ToSiren()` and `SirenConverter` — any JSON difference is a real divergence in mapping logic
- Uses the real `QueryStringBuilder` (no stub needed)
- JSON normalization helper: `NormalizeJson(string json)` — parse via `JsonDocument`, re-serialize with `WriteIndented = true` via `System.Text.Json`. Eliminates formatting/whitespace differences between Newtonsoft (`SirenConverter`) and System.Text.Json (`ToSiren()`) output. If property ordering diverges between serializers, extend with key sorting.
- Parity helper: `AssertParityAndVerify(hto, resolver)` — normalizes both outputs, compares JSON, snapshots the result

#### Step 6.1: ✅ Basic entity mapping using existing properties POCO
- **Properties POCO already exists** — generated in Step 2.7.1a (`HypermediaCustomerHtoProperties`), reused here. No new POCO generation needed.
- Add Siren type constants to `SchemaTypeNames` (`SirenEntity`, `SirenLink`, `SirenAction`, `SirenField`, `SirenEmbeddedEntity`, `SirenSubEntity`, `SirenMapperOptions`, `IHypermediaRouteResolver`, `ResolvedRoute`, etc.)
- Create `SirenMapperOptions` class in `RESTyard.AspNetCore/Hypermedia/Siren/SirenMapperOptions.cs` with `AutoSelfLink` (default `true`) — needed so generated code compiles
- Emit `ToSiren()` extension method per HTO returning `SirenEntity<TProperties>`
- Emit `ToSirenEmbedded()` extension method per HTO returning `SirenEmbeddedEntity<TProperties>`, hidden via `[EditorBrowsable(EditorBrowsableState.Never)]` — called by parent HTOs for nested entities, avoids intermediate `SirenEntity` allocation
- Both methods share the same mapping logic internally in the generator
- Map `[HypermediaObject]` → `SirenEntity.Class`, `Title`. When `Classes` is null on the attribute, fall back to the type name (matches `SirenConverter` behavior)
- Map HTO data properties → generated properties POCO instance (assign `hto.PropertyName` → `poco.PropertyName` for each data property)
- Self link via `resolver.ObjectToRoute(hto)` when `options.AutoSelfLink != false`
- Gated on `[HypermediaAssembly(Siren = true)]`
- Verify tests: snapshot output for a simple HTO, property mapping correctness, `ToSirenEmbedded()` output

#### Step 6.2: Link resolution
- Resolve `ILink<T>` properties → `SirenLink` with URL from `resolver.ReferenceToRoute(link.Value)`
- Append query string from `reference.GetQuery()` via `QueryStringBuilder.CreateQueryString()` — required for query result links (same as `SirenConverter.ResolveReferenceRoute`)
- Populate `SirenLink.Type` from `ResolvedRoute.AvailableMediaTypes` (media type info from runtime resolver)
- Handle nullable links (omit when null)
- Deduplicate links by relations (same behavior as `SirenConverter` — if multiple `ILink` properties share the same `[Relations]`, last one wins)
- Handle `ExternalReference` links — use reference URI directly, no route resolver call
- Verify tests: mandatory link, optional/null link, external link, media type populated, query string appended, deduplication

#### Step 6.3: Action resolution
- Resolve action properties → `SirenAction` with URL from `resolver.ActionToRoute(hto, action)`
- Populate `SirenAction.Method` from `ResolvedRoute.HttpMethod`
- Populate `SirenAction.Class` with built-in action class markers from `ActionClasses` (`ParameterLessActionClass`, `ParameterActionClass`, `FileUploadActionClass`, `FileUploadActionWithParameterClass`) plus user-defined classes from `[HypermediaAction(Classes = [...])]`
- Null-safe check: `if (hto.Action?.CanExecute() == true)`
- Populate `SirenAction.Type`: `multipart/form-data` for file upload, `application/json` for parameterized, omitted for parameterless. Use `ResolvedRoute.AcceptableMediaType` when present (external actions), otherwise defaults.
- Map action parameters to `SirenField` entries:
  - Resolve parameter schema URL via `resolver.TryGetRouteByType(paramType, routeKeys)` with fallback to `resolver.RouteUrl(RouteNames.ActionParameterTypes, ...)` — put in `Fields[].Class`
  - Handle `IDynamicSchema.SchemaRouteKeys` for dynamic actions (custom route keys for schema resolution)
  - Include prefilled values via `action.GetPrefilledParameter()` in `Fields[].Value` — handle both string (parse as JSON) and object (serialize) cases
  - Set `Fields[].Type` to `"application/json"` for JSON parameters
- Handle file upload actions: `FileUploadHypermediaAction` / `FileUploadHypermediaAction<T>` with file field (`name = "UploadFiles"`, `type = "file"`, `accept`, `maxFileSizeBytes`, `allowMultiple`)
- Handle external actions: `HypermediaExternalAction` — use `ExternalUri` directly, `HttpMethod`, `AcceptedMediaType` from the external action base
- Verify tests: parameterless, with params, file upload, null/non-executable actions, external actions, action classes, prefilled values (string and object), dynamic schema route keys

#### Step 6.4: Embedded entity resolution
- **Resolved references** (`reference.IsResolved() == true`): call `ToSirenEmbedded()` on the instance (no intermediate `SirenEntity` allocation)
- **Unresolved references** (`reference.IsResolved() == false`): emit a `SirenLinkedEntity` (href + class + rel) instead of a full embedded representation — resolve URL via `resolver.ReferenceToRoute()`. Handle `HypermediaExternalObjectReference` (use URI directly, with external classes).
- Set `Rel` from `[Relations]` attribute on the parent HTO's embedded entity property
- Handle single (`IEmbeddedEntity<T>`) and collection (`List<IEmbeddedEntity<T>>`) embedded entities
- Handle nullable single embedded entities (omit when null)
- Verify tests: single embedded, collection, nullable, nested embedded entities, unresolved → linked sub-entity, external object reference

#### Step 6.5: SirenMapperOptions DI wiring
- `SirenMapperOptions` class already created in Step 6.1
- Add `WriteNullProperties` option (default `true`, matching `HypermediaConverterConfiguration.WriteNullProperties`) — controls whether null property values are included in the Siren JSON output. Maps to `JsonSerializerOptions.DefaultIgnoreCondition` at serialization time.
- Wire through DI: `AddHypermediaSirenMapper(Action<SirenMapperOptions>?)` or resolve from `IServiceProvider`
- Generated `ToSiren()` falls back to default options when `null` is passed
- Verify tests: `AutoSelfLink = false` omits self link, `WriteNullProperties = false` omits nulls, defaults include both

#### Step 6.6: Controller extension method `ToSiren(hto)`
- Add `ControllerBaseExtensions.ToSiren(this ControllerBase, IHypermediaObject hto)` returning `SirenEntity<TProperties>` wrapped in `OkObjectResult`
- Resolves `IHypermediaRouteResolver` from `HttpContext.RequestServices` — no need to inject resolver into controllers
- **Set response Content-Type to `application/vnd.siren+json`** — the existing `SirenHypermediaFormatter` sets this automatically, but since `ToSiren()` bypasses the formatter and returns a plain POCO, the extension method must set the media type explicitly (e.g., via `ContentResult` or by setting `ContentTypes` on the `OkObjectResult`)
- Usage: `return this.ToSiren(myHto);` instead of `return Ok(myHto.ToSiren(resolver))`
- This is the **recommended pattern for new APIs** — explicit return type enables correct OpenAPI schema generation (Swagger sees `SirenEntity<T>`, not the HTO class)
- Note: RESTyard's own `HypermediaApiSchema` is actually richer than OpenAPI for hypermedia APIs (describes the full hypermedia graph), but OpenAPI compatibility matters for mixed tooling ecosystems
- Verify tests: extension method returns correct type, resolves resolver from DI, response Content-Type is `application/vnd.siren+json`

### Phase 7: Generated Siren Output Formatter

**Goal:** Provide a drop-in replacement output formatter that uses the generated `ToSiren()` internally, for existing APIs that want the performance benefit without rewriting controllers.

#### Step 7.1: `GeneratedSirenFormatter` implementation
- Implement `GeneratedSirenFormatter` as an alternative to `SirenHypermediaFormatter` that uses `ToSiren()` instead of reflection-based `SirenConverter`
- Must be configurable: register via `AddHypermediaSirenMapper()` DI method (separate from `AddHypermediaExtensions()`, consistent with `AddHypermediaSchema()`)
- When registered, replaces the existing `SirenHypermediaFormatter` for HTOs that have generated `ToSiren()` methods; falls back to `SirenConverter` for HTOs without generated mappers (allows incremental migration)
- Discover available `ToSiren()` mappers at startup — similar to registry pattern from schema generation

#### Step 7.2: Formatter configuration and registration
- `AddHypermediaSirenMapper()` registers the `GeneratedSirenFormatter` and replaces or wraps the existing output formatter
- Configuration: opt-in per assembly via `[HypermediaAssembly(Siren = true)]` (already designed)
- Must work alongside existing `SirenHypermediaFormatter` for assemblies without `Siren = true`
- Respect `ControllerAndHypermediaAssemblies` for formatter scope

#### Step 7.3: Documentation — alternative formatter and migration path
- Document the two approaches for using `ToSiren()`:
  - **Option 1 (recommended for new APIs):** Direct return via `this.ToSiren(hto)` controller extension — explicit, OpenAPI-compatible, full serialization control
  - **Option 2 (migration path for existing APIs):** `GeneratedSirenFormatter` — drop-in replacement, no controller changes, transparent performance improvement
- Document migration path: existing API → add `[HypermediaAssembly(Siren = true)]` + `AddHypermediaSirenMapper()` → formatter handles `ToSiren()` automatically → optionally migrate controllers to `this.ToSiren(hto)` one by one → remove formatter when fully migrated
- Document trade-offs: Option 2 has same OpenAPI limitation as current formatter (Swagger sees HTO type, not Siren shape); Option 1 fixes this

### Phase 8 (Optional): Migration and Parity

**Goal:** Ensure generated output matches the existing reflection-based formatter. This phase is optional — the schema and `ToSiren()` are independently useful without migrating away from the existing formatter.

#### Step 8.1: Parity tests
- For every HTO in CarShack: compare `SirenConverter` JSON output vs `ToSiren()` JSON output
- Fix any discrepancies in the generator

#### Step 8.2: Opt-in migration in CarShack
- Migrate CarShack controllers one by one to use `hto.ToSiren(resolver)`
- Keep existing formatter active for non-migrated controllers
- Integration tests pass for both paths

#### Step 8.3: Deprecate reflection-based formatter
- Mark `SirenHypermediaFormatter` and `SirenConverter` as `[Obsolete]`
- Document migration path in Docs/HypermediaSchema/

#### Step 8.4: Update contract-first generator to emit `ResultType` and migrate to `[HypermediaActionEndpoint<T>]`
- **Deferred from Step 2.13.1** — the contract-first generator currently emits legacy `[Http*HypermediaAction]` attributes, not `[HypermediaActionEndpoint<T>]`. Updating `ResultType` on the legacy attributes was done as part of Step 2.13, but the template should be migrated to the new attribute pattern as part of the overall migration.
- **only new template engine** make sure only to update new aproach using razor templates (v5). also regenerate onyl using this
- Update the server controller template (`server/csharp-controller/v4` or new `v5`) to emit `[HypermediaActionEndpoint<THto>]` instead of `[Http*HypermediaAction]`
- Emit `ResultType = typeof(...)` on the new attribute when `operation.resultDocument` is set in the XML schema
- **Implementation insights from Step 2.13.1:**
  - The XML schema's `OperationType.resultDocument` contains the result document name — access via `operation.resultDocument` in Scriban (not `operation.result_document` — Scriban uses the exact C# property name on .NET objects)
  - The `isNotEmpty` helper from `_common.sbn` works for checking if resultDocument is set
  - CarShack has 6 operations with `resultDocument` (UploadCarImage, UploadInsuranceScan, UpdateInspection, CreateCustomer, CreateQuery, BuyCar)
  - The legacy `HttpMethodHypermediaAction` base class now has `ResultType` property — source generator scans it via `InheritsFrom` helper
- **Legacy cleanup:** When this step is done, remove legacy support from the source generator:
  - Remove `HttpMethodHypermediaActionBaseFullName` constant
  - Remove `InheritsFrom` scan in `ExtractActionResultMappings`
  - Remove `InheritsFrom` helper method
  - The test `Legacy_HttpMethodHypermediaAction_type_exists` in `HtoSchemaGeneratorTests` will fail when the legacy type is removed — this is intentional as a reminder to clean up the generator code
- Verify with CarShack: regenerate controllers with new template, confirm `ResultType` appears, all tests pass
- Extend `Hypermedia.xsd` and its C# model classes to support `accessGroups` on documents (entity types), operations (actions), and references (links/embedded entities)
- Extend Razor templates (v5) to emit `[HypermediaAccessGroup("group1", "group2")]` on generated HTO classes, action properties, and link properties when specified in the XML schema
- This enables full access group coverage for contract-first APIs — currently Step 4.4 (CarShack demo) only covers entity-level access groups from hand-written partial classes because action/link attributes can't be added from partials
- Add CarShack XML schema examples with access groups and regenerate to verify end-to-end
- Make sure mappers (mermaid and markdown) are supporting  access groups on actiosn, links subentities. Prompt the user to check visaually.

#### Step 8.5: Migrate `ActionParameterTypes` endpoint to minimal API
- minimal api support is notyet implemented. work can be found e.g. here: https://github.com/RESTyard/RESTyard/blob/feature/restyard-minimal-apis/Source/RESTyard.AspNetCore/MinimalApi/Extensions/EndpointRouteBuilderExtensions.cs
- The current `ActionParameterTypesController` is an MVC controller registered automatically via `AddHypermediaExtensions`. It serves JSON Schema for action parameter types. Users cannot add authorization policies to it (only global MVC filters apply).
- Migrate to a minimal API endpoint similar to `MapHypermediaSchema()` — e.g., `MapActionParameterTypes()` returning `IEndpointConventionBuilder` so users can chain `.RequireAuthorization()`.
- **Configuration bridge:** Currently `HypermediaExtensionsOptions` controls whether the endpoint is registered. Maintain this: if the user calls `MapActionParameterTypes()` explicitly, the old MVC controller is not registered. If they don't call it, the existing MVC controller behavior is preserved for backwards compatibility. Consider a flag like `HypermediaExtensionsOptions.AutoRegisterParameterTypeEndpoint = true` (default, current behavior) that can be set to `false` when the user opts into the minimal API version.
- **Breaking change potential:** Users relying on global MVC filters (e.g., `options.Filters.Add(new AuthorizeFilter())`) for auth on the parameter types endpoint will find that the minimal API version doesn't inherit those filters. Document this clearly with migration examples.
- **Documentation required:**
  - Migration guide: old (automatic MVC) → new (explicit minimal API with `.RequireAuthorization()`)
  - Explain why: consistent with `MapHypermediaSchema()`, enables per-endpoint auth policies
  - Show both patterns: keep auto-registration (no change needed) vs. opt into minimal API
- Verify with CarShack: migrate, confirm parameter type endpoints still work, integration tests pass

### Phase 9: Revisit Open Questions

**Goal:** With a working implementation in hand, revisit the open questions from the spec and decide which to address.

#### Step 9.1: Review open questions
- Read through the Open Questions section in `HypermediaSchema-Design.md`
- For each question, decide: resolve now, defer, or close as won't-do
- Update the spec accordingly — move resolved items to Design Decisions, remove closed items

#### Step 9.2: Evaluate deferred features
- **Parameter validation routes** — is there a concrete use case from CarShack or real projects?
- **Example values** — would CarShack benefit from examples in the schema?
- **Tag groups** — is there a grouping need beyond the entity graph?
- **Mermaid customization** — filtering by reachability from entry point
- For each: implement if justified, otherwise document the decision to defer in the spec

### Phase 10: Documentation

**Goal:** User-facing documentation for all schema and source generation features.

#### Step 10.1: Update Docs/HypermediaSchema/
- Document the schema endpoint, model, and Mermaid mapper
- Document the CLI generation mode (`GenerateSchemaIfRequested`) and all CLI args
- Document `MermaidMapperOptions` (`IncludeProperties`, `IncludeActions`) and `MarkdownMapperOptions` (`IncludeTableOfContents`, `IncludeDiagram`) — API usage and corresponding CLI args (`--mermaid-include-properties`, `--mermaid-include-actions`, `--markdown-include-toc`, `--markdown-include-diagram`)
- Document access group filtering (if implemented in Phase 4)
- Add migration guide for existing users

#### Step 10.2: Document `ToSiren()` migration path (Phase 8)
- Document how to migrate from the reflection-based `SirenHypermediaFormatter` to the source-generated `ToSiren()` extension methods
- Cover: per-controller opt-in, how to call `hto.ToSiren(resolver)` in controllers, how to verify parity with the existing formatter
- Document `SirenMapperOptions` (`AutoSelfLink`) and how to configure via DI or explicit parameter
- Explain the generated Siren POCOs (`SirenEntity<TProperties>`) and how attribute forwarding works (serializer attributes, `[HypermediaProperty(Name)]` applied structurally)
- List known behavioral differences (if any discovered during Phase 8 parity testing)
- Provide a checklist for migrating a full project: enable generator → migrate controllers one by one → run parity tests → deprecate formatter
- **Not designed yet** — to be explored after basic filtering is stable
- Serve the schema as `HypermediaSchemaHto` — a proper RESTyard hypermedia resource
- Query action accepts `accessGroups` / `excludeAccessGroups` as parameters, returns filtered schema
- `AvailableAccessGroups` property lists only the groups the current user can query (post-sanitization via `ISchemaAccessGroupSanitizer`)
- Query action parameter is a string list — client selects from `AvailableAccessGroups`
- Stays within RESTyard's hypermedia design: client discovers filtering via the HTO's actions
- Trade-off: more complex (controller, route registration, Siren serialization) vs. the simple JSON endpoint
- Both query parameters and `AvailableAccessGroups` are sanitized by `ISchemaAccessGroupSanitizer`
- Schema is still a JSON download link (not rendered as Siren) — the HTO wraps the query/filtering, not the schema content
- Consider making this a default endpoint (auto-registered like action parameter schema endpoints)
