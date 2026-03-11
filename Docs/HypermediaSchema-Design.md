# Hypermedia Schema — Design Document

## Table of Contents

- [Goal](#goal)
- [Architecture Overview](#architecture-overview)
- [Schema Model](#schema-model)
  - [Top-Level: HypermediaApiSchema](#top-level-hypermediaapischemahttpschema)
  - [Entity Type: EntityTypeSchema](#entity-type-entitytypeschema)
  - [Data Shapes and Shared Definitions](#data-shapes-and-shared-definitions)
  - [Hypermedia Graph: Links](#hypermedia-graph-links)
  - [Hypermedia Graph: Actions](#hypermedia-graph-actions)
  - [Hypermedia Graph: Embedded Entities](#hypermedia-graph-embedded-entities)
- [Source Generator](#source-generator)
  - [Project Setup](#project-restyard-htosourcegenerators-netstandard20)
  - [Input Analysis](#input-analysis)
  - [Generated Output per HTO](#generated-output-per-hto)
  - [Generated Schema Registry](#generated-schema-registry)
- [Runtime Schema Endpoint](#runtime-schema-endpoint)
- [Mermaid Diagram Mapper](#mermaid-diagram-mapper)
- [Replacing the Siren Formatter](#replacing-the-siren-formatter)
- [Siren POCO Model](#siren-poco-model)
- [Project Structure](#project-structure)
- [Intentionally Excluded from Schema](#intentionally-excluded-from-schema)
- [Design Decisions](#design-decisions)
- [Open Questions](#open-questions)

## Goal

Replace the runtime reflection-based Siren serialization with source-generated mappers, and introduce a self-describing schema model for the hypermedia API. The schema model serves as a single source of truth for:

- **Runtime Siren serialization** — generated `ToSiren()` methods per HTO, replacing `SirenConverter` and `SirenHypermediaFormatter`
- **Runtime schema endpoint** — dedicated endpoint serving the full API schema as JSON
- **Documentation UI** — consumes the schema endpoint to render an interactive API explorer
- **Mermaid diagram export** — generates Mermaid class/graph diagrams for markdown-based documentation
- **Client code generation** — generates typed clients in C#, TypeScript, Python from the schema

## Architecture Overview

```
  HTO source code + Controller source code
  (C# classes, attributes, XML doc comments)
              |
              | [Source Generator — compile time]
              v
      +-------+-------+
      |               |
  ToSiren()      GetSchema()
  extension      extension
  method         method
  per HTO        per HTO
      |                |
      |                +---> Per-assembly Schema Registry (generated)
      |                         |
      |                         +---> Aggregated via DI at startup (no reflection)
      |                         |
      |                         +---> JSON endpoint (runtime)
      |                         +---> Mermaid mapper (runtime or build-time)
      |                         +---> Documentation UI (consumes endpoint)
      |                         +---> Client generators (consume JSON)
      |
      +---> Called directly in controllers
              -> returns SirenEntity POCO
              -> standard JSON serialization
```

## Schema Model

The schema spec describes two things that Siren itself does not provide:

1. **Data shapes** — JSON Schemas for entity properties and action parameters
2. **Hypermedia graph** — how entity types connect via links, actions, and embedded entities

The Siren spec already defines what an entity *looks like* at runtime (class, title, properties, links, actions, entities). This schema does not duplicate that. Instead it provides the **type-level metadata** that Siren responses lack: what properties a Customer *always* has, what actions are available, what parameters they expect, and how entity types relate to each other.

### Top-Level: `HypermediaApiSchema`

```csharp
public class HypermediaApiSchema
{
    public string SchemaVersion { get; set; }               // Schema format version (semver of this spec format)
    public string? ApiVersion { get; set; }                 // Version of the API described by this schema
    public string? Title { get; set; }                      // API title
    public string? Description { get; set; }                // API description
    public string? ExternalDocsUrl { get; set; }            // Link to external documentation (e.g., RESTyard-Docs site)
    public string EntryPointName { get; set; }                   // References EntityTypeSchema.Name of the entry point
    public IReadOnlyList<EntityTypeSchema> EntityTypes { get; set; }
    public IDictionary<string, JsonElement> Definitions { get; set; } // Shared type definitions, referenced via $ref
}
```

**Error responses:** All actions are expected to return [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457) on failure. This is a convention of RESTyard APIs and does not need per-action error schema definitions. Client generators should always handle `application/problem+json` responses. The `HypermediaProblem` type on the server side and `IProblemStringReader` on the client side already implement this convention.

### Entity Type: `EntityTypeSchema`

Describes one type of Siren entity — its data shape and its hypermedia connections.

```csharp
public class EntityTypeSchema
{
    public string Name { get; set; }                        // Unique name, defaults to full class name (e.g., "HypermediaCustomerHto"), overridable via [HypermediaSchemaName]
    public IReadOnlyList<string> Classes { get; set; }      // Siren classes that identify this entity type
    public string? Title { get; set; }                      // From attribute or XML doc <summary>
    public string? Description { get; set; }                // From XML doc <remarks> or attribute

    // Data shape — JSON Schema for the Siren "properties" bag
    public JsonElement? PropertiesSchema { get; set; }

    // Hypermedia graph — how this entity type connects to others
    public IReadOnlyList<LinkDescription> Links { get; set; }
    public IReadOnlyList<ActionDescription> Actions { get; set; }
    public IReadOnlyList<EmbeddedEntityDescription> EmbeddedEntities { get; set; }

    public bool IsDeprecated { get; set; }                  // Entire entity type marked for removal
    public string? DeprecationMessage { get; set; }         // Why deprecated and what to use instead
}
```

### Data Shapes and Shared Definitions

Entity properties (`PropertiesSchema`) and action parameters (`ActionDescription.ParameterSchema`) are both described using standard JSON Schema. Complex types (classes, records) and enums are always extracted to the top-level `Definitions` dictionary and referenced via `$ref`. Primitives and simple collections are inlined.

**JSON Schema representation:** All JSON Schema fields use `System.Text.Json.JsonElement` (or `JsonElement?` for nullable). This avoids a dependency on `JsonSchema.Net` in the schema model library, keeps the model serialization-framework-agnostic, and round-trips naturally with `System.Text.Json`. The source generator produces the JSON Schema strings at compile time; they are parsed into `JsonElement` values for the schema model.

Example `PropertiesSchema` referencing a shared `Address` definition:

```json
{
  "type": "object",
  "properties": {
    "name": { "type": "string", "title": "The customer's full name" },
    "age": { "type": "integer", "title": "Age in years" },
    "address": { "$ref": "#/definitions/Address" },
    "tags": { "type": "array", "items": { "type": "string" } }
  },
  "required": ["name", "age"]
}
```

Example `Definitions`:

```json
{
  "Address": {
    "type": "object",
    "title": "Postal address",
    "properties": {
      "street": { "type": "string" },
      "city": { "type": "string" },
      "zipCode": { "type": "string" }
    },
    "required": ["street", "city", "zipCode"]
  },
  "CustomerSortProperties": {
    "type": "string",
    "enum": ["name", "age", "createdAt"]
  }
}
```

The source generator populates JSON Schema `title` and `description` from HTO source code (priority: attribute values, then XML doc `<summary>` for title, then `<remarks>` for description). This applies at every level including nested object properties.

### Hypermedia Graph: Links

Describes which relations an entity type exposes and what entity type they point to.

```csharp
public class LinkDescription
{
    public IReadOnlyList<string> Relations { get; set; }    // From [Relations]
    public string TargetName { get; set; }                  // Name of the target entity type (references EntityTypeSchema.Name)
    public IReadOnlyList<string> TargetClasses { get; set; }// Siren classes of the target entity type
    public string? MediaType { get; set; }                   // Media type (e.g., "application/json") — maps to Siren link "type" field
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsMandatory { get; set; }                   // Non-nullable ILink<T>
    public bool IsDeprecated { get; set; }                  // Marked for removal in a future version
    public string? DeprecationMessage { get; set; }         // Why deprecated and what to use instead
}
```

### Hypermedia Graph: Actions

Describes which actions an entity type offers and what parameters they accept.

```csharp
public class ActionDescription
{
    public string Name { get; set; }                        // From [HypermediaAction(Name)]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ContentType { get; set; }                // Inferred: multipart/form-data for file uploads, application/json otherwise
    public JsonElement? ParameterSchema { get; set; }        // JSON Schema for the parameter type (null if parameterless)
    public string? ResultName { get; set; }                    // Name of the result entity type (null if no result)
    public IReadOnlyList<string>? ResultClasses { get; set; } // Siren classes of the result entity (null if no result)
    public bool IsMandatory { get; set; }                   // Non-nullable action property (always present, may still have CanExecute guard)
    public bool IsFileUpload { get; set; }                  // FileUploadHypermediaAction
    public bool IsDeprecated { get; set; }                  // Marked for removal in a future version
    public string? DeprecationMessage { get; set; }         // Why deprecated and what to use instead
}
```

**`ResultName` / `ResultClasses`**: When non-null, indicates the action creates/returns a resource (HTTP `Location` header). `ResultName` references the target `EntityTypeSchema.Name`, `ResultClasses` provides the Siren classes for wire-format matching. Essential for client generators (e.g., `CreateCustomer` → `Customer`) since Siren itself doesn't describe action results. `null` = fire-and-forget.

### Hypermedia Graph: Embedded Entities

Describes which entity types can appear as embedded sub-entities.

```csharp
public class EmbeddedEntityDescription
{
    public IReadOnlyList<string> Relations { get; set; }
    public string TargetName { get; set; }                  // Name of the target entity type (references EntityTypeSchema.Name)
    public IReadOnlyList<string> TargetClasses { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsCollection { get; set; }                  // List of embedded entities
    public bool IsMandatory { get; set; }                   // Non-nullable embedded entity
    public bool IsDeprecated { get; set; }                  // Marked for removal in a future version
    public string? DeprecationMessage { get; set; }         // Why deprecated and what to use instead
}
```

## Source Generator

### Project: `RESTyard.HtoSourceGenerators` (netstandard2.0)

Required by Roslyn: source generators must target netstandard2.0.

### Input Analysis

The generator finds all types implementing `IHypermediaObject` in the compilation and extracts:

| Source | Data |
|---|---|
| `[HypermediaObject(Title, Classes)]` | Entity classes, title |
| `[HypermediaSchemaName("Name")]` | Custom schema name (default: full class name) |
| `[HypermediaProperty(Name)]` | Property name override |
| `[FormatterIgnoreHypermediaProperty]` | Property exclusion |
| `[Relations(rels)]` on `ILink<T>` | Link relations, target type |
| `[HypermediaAction(Name, Title)]` | Action name, title |
| `HypermediaAction<TParam>` | Parameter type |
| `IHypermediaActionParameter` properties | Action fields |
| `[Mandatory]` | Required fields |
| `[Key("name")]` | Route key properties |
| `FileUploadHypermediaAction` | File upload flag |
| `CanExecute()` method | Optional action flag (null property = omitted) |
| `[Title("...")]` (`JsonSchema.Net.Generation`) | Title (primary) |
| `[Description("...")]` (`JsonSchema.Net.Generation`) | Description (primary) |
| XML doc `<summary>` | Title (fallback when no `[Title]` attribute) |
| XML doc `<remarks>` | Description (fallback when no `[Description]` attribute) |
| XML doc `<summary>` on HTO class | `EntityTypeSchema.Title` (fallback when no `[HypermediaObject(Title)]`) |
| XML doc `<remarks>` on HTO class | `EntityTypeSchema.Description` |
| XML doc `<summary>` on link property | `LinkDescription.Description` |
| XML doc `<summary>` on action property | `ActionDescription.Description` (fallback when no `[HypermediaAction(Title)]` for title) |
| XML doc `<summary>` on embedded entity property | `EmbeddedEntityDescription.Description` |
| Nullable annotations | Nullability of properties, links |
| `[HypermediaActionEndpoint<THto>("prop")]` on controllers | Action-to-HTO mapping |

**Constraint:** Each HTO has exactly one `[HypermediaObjectEndpoint<THto>]` and each action has exactly one `[HypermediaActionEndpoint<THto>("prop")]` in the codebase. This one-to-one mapping simplifies controller scanning — the generator can find the single matching endpoint for any HTO or action without disambiguation. If the generator finds zero or multiple endpoints for the same HTO/action, it should emit a diagnostic error.

### Generated Output per HTO

For each HTO class `HypermediaCustomerHto`, the generator emits an extension method class and a schema method:

```csharp
// <auto-generated/>
public static class HypermediaCustomerHtoSirenMapper
{
    public static SirenEntity<HypermediaCustomerHtoSirenProperties> ToSiren(
        this HypermediaCustomerHto hto, IHypermediaRouteResolver resolver,
        SirenMapperOptions? options = null)
    {
        var entity = new SirenEntity<HypermediaCustomerHtoSirenProperties>
        {
            Class = ["Customer"],
            Title = "A Customer",
            Properties = new HypermediaCustomerHtoSirenProperties
            {
                Name = hto.Name,
                Age = hto.Age,
                // ... mapped properties
            }
        };

        // Self link — auto-generated when options.AutoSelfLink is true (default)
        if (options?.AutoSelfLink != false)
        {
            entity.Links.Add(new SirenLink
            {
                Rel = ["self"],
                Href = resolver.ResolveSelfUrl(hto)
            });
        }

        // Links — resolve URLs at runtime
        if (hto.BestFriend is { } bestFriendLink)
        {
            entity.Links.Add(new SirenLink
            {
                Rel = ["bestFriend"],
                Href = resolver.ResolveLinkUrl(bestFriendLink)
            });
        }

        // Actions — resolve URLs at runtime, inline parameter schema
        // Null action property = action not available (omitted from output)
        if (hto.MarkAsFavorite?.CanExecute() == true)
        {
            entity.Actions.Add(new SirenAction
            {
                Name = "MarkAsFavorite",
                Title = "Mark as Favorite",
                Href = resolver.ResolveActionUrl(hto.MarkAsFavorite),
                Fields = [ /* mapped from TParameter properties */ ]
            });
        }

        // Embedded entities — recursive
        foreach (var car in hto.Cars)
        {
            entity.Entities.Add(new SirenEmbeddedEntity
            {
                Rel = ["item"],
                Entity = car.ToSiren(resolver, options)
            });
        }

        return entity;
    }

    // GetSchema() — returns EntityTypeSchema with PropertiesSchema, Links, Actions, EmbeddedEntities
    // populated from compile-time analysis (see Schema Model section for structure)
    public static EntityTypeSchema GetSchema() { /* ... */ }
}

// Generated properties POCO per HTO — the Siren "properties" bag as a strongly-typed class.
// [HypermediaProperty(Name)] is applied structurally: the C# property name IS the Siren name.
// [FormatterIgnoreHypermediaProperty] properties are omitted entirely.
// All other attributes from the HTO property are forwarded as-is (serializer attributes,
// converters, validation, third-party — the generator copies them without interpreting them).
public class HypermediaCustomerHtoSirenProperties
{
    public string? Name { get; set; }
    public int? Age { get; set; }

    // Attribute forwarded from HTO property — generator doesn't know what it does
    [JsonConverter(typeof(MoneyJsonConverter))]
    public Money? Balance { get; set; }

    // [HypermediaProperty(Name = "fullName")] on HTO → C# property named "fullName"
    // If user also has [JsonPropertyName("x")] on the HTO, it gets forwarded and
    // the serializer applies it on top — the user explicitly wanted that override.
    public string? fullName { get; set; }

    // [FormatterIgnoreHypermediaProperty] → property not present here at all
    // [Key("id")] → not forwarded (RESTyard routing concern)
    // [Relations] → not forwarded (link metadata, not a property)
}
```

### Generated Schema Registry

One registry per assembly. APIs spanning multiple assemblies get one registry each; they are aggregated at startup via DI.

```csharp
// <auto-generated per assembly/>
public static class HypermediaSchemaRegistry_MyAssembly
{
    public static IReadOnlyList<EntityTypeSchema> Schemas => [
        HypermediaCustomerHtoSirenMapper.GetSchema(),
        HypermediaEntrypointHtoSirenMapper.GetSchema(),
        HypermediaCarsRootHtoSirenMapper.GetSchema(),
        // ... all discovered HTOs in this assembly
    ];
}
```

At startup, all per-assembly registries are collected and composed into the full API schema:

```csharp
// Aggregation via DI — collects registries from all configured assemblies
builder.Services.AddHypermediaSchema(options =>
{
    options.Title = "My API";
    options.Description = "Customer management API";
    options.EntryPointName = "EntryPoint";
    // Assemblies are already known from AddHypermediaExtensions
});
```

This produces a singleton `HypermediaApiSchema` available via DI, combining all per-assembly registries.

**Startup validation:** At startup, the aggregated schema should validate that all `TargetName` references in `LinkDescription`, `ActionDescription.ResultName`, and `EmbeddedEntityDescription` resolve to an existing `EntityTypeSchema.Name`. Dangling references (e.g., a link to `"Order"` when no `OrderHto` exists) indicate a missing or unregistered HTO and should log a warning. An option `AllowUnresolvedReferences = true` (default `false`) can be provided for development scenarios — when enabled, unresolved references generate placeholder `EntityTypeSchema` entries (with empty links/actions/properties) so the schema endpoint and Mermaid diagrams remain functional while the API is still being built.

## Runtime Schema Endpoint

Served via a dedicated ASP.NET Core endpoint:

```csharp
app.MapHypermediaSchema("/_schema");

// Internally resolves HypermediaApiSchema from DI and serializes it
```

Returns JSON. Content type: `application/vnd.restyard.schema+json`.

The schema intentionally does not contain resolved URLs or route templates. Clients discover URLs at runtime by navigating the hypermedia API starting from the entry point — this is a core principle of hypermedia. The schema describes the *shape* of the API (entities, relations, actions, properties) not the *location* of resources.

## Mermaid Diagram Mapper

Converts `HypermediaApiSchema` to Mermaid diagram strings. Two diagram types:

### Entity Relationship Diagram

Shows HTO types, their links, and embedded entity relationships:

```mermaid
graph LR
    EntryPoint["EntryPoint"]
    CustomersRoot["CustomersRoot"]
    Customer["Customer"]
    CarsRoot["CarsRoot"]
    Car["Car"]

    EntryPoint -- "customers" --> CustomersRoot
    CustomersRoot -- "item" --> Customer
    Customer -- "cars" --> CarsRoot
    CarsRoot -- "item" --> Car
```

### Detailed Class Diagram

Shows properties and actions per entity:

```mermaid
classDiagram
    class Customer {
        +string name
        +int age
        +MarkAsFavorite()
        +BuyCar(carId)
    }
    class EntryPoint {
    }
    EntryPoint --> CustomersRoot : customers
    Customer --> CarsRoot : cars
```

### MermaidMapperOptions

```csharp
public class MermaidMapperOptions
{
    /// When true (default), the class diagram includes entity properties.
    /// Set to false to reduce diagram size for APIs with many properties.
    public bool IncludeProperties { get; set; } = true;

    /// When true (default), the class diagram includes actions.
    /// Set to false to reduce diagram size for APIs with many actions.
    public bool IncludeActions { get; set; } = true;
}
```

### API

```csharp
public static class MermaidMapper
{
    public static string ToEntityGraph(HypermediaApiSchema schema);       // graph LR
    public static string ToClassDiagram(HypermediaApiSchema schema, MermaidMapperOptions? options = null); // classDiagram
}
```

Can be used at runtime (via endpoint) or at build time (MSBuild task or CLI tool writing `.md` files).

**JSON Schema parsing limitations:** The class diagram extracts property names and types from `PropertiesSchema` by reading only the top-level `type` field of each property in the JSON Schema `properties` object. Complex constructs (`$ref`, `allOf`/`anyOf`/`oneOf`, nested objects, array item types) are not resolved and display as `object`. This is intentional — the diagram is a visualization aid, not a schema validator.

## Markdown Documentation Mapper

Converts `HypermediaApiSchema` to a Markdown API reference document. Designed for human consumption — describes entity types, their properties, links, actions, and embedded entities. No URL layout.

### Document Structure

1. **Header** — API title, description, version, external docs link (from `HypermediaApiSchema` top-level fields)
2. **Table of Contents** — anchor links to each entity section (optional, default: included)
3. **API Map** — embedded Mermaid entity graph via `MermaidMapper.ToEntityGraph()` (optional, default: included)
4. **Entity sections** — one `##` section per entity type, ordered by BFS from entry point (cycle-safe via visited set)

### Entity Section Layout

Each entity section contains:

- **Title** — entity name as `##` heading, with `[Deprecated]` badge if applicable
- **Description** — from `EntityTypeSchema.Title` and `Description`
- **Siren classes** — listed for reference
- **Properties table** — from `PropertiesSchema` JSON Schema:

  | Property | Type | Required | Description |
  |---|---|---|---|
  | name | string | yes | The customer's full name |
  | age | integer | yes | |
  | address | object | no | |

- **Links table**:

  | Relation | Target | Description |
  |---|---|---|
  | self | [Customer](#customer) | |
  | bestFriend *(optional)* | [Customer](#customer) | The customer's best friend |
  | orders | [OrdersRoot](#ordersroot) | Order history |

- **Actions table**:

  | Action | Description |
  |---|---|
  | MarkAsFavorite | Mark as favorite |
  | CreateOrder | Creates a new order. Returns: [Order](#order) |

  For actions with parameters, an indented parameter sub-table follows:

  | Parameter | Type | Required |
  |---|---|---|
  | carId | integer | yes |
  | color | string | no |

- **Embedded Entities table**:

  | Relation | Target | Collection | Description |
  |---|---|---|---|
  | item | [Car](#car) | yes | |

### Rendering Rules

- **Deprecation**: Bold `**[Deprecated]**` badge before the name, `DeprecationMessage` shown as a note underneath. Applies to entities, actions, links, and embedded entities.
- **Optional indicators**: Non-mandatory links, actions, and embedded entities get an `*(optional)*` suffix on their name/relation. Mandatory is the default — no annotation.
- **Self links**: Included in the links table (unlike the Mermaid mapper which skips them).
- **Action results**: When `ActionDescription.ResultName` is set, the action description includes "Returns: [TargetName](#anchor)".
- **Cross-links**: Target names in links and embedded entities are rendered as Markdown anchor links `[Name](#anchor)` pointing to the corresponding entity section.
- **JSON Schema parsing**: Same limitation as the Mermaid mapper — only top-level `type` is read, complex types show as `object`. `$ref` is not resolved. See Open Questions for planned improvement.

### MarkdownMapperOptions

```csharp
public class MarkdownMapperOptions
{
    /// When true (default), includes a Table of Contents at the top.
    public bool IncludeTableOfContents { get; set; } = true;

    /// When true (default), embeds a Mermaid entity graph diagram after the header.
    public bool IncludeDiagram { get; set; } = true;
}
```

### API

```csharp
public static class MarkdownMapper
{
    public static string ToDocumentation(HypermediaApiSchema schema, MarkdownMapperOptions? options = null);
}
```

## Replacing the Siren Formatter

### Current Flow (reflection-based)

```
Controller returns IHypermediaObject
    -> SirenHypermediaFormatter (output formatter)
        -> SirenConverter (reflection: walks properties, attributes)
            -> IHypermediaRouteResolver (resolves URLs)
        -> JSON serialization
    -> HTTP response
```

### New Flow (source-generated, no formatter)

```
Controller calls hto.ToSiren(resolver, options)
    -> returns SirenEntity<TProperties> (plain POCO with generated properties type)
    -> standard ASP.NET Core JSON serialization (respects all attributes on generated properties POCO)
    -> HTTP response
```

Controllers call `ToSiren()` directly — no output formatter needed:

```csharp
[HttpGet("{id}")]
[HypermediaObjectEndpoint<HypermediaCustomerHto>(typeof(CustomerRouteKeyProducer))]
public IActionResult Get(int id, [FromServices] IHypermediaRouteResolver resolver)
{
    var customer = _customerService.Get(id);
    var hto = new HypermediaCustomerHto(customer);
    return Ok(hto.ToSiren(resolver));
}
```

The `SirenEntity<TProperties>` is a plain POCO — serialized as regular JSON by ASP.NET Core. The `TProperties` is a generated properties class per HTO (see [Generated Output per HTO](#generated-output-per-hto)) that carries all forwarded attributes from the HTO's properties. This ensures user-defined serializer attributes (`[JsonConverter]`, `[JsonPropertyName]`, third-party attributes, etc.) work correctly without RESTyard needing to interpret them. This removes `SirenHypermediaFormatter`, `SirenConverter`, and all custom output formatter infrastructure.

**JSON serialization note:** Siren uses `class` as a property name, which is a C# keyword. The Siren POCOs must use `[JsonPropertyName("class")]` on the `Class` properties (or configure a naming policy that lowercases property names). Ensure the serializer is configured with `PropertyNamingPolicy = JsonNamingPolicy.CamelCase` or explicit `[JsonPropertyName]` attributes on all properties.

### SirenMapperOptions

```csharp
public class SirenMapperOptions
{
    /// When true (default), the generated ToSiren() automatically adds a "self" link
    /// by resolving the HTO's own route via the route resolver.
    /// Set to false if self links are managed manually via explicit ILink properties.
    public bool AutoSelfLink { get; set; } = true;
}
```

Can be configured via DI (resolved by controller) or passed explicitly per call.

### Migration Path

1. Ship `ToSiren()` alongside existing formatter (opt-in per controller)
2. Verify parity via integration tests (compare JSON output)
3. Migrate controllers one by one from returning HTOs to returning `hto.ToSiren(resolver)`
4. Deprecate `SirenHypermediaFormatter` and `SirenConverter`
5. Remove in next major version

## Siren POCO Model

Needed for `ToSiren()` return type. Plain C# classes matching the Siren JSON spec. These types are **emitted by the source generator** into the consuming project (not shipped as a separate assembly). Since they are simple data classes with no logic, source generation is ideal — users get the types automatically when they reference the generator package, with no extra NuGet dependency.

`SirenEntity` is split into a non-generic base and a generic `SirenEntity<TProperties>`. The generic type is returned by `ToSiren()`, giving compile-time type safety for the properties bag. The non-generic base is used in collections (embedded entities) where different entity types with different property types coexist. The generated properties POCO per HTO (see [Generated Output per HTO](#generated-output-per-hto)) serves as `TProperties`.

```csharp
// Non-generic base — used in embedded entity collections where property types are heterogeneous
public class SirenEntity
{
    public IReadOnlyList<string>? Class { get; set; }
    public string? Title { get; set; }
    public IList<SirenLink>? Links { get; set; }
    public IList<SirenAction>? Actions { get; set; }
    public IList<SirenSubEntity>? Entities { get; set; }    // embedded or linked
}

// Generic — returned by ToSiren(), TProperties is the generated properties POCO
public class SirenEntity<TProperties> : SirenEntity
{
    public TProperties? Properties { get; set; }
}

public class SirenLink
{
    public IReadOnlyList<string> Rel { get; set; }
    public string Href { get; set; }
    public string? Title { get; set; }
    public string? Type { get; set; }
}

public class SirenAction
{
    public string Name { get; set; }
    public IReadOnlyList<string>? Class { get; set; }       // Action classification (e.g., ParameterActionClass)
    public string? Title { get; set; }
    public string Method { get; set; }
    public string Href { get; set; }
    public string? Type { get; set; }                       // Content-Type
    public IReadOnlyList<SirenField>? Fields { get; set; }
}

public class SirenField
{
    public string Name { get; set; }
    public IReadOnlyList<string>? Class { get; set; }       // Field classification
    public string? Type { get; set; }                       // HTML input type
    public string? Title { get; set; }
    public object? Value { get; set; }                      // Prefilled value
}

public abstract class SirenSubEntity
{
    public IReadOnlyList<string> Rel { get; set; }
}

public class SirenEmbeddedEntity : SirenSubEntity
{
    public SirenEntity Entity { get; set; }                 // Full inline entity (non-generic base, runtime type is SirenEntity<T>)
}

public class SirenLinkedEntity : SirenSubEntity
{
    public string Href { get; set; }                        // Link to entity
    public IReadOnlyList<string>? Class { get; set; }
    public string? Title { get; set; }
    public string? Type { get; set; }
}
```

## Project Structure

```
Source/
  RESTyard.Schema/          # Schema model classes, mappers (regular library)
    Model/
      HypermediaApiSchema.cs
      EntityTypeSchema.cs
      LinkDescription.cs
      ActionDescription.cs
      EmbeddedEntityDescription.cs
    Mermaid/
      MermaidMapper.cs
      MermaidMapperOptions.cs
    Markdown/
      MarkdownMapper.cs
      MarkdownMapperOptions.cs
  RESTyard.HtoSourceGenerators/        # Source generator (netstandard2.0)
    HtoSirenGenerator.cs               # Emits ToSiren() per HTO
    HtoSchemaGenerator.cs              # Emits GetSchema() per HTO
    HtoRegistryGenerator.cs            # Emits HypermediaSchemaRegistry per assembly
    Emitted/                           # Source templates for types emitted into consuming project
      SirenEntity.cs
      SirenLink.cs
      SirenAction.cs
      SirenField.cs
      SirenSubEntity.cs
      SirenEmbeddedEntity.cs
      SirenLinkedEntity.cs
  RESTyard.AspNetCore/                 # Existing — references Schema, adds endpoint
    Schema/
      HypermediaSchemaEndpoint.cs      # MapHypermediaSchema("/_schema")
  RESTyard.Schema.Test/     # xunit — unit tests for schema model and Mermaid mapper
  RESTyard.HtoSourceGenerators.Test/   # xunit + Verify — snapshot tests for generated ToSiren(), GetSchema(), registry
```

**Why Siren POCOs are emitted, not in a library:** The `ToSiren()` return type (`SirenEntity`) must be available in the consuming project. Emitting these types via the source generator avoids an extra NuGet dependency. The types are simple data classes with no logic — ideal for source generation.

**Why Schema is a separate library:** The schema model (`HypermediaApiSchema`, `EntityTypeSchema`, etc.) needs to be consumed by external tools — client generators, Mermaid CLI, documentation UIs — that deserialize the `/_schema` JSON endpoint. These tools should not need to reference the full ASP.NET Core server library. Keeping the schema model in a lightweight standalone package enables this.

```
RESTyard.Schema       (netstandard2.0 — schema model + Mermaid)
    ^                   ^
    |                   |
RESTyard.AspNetCore     External tools (client generators, docs UIs)
(schema endpoint)       (deserialize /_schema JSON)
```

## Intentionally Excluded from Schema

The existing contract-first XML schema (`Hypermedia.xsd` / `Hypermedia.cs`) contains several concepts that are **not** included in this runtime schema spec. These were evaluated and excluded for the following reasons:

- **Policies / Permissions** — Authorization is a server-side concern. The schema describes the shape of the API, not access control. Clients discover what operations are available by inspecting the hypermedia responses at runtime — if an action is present, the client is permitted to attempt it. Embedding permission metadata in the schema would couple clients to a specific authorization model.

- **Explicit query model** — The generator models queries as a distinct concept (`query` attribute on links, `isQueryResult` on documents) because it needs to generate `Link.ByQuery<T>()` code. From the schema's perspective, a query is simply an action that returns a result entity — the client POSTs parameters and follows the `Location` header. The existing `ActionDescription.ResultClasses` already captures this relationship. A separate query concept would leak server-side implementation details.

- **Property `isKey`** — Key properties identify which route parameters map to an entity. This is a server-side routing concern. Clients should treat `self` links as opaque identifiers (a core HATEOAS principle) and must not attempt to decompose or construct URLs from entity properties.

- **Property `hidden`** — This is a code-generation hint for UI scaffolding, not a structural API description. If needed, it could be expressed as a JSON Schema extension (`x-hidden`) on individual properties, but it does not warrant a first-class schema concept.

- **`contentType` on links, `parameterTypeName` on links, `collectionName` on entities, `ExternalParameterType`** — These are code-generation plumbing used by the contract-first generator to emit the right C# types and constructors. They have no meaning in a runtime API description.

## Design Decisions

- **Schema versioning**: The schema format has its own semver (`SchemaVersion`), independent of the RESTyard package version. This allows the spec format to evolve at its own pace — a RESTyard update that doesn't change the schema shape doesn't bump the schema version, and vice versa.
- **External links/actions**: `ExternalLink` and `HypermediaExternalAction` have fixed URLs not resolved via route resolver. This is not a schema concern — the schema describes entity types and their relationships, not runtime URLs. External links are just links from the client's perspective; the client does not distinguish between internal and external.
- **Schema endpoint media type**: `application/vnd.restyard.schema+json`.
- **Incremental generator**: Use `IIncrementalGenerator` (the modern Roslyn API, better IDE performance).
- **HTO inheritance**: Flatten to concrete types. Siren has no inheritance concept, and flattening is simpler for client generators. Each concrete HTO becomes one `EntityTypeSchema`.
- **Deprecation**: The source generator reads C#'s built-in `[Obsolete("message")]` attribute. The message maps to `DeprecationMessage`, presence maps to `IsDeprecated = true`.
- **Shared definitions**: All complex types (classes, records) and enums are always extracted to the top-level `Definitions` dictionary and referenced via `$ref`. Primitives and simple collections are inlined. This keeps the generator logic simple — no need to track reuse counts.
- **Entity naming**: Each `EntityTypeSchema` has a `Name` used as the primary identifier in cross-references (`TargetName`), Mermaid diagrams, and documentation. By default, the name is the **full C# class name** (e.g., `"HypermediaCustomerHto"`) — no convention-based stripping is applied since user naming conventions may vary. To use a shorter or custom name, apply `[HypermediaSchemaName("Customer")]` on the HTO class:
  ```csharp
  [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
  [HypermediaSchemaName("Customer")]
  public class HypermediaCustomerHto : HypermediaObject { }
  ```
  This is a separate attribute (not on `[HypermediaObject]`) because it is a schema concern, not a Siren serialization concern. The generator resolves `TargetName` for links and embedded entities by reading `[HypermediaSchemaName]` from the target HTO type, falling back to the full class name. Siren `Classes` are retained for wire-format matching but are not used for referencing within the schema. The generator must verify that both `Name` and `Classes` are unique across all entity types and emit a diagnostic error on collision. `[HypermediaSchemaName]` also serves to resolve name collisions in multi-assembly APIs — e.g., if both `Billing.CustomerHto` and `Shipping.CustomerHto` exist, they would collide on the default name `"CustomerHto"`. Applying `[HypermediaSchemaName("BillingCustomer")]` and `[HypermediaSchemaName("ShippingCustomer")]` resolves the collision explicitly.
- **Description population via XML doc extraction**: The source generator populates `Description` fields on `EntityTypeSchema`, `LinkDescription`, `ActionDescription`, and `EmbeddedEntityDescription` by extracting XML doc comments from the C# source via `ISymbol.GetDocumentationCommentXml()`. For **entity types**, `<summary>` maps to `Title` (fallback when `[HypermediaObject(Title)]` is not set) and `<remarks>` maps to `Description`. For **links, actions, and embedded entities** (which are properties on the HTO class), `<summary>` maps to `Description` since these elements already get their `Title` from attributes. This requires no new attributes — developers use standard XML doc comments that also serve IntelliSense. The same extraction pattern already applies to JSON Schema `title`/`description` on properties (lines 248-251). Example:
  ```csharp
  /// <summary>A customer with profile and order history.</summary>
  /// <remarks>Represents an active customer account in the system.</remarks>
  [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
  public class HypermediaCustomerHto : HypermediaObject
  {
      /// <summary>The customer's complete order history.</summary>
      [Relations(["orders"])]
      public ILink<HypermediaOrderListHto>? Orders { get; set; }

      /// <summary>Creates a new order for this customer.</summary>
      [HypermediaAction(Name = "CreateOrder", Title = "Create Order")]
      public CreateOrderAction? CreateOrder { get; set; }
  }
  ```
  This produces: `EntityTypeSchema.Description = "Represents an active customer account in the system."`, `LinkDescription.Description = "The customer's complete order history."`, `ActionDescription.Description = "Creates a new order for this customer."`. This is particularly valuable for the Generic MCP Server design (see `GenericMcp-Design.md`) where descriptions become MCP tool descriptions that help LLMs understand what each action/link does.
- **No HTTP method in schema**: `ActionDescription` intentionally omits the HTTP method. Clients discover it at runtime from the Siren action's `method` field. Client generators emit generic "execute action" calls — the runtime Siren response dictates the transport details. This keeps the schema focused on type-level metadata, not transport concerns.
- **Definition name collisions**: `Definitions` dictionary keys use simple class names (e.g., `"Address"`). When two types share the same simple name but differ by namespace (e.g., `Billing.Address` and `Shipping.Address`), the generator disambiguates by prefixing with the namespace segment: `"Billing_Address"`, `"Shipping_Address"`. Only the colliding names are qualified — non-colliding names stay short. The generator detects collisions across all types discovered in the compilation.
- **Siren properties as generated POCO, not dictionary**: The Siren `properties` bag is represented as a generated strongly-typed class per HTO (`SirenEntity<TProperties>`) rather than `Dictionary<string, object?>`. This preserves user-defined attributes on HTO properties — serializer converters, naming, third-party attributes — because the generator forwards them to the generated POCO. A dictionary would lose per-property attributes since the serializer would only see `object?` values. The non-generic `SirenEntity` base is used for embedded entity collections where property types are heterogeneous; STJ in .NET 8 serializes `SirenEntity<T>` using the runtime type when referenced through the base.
- **RESTyard attributes applied structurally by generator**: `[HypermediaProperty(Name)]` and `[FormatterIgnoreHypermediaProperty]` are consumed by the source generator and applied structurally to the generated properties POCO — `[HypermediaProperty(Name = "x")]` sets the C# property name to `x`, `[FormatterIgnoreHypermediaProperty]` omits the property entirely. These attributes are **not** forwarded to the generated code (they've already been applied). All other attributes on HTO properties are forwarded verbatim. This keeps RESTyard attributes as serializer-agnostic declarations of intent, while letting serializer-specific attributes compose on top. If a user has both `[HypermediaProperty(Name = "sirenName")]` and `[JsonPropertyName("jsonName")]`, the generated POCO property is named `sirenName` with `[JsonPropertyName("jsonName")]` forwarded — the serializer overrides the name, and that's the user's explicit choice.
- **JSON Schema generation**: Use a small internal `JsonSchemaBuilder` class (~200-300 lines) inside the source generator. No external dependencies. Maps Roslyn `ITypeSymbol` to JSON Schema strings. Covers: primitives, string, DateTime/DateOnly/TimeOnly, Uri, Guid, enums (`[EnumMember]`), `Nullable<T>`, arrays/lists, nested objects via `$ref`. Needs cycle detection for recursive types. Edge cases deferred: `Dictionary<string, T>`, polymorphic `oneOf`, data annotation constraints.

## Future Idea: Access Groups

> **Status:** Future idea — not part of the initial implementation. To be revisited after the core schema and source generator are stable.

### Motivation

An AI agent or GUI consuming the hypermedia schema benefits from knowing upfront which actions and links require which permissions. Today, a client only discovers this at runtime — if an action is absent from a Siren response, it could mean "you lack permission" or "the entity's current state doesn't allow it." The schema can separate these two dimensions: **state-dependent availability** is already handled by `CanExecute` / nullable action properties, while **permission-dependent availability** is currently invisible.

This is purely descriptive metadata — the server still enforces authorization at runtime and omits actions/links from Siren responses based on actual permissions. Access groups tell the client what *could* be there given sufficient permissions.

### Approach: Access Groups on Elements

A `RequiredAccessGroups` string list on `LinkDescription`, `ActionDescription`, and `EmbeddedEntityDescription`. Declared via a `[HypermediaAccessGroup("...")]` attribute on HTO members. The source generator reads the attribute and emits the strings into the schema. `null` = no restriction (public). All discovered access groups are collected into `HypermediaApiSchema.DeclaredAccessGroups` automatically.

### Schema Model Additions

```csharp
public class HypermediaApiSchema
{
    // ... existing fields ...
    public IReadOnlyList<string>? DeclaredAccessGroups { get; set; }  // All access groups found in the API, collected automatically
}

public class ActionDescription
{
    // ... existing fields ...
    public IReadOnlyList<string>? RequiredAccessGroups { get; set; }  // null = no restriction (public)
}

public class LinkDescription
{
    // ... existing fields ...
    public IReadOnlyList<string>? RequiredAccessGroups { get; set; }
}

public class EmbeddedEntityDescription
{
    // ... existing fields ...
    public IReadOnlyList<string>? RequiredAccessGroups { get; set; }
}
```

### Developer Usage

Access groups are declared close to the code via attributes:

```csharp
[HypermediaAction(Name = "DeleteCustomer")]
[HypermediaAccessGroup("admin")]
public HypermediaAction? DeleteCustomer { get; set; }

[HypermediaAction(Name = "MarkAsFavorite")]
[HypermediaAccessGroup("write")]
public HypermediaAction? MarkAsFavorite { get; set; }

[Relations(["orders"])]
[HypermediaAccessGroup("read")]
public ILink<HypermediaOrdersHto>? Orders { get; set; }
```

No startup configuration needed — the source generator collects everything from attributes.

### Example Schema JSON

```json
{
  "declaredAccessGroups": ["read", "write", "admin"],
  "entityTypes": [
    {
      "name": "Customer",
      "actions": [
        { "name": "MarkAsFavorite", "requiredAccessGroups": ["write"] },
        { "name": "DeleteCustomer", "requiredAccessGroups": ["admin"] }
      ],
      "links": [
        { "relations": ["orders"], "requiredAccessGroups": ["read"] }
      ]
    }
  ]
}
```

### Filtered Schema Endpoint

The `/_schema` endpoint supports an `accessGroups` query parameter to return a filtered sub-document containing only the elements visible to the given access groups:

```
GET /_schema                              → full schema (all access groups)
GET /_schema?accessGroups=read            → filtered: only elements requiring "read" or no access groups
GET /_schema?accessGroups=read,write      → filtered: elements requiring "read", "write", or no access groups
```

Filtering logic for a given set of granted access groups:
1. Remove actions where `RequiredAccessGroups` contains any group not in the granted set
2. Remove links where `RequiredAccessGroups` contains any group not in the granted set
3. Same for embedded entities
4. Remove entity types that become unreachable (no links, no actions, no embedded entities, and not referenced by any remaining element)
5. Strip `DeclaredAccessGroups` from the filtered output (irrelevant)

```csharp
public static class HypermediaSchemaFilter
{
    public static HypermediaApiSchema ForAccessGroups(
        HypermediaApiSchema fullSchema, IReadOnlySet<string> grantedAccessGroups);
}
```

Clients typically know their access groups from their auth context (JWT claims, API key metadata, etc.) and pass them directly — no need for a server-side role-to-group mapping layer.

### Prior Art

The existing contract-first XML schema (`Hypermedia.cs`) already models scopes on documents (`DocumentType.Policies`) and operations (`OperationType.Policies`) via `PermissionType.scope`. Our design extends this with access groups on links and embedded entities, and a runtime-filtered endpoint.

### Design Notes

- **No ASP.NET Core coupling**: `[HypermediaAccessGroup]` is a RESTyard attribute read by the source generator. It has no connection to `[Authorize]`, policies, or claims. Developers are free to align them or not.
- **Access groups are freeform strings**: No predefined vocabulary. Each API defines its own group names.
- **The source generator collects `DeclaredAccessGroups`** automatically from all `[HypermediaAccessGroup]` attributes found in the compilation. This enables tooling to detect typos.
- **State vs. permissions are orthogonal**: A `null` action in a Siren response means either "no permission" or "state doesn't allow it." Access groups describe only the permission dimension. State-dependent availability is already captured by `IsMandatory` and the `CanExecute` pattern.
- **No permission profiles**: Role-to-group mappings (e.g., "admin = read + write + admin") are an identity/authorization concern, not an API schema concern. Clients know their access groups from their auth context and filter directly. This avoids duplicating role definitions that already exist in identity providers.

## Open Questions

- **Full `$ref` resolution in mappers**: All three mappers (Mermaid class diagram, Mermaid entity graph, Markdown documentation) currently show `object` for complex types and do not resolve `$ref` references. A future improvement should resolve `$ref` to the definition name (e.g., show `Address` instead of `object`) using the `Definitions` dictionary. When implementing this, also revisit whether the Markdown mapper should include a dedicated **Definitions** section at the end listing shared types as their own tables, with cross-links from property/parameter tables.
- **Mermaid customization**: Should the mapper support filtering (e.g., only show entities reachable from entry point)? Not for v1.
- **Parameter validation routes in Siren**: Allow UIs to validate action parameters before form submission by calling a server-side validation endpoint. This requires a Siren format extension — e.g., a `validationHref` field on actions that points to a validation endpoint returning field-level errors. Needs design for: the Siren extension format, the validation request/response contract, how the source generator discovers validation endpoints, and how the schema describes validation availability per action.
- **Example values**: Add support for example values on entity properties and action parameters in the schema (similar to OpenAPI's `example` keyword). Useful for documentation UIs to show realistic sample data and for client generators to emit test fixtures. Could be expressed as JSON Schema `examples` keyword or as a separate field on `EntityTypeSchema`/`ActionDescription`. To be designed in a future iteration.
- **Tag groups**: Allow grouping entity types by tags for documentation UIs (e.g., "Admin", "Public", "Billing"). The entity graph already provides natural grouping, but cross-cutting concerns that span multiple entities may benefit from explicit tags. To be designed if a concrete use case arises.
- **Target framework**: `RESTyard.Schema` currently targets `netstandard2.0` for broad compatibility (e.g., `RESTyard.Client` multi-targets `netstandard2.0;net8.0`). Reconsider moving to `net10` once all consuming projects have dropped `netstandard2.0` support.
