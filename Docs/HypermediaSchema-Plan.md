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

#### Step 2.6: Embedded entity analysis
- Scan embedded `IHypermediaObject` properties and collections
- Populate `EmbeddedEntityDescription`
- Verify tests: single embedded, collection

#### Step 2.7: Title and description harvesting
- Primary source: `[Title("...")]` and `[Description("...")]` from `JsonSchema.Net.Generation` (already in the dependency tree, used on `IHypermediaActionParameter` types)
- Fallback: XML doc `<summary>` → title, `<remarks>` → description
- Attributes take precedence over XML docs when both are present
- Apply to entity types, properties, actions, links
- Verify tests: HTO with attributes, with XML docs, with both (attribute wins)

#### Step 2.8: Deprecation support
- Read `[Obsolete("message")]` → `IsDeprecated`, `DeprecationMessage`
- Apply to entity types, actions, links, embedded entities
- Verify tests

#### Step 2.9: Schema registry generation
- Emit per-assembly `HypermediaSchemaRegistry_<AssemblyName>` class with static `GetSchemas(IJsonSchemaFactory)` collecting all `GetSchema()` results
- Emit `[assembly: HypermediaSchemaRegistryAttribute(typeof(Registry))]` attribute for discovery
- Define `HypermediaSchemaRegistryAttribute` in `RESTyard.AspNetCore` (so it's available at runtime for the CLI extension method to scan)
- The registry method calls each HTO's `GetSchema()` — passing `IJsonSchemaFactory` where needed, parameterless where not
- Verify tests: snapshot the generated registry for a multi-HTO source, verify attribute is emitted
- Verify with CarShack: registry lists all its entity types

#### Step 2.9.1: `HypermediaSchemaOptions` and DI integration
- Define `HypermediaSchemaOptions` class in `RESTyard.AspNetCore`: `Title`, `Description`, `ApiVersion`, `EntryPointName`, `ExternalDocsUrl` — all nullable with sensible defaults (assembly name for title, assembly version for ApiVersion, auto-detect entry point from entity with Siren class `"EntryPoint"`)
- Add `SchemaOptions` property to `HypermediaExtensionsOptions` (type `HypermediaSchemaOptions`, default `new()`)
- Register `HypermediaSchemaOptions` as singleton via DI (resolved from `HypermediaExtensionsOptions.SchemaOptions`)
- In `AddHypermediaExtensions`, aggregate per-assembly registries (via `[HypermediaSchemaRegistryAttribute]`) and `HypermediaSchemaOptions` into a singleton `HypermediaApiSchema` available via DI — this is the single source of truth for the schema at runtime
- Reference `RESTyard.Schema` from `RESTyard.AspNetCore` (already added as project reference)
- Add `HypermediaSchemaBuilder.Build(IServiceProvider, HypermediaSchemaOptions? options = null)` as standalone helper for programmatic use (tests, custom tooling)
- Test: resolve `HypermediaApiSchema` from CarShack DI, verify it contains all entity types with correct metadata from `SchemaOptions`

#### Step 2.9.2: CLI schema generation (`GenerateSchemaIfRequested`)
- Add `GenerateSchemaIfRequested(this IHost host, string[] args, HypermediaSchemaOptions? options = null)` extension method in `RESTyard.AspNetCore` — extends `IHost` (not `WebApplication`) so it works with generic host and non-web scenarios. When `options` is passed explicitly, it rebuilds the schema with the overridden options instead of using the DI singleton (xmldoc documents this).
- Parse CLI args: `--generate-schema` (trigger), `--schema-output <path>` (default: `./generated-schema`), `--schema-format <formats>` (default: all)
- Resolve `HypermediaApiSchema` singleton from DI (already aggregated during `AddHypermediaExtensions`); if explicit `options` passed, rebuild with overridden options
- Generate requested output files using `RESTyard.Schema` mappers (JSON serialization, `ToApiMap()`, `ToClassDiagram()`, `ToDocumentation()`)
- Return `true` if `--generate-schema` was present, `false` otherwise
- Test with CarShack: configure `SchemaOptions` in `AddHypermediaExtensions`, run `dotnet run -- --generate-schema --schema-output ./test-output`, verify all four files produced with correct metadata, process exits with code 0
- Schema format selection: `--schema-format json` produces only `schema.json`, `--schema-format mermaid-map,markdown` produces only those two
- Acceptance: CarShack `Program.cs` has one added line, schema JSON contains configured title/description

#### Step 2.9.3: Document the HypermediaApiSchema for users
- Write user-facing documentation for the schema model in RESTyard-Docs
- **Schema overview**: what the schema describes (type-level metadata, not runtime URLs), how it complements Siren responses
- **Top-level `HypermediaApiSchema`**: explain each field — `SchemaVersion` (format versioning), `ApiVersion` (user's API version), `EntryPointName` (navigation start), `Definitions` (shared JSON Schema types referenced via `$ref`)
- **`EntityTypeSchema`**: `Name` (identifier for cross-references, derived from class name or `[HypermediaSchemaName]`), `Classes` (Siren wire-format matching), `PropertiesSchema` (JSON Schema as `JsonDocument` — type, required, descriptions), relationship to the Siren `properties` bag
- **`LinkDescription`**: `Relations` (Siren rel array), `TargetName`/`TargetClasses` (cross-reference to another entity type), `IsMandatory` (nullability-derived — always present vs. conditional), `MediaType` (default `application/vnd.siren+json`, verify at runtime)
- **`ActionDescription`**: `Name`/`Title` (from `[HypermediaAction]`), `ParameterSchema` (JSON Schema for the action parameter type, null if parameterless), `IsFileUpload`, `IsMandatory`, `ContentType` (inferred), `ResultName`/`ResultClasses` (action returns a resource)
- **`EmbeddedEntityDescription`**: `Relations`, `TargetName`/`TargetClasses`, `IsCollection`, `IsMandatory`
- **Target audience**: client generator authors, documentation tool authors, AI agents consuming the schema — explain what each field is useful for and when it can be null
- **Examples**: annotated JSON snippets showing a real schema (e.g., from CarShack) with callouts explaining each section
- **CLI usage**: how to generate schema artifacts with `--generate-schema`, format selection

#### Step 2.10: Document the source generator for server developers (RESTyard-Docs)
- Getting started guide: how the source generator is enabled (bundled in NuGet after 2.10), what it generates (`GetSchema()`, schema registry, assembly attribute)
- Explain `[HypermediaSchemaName]` for custom entity names, when and why to use it (multi-assembly collisions, shorter names for docs/diagrams)
- What `GetSchema()` produces and how it uses `IJsonSchemaFactory` at runtime
- How to verify generation works: check for `*SirenMapper.g.cs` in build output, common troubleshooting (missing assembly reference, generator not running)
- How to use `GenerateSchemaIfRequested` in `Program.cs` — one-line setup, CLI args reference
- How to use `HypermediaSchemaBuilder.Build(IServiceProvider)` for programmatic access

#### Step 2.11: Bundle source generator into `RESTyard.AspNetCore` NuGet (deferred)
- Add the source generator DLL to the `RESTyard.AspNetCore` NuGet package alongside the existing analyzers:
  ```xml
  <None Include="..\RESTyard.HtoSourceGenerators\bin\$(Configuration)\netstandard2.0\RESTyard.HtoSourceGenerators.dll"
        Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
  ```
- This enables consumers who reference only `RESTyard.AspNetCore` to get both the analyzers and the source generator automatically
- Defer to after the source generator is feature-complete (link analysis, action analysis, etc.)

### Phase 3: Schema Endpoint

**Goal:** Serve the schema at runtime via `/hypermedia-schema`. DI integration (singleton `HypermediaApiSchema`) is already done in Step 2.9.1.

#### Step 3.1: Schema endpoint
- `MapHypermediaSchema()` endpoint (default route: `/hypermedia-schema`, configurable in HypermediaSchemaOptions )
- Returns `HypermediaApiSchema` as JSON (`application/vnd.restyard.schema+json`)
- Integration test: CarShack → `WebApplicationFactory` → `GET /hypermedia-schema` → verify JSON structure

### Phase 4: Source Generator — Siren POCOs

**Goal:** Emit the Siren POCO types into the consuming project.

#### Step 4.1: Emit Siren POCO types
- Generator emits `SirenEntity` (non-generic base), `SirenEntity<TProperties>` (generic), `SirenLink`, `SirenAction`, `SirenField`, `SirenSubEntity`, `SirenEmbeddedEntity`, `SirenLinkedEntity` into the consuming project
- `SirenEntity` has no `Properties` — only structural fields (Class, Title, Links, Actions, Entities)
- `SirenEntity<TProperties> : SirenEntity` adds `TProperties? Properties`
- `SirenEmbeddedEntity.Entity` is typed as `SirenEntity` (non-generic base)
- Verify: CarShack can reference the emitted types, compile, and use them in a trivial test

### Phase 5: Source Generator — ToSiren() Emission

**Goal:** Generate `ToSiren()` extension methods replacing the reflection-based `SirenConverter`.

#### Step 5.1: Generate properties POCOs and basic entity mapping
- For each HTO, emit a properties POCO class (e.g., `HypermediaCustomerHtoSirenProperties`)
  - Include only data properties (exclude `[FormatterIgnoreHypermediaProperty]`, links, actions, keys, embedded entities)
  - Apply `[HypermediaProperty(Name = "x")]` structurally: use `x` as the C# property name on the POCO
  - Forward all other attributes from the HTO property verbatim (serializer attributes, converters, third-party — generator copies without interpreting)
  - Do NOT forward RESTyard-specific attributes: `[Key]`, `[Relations]`, `[HypermediaAction]`, `[HypermediaProperty]`, `[FormatterIgnoreHypermediaProperty]`
- Emit `ToSiren()` extension method per HTO returning `SirenEntity<TProperties>`
- Map `[HypermediaObject]` → `SirenEntity.Class`, `Title`
- Map properties → generated properties POCO instance
- Self link via `IHypermediaRouteResolver`
- Verify tests: snapshot output for a simple HTO, attribute forwarding, property name override via `[HypermediaProperty]`

#### Step 5.2: Link resolution
- Resolve `ILink<T>` properties → `SirenLink` with URL from `IHypermediaRouteResolver`
- Handle nullable links (omit when null)
- Verify tests

#### Step 5.3: Action resolution
- Resolve action properties → `SirenAction` with URL from `IHypermediaRouteResolver`
- Null-safe check: `if (hto.Action?.CanExecute() == true)`
- Map action parameters to `SirenField` entries with prefilled values
- Verify tests: parameterless, with params, file upload, null/non-executable actions

#### Step 5.4: Embedded entity resolution
- Recursive `ToSiren()` calls for embedded entities
- Handle single and collection embedded entities
- Verify tests

#### Step 5.5: SirenMapperOptions
- `AutoSelfLink` toggle (default true)
- Wire through DI or explicit parameter
- Verify tests

### Phase 6 (Optional): Migration and Parity

**Goal:** Ensure generated output matches the existing reflection-based formatter. This phase is optional — the schema and `ToSiren()` are independently useful without migrating away from the existing formatter.

#### Step 6.1: Parity tests
- For every HTO in CarShack: compare `SirenConverter` JSON output vs `ToSiren()` JSON output
- Fix any discrepancies in the generator

#### Step 6.2: Opt-in migration in CarShack
- Migrate CarShack controllers one by one to use `hto.ToSiren(resolver)`
- Keep existing formatter active for non-migrated controllers
- Integration tests pass for both paths

#### Step 6.3: Deprecate reflection-based formatter
- Mark `SirenHypermediaFormatter` and `SirenConverter` as `[Obsolete]`
- Document migration path in RESTyard-Docs

### Phase 7: Revisit Open Questions

**Goal:** With a working implementation in hand, revisit the open questions from the spec and decide which to address.

#### Step 7.1: Review open questions
- Read through the Open Questions section in `HypermediaSchema-Design.md`
- For each question, decide: resolve now, defer, or close as won't-do
- Update the spec accordingly — move resolved items to Design Decisions, remove closed items

#### Step 7.2: Evaluate deferred features
- **Full `$ref` resolution in all mappers** — resolve `$ref` to definition names (e.g., `Address` instead of `object`) in the Mermaid class diagram, Mermaid entity graph, and Markdown documentation mapper. When implementing, revisit whether the Markdown mapper should add a dedicated Definitions section with cross-links from property/parameter tables.
- **Parameter validation routes** — is there a concrete use case from CarShack or real projects?
- **Example values** — would CarShack benefit from examples in the schema?
- **Tag groups** — is there a grouping need beyond the entity graph?
- **Mermaid customization** — filtering by reachability from entry point
- For each: implement if justified, otherwise document the decision to defer in the spec

### Phase 8: Documentation and CLI Tooling

**Goal:** Provide a CLI mechanism for developers and CI pipelines to generate schema JSON, Mermaid diagrams, and Markdown documentation — with access group filtering — without manually running the server.

#### Step 8.1: Investigate generate-and-exit mechanism
- Spike the approaches described in the design doc (command-line argument on server app, `IHostedService`, separate CLI tool, MSBuild task)
- Must be a lib functionality that can be added to a server
- Evaluate: how cleanly can the full DI container and schema registries be accessed without actually listening for HTTP requests?
- Decide on the approach and document the decision in the design doc
- Acceptance criteria: a CarShack invocation that produces `schema.json` and exits

#### Step 8.2: Implement generate-and-exit mode
- Implement the chosen approach with support for:
  - `--generate-schema` flag to trigger generation mode
  - `--schema-output <path>` for output directory
  - `--schema-format <formats>` to select which artifacts to produce (json, mermaid-map, mermaid-class, markdown)
  - Mapper options pass-through (`--mermaid-include-properties`, `--mermaid-include-actions`, `--markdown-include-toc`, `--markdown-include-diagram`)
- Test with CarShack: verify all four output formats are produced correctly

#### Step 8.3: Access group filtering in CLI
- Add `--access-groups <groups>` (include mode) and `--exclude-access-groups <groups>` (exclude mode) parameters
- Reuse `HypermediaSchemaFilter.ForAccessGroups` / `ExcludeAccessGroups` from Phase 9
- Validate mutual exclusivity (error if both specified)
- Test with CarShack: generate filtered schema/diagrams for specific access group combinations

#### Step 8.4: Update RESTyard-Docs
- Document the schema endpoint, model, and Mermaid mapper
- Document the CLI generation mode and access group filtering
- Document `MermaidMapperOptions` (`IncludeProperties`, `IncludeActions`) and `MarkdownMapperOptions` (`IncludeTableOfContents`, `IncludeDiagram`) — API usage and corresponding CLI args (`--mermaid-include-properties`, `--mermaid-include-actions`, `--markdown-include-toc`, `--markdown-include-diagram`)
- Add migration guide for existing users

#### Step 8.5: Document `ToSiren()` migration path (Phase 6)
- Document how to migrate from the reflection-based `SirenHypermediaFormatter` to the source-generated `ToSiren()` extension methods
- Cover: per-controller opt-in, how to call `hto.ToSiren(resolver)` in controllers, how to verify parity with the existing formatter
- Document `SirenMapperOptions` (`AutoSelfLink`) and how to configure via DI or explicit parameter
- Explain the generated Siren POCOs (`SirenEntity<TProperties>`) and how attribute forwarding works (serializer attributes, `[HypermediaProperty(Name)]` applied structurally)
- List known behavioral differences (if any discovered during Phase 6 parity testing)
- Provide a checklist for migrating a full project: enable generator → migrate controllers one by one → run parity tests → deprecate formatter

### Phase 9 (Optional): Access Groups

> **Optional.** See the "Future Idea: Access Groups" section in `HypermediaSchema-Design.md` for the full design. Only pursue after the core schema and source generator are stable and a concrete use case demands it.

**Goal:** Allow the schema to describe which actions, links, and embedded entities require which access groups, and let clients request a filtered schema.

#### Step 9.1: `[HypermediaAccessGroup]` attribute and generator support
- Define `[HypermediaAccessGroup("groupName")]` attribute in `RESTyard.AspNetCore`
- Extend the source generator to read `[HypermediaAccessGroup]` from actions, links, and embedded entity properties
- Emit `RequiredAccessGroups` on `ActionDescription`, `LinkDescription`, `EmbeddedEntityDescription`
- Collect all discovered access groups into `HypermediaApiSchema.DeclaredAccessGroups`
- Verify tests: HTO with grouped and ungrouped elements, `DeclaredAccessGroups` completeness

#### Step 9.2: Filtered schema endpoint — include mode
- Implement `HypermediaSchemaFilter.ForAccessGroups(schema, grantedAccessGroups)`
  - Remove elements whose `RequiredAccessGroups` are not satisfied by the granted set
  - Remove unreachable entity types
  - Strip `DeclaredAccessGroups` from filtered output
- Extend `/hypermedia-schema` endpoint to accept `?accessGroups=read,write` query parameter
- Integration test: CarShack with access groups, verify filtered output for different group combinations

#### Step 9.2b: Filtered schema endpoint — exclude mode
- Implement `HypermediaSchemaFilter.ExcludeAccessGroups(schema, excludedAccessGroups)`
  - Remove elements whose `RequiredAccessGroups` intersect with the excluded set
  - Remove unreachable entity types
  - Strip `DeclaredAccessGroups` from filtered output
- Extend `/hypermedia-schema` endpoint to accept `?excludeAccessGroups=admin` query parameter
- Integration test: CarShack excluding specific access groups, verify elements are removed correctly

#### Step 9.3: `ISchemaAccessGroupSanitizer` hook
- Define `ISchemaAccessGroupSanitizer` interface in `RESTyard.AspNetCore`: `SanitizeRequestedGroups(IReadOnlySet<string> requestedGroups, HttpContext httpContext)` → returns the groups the user is allowed to query
- Default behavior when no implementation registered: pass through unchanged (schema is public)
- Wire into the `/hypermedia-schema` endpoint: sanitize before calling `HypermediaSchemaFilter`
- Unit test: sanitizer removes groups, verify filtered output reflects sanitized set
- Integration test: register a role-based sanitizer in CarShack, verify non-admin can't query admin-only groups

#### Step 9.4: CarShack demo
- Add `[HypermediaAccessGroup]` to selected CarShack actions and links
- Register a sample `ISchemaAccessGroupSanitizer` that restricts `admin` group to admin users
- Verify the full and filtered schema endpoints work end to end

#### Step 9.5: Access group filtering in CLI
- Add `--access-groups <groups>` (include mode) and `--exclude-access-groups <groups>` (exclude mode) to `GenerateSchemaIfRequested`
- Reuse `HypermediaSchemaFilter.ForAccessGroups` / `ExcludeAccessGroups` — apply filter before passing schema to mappers
- Validate mutual exclusivity (error if both specified)
- Note: CLI does not use `ISchemaAccessGroupSanitizer` (no HTTP context) — the caller is trusted
- Test with CarShack: generate filtered schema/diagrams for specific access group combinations

#### Step 9.6: Update documentation for access groups
- Document the `[HypermediaAccessGroup]` attribute: usage, semantics (descriptive not enforcing), relation to `[Authorize]`
- Document `RequiredAccessGroups` on `ActionDescription`, `LinkDescription`, `EmbeddedEntityDescription` — what null vs. populated means
- Document `DeclaredAccessGroups` on `HypermediaApiSchema` — auto-collected, useful for typo detection
- Document the filtered `/hypermedia-schema` endpoint: `?accessGroups=` and `?excludeAccessGroups=` query parameters, include vs. exclude semantics, mutual exclusivity
- Document `ISchemaAccessGroupSanitizer`: purpose, default behavior, example implementation
- Document the CLI access group args: `--access-groups`, `--exclude-access-groups`, examples
- Add examples: annotated JSON showing filtered vs. full schema, CarShack access group setup

#### Step 9.7 (Future idea): Schema as RESTyard HTO with query action in SchemaRootHto
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
