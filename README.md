# Web Api Hypermedia Extensions (beta)
This project consists of some Extensions for a Web Api 2 Core. The target is to
assist in building a restful Web services using [Siren Hypermedia Format](https://github.com/kevinswiber/siren) with much less coding.
Using the Extensions it is possible to return HypermediaObjects as C# classes. Routes for HypermediaObjects and Actions are build using extended attribute routing.

Of course there migth be some edge cases or flaws. Comments, suggestions, remarks and critic is very welcome.

For a first feel there is a demo project called `CarShack` which shows a great portion of the Extensions in use. Is also shows how the routes were intended to be designed, although  it is not enforced.

## Using it in a project
To use the Extensions just call `AddHypermediaExtensions()` when adding MCV in `Startup.cs`:

``` json
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
The Extensions allow to build a restful web server without building a Siren C# class by hand and assigning URI's to Links and embedded Entities. For this the Extensions provide two main components: the `HypermediaObject` class and new RouteAttributes extending the Web Api RouteAttributes.

HypermediaObjects returned from Controllers will be formatted as Siren. All contained referenced HypermediaObjects (e.g. Links and embedded Entities), Actions, and Parameter types (of Actions) are automatically resoved and propperly inserted into the Siren document, by looking up attributed routes.

### HypermediaObject
This is the base class for all entities (in Siren format) which shall be returned from the server. They will be formatted as Siren Hypermedia by the included formatter. An Example from the demo project CarShack:

```json
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
- Properties with a HypermediaAction type will be added as Actions, but only if CanExecute returns true. If they require parameters it will be added ind the "fields" section of teh Siren document.
- Other `HypermediaObject`s can be embedded by adding them as a `HypermediaObjectReferenceBase` type to the entities collection Property. (Not in this example, see HypermediaCustomerQueryResult in the demo projeect)
- Links to other `HypermediaObject` can be added to the Links collection Property, also as `HypermediaObjectReferenceBase`.
  (Not in this example, see HypermediaCustomersRoot in the demo projeect)
- Properties, Actions and `HypermediaObject`s them selve can be attributed e.g. to give a fixed name.

### Attributed routes
The included SirenFormatter will build required links to other routes. At startup all routes attributed with:
- `HttpGetHypermediaObject`
- `HttpPostHypermediaAction`
- `HttpGetHypermediaActionParameterInfo`

will be placed in an internal register.

So for every `HypermediaObject` there must be a route with matching type.
Example form the demo project CustomerRootController:
``` json 
[HttpGetHypermediaObject("", typeof(HypermediaCustomersRoot))]
public ActionResult GetRootDocument()
{
    return Ok(customersRoot);
}
```

The same is valid for Actions:

```json
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
Siren specifies that to trigger an action a array of parameters is posted to the action route. To avoid wrapping parameters in a array class there is the SingleParameterBinder for convenience.

Parameters for actions may define a route which provide additional type information to the client. These routes will be added to the Siren fields object as "class".

```json
[HttpGetHypermediaActionParameterInfo("CreateCustomerParametersType", typeof(CreateCustomerParameters))]
public ActionResult NewCustomerRequestType()
{
    var schema = JsonSchemaFactory.Generate(typeof(CreateCustomerParameters));
    return Ok(schema);
}
```

####Routes with a key in the route template
By design the Extension encurage that no route has multiple keys in the route template. Also only routes to HypermediaObject may have a key. If so the RouteAttribute needs a `RouteKeyProducer` type:

``` json
[HttpGetHypermediaObject("", typeof(HypermediaCustomer), typeof(CustomerRouteKeyProducer))]
public async Task<ActionResult> GetEntity(int key)
{
    ...
}
```

When resolving routes to such an `HypermediaObject` the Formatter calls the `RouteKeyProducer` providing the `HypermediaObject` to generate a key from it. See `CustomerRouteKeyProducer` for an example.

#### Queries
Client shall not build query strings. Instead they post a json object to an `HypermediaAction` and receive the URI to the desired queryresult in the `Location` header.
``` json
[HttpPostHypermediaAction("CreateQuery", typeof(HypermediaAction<CustomerQuery>))]
public ActionResult NewQueryAction([SingleParameterBinder(typeof(CustomerQuery))] CustomerQuery query)
{
    ...
    // Will create a Location header with a URI to the result.
    return this.CreatedQuery(typeof(HypermediaCustomerQueryResult), query);
}
```

Ther must be a companion route which receives the query object and returns the query result:
``` json
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
Building URI's containing a query string uses the QueryStringBuilder to serialize C# Objects. This builder migth not work for complex types. If so you can provide your own implementation in on init.

### Embedded entities do not conatain a "rel"
This will be done in the future.