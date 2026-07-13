# API Guide Endpoint

Serves an authored API **guide/manual** (Markdown) from a well-known, configurable minimal-API
endpoint — a sibling to the schema endpoint (`MapHypermediaSchema()`, see
[HypermediaApiSchema.md](HypermediaApiSchema.md)). The schema is the *generated*, machine-readable
description of types, links, and actions; the guide is the *authored* layer on top: workflows,
conventions, and semantics that cannot be derived from types.

The feature is opt-in: nothing is registered unless you map the endpoint.

## Setup

```csharp
// Serve a static Markdown file (relative paths resolve against the content root)
app.MapApiGuide("api-guide.md");
```

This maps `GET /api-guide` returning the raw Markdown with content type
`text/vnd.restyard.api-guide+markdown` (a RESTyard vendor media type; the body is plain
Markdown). The file must exist when the endpoint is mapped — a missing file fails at startup
with a `FileNotFoundException`, not with request-time errors.

**Custom route:**

```csharp
app.MapApiGuide("api-guide.md", o => o.Route = "/manual");
```

## Dynamic content

For per-user, localized, or assembled-at-request-time content, implement
`IApiGuideProvider` and use the provider overload:

```csharp
class MyGuideProvider : IApiGuideProvider
{
    public Task<string> GetGuideAsync(HttpContext context)
        => Task.FromResult(BuildGuideFor(context.User));
}

app.MapApiGuide(new MyGuideProvider());
// or resolved from DI:
app.MapApiGuide(app.Services.GetRequiredService<IApiGuideProvider>());
```

## Authorization

`MapApiGuide()` returns an `IEndpointConventionBuilder`, so standard minimal-API policies
chain directly:

```csharp
app.MapApiGuide("api-guide.md").RequireAuthorization();
```

## Cache headers (opt-in)

The guide changes only on deploy. Set `CacheMaxAge` to emit a `Cache-Control` header —
no header is emitted by default:

```csharp
app.MapApiGuide("api-guide.md", o => o.CacheMaxAge = TimeSpan.FromMinutes(5));
// -> Cache-Control: private, max-age=300
```

Visibility defaults to `private`. Set `o.CacheVisibility = CacheVisibility.Public` only when the
content is identical for every caller — always true for the file overload, your call for a
provider. `public` lets shared caches (proxies, CDNs) store the response.

## Linking from the entry point

Advertise the guide on your entry-point HTO under the `api-guide` relation so clients (and the
future agent interface) can discover it:

```csharp
public partial class HypermediaEntrypointHto
{
    [Relations([DefaultHypermediaRelations.ApiGuide])]
    public ExternalLink Guide { get; init; } = ApiGuide.Link();
}
```

The link resolves to the mapped guide route with the guide media type.

See the CarShack demo (`Source/CarShack/api-guide.md`, `Program.cs`, `Hypermedia.Server.cs`)
for a complete example.
