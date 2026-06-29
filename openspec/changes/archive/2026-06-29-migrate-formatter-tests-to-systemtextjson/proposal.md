## Why

`SirenConverter` and `IHypermediaJsonConverter.ConvertToJson` were migrated to System.Text.Json (`ConvertToJson` now returns `System.Text.Json.Nodes.JsonObject`). The formatter unit tests in `Source/RESTyard.AspNetCore.Test/WebApi/Formatter` still assert against the old Newtonsoft.Json (`JObject`/`JArray`/`JToken`/`JValue`/`JTokenType`) API, so `RESTyard.AspNetCore.Test` no longer compiles. Migrating these tests to System.Text.Json restores the project build and re-establishes verification of the Siren output converter.

## What Changes

- Migrate all formatter test files and their shared harness in `Source/RESTyard.AspNetCore.Test/WebApi/Formatter` from Newtonsoft.Json to System.Text.Json (`System.Text.Json.Nodes`):
  - `SirenBuilderTestBase.cs` and `Properties/PropertyHelpers.cs` (shared assertion helpers).
  - `SirenBuilderActionsTest.cs`, `SirenBuilderEntitiesTest.cs`, `SirenBuilderLinksTest.cs`.
  - `Properties/SirenBuilderPropertiesTest.cs`, `Properties/SirenBuilderListPropertiesTest.cs`, `Properties/SirenBuilderObjectPropertiesTest.cs`.
- Replace Newtonsoft types/idioms with System.Text.Json equivalents: `JObject`→`JsonObject`, `JArray`→`JsonArray`, `JToken`→`JsonNode`, `JValue`→`JsonValue`; `JTokenType.X` checks → `is JsonObject`/`is JsonArray`/`is null`; `.Value<T>()`/casts → `GetValue<T>()`; `.Properties().Count()` → `.Count`; `.Values<string>()` → element enumeration.
- Adjust assertions for System.Text.Json value-formatting differences (semantic comparisons rather than `.ToString()` string matches): notably `bool` (`"True"` vs `"true"`), `DateTime`/`DateTimeOffset`, and numeric types.
- No production code changes. The `Newtonsoft.Json` package reference for `RESTyard.AspNetCore.Test` stays (other test files in the project still use it).

## Capabilities

### New Capabilities
<!-- None: test-only change. -->

### Modified Capabilities
- `action-parameter-deserialization`: No behavioral change; the existing "Siren output conversion uses System.Text.Json" requirement gains a scenario asserting that its verifying unit tests use System.Text.Json.

## Impact

- **Affected tests**: `Source/RESTyard.AspNetCore.Test/WebApi/Formatter/**` (8 files).
- **Build**: restores `RESTyard.AspNetCore.Test` to a compiling, runnable state.
- **No production impact**: no changes to `RESTyard.AspNetCore` or any shipped code; no dependency changes.
- **Risk**: value-formatting parity (bool casing, date formatting, number rendering) between the old Newtonsoft assertions and System.Text.Json; mitigated by comparing typed values via `GetValue<T>()` instead of raw `.ToString()`.
