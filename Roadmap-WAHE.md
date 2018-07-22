# Roadmap WAHE 2.0

- [done] make generic route key producer
- [done] generate method to decompose Url from ActionParameter to HypermediaObject (key) so self links can be used easy as ActionParameters
- [done] move client to separate Nuget
- [done] optional generic route for ActionParameter schemas
- [done] serialize reference properties to siren properties as json object
	- serialize Lists/Collections
	- serialize Objects into properties
- [done] add development options
	- allow serialization of objects with no corresponding routes to ease development when creating a lot of HTO first

- remove SingleParameterBinder and use schema references as discused in [this thread](https://github.com/kevinswiber/siren/issues/84)
- refactor HypermediaQueryResult and related to HypermediaActionResult, as it may be needed by other use cases
- fix: content type of problem json: return "application/problem+json", ensure formatter is pressent
- fix: use Uri in problem json to be in line with RFC 7807, add remaining properties from rfc, adapt client
- rework demo app to have better operation names
	- rework MarkAsFavorite action on customer so it nolonger accepts a link, but an id. Example is misleading
	- add list/object property examples
- fix: prevent endless recursion for cyclic object property serialization