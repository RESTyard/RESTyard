# Roadmap WAHE 2.0

- make generic route key producer
- refactor HypermediaQueryResult and related to HypermediaActionResult, as it may be needed by other use cases
- generate method to decompose Url from ActionParameter to HypermediaObject (key) so self links can be used easy as ActionParameters
- move client to separate Nuget
- remove SingleParameterBinder and use schema references as discused in [this thread](https://github.com/kevinswiber/siren/issues/84)
- optional generic route for ActionParameter schemas
- serialize reference properties to siren properties as json object
	- serialize Lists/Collections
	- serialize Objects into properties
- serialize TimeSpan to Siren
- add development options
	- allow serialization of objects with no corresponding routes to ease development when creating a lot of HTO first
- rework MarkAsFavorite action on customer so it nolonger accepts a link, but an id. Example is misleading
- fix: content type of problem json: return "application/problem+json", ensure formatter is pressent
- fix: use Uri in problem json to be in line with RFC 7807, add remaining properties from rfc, adapt client
