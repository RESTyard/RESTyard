# Roadmap WAHE 2.0
- create client nugget
- remove SingleParameterBinder and use schema references as discussed in [this thread](https://github.com/kevinswiber/siren/issues/84)
- refactor HypermediaQueryResult and related to HypermediaActionResult, as it may be needed by other use cases
- fix: content type of ProblemJson: return "application/problem+json", ensure formatter is pressent
- fix: use Uri in ProblemJson to be in line with RFC 7807, add remaining properties from rfc, adapt client
- rework demo app to have better operation names
	- rework MarkAsFavorite action on customer so it no longer accepts a link, but an id. Example is misleading
	- add list/object property examples
- fix: prevent endless recursion for cyclic object property serialization