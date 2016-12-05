# Web Api Hypermedia Extensions
This project consists of a set of Extensions for Web Api 2 Core projects. The purpose is to
assist in building restful Web services using the [Siren Hypermedia Format](https://github.com/kevinswiber/siren) with much less coding.
Using the Extensions it is possible to return HypermediaObjects as C# classes. Routes for HypermediaObjects and Actions are built using extended attribute routing.

Of course there might be some edge cases or flaws. Comments, suggestions, remarks and criticism are very welcome.

For a first feel there is a demo project called `CarShack` which shows a great portion of the Extensions in use. It also shows how the routes were intended to be designed, although this is not enforced.

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

## Key concepts
The Extensions allow you to build a restful web server without building a Siren C# class by hand and assigning URIs to Links and embedded Entities. For this the Extensions provide two main components: the `HypermediaObject` class and new RouteAttributes extending the Web Api RouteAttributes.

HypermediaObjects returned from Controllers will be formatted as Siren. All contained referenced HypermediaObjects (e.g. Links and embedded Entities), Actions, and Parameter types (of Actions) are automatically resolved and properly inserted into the Siren document, by looking up attributed routes.

### HypermediaObject
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

### Attributed routes
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

#### Routes with a key in the route template
By design the Extension encourages routes to not have multiple keys in the route template. Also only routes to HypermediaObject may have a key. In that case the RouteAttribute needs a `RouteKeyProducer` type:

``` csharp
[HttpGetHypermediaObject("", typeof(HypermediaCustomer), typeof(CustomerRouteKeyProducer))]
public async Task<ActionResult> GetEntity(int key)
{
    ...
}
```

When resolving routes to such a `HypermediaObject` the Formatter calls the `RouteKeyProducer` providing the `HypermediaObject` to generate a key from it. See `CustomerRouteKeyProducer` for an example.

#### Queries
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

## Known Issues
### QueryStringBuilder
Building URIs containing a query string uses the QueryStringBuilder to serialize C# Objects. This builder might not work for complex types. In that case you can provide your own implementation on init.

### Embedded entities do not contain a "rel"
This will be done in the future.