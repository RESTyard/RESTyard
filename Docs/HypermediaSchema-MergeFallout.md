# `feature/hypermedia-schema` — Merge fallout TODOs (from merging `develop` into the branch, 2026-09-22)

Items discovered while resolving the Step 0 merge that are **not** fixed by the merge commit itself.
Semantic fallout that blocks the build is tracked in the Step 0 table of [`HypermediaSchema-MergeCandidates.md`](HypermediaSchema-MergeCandidates.md); this list is what remains.

**Code — branch**

- [x] `HypermediaActionEndpoint.ResultType` XML doc says "produces a `Location` header" — extend to inline
  results (`InlineQueryResult`, #131). Relates to Plan 8.6 (Location-header helper).
- [x] `server/csharp-controller/v4.sbn` was re-added by accident in `f0a3d2a` (#131 deleted it) — `git rm` it.
  *Cause: modify/delete conflict left the branch copy on disk as untracked; the next commit picked it up.*
- [ ] Represent the HTTP `QUERY` action with inline (embedded) result in the hypermedia schema (#131).
  Today `ActionDescription` only has `resultName`, implicitly meaning "`Location` points to it".
  Design decided, see [Design: action result delivery](#design-action-result-delivery). Timing open
  (before S1 recommended: it changes the S1 model).
- [x] `HypermediaQueryResult` base type removed (#131); generated query-result HTOs now carry `Query`
  themselves — verify the source generator's property classification (schema data properties,
  Properties POCO, `ToSiren()`) handles `Query` correctly. *Verified by inspection: CarShack's
  generated `HypermediaCustomerQueryResultHtoProperties` excludes `Query` (`[FormatterIgnoreHypermediaProperty]`)
  and `HtoTitle`. No dedicated test.*
- [x] Target frameworks: #131 bumped all projects net8 → net10. Align the branch-added projects
  (`RESTyard.Schema`, `RESTyard.HtoSourceGenerators*`, `RESTyard.Schema.Test`) — keep `netstandard2.0`
  where required (generator, analyzers). *Done in `5aaba94`.*
- [x] `HypermediaExtensionsOptions.ImplicitHypermediaActionParameterBinders` XML doc (~line 40) still describes
  the removed body binder (`HypermediaActionParameterFromBodyAttribute`, `KeyFromUri`). The option itself is
  still live — it switches the form binder between implicit and attributed-only (`StartupExtensions.cs:176`) —
  so keep it and fix the doc only. *Also fixed the same text on `AddHypermediaParameterBinders`.*
- [x] Integration test: `QUERY` action + `[FromBody]` parameter binding (CarShack `NewQueryAction`). *Covered by
  `CallAction_CreateQuery_WithManualResolve` / `_WithExecuteAndResolve` (green).*
- [x] `HypermediaParameterFromFormBinder`: B1 ported it to System.Text.Json, #131 restricted it to explicit
  form / form-file usage — merged cleanly, verify with the file-upload Integration tests that both
  changes survived together. *Covered by `FileUpload` integration test (green).*
- [x] Regenerate CarShack schema artifacts (`Source/CarShack/schema-output`) — title/description sourcing
  changed (no `<summary>` titles; description = `[Description]` or summary + remarks). Not because of
  `QUERY`: `ActionDescription` carries no HTTP method.

- [x] Nullable complex properties (`Country? MostPopularIn`, `AddressTo? Address`) are inlined in the JSON
  Schema instead of `$ref` since `f9f82d0` (GEN-16: Properties POCO keeps nullable annotations). Effects:
  the `$id` is repeated at every use (invalid), the schema is bloated, and Markdown/Mermaid show `object`
  (`JsonSchemaExtensions.SchemaToTypeString` needs a `$ref`). Found while regenerating the CarShack
  schema output — do not commit that output until fixed. Belongs to S0. *Fixed: JsonSchema.Net (5.1.1 and
  6.0.0, `MemberGenerationContext.GenerateIntents`) always inlines nullable reference members; the
  refiner's use-site `$id` is load-bearing (it defeats the library's single-use inlining), so
  `DefinitionReferenceNormalizer` post-processes the finished schema: nullable → `oneOf [$ref, null]`,
  use-site `$id` removed. Renderers unwrap `oneOf`/`anyOf [X, null]`.*
- [x] Check HUI with the nullable reference shape in action `parameterSchema` (served at runtime via
  `IJsonSchemaFactory`). *Traced (HUI `main`, formly 7.0.0): `anyOf [X, null]` becomes an unlabeled
  multi-select of alternatives; `oneOf [X, null]` is flattened by `SchemaSimplifier.fixNullablesInOneOf`
  to a nullable object, same as the previous inline shape → emit `oneOf`. Not executed in a browser.*
- [x] Mermaid shows the `$defs` key fallback (`AddressInComplexTypeDefinitionRefinerTests`) where Markdown
  shows the `$id` name (`ComplexTypeDefinitionRefinerTests+Address`): `MermaidMapper.cs:126` calls
  `SchemaToTypeString` without the parent schema. Only visible for nested classes. *Fixed: parent schema
  passed; CarShack output unchanged.*

**RESTyard-HUI (raise there)**

- [ ] `SchemaSimplifier`: treat `anyOf [X, { type: null }]` like `oneOf` (extend `fixNullablesInOneOf`), so
  schemas from other generators (e.g. OpenAPI 3.1 style) render as a nullable object instead of an
  unlabeled multi-select. Also keep a sibling `description` when hoisting the non-null branch (dropped today).

**Code — `develop` (raise there, not on the branch)**

- [ ] Contract-first derived HTOs hide `HtoTitle` (CS0108 in CarShack `Hypermedia.Server.g.cs:205, 249`):
  `csharp-base/Document.razor:36` emits `public string HtoTitle` on every document, including derived ones.
  The interface stays mapped to the base class member, so the Siren title of a `DerivedCar` is the base
  title "A Car" (#132). Fix: emit `virtual` on base documents and `override` on derived ones.
- [ ] Contract-first: the XML `title` is now only the Siren title (`HtoTitle`), so generated HTOs have no
  schema display name. Add a separate attribute (e.g. `displayName="Customers"`) to the XSD that the
  templates emit as `[Title]`. The CarShack API docs lost their titles because of this.

- [x] `InlineQueryResult` sets `Location` on a `200` response — non-standard per RFC 9110 (`Content-Location`
  is the correct header, and it is set too). Either document the compat reason or drop `Location`.
  resolved: this is intentional. keep it
- [x] Centralize target frameworks in `Directory.Build.props` (e.g. `$(RestyardAppTfm)`) instead of per
  `.csproj` — #131's net8 → net10 bump could not reach projects created on feature branches, which broke
  restore after the merge (NU1201). Refactoring for all projects; do it on `develop`, then merge.
  resolved: by design, keep it
- [ ] `RESTyard.Generator/Properties/launchSettings.json` profile still uses `--template
  server/csharp-controller/v4`, which #131 deleted — switch to `v5`.
- [ ] Unresolved crefs to the removed `HttpGetHypermediaActionParameterInfo` (#131, CS1574):
  `HypermediaExtensionsOptions.cs:30`, `DynamicHypermediaAction.cs:9` — point to `HypermediaActionParameterInfoEndpoint<T>`.
- [ ] `RY0002` title/message say "SirenTitle" (and the analyzer test name), but the property is `HtoTitle`
  (`HypermediaObjectTitleAnalyzer.cs:17-18`).
- [ ] Move `HttpQueryAttribute` from CarShack (`CustomersRootController.cs:128`) into `RESTyard.AspNetCore`
  (namespace `RESTyard.AspNetCore.WebApi.AttributedRoutes`). Contract-first is broken without it: the XSD
  allows `method="Query"` (`Hypermedia.xsd:167`), `V5.razor:29` emits `[HttpQuery(...)]`, and no library
  type exists, so generated controllers do not compile unless the user writes one (#131). ASP.NET 10 ships
  only the `HttpMethods.Query` constant, no MVC attribute. Decisions:
  - keep the name `HttpQuery`; XML doc states it is a placeholder until ASP.NET Core ships its own
    `[HttpQuery]`, then it becomes `[Obsolete]` pointing to the built-in one (CS0104 risk when both
    namespaces are imported is accepted)
  - use `HttpMethods.Query` instead of the `"QUERY"` literal
  - delete the CarShack copy; add a Generator.Test snapshot with `method="Query"` that compiles
  - CHANGELOG (Added): `[HttpQuery]`; contract-first `method="Query"` compiles without a user-defined attribute
  - prerequisite for the RY0035 hint in [Design: action result delivery](#design-action-result-delivery)
- [ ] `CustomersRootController.NewQueryAction` comment "Provides a link to the result Query." is stale
  (result is returned inline).
- [ ] Remove the obsolete sourcelink#572 `TargetFrameworkMonikerAssemblyAttributesPath` workaround from
  `Source/Directory.Build.props` (done on the branch, still to raise on `develop`). Evaluated in props, before the SDK sets its inputs, so it
  resolves to `<projectdir>/.AssemblyAttributes`, which all TFMs of a project share. Parallel multi-TFM builds
  (Rider) race on that file → `CS2001`. The SDK default already puts it in `obj/`.

**Docs**

- [x] Title mechanics (see *Title handling*): `HypermediaApiSchema.md` (entity `title` row ~l.63: display
  name from `[Title]` only, not the Siren title; `description` source changes), `SourceGenerator.md`
  (title/description source tables ~l.211, l.286–295), migration guide: `HypermediaObject(Title)` →
  `HtoTitle` (Siren, runtime, optional) + `[Title]` (schema display name); class `<summary>` now feeds
  `description`; same rule for links / actions / embedded (member `<summary>` no longer a title);
  `GenerateDocumentationFile=true` required for XML-doc sourcing. Code examples still using
  `[HypermediaObject(Title = ...)]` / `: HypermediaObject`: `HypermediaApiSchema.md:287`,
  `SourceGenerator.md:85, 162, 229`. *Done; `GenerateDocumentationFile` requirement verified with a scratch
  build (without it: no `Description` emitted, no warning).*
- [x] Decide one attribute family for schema metadata: JsonSchema.Net (`[Title]`, `[Description]`, used by
  the generator) vs BCL `System.ComponentModel` (`[DisplayName]`, `[Description]`, mapped by
  `JsonSchemaFactory` for parameters). Today both coexist. *Decided: accept both everywhere, JsonSchema.Net
  preferred and wins when both are set. The generator now also reads the BCL pair (was silently ignored);
  documented in `SourceGenerator.md` (Metadata attribute families).*
- [x] `RESTyard.Schema/Model/HypermediaSchemaNameAttribute.cs` XML doc example still uses
  `[HypermediaObject(Title = ...)]`. *Done in the fallout commit.*
- [x] `CLAUDE.md` (repo root): controller template is now `server/csharp-controller/v5` (`V5.razor`),
  not `v4`; analyzers RY0010–RY0015 removed; `Generator.Test.Output`/`OutputV5` removed from the
  test-project table; .NET version is 10 (project descriptions l.36–53, AspNetCore.Test row "net8.0 +
  net9.0", Key Technical Details).
- [x] `HypermediaSchema-Plan.md` Step 8.4 references `server/csharp-controller/v4`. *Step 8.4 is otherwise
  outdated too: `V5.razor` already emits `HypermediaActionEndpoint<T>` (#131); only `ResultType` is left.*
- [x] `migration-guide.md`: mention `HtoTitle` replacing `HypermediaObject(Title)` for anyone migrating
  HTOs together with the schema opt-in.

---

## Design: action result delivery

**Problem:** a generated client must know how an action's result arrives to parse it. #131 added inline
QUERY results (`InlineQueryResult()`: `200` + Siren body + `Content-Location`) next to the existing
`Created()` shape (`201` + `Location`). The schema only has `resultName`, which implicitly means "follow
`Location`".

**Why not inferred:** the verb does not decide delivery (QUERY may return `Location`, POST may return
inline), `[HttpQuery]` hides its verb in a static field, the result type (`IHypermediaQueryResult`) was
delivered via `Location` before #131, and scanning method bodies for `InlineQueryResult()` is brittle.
So delivery is **declared explicitly** on the endpoint.

**Endpoint attribute** (`RESTyard.AspNetCore`):

```csharp
public enum ActionResultDelivery { Location, Inline }

[HttpQuery("Queries")]                                   // verb + route only, no RESTyard metadata
[HypermediaActionEndpoint<HypermediaCustomersRootHto>(
    nameof(HypermediaCustomersRootHto.CreateQuery),
    ResultType = typeof(HypermediaCustomerQueryResultHto),
    ResultDelivery = ActionResultDelivery.Inline)]       // result metadata, same place for every verb
```

- Result metadata lives on `HypermediaActionEndpoint` for every verb, never on `HttpQuery`: that attribute is
  routing-only (MVC convention) and a placeholder to be swapped for the built-in one.

- `ResultDelivery` defaults to `Location`; only meaningful with `ResultType`.
- XML doc names the HTTP shapes: `Location` = `201` + `Location` header (`Created()`),
  `Inline` = `200` + Siren body + `Content-Location` (`InlineQueryResult()`).
- Enum, not bool: room for e.g. `303 See Other` later.

**Hypermedia schema** — nested `result` object replaces the flat `resultName` / `resultClasses`
(the model is unreleased, so restructuring is free until S1 ships):

```json
{
  "name": "CreateQuery",
  "parameterSchema": { "...": "..." },
  "result": {
    "name": "CustomerQueryResult",
    "classes": ["CustomerQueryResult"],
    "delivery": "inline"
  },
  "isMandatory": true
}
```

- `result` omitted = the action returns no entity. `delivery` is always written when `result` is present.
- `delivery` values are lowercase strings: `location`, `inline`.

**What a client derives from it**

| `delivery` | Expected response | Client behaviour | Typed signature (e.g.) |
|---|---|---|---|
| `location` | `201`, empty body, `Location` | follow `Location` → GET → parse Siren | `Task<Link<ResultHco>>` |
| `inline` | `200`, Siren body of `result.name`, `Content-Location` | parse the body; `Content-Location` is the entity URI | `Task<ResultHco>` + URI |
| no `result` | `200` / `204` | nothing to parse | `Task` |

- The schema is a promise, not enforced at runtime: generated clients still check the status code and fail
  clearly on a mismatch. `RESTyard.Client`'s `LinkOrEntity<T>` (`Link_` / `Entity_(Value, Location)`)
  already handles both at runtime; the schema lets generated clients pick the precise type.
- `Content-Location` is required for `inline` (set by `InlineQueryResult()`); document it in
  `HypermediaApiSchema.md` so clients can rely on it.

**Diagnostics** (source generator):

- **RY0034** (warning): `ResultDelivery = Inline` without `ResultType`, or `ResultType` does not implement
  `IHypermediaQueryResult` (what `InlineQueryResult()` accepts).
- **RY0035** (info): endpoint uses `[HttpQuery]` with a `ResultType` but no explicit `ResultDelivery` —
  hint that QUERY results are usually `Inline`. Needs the library `HttpQueryAttribute` (develop item above)
  as a well-known type name.

**Touch points**

| Layer | Change |
|---|---|
| `HypermediaActionEndpointAttribute` | `ResultDelivery` property + XML doc |
| `ActionResultMappingExtractor` | read the named argument (int → string constant); RY0034/RY0035 |
| `ActionResultMapping` / `ActionMetadata` / `SchemaEmitter` | carry delivery; emit the nested `result` |
| `ActionDescription` | `Result` (`ActionResultDescription { Name, Classes, Delivery }`) replaces `ResultName` / `ResultClasses` |
| `HypermediaSchemaBuilder` (cross-assembly mapping, dangling check), `HypermediaSchemaFilter` | read `Result.Name` |
| Markdown | `**Returns:** [X](#x) (inline)` / `(via Location)` |
| Mermaid | no change (does not render action results) |
| Docs | `HypermediaApiSchema.md` (action table, JSON example, client guidance), `SourceGenerator.md` (RY0034/35) |
| CarShack | `ResultDelivery = Inline` on `NewQueryAction`; regenerate `schema-output` |
| Contract-first (develop, Plan 8.4) | XSD attribute for delivery, emitted by `V5.razor` together with `ResultType` |

**Order:** develop `HttpQueryAttribute` item → merge `develop` back → this design on the branch.
RY0034 and the schema change do not need the develop item; only RY0035 does.
