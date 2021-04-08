# TODO
List of things to come...

## Framework

- rename private infrastructure function and make it public so core components can be changed again `WebApi.HypermediaExtensions.WebApi.ExtensionMethods.MvcOptionsExtensions.AddHypermediaExtensions`

## Actions should return a defined resource
Develop a HTO which should be returned when executing an Action. This HTO represents the Action resource which should be accessible by a client.

Properties which might be worth considering
- State: New, Pending, Wait, Finished, Error..
- Requested by user
- Start timestamp
- End timestamp
- As embedded resource the result of the action e.g. a Query, NewCustomer
- As embedded resource the parameters
- In error case: As embedded resource an error object, close to ProblemJson

## Add complete list of default relations
- see RFC [Link Relations](https://www.iana.org/assignments/link-relations/link-relations.xhtml)

## Siren classes
- Inherit classes so HTO combines all of them. Use multiple inheritance.
- Siren class can be set at runtime not only statically via attribute. Perhaps have classes collection on HypermediaObject and HypermediaObjectKeyReference?.

## Implement a HTO comparer
- To ease unit testing

## Speedup serialization
Generate expressions and compile function for entities on first serialization and cache it. This way reflections can be reduced.

## Abstract from entities list in HTO
Just allow References to HTOs and lists of HTOs.

- Less coupling to Siren format
- More natural to C#

## Add logging
- Configurable

# Future
Just some ideas

## Automatically generate Controllers and routes
Basically there seems to be only 3 Operations: Get one Entity, Get multiple Entities, execute Action (Query/Command?)
Maybe the controllers can be generic so we only have to implement some functions/interfaces and pass those

## Automatically generate API maps
- Provide them as resource

## Provide a route which returns all HTO classes and Action parameters as JsonSchema
- Can be used by clients while testing, to ensure the server is still what it expects him to be
- Can be used by clients to generate HCO (HypermediaClientObject)
