## Context

The formatter unit tests under `Source/RESTyard.AspNetCore.Test/WebApi/Formatter` exercise `SirenConverter` by calling `ConvertToJson(hmo)` and asserting against the resulting JSON DOM. They were written against Newtonsoft.Json (`JObject`/`JArray`/`JToken`/`JValue`/`JTokenType`). After the converter migration, `ConvertToJson` returns `System.Text.Json.Nodes.JsonObject`, so the tests no longer compile (~hundreds of type/cast errors across 8 files). The shared harness is `SirenBuilderTestBase.cs` (link/class/action/entity assertion helpers) and `Properties/PropertyHelpers.cs` (property-value comparisons); the rest build on them.

This is a test-only change. The production converter already preserves the Siren wire format; the work is translating assertions to the System.Text.Json DOM and accounting for value-rendering differences between the two libraries.

## Goals / Non-Goals

**Goals:**
- Restore `RESTyard.AspNetCore.Test` to a compiling, green state.
- Keep test intent and coverage identical — same things asserted, expressed against `System.Text.Json.Nodes`.
- Remove Newtonsoft.Json usage from the `WebApi/Formatter` test files.

**Non-Goals:**
- No production code changes.
- No removal of the `Newtonsoft.Json` package from the test project (other test files still use it, e.g. `JsonSchema/JsonSchemaGeneratorTest.cs`).
- No new test cases beyond what is needed to keep parity (and to compile).
- No changes to tests outside `WebApi/Formatter`.

## Decisions

### Decision 1: Migrate assertions to `System.Text.Json.Nodes`, not a Newtonsoft shim

Translate the assertions to operate on the `JsonObject`/`JsonArray`/`JsonNode`/`JsonValue` DOM returned by `ConvertToJson`.

- **Why:** The user asked to "use System.Text.Json". A shim (`JObject.Parse(converter.ConvertToString(...))`) would keep Newtonsoft in the tests and defeat the intent.
- **Alternative considered:** Parse the converter's string output back into Newtonsoft `JObject` to leave assertions untouched — rejected; keeps the Newtonsoft dependency and hides the new return type.

### Decision 2: Type/idiom translation table

Apply consistently across all files:

| Newtonsoft | System.Text.Json.Nodes |
|---|---|
| `JObject` | `JsonObject` |
| `JArray` | `JsonArray` |
| `JToken` | `JsonNode` |
| `JValue` | `JsonValue` |
| `x.Type == JTokenType.Array` | `x is JsonArray` |
| `x.Type == JTokenType.Object` | `x is JsonObject` |
| `x.Type == JTokenType.Null` | `x is null` |
| `(JArray)x` / `(JObject)x` | `x!.AsArray()` / `x!.AsObject()` |
| `x.First` | `x!.AsArray()[0]` (or `.AsArray().First()`) |
| `x.Value<string>()` / `((JValue)x).Value<string>()` | `x!.GetValue<string>()` |
| `(float)x` / `(double)x` | `x!.GetValue<float>()` / `x!.GetValue<double>()` |
| `arr.Values<string>()` | `arr.Select(n => n!.GetValue<string>())` |
| `obj.Properties().Count()` | `obj.Count` |
| `obj.Properties()` (iterate name/value) | iterate `obj` as `KeyValuePair<string, JsonNode?>` |
| `obj["x"]` (missing → null) | `obj["x"]` (missing → null; same) |

### Decision 3: Compare typed values, not raw `.ToString()`, where rendering differs

Newtonsoft and System.Text.Json render some scalars differently in `.ToString()`:
- `bool`: Newtonsoft `JValue(true).ToString()` → `"True"`; System.Text.Json `JsonValue` → `"true"`. Assert via `GetValue<bool>()` against the expected `bool`.
- `DateTime`/`DateTimeOffset`: assert via `GetValue<DateTime>()`/`GetValue<DateTimeOffset>()` (or compare the emitted ISO string) rather than `(IFormattable)token`.
- numbers: assert via `GetValue<int/long/float/double/decimal>()`.

Where the existing test compared `expected.ToString()` against `token.ToString()`, switch to comparing the strongly-typed value (`token.GetValue<T>()`) against the expected value. This keeps the assertion semantically identical and robust to renderer differences. The converter's own value formatting (enum member names, `DateOnly` `yyyy-MM-dd`, `TimeOnly` `HH:mm:ss`, `Uri` as string, `Type` as `FullName`) is unchanged, so those assertions stay as string comparisons.

### Decision 4: Migrate the shared harness first

Port `SirenBuilderTestBase.cs` and `Properties/PropertyHelpers.cs` first (their helper signatures take `JObject`/`JArray`), then the per-feature files that call them. This makes the per-file edits mostly signature-driven.

## Risks / Trade-offs

- **Hidden value-format mismatches** (a green-looking translation that changes what's asserted) → Mitigation: prefer `GetValue<T>()` typed comparisons; run the suite and reconcile failures against the converter's documented formatting.
- **`JsonNode` single-parent constraint** is irrelevant for read-only assertions but avoid re-inserting fetched nodes.
- **`GetValue<T>()` numeric exactness** (e.g. `float` vs `double` backing) → use the same type the property declares.
- **Overload/extension ambiguity** when indexing (`JsonNode?` indexer throws if the node is not an object/array) → guard with `is JsonObject`/`is JsonArray` before indexing, mirroring the existing `JTokenType` guards.

## Migration Plan

1. Port `SirenBuilderTestBase.cs` (helpers + `SirenConverter` field already System.Text.Json).
2. Port `Properties/PropertyHelpers.cs`.
3. Port `Properties/Htos.cs` only if it references Newtonsoft (otherwise leave).
4. Port the three top-level test files and the three property test files.
5. Build `RESTyard.AspNetCore.Test`; fix residual errors.
6. Run the formatter tests; reconcile value-format failures per Decision 3.

**Rollback:** revert the test-file edits; no production impact.

## Open Questions

- None. Scope and approach are determined by the user's instruction and the existing converter behavior.
