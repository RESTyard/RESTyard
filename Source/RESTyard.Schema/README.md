# RESTyard.Schema

Schema model for [RESTyard](https://github.com/RESTyard/RESTyard) hypermedia APIs.

`HypermediaApiSchema` is a serializable description of a Siren hypermedia API — its entity types,
links, actions, and embedded entities — together with generators that turn it into JSON, Mermaid
diagrams, and Markdown documentation. Use it for API documentation, client generation, and tooling.

## Install

```bash
dotnet add package RESTyard.Schema
```

Targets `netstandard2.0` and `net8.0`.

## Register the schema (ASP.NET Core / DI)

`AddHypermediaSchema` registers a singleton `HypermediaApiSchema`, built by auto-discovering the
per-assembly schema registries emitted by the RESTyard source generator (requires
`[assembly: HypermediaAssembly]` and a registered `IJsonSchemaFactory`, e.g. via
`AddHypermediaExtensions()`).

```csharp
builder.Services.AddHypermediaSchema(o =>
{
    o.Title = "My API";
    o.Description = "...";
    o.Version = "1.0.0";
});
```

## Generate artifacts

Resolve the `HypermediaApiSchema` and generate output files directly:

```csharp
var schema = app.Services.GetRequiredService<HypermediaApiSchema>();

HypermediaSchemaGenerator.Generate(
    schema,
    outputPath: "./generated-schema",
    formats: SchemaOutputFormats.All);
```

Or drive generation from command-line arguments — `GenerateIfRequested` returns `true` when it
handled `--generate-schema` / `--schema-help`, so the app can generate and exit:

```csharp
if (HypermediaSchemaGenerator.GenerateIfRequested(schema, args))
{
    return;
}
```

```bash
myapp --generate-schema --schema-output ./out --schema-artifacts markdown-api-documentation
myapp --schema-help
```

Available formats: `json-hypermedia-api-schema`, `mermaid-api-map`, `mermaid-htos`,
`markdown-api-documentation`, `all`.

## License

MIT — see the [RESTyard repository](https://github.com/RESTyard/RESTyard).
