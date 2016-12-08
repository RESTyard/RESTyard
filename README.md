# Web Api Hypermedia Extensions
This project consists of a set of Extensions for Web Api 2 Core projects. The purpose is to
assist in building restful Web services using the [Siren Hypermedia Format](https://github.com/kevinswiber/siren) with much less coding.
Using the Extensions it is possible to return HypermediaObjects as C# classes. Routes for HypermediaObjects and Actions are built using extended attribute routing.

Of course there might be some edge cases or flaws. Comments, suggestions, remarks and criticism are very welcome.

For a first feel there is a demo project called [CarShack](https://github.com/bluehands/WebApiHypermediaExtensions/tree/master/Source/DemoServer/CarShack/src/CarShack) which shows a great portion of the Extensions in use. It also shows how the routes were intended to be designed, although this is not enforced.

The Extensions on nuget.org: [https://www.nuget.org/packages/WebApiHypermediaExtensionsCore](https://www.nuget.org/packages/WebApiHypermediaExtensionsCore)

## Key concepts
The Extensions allow you to build a restful web server without building a Siren C# class by hand and assigning URIs to Links and embedded Entities. For this the Extensions provide two main components: the `HypermediaObject` class and new RouteAttributes extending the Web Api RouteAttributes.

HypermediaObjects returned from Controllers will be formatted as Siren. All contained referenced HypermediaObjects (e.g. Links and embedded Entities), Actions, and Parameter types (of Actions) are automatically resolved and properly inserted into the Siren document, by looking up attributed routes.

## Using it in a project
To use the Extensions just call `AddHypermediaExtensions()` when adding MCV in `Startup.cs`:

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    var builder = services.AddMvc(options =>
    {
        ...
        // Initializes and adds the Hypermedia Extensions
        options.AddHypermediaExtensions();
    });
            
    // Required by Hypermedia Extensions
    services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
    ...
}
```
Also the Extension needs a `IActionContextAccessor` service to work.

## HypermediaObject
This is the base class for all entities (in Siren format) which shall be returned from the server. They will be formatted as Siren Hypermedia by the included formatter. An Example from the demo project CarShack:

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
- Properties with a HypermediaAction type will be added as Actions, but only if CanExecute returns true. Any required parameters will be added in the "fields" section of the Siren document.
- Other `HypermediaObject`s can be embedded by adding them as a `HypermediaObjectReferenceBase` type to the entities collection Property (not shown in this example, see HypermediaCustomerQueryResult in the demo project).
- Links to other `HypermediaObject`s can be added to the Links collection Property, also as `HypermediaObjectReferenceBase` (not shown in this example, see HypermediaCustomersRoot in the demo project).
- Properties, Actions and `HypermediaObject`s themselves can be attributed e.g. to give them a fixed name.

**Important**
All `HypermediaObject`'s used in a Link or as embedded Entity and all `HypermediaAction`'s in a `HypermediaObject` require that there is an attributed route for their Type. Otherwise the formatter is not able to resolve the URI and will throw an Exception.

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
[HttpPostHypermediaAction("CreateCustomer", typeof(HypermediaAction<CreateCustomerParameters, Task<Customer>>))]
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

Parameters for actions may define a route which provides additional type information to the client. These routes will be added to the Siren fields object as "class".

```csharp
[HttpGetHypermediaActionParameterInfo("CreateCustomerParametersType", typeof(CreateCustomerParameters))]
public ActionResult NewCustomerRequestType()
{
    var schema = JsonSchemaFactory.Generate(typeof(CreateCustomerParameters));
    return Ok(schema);
}
```

### Routes with a placeholder in the route template
For access to entities a route template may contain placeholder variables like _key_ in the example below. If a `HypermediaObject` is referenced, e.g. the self link or a link to another Customer, the formatter must be able to create the URI to the linked `HypermediaObject`. To propperly fill the placeholder variables for such routes a `RouteKeyProducer` is required. The formatter will call the producer if he has a instance of the referenced Object (e.g. from `HypermediaObjectReference.Resolve()`) and passes it to the `IRouteKeyProducer:GetKey()` function. This function must return an anonymous object filled with a property for each placeholder variable to be filled in the `HypermediaObject`'s route, here _key_.

A `RouteKeyProducer` is added directly to the Attributed route as a Type and will be instantiated once by the framework.

``` csharp
[HttpGetHypermediaObject("Customers/{key:int}", typeof(HypermediaCustomer), typeof(CustomerRouteKeyProducer))]
public async Task<ActionResult> GetEntity(int key)
{
    ...
}
```

For a `HypermediaObjectKeyReference` the formatter creates an anonymous object by filling it with the key retrieved from the reference. So the placeholder variable must be called _key_ for such routes.
``` csharp
new { key = reference.GetKey() };

```

By design the Extension encourages routes to not have multiple keys in the route template. Also only routes to `HypermediaObject` may have a key. It is recomended that a route template has at most one placeholder variable and it is named `key`.
See `CustomerRouteKeyProducer` in the demo project for an example.

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

## Recommendations for route design
The extensions were build with some idears about how routes should be build in mind. The Extensions do not enforce this design but it is useful to know the basic idears.

- The api is entered by a root document which leads to all or some of the other `HypermediaObject`'s (see `HypermediaEntryPoint` in CarShack)
Examples
```
http://localhost:5000/entrypoint
```

- Collections like `Customers` are accessed through a root object (see `HypermediaCustomersRoot` in CarShack) which handles all actions which are not related to a specific customer. Tis also avoids that a colection directly answers with potentially unwanted Customers.
Examples
```
http://localhost:5000/Customers
http://localhost:5000/Customers/CreateQuery
http://localhost:5000/Customers/CreateCustomer
```

- Entities are accessed through a collection but do not host child Entities. These should be handled in their own collections. The routes to the actual objects should not matter, so no need to nest them. This helps to flatten the Controller hierarchy and avoids deep routes. If a placeholder variable is required in the route templae name it _key_ (see Known Issues below).
Examples
```
http://localhost:5000/Customers/1
http://localhost:5000/Customers/1/Move
```

## Known Issues
### QueryStringBuilder
Building URIs containing a query string uses the QueryStringBuilder to serialize C# Objects. This builder might not work for complex types. In that case you can provide your own implementation on init.

### HypermediaObjectKeyReference filling of placeholder variables
When using a `HypermediaObjectKeyReference` for a `HypermediaObject` which has placeholder variables in it's route template the formatter generated anonymous object.
It contains only one property named `key`. So if a route template has more than one placeholder variable or it is named differently the route can not be resolved. In such a scenario the `RouteKeyProvider` is not called.

##Release Notes
###WebApiHypermediaExtensions v1.1.0
- Added relations support for embedded Entities. The entities list is now filled with EmbeddedEntity objects
- Added extension methods for easy adding of embedded Entities `AddRange(..)` and `Add(..)`
- Updated CarShack demo project
- Added net452 as target framework
- Some renaming `DefaultHypermediaLinks` -> `DefaultHypermediaRelations`
- Work on README.md

###WebApiHypermediaExtensions v1.0.1
- Added XML Comments file

###WebApiHypermediaExtensions v1.0.0 release notes
- Initial release