# Hypermedia Schema — Implementation Plan

Companion to [HypermediaSchema-Design.md](HypermediaSchema-Design.md) (the spec).

## Working Guidelines

- **Keep the spec updated.** As implementation progresses, update `HypermediaSchema-Design.md` with any design decisions, edge cases, behavior clarifications, or spec changes discovered during implementation. The spec is the single source of truth and will later be used to create user documentation — treat it as living memory.
- **Small steps.** Each step below is scoped to be implementable and testable independently.
- **Test as you go.** Every step includes verification. Don't batch testing to the end.

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
- Schema endpoint integration test: start CarShack via `WebApplicationFactory`, call `/_schema`, verify the returned JSON matches expected structure
- Validate that the schema JSON is stable (snapshot test) — breaking changes in the schema format should be caught
- Mermaid mapper output: snapshot tests for both diagram types against known schemas

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
- `MermaidMapper.ToEntityGraph()` and `MermaidMapper.ToClassDiagram()`
- Unit tests with snapshot verification against hand-crafted schema inputs

### Phase 2: Source Generator — Project Setup and Schema Generation

**Goal:** Set up the generator project and generate `GetSchema()` methods that produce `EntityTypeSchema` per HTO.

#### Step 2.1: Create `RESTyard.HtoSourceGenerators` project and test project
- New `netstandard2.0` class library with `<IsRoslynComponent>true</IsRoslynComponent>`
- New `RESTyard.HtoSourceGenerators.Test` xunit + Verify test project
- Implement `IIncrementalGenerator` skeleton
- Reference from CarShack to verify the generator loads without errors

#### Step 2.2: HTO discovery and basic metadata
- Find all `IHypermediaObject` types in compilation
- Extract `[HypermediaObject(Title, Classes)]`
- Emit `GetSchema()` returning `EntityTypeSchema` with `Name`, `Classes`, `Title`
- Verify tests: snapshot + assertion for a minimal HTO

#### Step 2.3: Property analysis → JSON Schema
- Implement `JsonSchemaBuilder` (Roslyn `ITypeSymbol` → JSON Schema string)
- Handle primitives, strings, DateTime, enums, nullable, arrays/lists, nested objects via `$ref`
- Populate `PropertiesSchema` on `EntityTypeSchema`
- Respect `[HypermediaProperty(Name)]`, `[FormatterIgnoreHypermediaProperty]`
- Verify tests: HTO with various property types

#### Step 2.4: Link analysis
- Scan `ILink<T>` properties with `[Relations]`
- Populate `LinkDescription` (relations, target name/classes, mandatory from nullability)
- Verify tests: HTO with mandatory/optional links

#### Step 2.5: Action analysis
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
- Emit per-assembly `HypermediaSchemaRegistry` collecting all `GetSchema()` results
- Verify: CarShack registry lists all its entity types

### Phase 3: Schema Endpoint

**Goal:** Serve the schema at runtime via `/_schema`.

#### Step 3.1: DI integration
- `AddHypermediaSchema(options => { ... })` extension method in `RESTyard.AspNetCore`
- Aggregates per-assembly registries into singleton `HypermediaApiSchema`
- Reference `RESTyard.Schema`

#### Step 3.2: Schema endpoint
- `MapHypermediaSchema("/_schema")` endpoint
- Returns `HypermediaApiSchema` as JSON (`application/vnd.restyard.schema+json`)
- Integration test: CarShack → `WebApplicationFactory` → `GET /_schema` → verify JSON structure

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
- **Parameter validation routes** — is there a concrete use case from CarShack or real projects?
- **Example values** — would CarShack benefit from examples in the schema?
- **Tag groups** — is there a grouping need beyond the entity graph?
- **Mermaid customization** — filtering by reachability from entry point
- For each: implement if justified, otherwise document the decision to defer in the spec

### Phase 8: Documentation and Tooling

#### Step 8.1: Mermaid diagram integration
- CLI command or MSBuild task to generate Mermaid `.md` files from `/_schema`
- Test with CarShack

#### Step 8.2: Update RESTyard-Docs
- Document the schema endpoint, model, and Mermaid mapper
- Add migration guide for existing users

### Phase 9 (Optional): Access Groups

> **Optional.** See the "Future Idea: Access Groups" section in `HypermediaSchema-Design.md` for the full design. Only pursue after the core schema and source generator are stable and a concrete use case demands it.

**Goal:** Allow the schema to describe which actions, links, and embedded entities require which access groups, and let clients request a filtered schema.

#### Step 9.1: `[HypermediaAccessGroup]` attribute and generator support
- Define `[HypermediaAccessGroup("groupName")]` attribute in `RESTyard.AspNetCore`
- Extend the source generator to read `[HypermediaAccessGroup]` from actions, links, and embedded entity properties
- Emit `RequiredAccessGroups` on `ActionDescription`, `LinkDescription`, `EmbeddedEntityDescription`
- Collect all discovered access groups into `HypermediaApiSchema.DeclaredAccessGroups`
- Verify tests: HTO with grouped and ungrouped elements, `DeclaredAccessGroups` completeness

#### Step 9.2: Filtered schema endpoint
- Implement `HypermediaSchemaFilter.ForAccessGroups(schema, grantedAccessGroups)`
  - Remove elements whose `RequiredAccessGroups` are not satisfied by the granted set
  - Remove unreachable entity types
  - Strip `DeclaredAccessGroups` from filtered output
- Extend `/_schema` endpoint to accept `?accessGroups=read,write` query parameter
- Integration test: CarShack with access groups, verify filtered output for different group combinations

#### Step 9.3: CarShack demo
- Add `[HypermediaAccessGroup]` to selected CarShack actions and links
- Verify the full and filtered schema endpoints work end to end