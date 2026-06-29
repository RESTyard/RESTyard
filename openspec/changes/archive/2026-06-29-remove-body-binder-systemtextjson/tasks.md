## 1. Port JsonDeserializer to System.Text.Json

- [x] 1.1 Rewrite `JsonSchema/JsonDeserializer.cs` to use System.Text.Json (`JsonSerializer.Deserialize`), removing all `Newtonsoft.Json` / `Newtonsoft.Json.Linq` usings.
- [x] 1.2 Change its API to accept a `JsonSerializerOptions` (so DI-registered converters flow through) for both the stream and parsed-node deserialize paths.
- [x] 1.3 Verify no other callers of `JsonDeserializer` break (search the solution); adjust signatures/usages as needed.

## 2. Port the form binder to System.Text.Json

- [x] 2.1 Rewrite `JsonSchema/HypermediaParameterFromFormBinder.cs` to parse the form parameter field with System.Text.Json instead of `JsonConvert`/`JObject`; remove Newtonsoft usings.
- [x] 2.2 Resolve `IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>` from `bindingContext.HttpContext.RequestServices` and pass `SerializerOptions` into `JsonDeserializer`.
- [x] 2.3 Preserve existing behavior: model-type/HTTP-method/form-content checks, file collection binding, and the no-parameter (files-only) case.
- [x] 2.4 Decide and implement handling of a legacy array-wrapper value in the form field (keep tolerant unwrap or drop it per the plain-object contract); document the choice inline.

## 3. Remove the body binder

- [x] 3.1 Delete `JsonSchema/HypermediaParameterFromBodyBinder.cs` (both `HypermediaParameterFromBodyBinderProvider` and `HypermediaParameterFromBodyBinder`).
- [x] 3.2 In `WebApi/ExtensionMethods/StartupExtensions.cs` `AddHypermediaParameterBinders`, remove the body-binder registration; keep only `HypermediaParameterFromFormBinderProvider`.
- [x] 3.3 Rebase `WebApi/HypermediaActionParameterFromBodyAttribute.cs` on `FromBodyAttribute` (remove the `BinderType` reference) and add `[Obsolete("Use [FromBody] instead.")]`.

## 4. Bridge DI converters across MVC and minimal APIs

- [x] 4.1 Add a startup step that copies converters from `Microsoft.AspNetCore.Http.Json.JsonOptions` into `Microsoft.AspNetCore.Mvc.JsonOptions.JsonSerializerOptions.Converters`, ordered so user-registered converters are present before use.
- [x] 4.2 Ensure the controller `[FromBody]` path and minimal-API path both resolve to the same converter set; document the single configuration entry point (`ConfigureHttpJsonOptions`).

## 5. Mark client serializers obsolete

- [x] 5.1 Mark `SingleNewtonsoftJsonObjectParameterSerializer` `[Obsolete]` with a message pointing to `NewtonsoftJsonObjectParameterSerializer`.
- [x] 5.2 Mark `SingleSystemTextJsonObjectParameterSerializer` `[Obsolete]` with a message pointing to `SystemTextJsonObjectParameterSerializer`.
- [x] 5.3 Mark `WithSingleNewtonsoftJsonObjectParameterSerializer` `[Obsolete]` with a hint to use `WithNewtonsoftJsonObjectParameterSerializer`.
- [x] 5.4 Mark `WithSingleSystemTextJsonObjectParameterSerializer` `[Obsolete]` with a hint to use `WithSystemTextJsonObjectParameterSerializer`.

## 6. Restore and migrate SirenConverter to System.Text.Json

- [x] 6.0a Restore `WebApi/Formatter/SirenConverter.cs` and `WebApi/Formatter/IHypermediaJsonConverter.cs` from git history; confirm unused Newtonsoft-only `JObjectExtensions` and `SingleParameterBinder` stay deleted.
- [x] 6.0b Port `SirenConverter` to System.Text.Json (`JObject`→`JsonObject`, `JArray`→`JsonArray`, `JToken`→`JsonNode`, `JValue`→`JsonValue`); preserve flat `class`/`rel` string arrays.
- [x] 6.0c Change `IHypermediaJsonConverter.ConvertToJson` to return `JsonObject`.
- [x] 6.0d Re-wire `SirenHypermediaConverterFactory.CreateSirenConverter` to construct the restored converter.
- [ ] 6.0e Migrate `RESTyard.AspNetCore.Test/WebApi/Formatter/SirenBuilder*` assertions from Newtonsoft `JObject`/`JArray` to System.Text.Json (`JsonObject`/`JsonNode`). (Blocked/coordination: concurrent in-flight rework of these test files by the repo owner.)

## 7. Update samples, docs, and tests

- [x] 7.1 Update CarShack `CustomerController.MarkAsFavoriteAction` (and any other `[HypermediaActionParameterFromBody]` usage) to plain `[FromBody]`. Also updated the ContractFirst generator template (`v4.sbn`) and its verified snapshot to emit `[FromBody]`.
- [x] 7.2 Update the migration guide to describe the plain-object action body contract and the obsolete attribute/serializers/extension methods.
- [x] 7.3 Add/extend integration tests. Added `ParameterConverterTests` (via `WithWebHostBuilder` + `ConfigureTestServices`) proving a converter registered through `ConfigureHttpJsonOptions` applies to (a) the file-upload form binder and (b) the controller `[FromBody]` path (bridged). Minimal-API context (c) skipped per decision (no endpoint). Existing `IntegrationTests` already cover plain-object body binding + file upload. Also registered `JsonStringEnumConverter` in CarShack to restore enum-as-string parity (Newtonsoft used to parse enum names; STJ needs the converter) — fixed the `CallAction_CreateQuery` regression.
- [x] 7.4 Build and confirm no remaining Newtonsoft.Json references exist in `RESTyard.AspNetCore` (production). All production projects (`RESTyard.AspNetCore`, `CarShack`) and `RESTyard.HtoSourceGenerators.Test` build with 0 errors. Fixed the `JsonDeserializer` deserialization test to pass `JsonSerializerOptions`.

## 8. Validate

- [x] 8.1 Run `openspec validate --change remove-body-binder-systemtextjson` and resolve any issues.
- [x] 8.2 Run the runnable test suites and confirm green: `RESTyard.Integration.Test` 25/25 (incl. the 2 new converter tests) and `RESTyard.Client.Extensions/Extensions.Test` 8/8. The full `RESTyard.AspNetCore.Test` project remains non-compiling pending 6.0e (formatter tests on Newtonsoft assertions) — owned by the repo owner.
