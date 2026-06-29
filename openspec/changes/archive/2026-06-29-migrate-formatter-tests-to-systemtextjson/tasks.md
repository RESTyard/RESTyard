## 1. Migrate the shared harness

- [x] 1.1 Port `WebApi/Formatter/SirenBuilderTestBase.cs`: change helper signatures and bodies from `JObject`/`JArray`/`JToken`/`JValue`/`JTokenType` to `JsonObject`/`JsonArray`/`JsonNode`/`JsonValue`; replace `JTokenType` guards with `is JsonObject`/`is JsonArray`/`is null`; replace `.First`, `.Value<string>()`, `.Values<string>()` per the translation table; swap `using Newtonsoft.Json.Linq;` for `using System.Text.Json.Nodes;`.
- [x] 1.2 Port `WebApi/Formatter/Properties/PropertyHelpers.cs`: migrate `GetPropertiesJObject`, `CompareHypermediaPropertiesAndJson`, `CompareNotNullProperties`, `CompareHypermediaPropertiesAndJsonNoNullProperties`, `CompareHypermediaListPropertiesAndJson`. Use `GetValue<T>()` typed comparisons for `bool`, numeric, `DateTime`/`DateTimeOffset`; keep enum/`Uri`/`Type`/`DateOnly`/`TimeOnly` as string comparisons (converter formatting unchanged). Replace `.Properties().Count()` with `.Count` and `JTokenType.Null`/`JTokenType.Array` checks with `is null`/`is JsonArray`.
- [x] 1.3 Check `WebApi/Formatter/Properties/Htos.cs` and `WebApi/Formatter/FakeLinkGenerator.cs` for Newtonsoft usage; migrate only if present.

## 2. Migrate the top-level formatter tests

- [x] 2.1 Port `WebApi/Formatter/SirenBuilderActionsTest.cs` to System.Text.Json assertions.
- [x] 2.2 Port `WebApi/Formatter/SirenBuilderEntitiesTest.cs` to System.Text.Json assertions.
- [x] 2.3 Port `WebApi/Formatter/SirenBuilderLinksTest.cs` to System.Text.Json assertions.

## 3. Migrate the property tests

- [x] 3.1 Port `WebApi/Formatter/Properties/SirenBuilderPropertiesTest.cs`.
- [x] 3.2 Port `WebApi/Formatter/Properties/SirenBuilderListPropertiesTest.cs`.
- [x] 3.3 Port `WebApi/Formatter/Properties/SirenBuilderObjectPropertiesTest.cs`.

## 4. Build and verify

- [x] 4.1 Build `RESTyard.AspNetCore.Test` (both target frameworks) and resolve all compile errors.
- [x] 4.2 Confirm no `Newtonsoft` references remain under `WebApi/Formatter` (`grep -rn Newtonsoft Source/RESTyard.AspNetCore.Test/WebApi/Formatter`).
- [x] 4.3 Run the formatter tests (`--filter "FullyQualifiedName~WebApi.Formatter"`) and reconcile any value-format failures per the design's typed-comparison guidance.
- [x] 4.4 Run the full `RESTyard.AspNetCore.Test` suite and confirm green (no regressions in other tests).

## 5. Validate

- [x] 5.1 Run `openspec validate migrate-formatter-tests-to-systemtextjson` and resolve any issues.
