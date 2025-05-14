# RESTyard

This project consists of a set of Extensions for ASP.NET Core projects. The purpose is to
assist in building restful Web services using the [Siren Hypermedia Format](https://github.com/kevinswiber/siren) with much less coding.
Using the Extensions it is possible to return HypermediaObjects as C# classes. Routes for HypermediaObjects and Actions are built using extended attribute routing.

Of course there might be some edge cases or flaws. Comments, suggestions, remarks and criticism are very welcome.

For a first feel there is a demo project called [CarShack](https://github.com/RESTyard/RESTyard/tree/master/Source/CarShack) which shows a great portion of the Extensions in use. It also shows how the routes were intended to be designed, although this is not enforced.

To develop C# client applications there is a generic REST client. 

## On nuget.org
The Extensions: [https://www.nuget.org/packages/RESTyard.AspNetCore](https://www.nuget.org/packages/RESTyard.AspNetCore)

The Client: [https://www.nuget.org/packages/RESTyard.Client/](https://www.nuget.org/packages/RESTyard.Client/)

## UI client
There is a partner project which aims for a generic UI client: [HypermediaUi](https://github.com/RESTyard/HypermediaUI)

## Documentation

Documentation can be found here: [RESTyard-Docs](https://restyard.github.io/RESTyard-Docs/)

## Predecessor

Formerly this project was named `Web Api Hypermedia Extensions`. We move to a new name so
we have a whole namespace for partner projects and it is more easy to find.

## 💚 Many thanks to our dear sponsors

<div style="display: flex; justify-content: space-around; align-items: flex-start;">
  <div style="margin-right: 10px;"> 
    <a href="https://www.bluehands.de" target="_blank" rel="noopener noreferrer">
      <img src="media/sponsors/bluehands-logo.png" alt="bluehands sponsor logo" style="height: 80px; width: auto; object-fit: cover;">
    </a>
  </div>
  <div style="margin-right: 10px;"> 
    <a href="https://data-cybernetics.com" target="_blank" rel="noopener noreferrer">
      <img src="media/sponsors/datacybernetics-logo.png" alt="data cybernetics sponsor logo" style="height: 80px; width: auto; object-fit: cover;">
    </a>
  </div>
</div>