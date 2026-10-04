# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versions follow `RESTyard.AspNetCore`; changes to other packages are listed under the same version and name their package.

## [Unreleased]

### Added

- **RESTyard.AspNetCore:** actions and functions can use the HTTP `QUERY` verb. ASP.NET Core has no built-in attribute for it;
  CarShack defines an `HttpQueryAttribute` as an example. A query result can be returned inline in the response body
  (`this.InlineQueryResult(result)`) instead of via a `Location` header. (#131)
- **RESTyard.AspNetCore:** the Siren `title` is now set per instance through `IHypermediaObject.HtoTitle`, so it can include
  property values. Analyzer `RY0002` with a code fix migrates `[HypermediaObject(Title = ...)]`. (#132)
- **RESTyard.Generator:** schema XML titles can interpolate properties (`title="Customer: {FullName}"`) and are generated as
  `HtoTitle`. (#132)
- **RESTyard.Client:** all public async methods accept a `CancellationToken`. (#130)
- **RESTyard.Client:** analyzer `RYC001` with a code fix suggests `ExecuteAndResolveAsync`, so a `QUERY` result can be
  returned inline instead of needing a second request. (#134)

### Changed

- **Breaking — all packages:** the application target framework is now .NET 10 (was .NET 8); `netstandard2.0` targets are
  unchanged. Move consuming apps to `net10.0`. (#131)
- **Breaking — RESTyard.Client and extensions:** the new `CancellationToken` parameters are binary-incompatible, so recompile
  against the new version. File-upload stream factories now receive a `CancellationToken` (`Func<CancellationToken, Task<Stream>>`),
  so existing lambdas need an extra parameter. (#130)
- **RESTyard.AspNetCore:** the hypermedia form binder only binds parameters that are explicitly bound from a form or form file.
  All other action parameters use standard ASP.NET Core binding. (#131)
- **Breaking — RESTyard.Client:** `IHypermediaResolver.ResolveFunctionAsync` returns `LinkOrEntity<T>` — either a link or the
  inline result entity with its location. Callers and custom implementations must handle both cases. (#131)
- **Breaking — RESTyard.Client:** custom resolvers overriding `VerifyIfCacheEntryCanBeUsedAsync` must return
  `HypermediaResult<CacheEntryVerificationResult<T>>` and create results via `CacheEntryVerificationResult<T>.CacheEntryMayBeUsed()` etc. (#130)

### Removed

- **Breaking — RESTyard.AspNetCore:** the legacy route attributes `HttpGetHypermediaObject`, `Http{Post,Put,Patch,Delete}HypermediaAction`
  and `HttpGetHypermediaActionParameterInfo` are removed, together with their analyzers `RY0010`–`RY0015`. Use `[HttpGet]`,
  `[HttpPost]` etc. combined with `HypermediaObjectEndpoint<T>`, `HypermediaActionEndpoint<T>` or `HypermediaActionParameterInfoEndpoint<T>`. (#131)
- **Breaking — RESTyard.AspNetCore:** the `HttpMethod` enum and all overloads taking it (e.g. `HypermediaExternalAction`,
  `ExternalFileUploadHypermediaAction` constructors) are removed; pass the method as a string (`"POST"`, `HttpMethods.Post`).
  The obsolete `HypermediaQueryResult` base class is removed; query-result HTOs declare `Query` themselves (the generator emits it). (#131)
- **Breaking — RESTyard.Client:** `PatternMatchExtensions` (`TypeMatch`) is removed; use C# pattern matching. (#130)
- **Breaking — RESTyard.AspNetCore:** `HypermediaObjectAttribute.Title` and the obsolete `HypermediaObject` base class are removed.
  Implement `IHypermediaObject` and provide `HtoTitle`; the `RY0002` code fix rewrites the attribute. (#132)
- **Breaking — RESTyard.Generator:** the `server/csharp-controller/v4` template is removed; use `server/csharp-controller/v5`. (#131)

### Fixed

- **RESTyard.AspNetCore:** a relative URI passed to a key-from-URI parameter now produces an error result instead of throwing. (#126)
- **RESTyard.AspNetCore:** an action whose parameter type has no schema route (`AutoDeliverJsonSchemaForActionParameterTypes`
  off and no custom route) now fails with an error naming the type and the fix, instead of an obscure JSON serialization error.
- **RESTyard.Generator:** a derived document (`parentDocument`) with its own `title` now gets that title in Siren instead of
  the parent's (`server/csharp/v5.2`, compiler warning CS0108). Without its own title it inherits the parent's. (#132)
- **RESTyard.AspNetCore:** analyzer `RY0002` told users to move the title to a `SirenTitle` property; it now names `HtoTitle`.
- **RESTyard.Client.Extensions.SystemNetHttp:** a network error while revalidating a cached response is returned as an error
  result instead of being thrown from `ResolveLinkAsync`. (#130)
