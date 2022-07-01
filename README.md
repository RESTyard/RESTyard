# Web Api Hypermedia Extensions
This project consists of a set of Extensions for Web Api 2 Core projects. The purpose is to
assist in building restful Web services using the [Siren Hypermedia Format](https://github.com/kevinswiber/siren) with much less coding.
Using the Extensions it is possible to return HypermediaObjects as C# classes. Routes for HypermediaObjects and Actions are built using extended attribute routing.

Of course there might be some edge cases or flaws. Comments, suggestions, remarks and criticism are very welcome.

For a first feel there is a demo project called [CarShack](https://github.com/bluehands/WebApiHypermediaExtensions/tree/master/Source/CarShack) which shows a great portion of the Extensions in use. It also shows how the routes were intended to be designed, although this is not enforced.

To develop C# client applications there is a generic REST client. It is in an early stage at the moment: [https://github.com/bluehands/WebApiHypermediaExtensions/tree/master/Source/HypermediaClient](https://github.com/bluehands/WebApiHypermediaExtensions/tree/master/Source/HypermediaClient)

## On nuget.org
The Extensions: [https://www.nuget.org/packages/WebApiHypermediaExtensionsCore](https://www.nuget.org/packages/WebApiHypermediaExtensionsCore)

The Client: [https://www.nuget.org/packages/Bluehands.Hypermedia.Client/](https://www.nuget.org/packages/Bluehands.Hypermedia.Client/)

## UI client
There is a partner project which aims for a generic UI client: [HypermediaUi](https://github.com/MathiasReichardt/HypermediaUi)

## Key concepts
The Extensions allow you to build a restful web server which responds with Siren documents without building a Siren class and assigning URIs to Links and embedded Entities. For this the Extensions provide two main components: the `HypermediaObject` class and new RouteAttributes extending the Web Api RouteAttributes.

HypermediaObjects returned from Controllers will be formatted as Siren. All contained referenced HypermediaObjects (e.g. Links and embedded Entities), Actions, and Parameter types (of Actions) are automatically resolved and properly inserted into the Siren document, by looking up attributed routes.

## Using it in a project
To use the Extensions just call `AddHypermediaExtensions()` on your DI container:

``` csharp
builder.Services.AddHypermediaExtensions(o =>
{
    o.ReturnDefaultRouteForUnknownHto = true; // useful during development
});
```

To configure the generated URLs in the Hypermedia documents pass a `HypermediaUrlConfig` to `AddHypermediaExtensionsInternal()`. In this way absolute URLs can be generated which have a different scheme or another host e.g. a load balancer.

## HypermediaObject
This is the base class for all entities (in Siren format) which shall be returned from the server. Derived types from HypermediaObjects can be thought of as kind of a DTO (Data Transfer Object). A fitting name would be HTO, Hypermedia Transfer Object. They accumulate all information which should be present in the formatted Hypermedia document and will be formatted as Siren Hypermedia by the included formatter.
An Example from the demo project CarShack:

```csharp
[HypermediaObject(Title = "A Customer", Classes = new[] { "Customer" })]
public class HypermediaCustomer : HypermediaObject
{
    private readonly Customer customer;

    // Add actions:
    // Each ActionType must be unique and a corresponding route must exist so the formatter can look it up.
    // See the CustomerController.
    [HypermediaAction(Name = "CustomerMove", Title = "A Customer moved to a new location.")]
    public HypermediaActionCustomerMoveAction MoveAction { get; private set; }

    [HypermediaAction(Title = "Marks a Customer as a favorite buyer.")]
    public HypermediaActionCustomerMarkAsFavorite MarkAsFavoriteAction { get; private set; }

    // Hides the Property so it will not be pressent in the Hypermedia.
    [FormatterIgnoreHypermediaProperty]
    public int Id { get; set; }

    // Assigns an alternative name, so this stays constant even if property is renamed
    [HypermediaProperty(Name = "FullName")]
    public string Name { get; set; }

    public int Age { get; set; }

    public string Address { get; set; }

    public bool IsFavorite { get; set; }

    public HypermediaCustomer(Customer customer)
    {
        this.customer = customer;

        Name = customer.Name;
        ...

        MoveAction = new HypermediaActionCustomerMoveAction(CanMove, DoMove);
        ...
    }
...
}

```

**In short:**
- Public Properties will be formatted to Siren Properties. 
- No Properties which hold a class will be serialized
- By default Properties which are null will not be added to the Siren document.
- It is recommended to represented optional values as Nullable<T>
- Properties with a `HypermediaActionBase` type will be added as Actions, but only if CanExecute returns true. Any required parameters will be added in the "fields" section of the Siren document.
- Other `HypermediaObject`s can be embedded by adding them as a `HypermediaObjectReferenceBase` type to the entities collection Property (not shown in this example, see HypermediaCustomerQueryResult in the demo project).
- Links to other `HypermediaObject`s can be added to the Links collection Property, also as `HypermediaObjectReferenceBase` (not shown in this example, see HypermediaCustomersRoot in the demo project).
- Properties, Actions and `HypermediaObject`s themselves can be attributed e.g. to give them a fixed name:
    - `FormatterIgnoreHypermediaPropertyAttribute`
    - `HypermediaActionAttribute`
    - `HypermediaObjectAttribute`
    - `HypermediaPropertyAttribute`

**Important**
All `HypermediaObject`'s used in a Link or as embedded Entity and all `HypermediaAction`'s in a `HypermediaObject` require that there is an attributed route for their Type. Otherwise the formatter is not able to resolve the URI and will throw an Exception.

### Embedded Entities and Links
References to other `HypermediaObjects` are represented by references which derive from `HypermediaObjectReferenceBase`. These references are the added to the Links dictionary or the Entities list of a `HypermediaObject`.

#### Option 1: If a instance of the referenced HypermediaObject is available
Use a `HypermediaObjectReference` to create a reference. This reference can then be added to the Links dictionary with an associated relation:

```
Links.Add("NiceCar", new HypermediaObjectReference(new HypermediaCar("VW", 2)));
```

or the Entities list (which can contain duplicates):
```
Entities.Add("NiceCar", new HypermediaObjectReference(new HypermediaCar("VW", 2)));
```

*Note*
The used function is an convenience extension contained in `WebApi.HypermediaExtensions.Hypermedia.Extensions`

#### Option 2: If no instance is available or not necessary
To allow referencing of HypermediaObjects without the need to instantiate them, for reference purpose only, there are two additional references available.

use a `HypermediaObjectKeyReference` if the object requires a key to be identified e.g. the Customers id.
```
Links.Add("BestCustomer", new HypermediaObjectKeyReference(typeof(HypermediaCustomer), 1));
```
The reference requires the type of the referenced HypermediaObject, here `HypermediaCustomer` and a key which is used by the related route to identify the desired entity. The framework will pass the key object to the `KeyProducer` instance which is assigned to the HypermediaObject's route, here `CustomerRouteKeyProducer`. Explicit assignment of RouteKeyProducers is optional. `KeyAttribute` can be used alternatively on key properties of the HypermediaObject. For more details on attributed routes see [Attributed routes](## Attributed routes).

Example from the CarShack demo project `CustomerController.cs`
```
[HttpGetHypermediaObject("{key:int}", typeof(HypermediaCustomer), typeof(CustomerRouteKeyProducer))]
public async Task<ActionResult> GetEntity(int key)
{
..
        var customer = await customerRepository.GetEnitityByKeyAsync(key);
        var result = new HypermediaCustomer(customer);
        return Ok(result);
...
}
```

The `CustomerRouteKeyProducer` is responsible for the translation of the domain specific key`object to a key which is usable in the WebApi route context. It must be an anonymous object where all propertys match the rout template parameters, here `{key:int}`.
```
public object CreateFromKeyObject(object keyObject)
{
    return new { key = keyObject };
}
```

#### Option 3: If a query result should be referenced
Use a `HypermediaObjectQueryReference` if the object requires also query object `IHypermediaQuery` to be created e.g. a result object which contains several Customers.
For a reference to a query result: `HypermediaQueryResult` it is also required to provide the query to the reference, so the link to the object can be constructed.

Example from `HypermediaCustomersRoot.cs`:
```
var allQuery = new CustomerQuery();
Links.Add(DefaultHypermediaRelations.Queries.All, new HypermediaObjectQueryReference(typeof(HypermediaCustomerQueryResult), allQuery));
```

#### External References
It might be necessary to reference a external source or a route which can not be build by the framework. In this case use the `ExternalReference`. This object works around the default route resolving process by providing its own URI. It can only be used in combination with `HypermediaObjectReference`.

Example reference of an external site:
```
Links.Add("GreatSite", new ExternalReference(new Uri("http://www.example.com/")));
```

## Attributed routes
The included SirenFormatter will build required links to other routes. At startup all routes attributed with:
- `HttpGetHypermediaObject`
- `HttpPostHypermediaAction`
- `HttpGetHypermediaActionParameterInfo`

will be placed in an internal register.

This means that for every `HypermediaObject` there must be a route with matching type.
Example from the demo project CustomerRootController:
``` csharp 
[HttpGetHypermediaObject("", typeof(HypermediaCustomersRoot))]
public ActionResult GetRootDocument()
{
    return Ok(customersRoot);
}
```

The same goes for Actions:

```csharp
[HttpPostHypermediaAction("CreateCustomer", typeof(HypermediaFunction<CreateCustomerParameters, Task<Customer>>))]
public async Task<ActionResult> NewCustomerAction([SingleParameterBinder(typeof(CreateCustomerParameters))] CreateCustomerParameters createCustomerParameters)
{
    if (createCustomerParameters == null)
    {
        return this.Problem(ProblemJsonBuilder.CreateBadParameters());
    }

    var createdCustomer = await customersRoot.CreateCustomerAction.Execute(createCustomerParameters);

    // Will create a Location header with a URI to the result.
    return this.Created(new HypermediaCustomer(createdCustomer));
}
```

Note:
Siren specifies that to trigger an action an array of parameters should be posted to the action route. To avoid wrapping parameters in an array class there is the SingleParameterBinder for convenience.

A valid JSON for this route would look like this:
``` json
[{"CreateCustomerParameters": 
	{
	  "Name":"Hans Schmid"
	}
}]
```

The parameter binder also allows to pass a parameter object without the wrapping array:
``` json
{"CreateCustomerParameters": 
	{
	  "Name":"Hans Schmid"
	}
}
```

Parameters for actions may define a route which provides additional type information to the client. These routes will be added to the Siren fields object as "class".

```csharp
[HttpGetHypermediaActionParameterInfo("CreateCustomerParametersType", typeof(CreateCustomerParameters))]
public ActionResult CreateCustomerParametersType()
{
    var schema = JsonSchemaFactory.Generate(typeof(CreateCustomerParameters));
    return Ok(schema);
}
```

Also see See: [extracting keys from action parameter URLs](#extracting-keys-from-action-parameter-urls)


### Routes with a placeholder in the route template
For access to entities a route template may contain placeholder variables like _key_ in the example below.
If a `HypermediaObject` is referenced, e.g. the self link or a link to another Customer, the formatter must be able to create the URI to the linked `HypermediaObject`.
To properly fill the placeholder variables for such routes a `KeyProducer` is required.

#### Use attributes to indicate keys
Use the `Key` attribute to indicate which properties of the HTO should be used to fill the route template variables.
If there is only one variable to fill it is enough to put the attribute above the desired HTO property.

Example:
The route template: `[HttpGetHypermediaObject("{key:int}", typeof(MyHypermediaObject))]`
The attributed HTO:
```csharp
public class MyHypermediaObject : HypermediaObject
{
    [Key]
    public int Id { get; set; }
...
```

If the route has more than one variable, the `Key` attribute receives the name of the related route template variable.

Example:
The route template: `[HttpGetHypermediaObject("{brand}/{key:int}", typeof(HypermediaCar))]`
The attributed HTO:
```csharp
[HypermediaObject(Title = "A Car", Classes = new[] { "Car" })]
public class HypermediaCar : HypermediaObject
{
    // Marks property as part of the objects key so it is can be mapped to route parameters when creating links.
    [Key("brand")]
    public string Brand { get; set; }

    // Marks property as part of the objects key so it is can be mapped to route parameters when creating links
    [Key("key")]
    public int Id { get; set; }

    ...
}
```

#### Use a custom `KeyProducer`
Us use a custom `KeyProducer` implement IKeyProducer and add it to the attributed routes:
`[HttpGetHypermediaObject("{key:int}", typeof(HypermediaCustomer), typeof(CustomerRouteKeyProducer))]` to tell the framework which `KeyProducer to use for the route.

The formatter will call the producer if he has a instance of the referenced
Object (e.g. from `HypermediaObjectReference.GetInstance()`) and passes it to the `IKeyProducer:CreateFromHypermediaObject()` function.
Otherwise it will call `IKeyProducer:CreateFromKeyObject()` and passes the object provided by `HypermediaObjectKeyReference:GetKey(IKeyProducer keyProducer)`.
The `KeyProducer` must return an anonymous object filled with a property for each placeholder variable to be filled in the `HypermediaObject`'s route, here _key_.

A `KeyProducer` is added directly to the Attributed route as a Type and will be instantiated once by the framework.
See `CustomerRouteKeyProducer` in the demo project for an example.

``` csharp
[HttpGetHypermediaObject("Customers/{key:int}", typeof(HypermediaCustomer), typeof(CustomerRouteKeyProducer))]
public async Task<ActionResult> GetEntity(int key)
{
    ...
}
```

By design the Extension encourages routes to not have multiple keys in the route template. Also only routes to a `HypermediaObject` may have a key. Actions related to
a `HypermediaObject` must be available as a sub route to its corresponding object so required route template variables can be filled for the actions host `HypermediaObject`.
Example:
```
http://localhost:5000/Customers/{key}
http://localhost:5000/Customers/{key}/Move
```

### Queries
Clients shall not build query strings. Instead they post a JSON object to a `HypermediaAction` and receive the URI to the desired query result in the `Location` header.
``` csharp
[HttpPostHypermediaAction("CreateQuery", typeof(HypermediaAction<CustomerQuery>))]
public ActionResult NewQueryAction([SingleParameterBinder(typeof(CustomerQuery))] CustomerQuery query)
{
    ...
    // Will create a Location header with a URI to the result.
    return this.CreatedQuery(typeof(HypermediaCustomerQueryResult), query);
}
```

There must be a companion route which receives the query object and returns the query result:
``` csharp
[HttpGetHypermediaObject("Query", typeof(HypermediaCustomerQueryResult))]
public async Task<ActionResult> Query([FromQuery] CustomerQuery query)
{
    ...

    var queryResult = await customerRepository.QueryAsync(query);
    var resultReferences = new List<HypermediaObjectReferenceBase>();
    foreach (var customer in queryResult.Entities)
    {
        resultReferences.Add(new HypermediaObjectReference(new HypermediaCustomer(customer)));
    }

    var navigationQuerys = NavigationQuerysBuilder.Build(query, queryResult);
    var result = new HypermediaCustomerQueryResult(resultReferences, queryResult.TotalCountOfEnties, query, navigationQuerys);
           
    return Ok(result);
}
```

## Options
For some configuration you can pass a `HypermediaExtensionsOptions` object to `AddHypermediaExtensions` method.

- `ReturnDefaultRouteForUnknownHto` if set to `true` the `IHypermediaRouteResolver` will return a default route (see: `DefaultRouteSegmentForUnknownHto`) if a HTO's route is unknown.
  This is useful during development time when first writing some HTO's and not all controllers are implemented. Default is `false`.

- `DefaultRouteSegmentForUnknownHto` a `string` which will be appended to `<scheme>://<authority>/` as default route.

- `AutoDeliverJsonSchemaForActionParameterTypes`: if set to `true` (default) the routes to action parameters (implementing `IHypermediaActionParameter`) will be generated automatically except when there is a explicit rout created.

- `ImplicitHypermediaActionParameterBinders`: if set to `true` (default) actions receiving a `IHypermediaActionParameter` will have a automatic binder added. 
  The binder allows to use the `KeyFromUriAttribute` attribute. See: [extracting keys from action parameter URLs](#extracting-keys-from-action-parameter-urls).

## Extracting keys from action parameter URLs
When it is required to reference an other resource as action parameter it is often required to get the identifying keys from the resources URL. 
Enable `ImplicitHypermediaActionParameterBinders` to use this feature.
This can be automated by using attributes in parameters.
If there is only one key in use in the URL it can be attributed like this:

``` csharp
public class FavoriteCustomer : IHypermediaActionParameter
{
    [Required]
    [KeyFromUri(typeof(HypermediaCustomer), schemaProperyName: "Customer")]
    public int CustomerId { get; set; }
}
```

- The first parameter `typeof(HypermediaCustomer)` gives the expected `HypermediaObject` so the frame work knows which route layout it should use, and there to what kind of Resource the provided URL should lead.
- The second parameter `schemaProperyName: "Customer"` is to identify the property which holds the URL in the payload JSON object.
  Note: when using `AutoDeliverJsonSchemaForActionParameterTypes` the delivered schemas are adapted so a URL property with the name `Customer` is required.

The post would look like:
```
[
  {
    "FavoriteCustomer": {
      "Customer": "http://localhost:5000/Customers/1"
    }
  }
]
```

The binder will then extract the customers value `1` from the URL and sets it to the parameter which is attributed.

### More than one key
If your resource is addressed using more than one key the binder needs additional information.

Example `HypermediaActionCustomerBuysCar.Parameter`:

``` csharp
public class Parameter : IHypermediaActionParameter
{
    [KeyFromUri(typeof(HypermediaCar), schemaProperyName: "CarUri", routeTemplateParameterName: "brand")]
    public string Brand { get; set; }
    [KeyFromUri(typeof(HypermediaCar), schemaProperyName: "CarUri", routeTemplateParameterName: "key")]
    public int CarId { get; set; }
    [Required]
    public double Price { get; set; }
}
```

Not two properties have an attribute indicating that they should be filled: `Brand` and `CarId`. Both share the same type of resource and `schemaProperyName` because the source of their value is a single URL in the JSON payload.
To configure which route template variable (see your attributed route) should be used to fill the parameter property `routeTemplateParameterName` is used.
The route template: `{brand}/{key:int}`. Be careful to match the variable name and `routeTemplateParameterName`.

The corresponding post would look like: 
``` csharp
[
  {
    "HypermediaActionCustomerBuysCar.Parameter": {
      "CarUri": "http://localhost:5000/Cars/VW/2",
      "Price": 133
    }
  }
]
```

## Recommendations for route design
The extensions were build with some ideas about how routes should be build in mind. The Extensions do not enforce this design but it is useful to know the basic ideas.

- The API is entered by a root document which leads to all or some of the other `HypermediaObject`'s (see `HypermediaEntryPoint` in CarShack)
Examples
```
http://localhost:5000/entrypoint
```

- Collections like `Customers` are accessed through a root object (see `HypermediaCustomersRoot` in CarShack) which handles all actions which are not related to a specific customer. This also avoids that a collection directly answers with potentially unwanted Customers.
Examples
```
http://localhost:5000/Customers
http://localhost:5000/Customers/CreateQuery
http://localhost:5000/Customers/CreateCustomer
```

- Entities are accessed through a collection but do not host child Entities. These should be handled in their own collections. The routes to the actual objects should not matter, so no need to nest them. This helps to flatten the Controller hierarchy and avoids deep routes. If a placeholder variable is required in the route template name it _key_ (see Known Issues below).
Examples
```
http://localhost:5000/Customers/1
http://localhost:5000/Customers/1/Move
```

## Notes on serialization
- Enums in `HypermediaObjects` and Action parameters can be attributed using `EnumMember` to specify fixed string names for enum values. If no attribute is present the value will be serialized using `ToString()`
- `DateTime` and `DateTimeOffset` will be serialized using ISO 8601 notation: e.g. `2000-11-22T18:05:32.9990000+02:00`

## Known Issues
### QueryStringBuilder
Building URIs which contain a query string uses the QueryStringBuilder to serialize C# Objects (tricky). If the provided implementation does not work for you it is possible to pass an alternative to the init function.

Tested for:
- Classes, with nesting
- Primitives
- IEnumerable (List, Array, Dictionary)
- DateTime, TimeSpan, DateTimeOffset
- String
- Nullable

## Release Notes

### WebApiHypermediaExtensions v1.8.0

- Reworked initialization of the framework so a single method call on the service collection is enough
  - more general approach
  - configuration is now done using a lambda
- Removed unneccessary ProblemJsonFormatter. Instead use the configured formatter.

### WebApiHypermediaExtensions v1.7.0

- Add new HTTP methods available for operations:
  - PATCH
  - DELETE

### WebApiHypermediaExtensions v1.6.1

- Bumped supported projects:
  - from netcoreapp3.0 to netcoreapp3.1
  - from netstandard1.6 to netstandard2.0

### WebApiHypermediaExtensions v1.6.0

- Fix: Fix options were not passed down on init, so they were not active.
- Move UrlConfiguration and converter configuration to options so it can be configured and is not hidden.
  This enables more configurations when using AddHypermediaExtensions()

### WebApiHypermediaExtensions v1.5.0

- Actions now can be created with a prefilled parameter object. Its will be passed as `value` in the `action`  fields`
  Example: `CreateCustomerAction = new HypermediaFunction<CreateCustomerParameters, Task<Customer>>(CanCreateCustomer, DoCreateCustomer, new CreateCustomerParameters{Name = "John Doe"});`
- Fix: nullable enums are now also serialized as string (like enums)

### WebApiHypermediaExtensions v1.4.2

- Add `netcoreapp3.0` as target to support ASP.net Core 3.x



### WebApiHypermediaExtensions v1.4.1

- Add option to generate lowercase URLs
- FIX bug where ParameterBinder was not triggered
- Type parameter route matching is now case insensitive
- Replace [controler] and [action] token in routes so deconstructing routes works using the template matcher.
- Better exception messages
- Fix nullreference exception when generating routes for controller without RouteAttribute
- Responses for ProblemJson now have media type: 'application/problem+json'
- Add SourceLink to GitHub

### WebApiHypermediaExtensions v1.4.0

#### Features:
- Action parameter objects no longer need to be wrapped in an array
- Automatic rout key producers added, so in most cases no RouteKeyProducer is needed
- Automatic mapping of HTO parameter URLs to parameter object values by URL decomposition, also represented in generated JsonSchema
- Option to auto deliver JsonSchema for ActionParameter types by crawling assemblies for parameter objects
- Option to deliver virtual URLs to HTOs which are not jet developed to ease development
- IEnumerable and arbitrary Objects in HTO properties are now serialized to Siren
- Configuration via `IMvcCoreBuilder`
- Work on HypermediaClient
    - Use a single HttpClient instance
    - Add basic authentication

#### Fixes:
- Update `Microsoft.AspNetCore.Mvc` for security reasons.

### WebApiHypermediaExtensions v1.3.0

#### Features:
- Routes with multiple variable templates are now supported, also the dependency on route variable names is removed. The KeyProducers now handle that. See documentation for details.
- Multiple Relations are now allowed for Links
- Add configuration option to SirenBuilder so writing null properties in the JSON output can be disabled
- Allow HypermediaObjects with no self link as specified by Siren
- An Exception is thrown if a route template has parameters but no KeyProvider is specified
- Add extension methods for convenience when working with embedded entities
- Add Exception if null is passed to HypermediaObjectReference
- Add ExternalReference to work around situations where a route or URI can not be constructed by the framework.
- Added a ApiMap to the CarShack project which shows an overview on the navigation possibilities for the API
- Updated CarShack project to show new features: see 'Cars' routes with multiple variable templates
- Updated README.md

#### Refactoring:
- Generalize Formatter concept so there can be other Formatters
- Renamed HypermediaAction with return value to HypermediaFunction
- Simplified Siren builder
- Rename RoutKeyProducer to KeyProducer because functionality is not tied to Web Api
- Remove some reflections
- Renaming for clarity
- Now using NJsonSchema to generate JSON schemas
- HypermediaQueryResult: Remove NavigationQueries from constructor
- HypermediaQueryResult: Entities are no longer added by constructor
- Rename EmbeddedEntity to RelatedEntity to make usage more general
- Cleanup solution and folder structure, so now there is only one solution
- Extracted some shared functionality to Hypermedia.Util project.

#### Fixes:
- Most Attributes are now sealed for performance reasons
- QueryString builder now accepts null and returns string.Empty
- Fix create customer action did not set customer name
- Fix HypermediaQueryResult exposed Query property

#### Hypermedia Client Prototype:
There is a new project: HypemediaClient. This is a *prototype* which explores a the possibilities of a generic client which still has strong types for Hypermedia documents. To execute it see the test project: HypermediaClient.Test. The client expects a local CarShack service to communicate with.

### WebApiHypermediaExtensions v1.2.0
- ADD: It is now possible to configure generated URIs by providing a HypermediaUrlConfig (host, scheme) for links
- ADD: QueryStringBuilder can serialize IEnumerable, so it is possible to have queries containing List<>
- ADD: QueryStringBuilder can handle DateTimeOffset
- ADD: HypermediaObjectQueryReference now accept a key
- ADD: Siren JSON creation now uses ISO 8601 format for DateTime and DateTimeOffset
- ADD: Unit test project
- UPDATE: readme.md
- CHANGE: Enum handling: If enum has no EnumMember attribute no exception is thrown anymore
- CHANGE: Serialize actions and entities of embedded entities according to Siren specification
- FIX: QueryStringBuilder did not serialize nullables
- FIX: NavigationQueryBuilder next page was build but not valid
- FIX: DisablePagination() did not set PageOffset to 0
- FIX: Siren JSON creation:
  - Primitives like integers and bools were not serialized properly (serialized as string)
  - Null properties were not serialized to null but a string "null"
  - Nested classes in properties were serialized
  - Enum values were serialized as string

### WebApiHypermediaExtensions v1.1.0
- Added relations support for embedded Entities. The entities list is now filled with EmbeddedEntity objects
- Added extension methods for easy adding of embedded Entities `AddRange(..)` and `Add(..)`
- Updated CarShack demo project
- Added net452 as target framework
- Some renaming `DefaultHypermediaLinks` -> `DefaultHypermediaRelations`
- Work on README.md

### WebApiHypermediaExtensions v1.0.1
- Added XML Comments file

### WebApiHypermediaExtensions v1.0.0 release notes
- Initial release
