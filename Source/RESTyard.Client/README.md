# Hypermedia Client Lib
_Experimental_ project to create a Client which allows typed access to a Rest Api.
- Fluent Link navigation
- Transparent Link and Action resolving with deserialization
- Action handling

## Usage

### Creating the resolver
- setting up the object register and the hypermedia reader
``` csharp
var builder = new HypermediaResolverBuilder()
    .ConfigureObjectRegister(register =>
    {
        register.Register<EntryPointHco>();
        register.Register<CustomerHco>();
        ...
    })
    .WithSirenHypermediaReader()
```

- with Newtonsoft.Json

=> reference Bluehands.Hypermedia.Client.Extensions.NewtonsoftJson nuget package
``` csharp
var builder = new HypermediaResolverBuilder()
    ...
    .WithSingleNewtonsoftJsonObjectParameterSerializer()
    .WithNewtonsoftJsonStringParser()
    .WithNewtonsoftJsonProblemReader()
```

- with System.Text.Json

=> reference Bluehands.Hypermedia.Client.Extensions.SystemTextJson
``` csharp
var builder = new HypermediaResolverBuilder()
    ...
    .WithSingleSystemTextJsonObjectParameterSerializer()
    .WithSystemTextJsonStringParser()
    .WithSystemTextJsonProblemReader()
```

- with System.Net.Http

=> reference Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
``` csharp
var httpClient = new HttpClient();
// configure the HttpClient, e.g. authorization
var resolver = new HypermediaResolverBuilder()
    ...
    .CreateHttpHypermediaResolver(httpClient);
```
or with a cached resolver
``` csharp
var httpClient = new HttpClient();
// configure the HttpClient, e.g. authorization
var resolver = new HypermediaResolverBuilder()
    ...
    .CreateHttpHypermediaResolver(
        httpClient,
        linkCacheImplementation);
```
if the HttpClient changes throughout the application lifetime, you can build a factory method that will create a new IHypermediaResolver for a given HttpClient, that is then used for that IHypermediaResolver:
``` csharp
var resolverFactory = new HypermediaResolverBuilder()
    ...
    .CreateHttpHypermediaResolverFactory();
...
var httpClient = new HttpClient();
// configure the HttpClient, e.g. authorization
var resolver = resolverFactory(httpClient);
```

There is still a lot to do:
- Exception handling
- Error responses by the api (http, authorization, hypermediaparsing and api), problem json, status codes
- (more) Authentification/Authorization
- Allow actions with schemas OR fields
- Add a attribute wich holds the json property name which should be deserialized into a hco property
...

Assumptions:
- Siren (single paramert json objects like in the CarShack demo)
- Deterministic Links, a link will always lead to the same class of ressource. E.g. a Customer.