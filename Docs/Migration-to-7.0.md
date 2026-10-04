# Migrating to RESTyard 7.0

Each section lists the steps a consumer must take. Each step has a **Find** (what to search for) and a **Do** (the change).
`CHANGELOG.md` has the full list of changes.

## Target framework

1. **Move to .NET 10.** Application targets are `net10.0` (was `net8.0`); `netstandard2.0` targets are unchanged.
   - Find: `<TargetFramework>net8.0</TargetFramework>` in projects that reference RESTyard packages.
   - Do: change it to `net10.0`.

## Server (RESTyard.AspNetCore)

1. **Replace the legacy route attributes.** They and their analyzers `RY0010`–`RY0015` are removed.

   | Find | Do |
   |---|---|
   | `[HttpGetHypermediaObject("route", typeof(THto), typeof(TKeyProducer))]` | `[HttpGet("route"), HypermediaObjectEndpoint<THto>(typeof(TKeyProducer))]` |
   | `[HttpPostHypermediaAction("route", typeof(TOp))]` (also `Put`, `Patch`, `Delete`) | `[HttpPost("route"), HypermediaActionEndpoint<THto>(nameof(THto.Action))]` |
   | `[HttpGetHypermediaActionParameterInfo("route", typeof(TParameter))]` | `[HttpGet("route"), HypermediaActionParameterInfoEndpoint<TParameter>]` |

   Upgrade to the last 6.x first and apply the `RY0010`–`RY0015` code fixes there; 7.0 no longer has them.
2. **Move titles to `HtoTitle`.** `HypermediaObjectAttribute.Title` is removed; the Siren title is per instance now.
   - Find: `[HypermediaObject(Title = ...)]`
   - Do: apply the `RY0002` code fix, or remove `Title` and add `public string? HtoTitle => "...";` (it may use property values).
3. **Drop the removed base classes.**

   | Find | Do |
   |---|---|
   | `: HypermediaObject` | implement `IHypermediaObject` and add `HtoTitle` |
   | `: HypermediaQueryResult` | implement `IHypermediaQueryResult` and declare `public IHypermediaQuery Query { get; }` |
4. **Pass HTTP methods as strings.** The `HttpMethod` enum and every overload taking it are removed.
   - Find: `HttpMethod.` in `HypermediaExternalAction` / `ExternalFileUploadHypermediaAction` constructors and similar calls.
   - Do: pass `HttpMethods.Post` (`Microsoft.AspNetCore.Http`) or `"POST"`.

## Client (RESTyard.Client and extensions)

1. **Recompile.** The new `CancellationToken` parameters are binary-incompatible; libraries built against 6.x must be rebuilt.
2. **Add a `CancellationToken` to file-upload stream factories.**
   - Find: `Func<Task<Stream>>` lambdas passed for file uploads.
   - Do: `async ct => ...`; the type is `Func<CancellationToken, Task<Stream>>`.
3. **Handle inline function results.** `IHypermediaResolver.ResolveFunctionAsync` returns `LinkOrEntity<T>`: either a link to the
   result or the result entity itself (QUERY with an inline result).
   - Find: callers of `ResolveFunctionAsync` and custom `IHypermediaResolver` implementations.
   - Do: prefer `ExecuteAndResolveAsync` (analyzer `RYC001` offers the fix); otherwise handle both cases with `linkOrEntity.Match(...)`.
4. **Update custom cache verification.**
   - Find: overrides of `VerifyIfCacheEntryCanBeUsedAsync`.
   - Do: return `HypermediaResult<CacheEntryVerificationResult<T>>`; create values with `CacheEntryVerificationResult<T>.CacheEntryMayBeUsed()` etc.
5. **Replace `TypeMatch`.** `PatternMatchExtensions` is removed.
   - Find: `.TypeMatch(`
   - Do: use C# `switch` / `is` pattern matching.

## Code generator (RESTyard.Generator)

1. **Use the v5 controller template.** `server/csharp-controller/v4` is removed.
   - Find: `--template server/csharp-controller/v4` in scripts, build targets and launch profiles.
   - Do: use `server/csharp-controller/v5` and regenerate.
