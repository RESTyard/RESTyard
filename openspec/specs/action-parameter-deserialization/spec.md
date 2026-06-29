# action-parameter-deserialization Specification

## Purpose

Defines how RESTyard deserializes hypermedia action parameter request bodies and produces Siren output, standardizing on System.Text.Json.

## Requirements

### Requirement: Action parameter bodies are deserialized with System.Text.Json

RESTyard SHALL deserialize hypermedia action parameter request bodies using System.Text.Json. The server-side parameter-binding path MUST NOT depend on Newtonsoft.Json.

#### Scenario: Plain JSON object body bound to action parameter

- **WHEN** a client invokes a hypermedia action with a request body containing a plain JSON object representing the `IHypermediaActionParameter` type
- **THEN** the parameter is deserialized into the strongly-typed parameter object using System.Text.Json
- **AND** no Newtonsoft.Json type participates in the deserialization

#### Scenario: File-upload action parameter bound with System.Text.Json

- **WHEN** a client invokes a file-upload action (`HypermediaFileUploadActionParameter<T>`) submitting the parameter object as a form field
- **THEN** the form binder deserializes the parameter field using System.Text.Json
- **AND** the uploaded files are bound to the `Files` property

### Requirement: The custom body binder is removed

RESTyard SHALL NOT provide a custom model binder for non-file hypermedia action parameter bodies. `HypermediaParameterFromBodyBinderProvider` and `HypermediaParameterFromBodyBinder` MUST NOT exist, and the binder registration MUST NOT register a body binder. Non-file action parameter bodies bind through the standard framework body path.

#### Scenario: Action parameter binds via the standard framework path

- **WHEN** a controller action declares an `IHypermediaActionParameter` parameter (with or without `[FromBody]`)
- **THEN** the framework's System.Text.Json input formatter deserializes the body
- **AND** binding succeeds without any RESTyard-specific body binder being registered

#### Scenario: Legacy array-wrapper body is no longer unwrapped

- **WHEN** a client sends the legacy Siren array-wrapper body `[{ "TypeName": { … } }]`
- **THEN** the server does not perform RESTyard-specific unwrapping of that envelope
- **AND** the supported wire format for action parameter bodies is the plain JSON object

### Requirement: The from-body attribute aliases FromBody and is obsolete

`HypermediaActionParameterFromBodyAttribute` SHALL inherit from `FromBodyAttribute` so existing usages continue to compile and bind from the body, and it MUST be marked `[Obsolete]` directing callers to use `[FromBody]`.

#### Scenario: Existing attribute usage still binds from body

- **WHEN** a controller action parameter is annotated with `[HypermediaActionParameterFromBody]`
- **THEN** the parameter binds from the request body exactly as `[FromBody]` would
- **AND** the compiler emits an obsolete warning recommending `[FromBody]`

### Requirement: Custom JSON converters from DI apply to action parameter deserialization

Custom `JsonConverter`s registered through `Microsoft.AspNetCore.Http.Json.JsonOptions` (`ConfigureHttpJsonOptions`) SHALL be applied when deserializing hypermedia action parameters. This MUST hold uniformly for the RESTyard form binder, for controller `[FromBody]` action bodies, and for minimal-API action bodies. `Http.Json.JsonOptions` is the single source consumers configure; RESTyard SHALL bridge those converters into `Microsoft.AspNetCore.Mvc.JsonOptions` so the controller body path uses the same converters.

#### Scenario: Custom converter applied in the form binder

- **WHEN** a custom `JsonConverter` is registered via `ConfigureHttpJsonOptions`
- **AND** a file-upload action parameter contains a property handled by that converter
- **THEN** the RESTyard form binder resolves the `Http.Json.JsonOptions` from request services and uses the converter during deserialization

#### Scenario: Custom converter applied to controller [FromBody] body

- **WHEN** a custom `JsonConverter` is registered via `ConfigureHttpJsonOptions`
- **AND** a controller action binds an `IHypermediaActionParameter` body containing a property handled by that converter
- **THEN** the converter is applied because RESTyard bridges the registered converters into `Mvc.JsonOptions`

#### Scenario: Custom converter applied in minimal-API body binding

- **WHEN** a custom `JsonConverter` is registered via `ConfigureHttpJsonOptions`
- **AND** a minimal-API endpoint binds an `IHypermediaActionParameter` body containing a property handled by that converter
- **THEN** the converter is applied via the native minimal-API `Http.Json.JsonOptions`

### Requirement: Siren output conversion uses System.Text.Json

The reflection-based `SirenConverter` SHALL produce Siren JSON using System.Text.Json (`JsonObject`/`JsonArray`/`JsonNode`). It MUST NOT depend on Newtonsoft.Json. `IHypermediaJsonConverter.ConvertToJson` SHALL return a `System.Text.Json.Nodes.JsonObject`. The converter MUST preserve the existing Siren wire format, including flat string arrays for `class` and `rel` (e.g. `"rel": ["self"]`).

#### Scenario: Hypermedia object rendered to Siren without Newtonsoft

- **WHEN** an `IHypermediaObject` is converted by `SirenConverter`
- **THEN** the resulting Siren JSON is built with System.Text.Json types
- **AND** no Newtonsoft.Json type participates in the conversion

#### Scenario: rel and class render as flat string arrays

- **WHEN** a hypermedia object with links/classes is converted
- **THEN** `rel` and `class` are emitted as flat arrays of strings (not nested arrays)

### Requirement: Legacy array-wrapper client serializers are obsolete

The client-side `SingleNewtonsoftJsonObjectParameterSerializer` and `SingleSystemTextJsonObjectParameterSerializer`, together with their registration extension methods `WithSingleNewtonsoftJsonObjectParameterSerializer` and `WithSingleSystemTextJsonObjectParameterSerializer`, SHALL be marked `[Obsolete]`. The obsolete message MUST name the plain-object replacement (`WithNewtonsoftJsonObjectParameterSerializer` / `WithSystemTextJsonObjectParameterSerializer` respectively). They emit the legacy `[{ "TypeName": {…} }]` array-wrapper that the server no longer unwraps; the plain-object parameter serializers are the supported replacements.

#### Scenario: Obsolete warning on legacy serializers

- **WHEN** application code references `SingleNewtonsoftJsonObjectParameterSerializer` or `SingleSystemTextJsonObjectParameterSerializer`
- **THEN** the compiler emits an obsolete warning pointing to the plain-object serializer as the replacement

#### Scenario: Obsolete warning on legacy registration extension methods

- **WHEN** application code calls `WithSingleNewtonsoftJsonObjectParameterSerializer` or `WithSingleSystemTextJsonObjectParameterSerializer` on the resolver builder
- **THEN** the compiler emits an obsolete warning whose message names the plain-object registration method to use instead
