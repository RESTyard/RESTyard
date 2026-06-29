## Context

Hypermedia action parameters are currently bound by two custom MVC model binders living in `RESTyard.AspNetCore.JsonSchema`:

- `HypermediaParameterFromBodyBinder(Provider)` — for non-file `IHypermediaActionParameter` bodies. It reads the raw body with Newtonsoft.Json, unwraps the legacy Siren array-wrapper `[{ "TypeName": {…} }]`, then deserializes via `JsonDeserializer`.
- `HypermediaParameterFromFormBinder(Provider)` — for `HypermediaFileUploadActionParameter<T>` (multipart file uploads). It pulls the parameter object out of a form field, unwraps the same array-wrapper, and deserializes via `JsonDeserializer`.

Both share the public `JsonDeserializer`, which today is Newtonsoft-based (`JObject.ToObject(type)`). On the client side, `SingleNewtonsoftJsonObjectParameterSerializer` and `SingleSystemTextJsonObjectParameterSerializer` produce the array-wrapper shape that the server unwraps.

With the hypermedia-schema work, action parameters are described and transmitted as plain JSON objects. The array-wrapper round-trip (and the Newtonsoft dependency it drags into the binding path) is legacy. The body binder no longer earns its keep: the framework's standard System.Text.Json body path can deserialize plain objects. The form binder must stay (multipart uploads still need custom handling) but should move to System.Text.Json. Custom converters must apply uniformly across MVC controllers and minimal APIs.

Registration is centralized in `StartupExtensions.AddHypermediaParameterBinders`, which inserts both providers at indices 0 and 1.

## Goals / Non-Goals

**Goals:**
- Remove `HypermediaParameterFromBodyBinderProvider` / `HypermediaParameterFromBodyBinder` entirely and stop registering a body binder.
- Keep non-file action-parameter bodies binding correctly via the standard framework body path.
- Keep `HypermediaActionParameterFromBodyAttribute` source-compatible by rebasing it on `FromBodyAttribute` and marking it `[Obsolete]`.
- Port `HypermediaParameterFromFormBinder` and `JsonDeserializer` to System.Text.Json; remove Newtonsoft.Json from the parameter-binding path.
- Apply DI-registered custom `JsonConverter`s uniformly to the form binder, controller `[FromBody]` bodies, and minimal-API bodies, configured from a single source (`Http.Json.JsonOptions`).
- Mark the two `Single*` client serializers `[Obsolete]`.

**Non-Goals:**
- Removing or obsoleting the non-`Single` client serializers (plain-object serializers) — they are the supported path.
- Removing Newtonsoft.Json from unrelated areas of the codebase (e.g. any Siren output paths that still use it).
- Changing the file-upload wire contract beyond the serializer technology.
- Designing a new schema format; that is the surrounding feature, not this change.

## Decisions

### Decision 1: Remove the body binder; rely on the framework body path

Delete `HypermediaParameterFromBodyBinder.cs` (both the provider and the binder). In `AddHypermediaParameterBinders`, register only `HypermediaParameterFromFormBinderProvider`. Non-file `IHypermediaActionParameter` parameters then bind through MVC's System.Text.Json input formatter (controllers) or minimal-API body binding.

- **Why:** The binder's only real job was unwrapping the legacy array-wrapper and routing through a Newtonsoft deserializer. With plain-object bodies, the framework already does this correctly.
- **Alternative considered:** Keep the binder but port it to System.Text.Json. Rejected — it would preserve a redundant binder and the legacy unwrapping behavior the schema work makes obsolete.

### Decision 2: Repoint `HypermediaActionParameterFromBodyAttribute` to `FromBodyAttribute`, mark `[Obsolete]`

The attribute currently sets `BinderType = typeof(HypermediaParameterFromBodyBinder)`, which won't compile once the binder is gone. Change its base class from `ModelBinderAttribute` to `FromBodyAttribute` and add `[Obsolete("Use [FromBody] instead.")]`.

- **Why:** Keeps every existing `[HypermediaActionParameterFromBody]` call site compiling and binding from the body, while steering users to `[FromBody]`. Avoids a hard, mechanical breaking change across consumer codebases.
- **Alternative considered:** Delete the attribute outright. Rejected as unnecessarily disruptive; the obsolete alias gives a clean migration window. (Per the proposal it remains a BREAKING change in the sense of an obsolete warning + behavioral simplification.)

### Decision 3: `Http.Json.JsonOptions` is the single converter source; bridge into `Mvc.JsonOptions`

There is no built-in JSON options object shared by MVC controllers and minimal APIs:
- `Microsoft.AspNetCore.Mvc.JsonOptions` drives controller input/output formatters and `[FromBody]`.
- `Microsoft.AspNetCore.Http.Json.JsonOptions` drives minimal-API body binding and `Results.Json`/`TypedResults`.

Treat `Http.Json.JsonOptions` (configured via `ConfigureHttpJsonOptions`) as the single, modern source consumers configure. Then:
- The form binder and `JsonDeserializer` resolve `IOptions<Http.Json.JsonOptions>` from `bindingContext.HttpContext.RequestServices` and deserialize with `options.SerializerOptions`.
- A RESTyard startup step bridges the converters registered in `Http.Json.JsonOptions` into `Mvc.JsonOptions.JsonSerializerOptions.Converters` so controller `[FromBody]` action bodies use the same converters.
- Minimal-API bodies already use `Http.Json.JsonOptions` natively.

- **Why:** Gives one configuration surface that behaves identically across hosting styles, using the modern minimal-API-native options while still covering traditional controllers via the bridge.
- **Alternatives considered:**
  - *RESTyard helper writes to both option types* — symmetric but adds a RESTyard-owned converter-registration API and a second place to think about.
  - *Read context-appropriate options, no bridge* — simplest in RESTyard but requires consumers to know which options object their host reads, defeating the "works the same way" goal.

### Decision 4: `JsonDeserializer` takes `JsonSerializerOptions`

`JsonDeserializer` becomes System.Text.Json-based. Its `Deserialize` operations accept (or are constructed with) the resolved `JsonSerializerOptions` so the converters flow through. It deserializes a parsed `JsonElement`/`JsonNode` or a stream into the target type via `JsonSerializer.Deserialize(..., type, options)`.

- **Why:** Converters must be applied at the deserialization call. Passing options in (rather than capturing a static default) is what lets DI-registered converters take effect.
- **Note:** Callers resolve options from `RequestServices` at bind time, not at provider-construction time, because options/DI are request-scoped relative to the binder's lifetime.

## Risks / Trade-offs

- **Newtonsoft vs System.Text.Json deserialization differences** (casing, default-value handling, polymorphism, missing-member behavior) → Mitigation: rely on the configured `Http.Json.JsonOptions` (typically camelCase, case-insensitive) and cover with integration tests for representative action parameters, including the `Uri`/`KeyFromUri` style parameters and the file-upload parameter object. **Confirmed during implementation:** enum-valued parameters sent as string names (e.g. `"PropertyName": "Age"`) broke, because Newtonsoft parsed enum names automatically but STJ requires `JsonStringEnumConverter`. Resolved by registering it via `ConfigureHttpJsonOptions` (documented in the migration guide; applied in the CarShack sample). This is the most likely real-world parity gap.
- **Converters silently not applied on the controller path** if the bridge step is missing or runs in the wrong order → Mitigation: implement the bridge in the same startup configuration that registers the binders, and add a test asserting a custom converter applies to a controller `[FromBody]` action body.
- **Legacy clients still sending the array-wrapper break** since the server no longer unwraps → Mitigation: documented as BREAKING in the proposal and migration guide; clients must use plain-object serializers.
- **Obsolete attribute behavior change** (`HypermediaActionParameterFromBody` now behaves exactly like `[FromBody]`) could surprise code relying on the old unwrapping → Mitigation: covered by the obsolete message and migration guide.
- **Bridge timing/ordering with user configuration of `ConfigureHttpJsonOptions`** → Mitigation: bridge by reading the resolved `Http.Json.JsonOptions` at the point converters are needed, or copy at options post-configuration, so user-registered converters are present before use.

## Migration Plan

1. Port `JsonDeserializer` to System.Text.Json (options-driven).
2. Port `HypermediaParameterFromFormBinder` to System.Text.Json, resolving options from `RequestServices`; remove its Newtonsoft usings.
3. Delete `HypermediaParameterFromBodyBinder.cs`; update `AddHypermediaParameterBinders` to register only the form binder.
4. Rebase `HypermediaActionParameterFromBodyAttribute` on `FromBodyAttribute`, mark `[Obsolete]`.
5. Add the converter bridge from `Http.Json.JsonOptions` into `Mvc.JsonOptions` in startup.
6. Mark the two `Single*` client serializers `[Obsolete]`, and their registration extension methods (`WithSingleNewtonsoftJsonObjectParameterSerializer`, `WithSingleSystemTextJsonObjectParameterSerializer`) `[Obsolete]` with a message naming the plain-object replacement method.
7. Update CarShack sample and migration guide; update/extend integration tests.

**Rollback:** Revert the change set; the deleted binder and Newtonsoft-based deserializer are restored from git history. No data migration is involved.

## Open Questions

- Should the non-`Single` client serializers' default formatting/options also be reviewed for parity with the server's `Http.Json.JsonOptions`, or is that out of scope here?

**Resolved:**
- ~~Does any existing consumer rely on the body binder's explicit HTTP-method validation (POST/PUT/PATCH only)?~~ Out of scope / irrelevant — routing already gates the verbs; the framework body path needs no RESTyard-specific method check.
