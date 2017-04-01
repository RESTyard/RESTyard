# Web Api Hypermedia Extensions Demo Project: CarShack

Demonstrates the use of the Web Api Hypermedia Extensions for building a restful API. The server will respond with [Siren](github.com/kevinswiber/siren) as Hypermedia format. It provides a Hypermedia specification which is well suited for DDD (Domain Driven Development) style of APIs.

A graph of the API can be found in `/ApiMap/CarShackApiMap.png`. Nodes contain the class of the hypermedia document and edges are labeled with the symbols leading to the next document.

For example calls to the API there is a postman collection with example routes in the project folder.

Note that a real client should **only** know the route to the EntryPoint and navigate the Rest API by the provided links (symbols).

## Fist places to look at
1. `/Controllers/EntryPoint/EntryPointController.cs`: Here the client starts using the API.

2. `/Controllers/Customers/CustomersRootController.cs`: Contains all routes used by the CustomersRoot Hypermedia.
 Provides access the collection of customers and actions not related to a single Customer. See HypermediaCustomersRoot for the counterpart of the routes.

3. `/Controllers/Customers/CustomerController.cs`: Contains all routes used by and for the Customers Hypermedia.
 Responsible for actions related to a specific Customer. See HypermediaCustomer for the counterpart of the routes.

4. `/Hypermedia/`: This folder contains the HypermediaObjects used by the Controllers.

5. `startup.cs:ConfigureServices()`: Shows how to add the Hypermedia Extensions to the project

## Example Routes
These routes are also contained in the Postman collection ready for import.

The entrypoint to the API. This is the only adressa a client must known.
`GET`: [http://localhost:5000/entrypoint](http://localhost:5000/entrypoint)

The collection of Customers.
`GET`: [http://localhost:5000/Customers](http://localhost:5000/Customers)

A specific Customer.
`GET`: [http://localhost:5000/Customers/1](http://localhost:5000/Customers/1)

###Actions
####Action which requires no parameters
`POST`: [http://localhost:5000/Customers/1/MarkAsFavorite](http://localhost:5000/Customers/1/MarkAsFavorite)

**Note**: This action can only be called once. It changes the Customer state and the action is no longer available. Calling a 2nd time will provide a ProblemJson.

####A action which requires a parameter as JSON
```json
[{
	"NewAddress": {
		"Address": "Mainstreet 3 Hometown"
	}
}]
```

`POST`: [http://localhost:5000/Customers/1/Move](http://localhost:5000/Customers/1/Move)

####Action parameter type information
Action may specify the allowed parameter in the `class` propertie in the Siren document. This link can be followed to retriefe type informatein, here JSON Schema.
`GET`: [http://localhost:5000/Customers/CreateCustomerParametersType](http://localhost:5000/Customers/CreateCustomerParametersType)

###Queries
Queries are build by the server to avoid building URIs and querie strings on the client.
`POST` the following JSON:

```json
[{"CustomerQuery": 
	{
	  "Filter": {
	  	"MinAge": "21"
	  },
	  "SortBy":{
	  	"PropertyName":"Age",
	    "SortType": "Ascending"
	  },
	  "Pagination": {
	  	"PageSize" : "4",
	  	"PageOffset":"2"
	  }
	}
}]
```
`POST`: [http://localhost:5000/Customers/CreateQuery](http://localhost:5000/Customers/CreateQuery)

In the response header will be a Location URI which will lead to the Query result:
[http://localhost:5000/Customers/Query?Pagination.PageSize=4&Pagination.PageOffset=2&SortBy.PropertyName=Age&SortBy.SortType=Ascending&Filter.MinAge=21](http://localhost:5000/Customers/Query?Pagination.PageSize=4&Pagination.PageOffset=2&SortBy.PropertyName=Age&SortBy.SortType=Ascending&Filter.MinAge=21)


