# Todo list for RESTyard NEXT

## HTO

- generate method to get info on hto for application model
- use key attributes to also generate an .ToLink method with strong typing
- allow for non typed properties, maybe as attribute `[IsJsonString]` so it is passed to formatter?
  - what about schemas?
  - Use `JsonElement`? use anonymous object to not be tied to system text json? 
- Allow for classes to be dynamic e.g. a aps net configuration gives a host for class schemas
- carry over default values from HTO to SirenHto for OpenApi 
- allow referencing parameter actions by aprameter type only e.g. `SetNameOp : HypermediaAction<SetNameParameters>`
  - `SetNameOp` seems redundant
  - maybe discover controller action by parameter alone,
  - how to do this for parameterless actions, maybe in Action attribute?
- add option/helper to add self link, maybe attribute `[AddSelfLink(relations=["self])]` or `SelfLink` type? 
- add `mandatory` attribute for links and entities
- review siren container if top level can also be an `entity`
- Add `ToPlain()` function wich only contains properties

## Controllers

- make action controllers declare what / if and action returns a link -> for API map
- Use `[ProducesResponseType<ProductHto>(StatusCodes.Status200OK)]` for route resolver so we do not need a custom attribute
  - also look into `[Consumes(MediaTypeNames.Application.Json)]` and `[Produces]`
  - see: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-9.0&tabs=minimal-apis#include-openapi-metadata-for-endpoints
  - see: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-9.0&tabs=controllers#describe-response-types
  - see: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-9.0&tabs=controllers#use-attributes-to-add-metadata
  - see: https://andrewlock.net/introduction-to-the-apiexplorer-in-asp-net-core/
  - see: https://learn.microsoft.com/en-us/aspnet/core/web-api/action-return-types?view=aspnetcore-9.0#actionresult-vs-iactionresult
- validate hto keys exist in route template, get from ApiExplorer
- move to non derived Hypermedia attributes
- create controller function `Created` which accepts a `myhto.ToLink()`
- improve `CreatedQuery` so it is not so special and not tied to a type -> maybe should work for all action parameters
  After all it is just a get (action?!) with all parameters in the query string
  - ensure that only query strings are build for existing gets (actions?!) which match the parameters 
- helper extension to set ContentType for siren results


## Application model

- generate api description and api map
  - add function to htos (source generated) which return linked, embedded entities and actions(with mandatory information)
- add endpoint for Hypermedia Api spec