# Roadmap WAHE 2.0

- make generic route key producer
- refactor HypermediaQueryResult and related to HypermediaActionResult, as it may be needed by other use cases
- generate method to decompose Url from ActionParameter to HypermediaObject (key) so self links can be used easy as ActionParameters
- move client to separate Nuget
- remove SingleParameterBinder and use schema references as discused in [this thread](https://github.com/kevinswiber/siren/issues/84)
- optional generic route for ActionParameter schemas

