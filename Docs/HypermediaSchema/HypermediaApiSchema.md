# Hypermedia API Schema

The `HypermediaApiSchema` is a type-level description of a RESTyard hypermedia API. It describes **what entity types exist**, **how they connect** (via links, actions, and embedded entities), and **what data they carry** — without including runtime URLs or instance-specific state.

It complements Siren responses: Siren tells the client what a *specific entity instance* looks like right now; the schema tells the client (or a tool) what *all entity types* look like in general.

## Who is this for?

- **Client generator authors** — generate typed clients from the schema
- **Documentation tool authors** — render API reference docs, diagrams
- **AI agents / MCP servers** — understand available actions, parameters, and navigation paths
- **API developers** — generate schema artifacts for documentation and CI

## Schema Overview

```mermaid
graph TD
    Schema["HypermediaApiSchema"]
    Schema --> EntityTypes["EntityTypeSchema[]"]
    EntityTypes --> Props["PropertiesSchema (JSON Schema)"]
    EntityTypes --> Links["LinkDescription[]"]
    EntityTypes --> Actions["ActionDescription[]"]
    EntityTypes --> Embedded["EmbeddedEntityDescription[]"]
    Actions --> ParamSchema["ParameterSchema (JSON Schema)"]
    Schema --> Definitions["Definitions (shared JSON Schema types)"]
```

## Top-Level: `HypermediaApiSchema`

| Field | Type | Description |
|---|---|---|
| `schemaVersion` | `string` | Version of the schema format itself (e.g., `"1.0.0"`). Bumped when the schema structure changes. |
| `apiVersion` | `string?` | Version of the described API. Set via `HypermediaSchemaOptions.ApiVersion`. Null if not configured. |
| `title` | `string?` | Human-readable API title. Defaults to the entry assembly name if not set. |
| `description` | `string?` | Human-readable API description. Null if not configured. |
| `externalDocsUrl` | `string?` | URL to external documentation. Null if not configured. |
| `entryPointName` | `string` | Name of the entry point entity type (references `EntityTypeSchema.Name`). Auto-detected from entity types with Siren class `"EntryPoint"`, or empty if none found. |
| `entityTypes` | `EntityTypeSchema[]` | All entity types in the API. |
| `definitions` | `Dictionary<string, JsonSchema>` | Shared JSON Schema type definitions, referenced via `$ref` from property and parameter schemas. |

**Example (from CarShack):**

```json
{
  "schemaVersion": "1.0.0",
  "title": "CarShack API",
  "description": "RESTyard demo API for managing cars and customers",
  "entryPointName": "Entrypoint",
  "entityTypes": [ ... ],
  "definitions": { }
}
```

## Entity Type: `EntityTypeSchema`

Describes one type of Siren entity — its data shape and hypermedia connections.

| Field | Type | Description |
|---|---|---|
| `name` | `string` | Unique identifier for cross-references. Derived from the C# class name (stripping `Hypermedia` prefix and `Hto` suffix). |
| `classes` | `string[]` | Siren classes that identify this entity type at runtime. Used for wire-format matching. |
| `title` | `string?` | From `[HypermediaObject(Title)]`, `[Title]` attribute, or XML doc `<summary>`. |
| `description` | `string?` | From `[Description]` attribute or XML doc `<remarks>`. |
| `propertiesSchema` | `JsonSchema?` | JSON Schema describing the Siren `properties` bag. Null for entities with no data properties. |
| `links` | `LinkDescription[]` | Hypermedia links this entity type exposes. |
| `actions` | `ActionDescription[]` | Hypermedia actions available on this entity type. |
| `embeddedEntities` | `EmbeddedEntityDescription[]` | Embedded sub-entities. |
| `isDeprecated` | `bool` | `true` when the HTO class has `[Obsolete]`. |
| `deprecationMessage` | `string?` | The message from `[Obsolete("message")]`. |

**Example:**

```json
{
  "name": "Customer",
  "classes": ["Customer"],
  "propertiesSchema": {
    "type": "object",
    "properties": {
      "FullName": { "type": "string" },
      "Age": { "type": ["integer", "null"] },
      "IsFavorite": { "type": "boolean" }
    }
  },
  "links": [ ... ],
  "actions": [ ... ],
  "embeddedEntities": [],
  "isDeprecated": false
}
```

## Links: `LinkDescription`

Describes a hypermedia link from one entity type to another.

| Field | Type | Description |
|---|---|---|
| `relations` | `string[]` | Siren relation types from `[Relations]` (e.g., `["self"]`, `["bestFriend"]`). |
| `targetName` | `string` | Name of the target entity type (references another `EntityTypeSchema.Name`). |
| `targetClasses` | `string[]` | Siren classes of the target entity type. |
| `title` | `string?` | From `[Title]` attribute or XML doc `<summary>`. |
| `description` | `string?` | From `[Description]` attribute or XML doc `<remarks>`. |
| `isMandatory` | `bool` | `true` when the link property is non-nullable — always present on the entity. |
| `mediaType` | `string?` | Declared media type hint (e.g., `"text/html"` for external links). Null for standard Siren links. |
| `isDeprecated` | `bool` | `true` when the link property has `[Obsolete]`. |
| `deprecationMessage` | `string?` | The message from `[Obsolete("message")]`. |

**Example:**

```json
{
  "relations": ["PurchaseHistory"],
  "targetName": "CustomerPurchaseHistory",
  "targetClasses": ["CustomerPurchaseHistory"],
  "isMandatory": true,
  "isDeprecated": false
}
```

## Actions: `ActionDescription`

Describes a hypermedia action (state transition) on an entity type.

| Field | Type | Description |
|---|---|---|
| `name` | `string` | Action name from `[HypermediaAction(Name)]` or the property name. |
| `title` | `string?` | Human-readable title from `[HypermediaAction(Title)]`, `[Title]`, or XML doc `<summary>`. |
| `description` | `string?` | From `[Description]` attribute or XML doc `<remarks>`. |
| `parameterSchema` | `JsonSchema?` | JSON Schema for the action's parameter type. Null for parameterless actions. |
| `contentType` | `string?` | Content type for the action request (e.g., `"multipart/form-data"` for file uploads). |
| `isMandatory` | `bool` | `true` when the action property is non-nullable. |
| `isFileUpload` | `bool` | `true` for `FileUploadHypermediaAction`. |
| `isDeprecated` | `bool` | `true` when the action property has `[Obsolete]`. |
| `deprecationMessage` | `string?` | The message from `[Obsolete("message")]`. |

**Example:**

```json
{
  "name": "CustomerMove",
  "title": "A Customer moved to a new location.",
  "parameterSchema": {
    "type": "object",
    "properties": {
      "Address": {
        "type": "object",
        "properties": {
          "Street": { "type": "string" },
          "City": { "type": "string" },
          "ZipCode": { "type": "string" }
        }
      }
    }
  },
  "isMandatory": true,
  "isFileUpload": false,
  "isDeprecated": false
}
```

## Embedded Entities: `EmbeddedEntityDescription`

Describes an embedded sub-entity within a parent entity type.

| Field | Type | Description |
|---|---|---|
| `relations` | `string[]` | Siren relation types from `[Relations]`. |
| `targetName` | `string` | Name of the embedded entity type (references `EntityTypeSchema.Name`). |
| `targetClasses` | `string[]` | Siren classes of the embedded entity type. |
| `isCollection` | `bool` | `true` when the property is a `List<IEmbeddedEntity<T>>` (collection of embedded entities). |
| `isMandatory` | `bool` | `true` when the property is non-nullable. |
| `title` | `string?` | From `[Title]` attribute or XML doc `<summary>`. |
| `description` | `string?` | From `[Description]` attribute or XML doc `<remarks>`. |
| `isDeprecated` | `bool` | `true` when the property has `[Obsolete]`. |
| `deprecationMessage` | `string?` | The message from `[Obsolete("message")]`. |

## Data Shapes and Definitions

Entity properties (`propertiesSchema`) and action parameters (`parameterSchema`) use standard [JSON Schema (Draft 2020-12)](https://json-schema.org/draft/2020-12/json-schema-core). Complex nested types (e.g., `Address`, `Pagination`) are automatically extracted to local `$defs` within each schema and referenced via `$ref`.

### Self-Contained Schemas

Each `propertiesSchema` and `parameterSchema` is a **valid, self-contained JSON Schema** — all `$ref` references resolve within the same document via local `$defs`. You can validate or process any individual schema without needing external context.

```json
{
  "type": "object",
  "$defs": {
    "address": { "type": "object", "properties": { "Street": { "type": "string" }, "City": { "type": "string" } } }
  },
  "properties": {
    "Name": { "type": "string" },
    "HomeAddress": { "$ref": "#/$defs/address" }
  }
}
```

### Top-Level `definitions` Catalog

The same complex types also appear in the top-level `HypermediaApiSchema.definitions` as a **deduplicated catalog**. If `Address` appears in both `Customer.propertiesSchema` and `Order.propertiesSchema`, it's listed once in `definitions`.

This catalog is useful for **tooling**:
- **Client generators**: iterate `definitions` first to generate one shared class per definition, then generate entity-specific code referencing the shared types. This avoids generating duplicate `Address` classes.
- **Documentation tools**: render a "Shared Types" section listing each definition with its properties.

The duplication (definitions in both local `$defs` and top-level `definitions`) is intentional — individual schemas remain self-contained and valid, while the catalog provides deduplication for tools that need it.

### JSON Schema Generation

The JSON Schema is generated at runtime by `IJsonSchemaFactory`, which supports:
- `[Title]` / `[Description]` attributes → JSON Schema `title` / `description` keywords
- `[DisplayName]` / `[Description]` from `System.ComponentModel` → `title` / `description`
- `[Obsolete]` → JSON Schema `deprecated: true`
- Complex types → extracted to `$defs` with `$ref` (opt-out via `new JsonSchemaFactory(extractComplexTypesToDefs: false)`)
- Custom temporal type handling (`DateOnly`, `TimeOnly`, `DateTimeOffset`, `TimeSpan`)
- User-extensible via `IAttributeHandler` registration

## Entity Relationship Graph

The schema describes a graph of entity types connected by links, actions, and embedded entities. This graph can be visualized using the Mermaid API Map:

```mermaid
graph LR
    Entrypoint["Entrypoint"]
    CarsRoot["CarsRoot"]
    CustomersRoot["CustomersRoot"]
    Customer["Customer"]
    CustomerPurchaseHistory["CustomerPurchaseHistory"]
    CustomerPurchase["CustomerPurchase"]
    Car["Car"]

    Entrypoint -- "CustomersRoot" --> CustomersRoot
    Entrypoint -- "CarsRoot" --> CarsRoot
    CustomersRoot -- "BestCustomer" --> Customer
    Customer -- "PurchaseHistory" --> CustomerPurchaseHistory
    CustomerPurchaseHistory -- "Purchases" --> CustomerPurchase
```

## CLI Schema Generation

Generate schema artifacts from your server application:

```bash
# Generate all artifacts
dotnet run --project MyApi -- --generate-schema --schema-output ./docs

# Generate only JSON schema
dotnet run --project MyApi -- --generate-schema --schema-artifacts json-hypermedia-api-schema --schema-output ./docs

# Generate with raw Mermaid (no Markdown wrapping)
dotnet run --project MyApi -- --generate-schema --schema-output ./docs --mermaid-wrap-markdown false
```

### Available Artifacts

| CLI value | Output file | Description |
|---|---|---|
| `json-hypermedia-api-schema` | `hypermedia-api-schema.json` | Full schema as JSON |
| `mermaid-api-map` | `api-map.md` | Entity relationship graph (Mermaid) |
| `mermaid-htos` | `htos.md` | HTO class diagram with properties/actions (Mermaid) |
| `markdown-api-documentation` | `api-documentation.md` | Full Markdown API reference |

### Mapper Options

| CLI arg | Default | Description |
|---|---|---|
| `--mermaid-include-properties` | `true` | Include properties in HTO diagram |
| `--mermaid-include-actions` | `true` | Include actions in HTO diagram |
| `--mermaid-wrap-markdown` | `true` | Wrap Mermaid in Markdown with title and code fence |
| `--markdown-include-toc` | `true` | Include table of contents |
| `--markdown-include-diagram` | `true` | Include Mermaid diagram in Markdown |

## Schema Endpoint

Serve the schema at runtime via a minimal API endpoint:

```csharp
app.MapHypermediaSchema();
```

This maps a `GET /hypermedia-schema` endpoint that returns the full `HypermediaApiSchema` as JSON with content type `application/vnd.restyard.schema+json`.

**Custom route:**

```csharp
app.MapHypermediaSchema(o => o.Route = "/api/schema");
```

**Authorization:** Since `MapHypermediaSchema()` returns an `IEndpointConventionBuilder`, you can chain standard minimal API policies:

```csharp
app.MapHypermediaSchema().RequireAuthorization("AdminOnly");
```

Make sure `AddHypermediaSchema()` was called during service registration to enable the schema feature.

## Programmatic Access

### ASP.NET Core

```csharp
// Register schema services
builder.Services.AddHypermediaSchema(o =>
{
    o.Title = "My API";
    o.Description = "My hypermedia API";
    o.ApiVersion = "1.0.0";
});

// Generate schema on CLI request
var app = builder.Build();
if (app.GenerateSchemaIfRequested(args)) return;
app.Run();
```

### Tooling (no ASP.NET Core)

```csharp
var factory = new JsonSchemaFactory();
var schema = HypermediaSchemaBuilder.Build(factory, new HypermediaSchemaOptions
{
    Title = "My API",
    ApiVersion = "1.0.0",
});

// Generate files
HypermediaSchemaGenerator.Generate(schema, "./output", SchemaOutputFormats.All);

// Or serialize directly
var json = schema.ToJson();

// Deserialize from JSON
var loaded = HypermediaApiSchema.FromJson(json);
```
