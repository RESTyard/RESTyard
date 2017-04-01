# Hypermedia Client Lib
_Experimental_ project to create a Client which allows typed access to a Rest Api.
- Fluent Link navigation
- Transparent Link and Action resolving with deserialization
- Action handling

There is still a lot to do:
- Exception handling
- Error responses by the api
- Authentification/Authorization
- Caching
...

Assumptions:
- Siren (single paramert json objects like in the CarShack demo)
- Deterministic Links, a link will always lead to the same class of ressource. E.g. a Customer.