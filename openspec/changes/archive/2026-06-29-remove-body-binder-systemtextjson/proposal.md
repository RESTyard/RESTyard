## Why

RESTyard still deserializes hypermedia action parameters with a bespoke Newtonsoft.Json model binder (`HypermediaParameterFromBodyBinder`) that exists only to unwrap the legacy Siren `[{ "TypeName": {…} }]` array-wrapper format. The hypermedia-schema work makes action parameters plain JSON objects, so the custom body binder — and the client serializers that produce the wrapper — are now dead weight. Keeping them also forces a Newtonsoft.Json dependency into the request-binding path, which conflicts with the goal of standardizing on System.Text.Json and supporting both MVC controllers and minimal APIs uniformly.

## What Changes

- **BREAKING** Remove `HypermediaParameterFromBodyBinderProvider` and `HypermediaParameterFromBodyBinder` entirely. Hypermedia action parameter bodies (`IHypermediaActionParameter`) now bind through the standard framework body path (System.Text.Json input formatter / minimal-API body binding).
- **BREAKING** `HypermediaActionParameterFromBodyAttribute` no longer references the removed binder. It is repointed to inherit `FromBodyAttribute` (acting as a plain `[FromBody]` alias) and marked `[Obsolete]` to guide callers toward `[FromBody]`. Existing usages keep compiling.
- Stop registering the body binder in `AddHypermediaParameterBinders`; only the form binder provider remains registered.
- **BREAKING** Mark the client-side `SingleNewtonsoftJsonObjectParameterSerializer` and `SingleSystemTextJsonObjectParameterSerializer` as `[Obsolete]`, plus their registration extension methods `WithSingleNewtonsoftJsonObjectParameterSerializer` and `WithSingleSystemTextJsonObjectParameterSerializer`. Each obsolete message points to the plain-object replacement (`WithNewtonsoftJsonObjectParameterSerializer` / `WithSystemTextJsonObjectParameterSerializer`). They emit the legacy array-wrapper shape that the server no longer needs; the plain-object serializers are the supported path.
- Convert `HypermediaParameterFromFormBinderProvider` / `HypermediaParameterFromFormBinder` (file-upload actions — **not** obsolete) from Newtonsoft.Json to System.Text.Json, removing its Newtonsoft dependency.
- Convert the public `JsonDeserializer` (`RESTyard.AspNetCore.JsonSchema`) from Newtonsoft.Json to System.Text.Json.
- Restore the reflection-based `SirenConverter` (and its `IHypermediaJsonConverter` interface) from git history and convert it from Newtonsoft.Json (`JObject`/`JArray`/`JToken`/`JValue`) to System.Text.Json (`JsonObject`/`JsonArray`/`JsonNode`/`JsonValue`). Re-wire `SirenHypermediaConverterFactory` to construct it. This removes Newtonsoft.Json from the Siren **output** path as well, completing Newtonsoft removal from `RESTyard.AspNetCore`. The unused, Newtonsoft-only `JObjectExtensions` and `SingleParameterBinder` (dead remnants of the removed body-binding path) stay deleted.
- Ensure custom `JsonConverter`s registered in DI are applied by the parameter deserializers. `Microsoft.AspNetCore.Http.Json.JsonOptions` (`ConfigureHttpJsonOptions`) is the single modern source of converters; the form binder and `JsonDeserializer` resolve it from `RequestServices`, and a RESTyard startup step bridges those converters into `Microsoft.AspNetCore.Mvc.JsonOptions` so controller `[FromBody]` action bodies are deserialized with the same converters.

## Capabilities

### New Capabilities
- `action-parameter-deserialization`: How RESTyard deserializes hypermedia action and file-upload action parameters from request bodies — the serializer technology (System.Text.Json), the supported wire format (plain JSON objects), where custom JSON converters come from, and how they apply uniformly across MVC controllers and minimal APIs.

### Modified Capabilities
<!-- No existing specs in openspec/specs/; nothing to modify. -->

## Impact

- **Affected code (server, `RESTyard.AspNetCore`)**:
  - `JsonSchema/HypermediaParameterFromBodyBinder.cs` — deleted (provider + binder).
  - `WebApi/HypermediaActionParameterFromBodyAttribute.cs` — rebased on `FromBodyAttribute`, marked `[Obsolete]`.
  - `JsonSchema/HypermediaParameterFromFormBinder.cs` — ported to System.Text.Json.
  - `JsonSchema/JsonDeserializer.cs` — ported to System.Text.Json, options sourced from DI.
  - `WebApi/ExtensionMethods/StartupExtensions.cs` — drop body-binder registration; add converter bridge from `Http.Json.JsonOptions` into `Mvc.JsonOptions`.
  - `WebApi/Formatter/SirenConverter.cs` + `WebApi/Formatter/IHypermediaJsonConverter.cs` — restored from git history and ported to System.Text.Json (`IHypermediaJsonConverter.ConvertToJson` now returns `JsonObject`).
  - `WebApi/Formatter/SirenHypermediaConverterFactory.cs` — re-wired to construct the restored `SirenConverter`.
- **Affected tests**: `RESTyard.AspNetCore.Test/WebApi/Formatter/SirenBuilder*` assert against the converter output via Newtonsoft `JObject`/`JArray`; they need migrating to System.Text.Json (`JsonObject`/`JsonNode`) assertions. The `JsonDeserializer` deserialization test must pass `JsonSerializerOptions`.
- **Affected code (client, `RESTyard.Client.Extensions`)**:
  - `NewtonsoftJson/SingleNewtonsoftJsonObjectParameterSerializer.cs` and `SystemTextJson/SingleSystemTextJsonObjectParameterSerializer.cs` — marked `[Obsolete]`.
  - `NewtonsoftJson/NewtonsoftJsonExtensions.cs` (`WithSingleNewtonsoftJsonObjectParameterSerializer`) and `SystemTextJson/SystemTextJsonExtensions.cs` (`WithSingleSystemTextJsonObjectParameterSerializer`) — registration extension methods marked `[Obsolete]` with a hint pointing to the plain-object equivalents.
- **Dependencies**: Removes Newtonsoft.Json usage from the server-side parameter-binding path (form binder + `JsonDeserializer`).
- **Public API / breaking changes**: Removal of the (internal) body binder types; `[Obsolete]` warnings on the action-parameter-from-body attribute and the two `Single*` client serializers. Clients still emitting the array-wrapper format will continue to work only if a host still accepts it — the server-side unwrapping is gone, so the wire contract for action bodies becomes the plain object.
- **Docs/samples**: CarShack sample (`CustomerController`) and the migration guide need updating to the plain `[FromBody]` / plain-object convention.
