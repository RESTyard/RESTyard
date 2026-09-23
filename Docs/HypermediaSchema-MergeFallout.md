# `feature/hypermedia-schema` — Merge fallout TODOs (from merging `develop` into the branch, 2026-09-22)

Items discovered while resolving the Step 0 merge that are **not** fixed by the merge commit itself.
Semantic fallout that blocks the build is tracked in the Step 0 table of [`HypermediaSchema-MergeCandidates.md`](HypermediaSchema-MergeCandidates.md); this list is what remains.

**Code — branch**

- [x] `HypermediaActionEndpoint.ResultType` XML doc says "produces a `Location` header" — extend to inline
  results (`InlineQueryResult`, #131). Relates to Plan 8.6 (Location-header helper).
- [x] `server/csharp-controller/v4.sbn` was re-added by accident in `f0a3d2a` (#131 deleted it) — `git rm` it.
  *Cause: modify/delete conflict left the branch copy on disk as untracked; the next commit picked it up.*
- [ ] Represent the HTTP `QUERY` action with inline (embedded) result in the hypermedia schema (#131).
  Today `ActionDescription` only has the method and `resultName`, implicitly meaning "`Location` points to
  it". Needed: how the result is delivered (inline body + `Content-Location` via `InlineQueryResult`
  vs. `201`/`Location`), derived from the controller endpoint (e.g. an endpoint-attribute flag or the
  `HttpQuery` verb), carried through `ActionDescription`, JSON, Markdown and Mermaid renderers.
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
- [ ] Regenerate CarShack schema artifacts (`Source/CarShack/schema-output`) — title/description sourcing
  changed (no `<summary>` titles; description = `[Description]` or summary + remarks). Not because of
  `QUERY`: `ActionDescription` carries no HTTP method.

**Code — `develop` (raise there, not on the branch)**

- [ ] `InlineQueryResult` sets `Location` on a `200` response — non-standard per RFC 9110 (`Content-Location`
  is the correct header, and it is set too). Either document the compat reason or drop `Location`.
- [ ] Centralize target frameworks in `Directory.Build.props` (e.g. `$(RestyardAppTfm)`) instead of per
  `.csproj` — #131's net8 → net10 bump could not reach projects created on feature branches, which broke
  restore after the merge (NU1201). Refactoring for all projects; do it on `develop`, then merge.
- [ ] `RESTyard.Generator/Properties/launchSettings.json` profile still uses `--template
  server/csharp-controller/v4`, which #131 deleted — switch to `v5`.
- [ ] Unresolved crefs to the removed `HttpGetHypermediaActionParameterInfo` (#131, CS1574):
  `HypermediaExtensionsOptions.cs:30`, `DynamicHypermediaAction.cs:9` — point to `HypermediaActionParameterInfoEndpoint<T>`.
- [ ] `CustomersRootController.NewQueryAction` comment "Provides a link to the result Query." is stale
  (result is returned inline).
- [ ] Remove the obsolete sourcelink#572 `TargetFrameworkMonikerAssemblyAttributesPath` workaround from
  `Source/Directory.Build.props` (done on the branch, still to raise on `develop`). Evaluated in props, before the SDK sets its inputs, so it
  resolves to `<projectdir>/.AssemblyAttributes`, which all TFMs of a project share. Parallel multi-TFM builds
  (Rider) race on that file → `CS2001`. The SDK default already puts it in `obj/`.

**Docs**

- [ ] Title mechanics (see *Title handling*): `HypermediaApiSchema.md` (entity `title` row ~l.63: display
  name from `[Title]` only, not the Siren title; `description` source changes), `SourceGenerator.md`
  (title/description source tables ~l.211, l.286–295), migration guide: `HypermediaObject(Title)` →
  `HtoTitle` (Siren, runtime, optional) + `[Title]` (schema display name); class `<summary>` now feeds
  `description`; same rule for links / actions / embedded (member `<summary>` no longer a title);
  `GenerateDocumentationFile=true` required for XML-doc sourcing. Code examples still using
  `[HypermediaObject(Title = ...)]` / `: HypermediaObject`: `HypermediaApiSchema.md:287`,
  `SourceGenerator.md:85, 162, 229`.
- [ ] Decide one attribute family for schema metadata: JsonSchema.Net (`[Title]`, `[Description]`, used by
  the generator) vs BCL `System.ComponentModel` (`[DisplayName]`, `[Description]`, mapped by
  `JsonSchemaFactory` for parameters). Today both coexist.
- [x] `RESTyard.Schema/Model/HypermediaSchemaNameAttribute.cs` XML doc example still uses
  `[HypermediaObject(Title = ...)]`. *Done in the fallout commit.*
- [ ] `CLAUDE.md` (repo + root): controller template is now `server/csharp-controller/v5` (`V5.razor`),
  not `v4`; analyzers RY0010–RY0015 removed; `Generator.Test.Output`/`OutputV5` removed from the
  test-project table; .NET version is 10 (project descriptions l.36–53, AspNetCore.Test row "net8.0 +
  net9.0", Key Technical Details).
- [x] `HypermediaSchema-Plan.md` Step 8.4 references `server/csharp-controller/v4`. *Step 8.4 is otherwise
  outdated too: `V5.razor` already emits `HypermediaActionEndpoint<T>` (#131); only `ResultType` is left.*
- [ ] `migration-guide.md`: mention `HtoTitle` replacing `HypermediaObject(Title)` for anyone migrating
  HTOs together with the schema opt-in.
