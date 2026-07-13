# CarShack API Guide

## What this API is for

CarShack is a demo API for a small car dealership. It lets a consumer manage the dealership's
**customers** and browse its **cars**, and it models a few realistic business processes on top of
those resources (finding customers, keeping customer records up to date, marking preferred
customers, and buying a car). It exists as the reference example for building a RESTyard Siren
hypermedia API — so alongside the business domain it also demonstrates the framework's conventions.

This guide explains the *intent* behind the API. The machine-readable structure of every type,
link, and action is served separately as the schema (`/hypermedia-schema`); this document covers
the meaning the schema cannot.

## Key resources

| Resource | What it represents | Notes |
|---|---|---|
| **Entry point** | The starting resource; the map to everything else | Always begin here (`GET /EntryPoint`) |
| **Customers root** | The collection of customers and the operations over it | Create customers, run filtered/paginated queries |
| **Customer** | A single person the dealership does business with | Has an address, an age, and a "favorite" flag |
| **Cars root** | The collection of cars offered | Browse-only in this demo |
| **Car** | A single vehicle in the catalogue | Read-only |

Domain vocabulary:

- **Favorite** — a customer the dealership treats as preferred. A customer is either a favorite or
  not; the `MarkAsFavorite` action is only offered when they are not already one.
- **Query** — a saved, parameterized search over customers (filter + sorting + pagination). Running
  a query yields a customer result list rather than the whole collection.

## Processes and workflows

These are the multi-step business flows the API supports. Each spans several resources and actions —
follow the links and actions offered on each response rather than constructing URLs yourself.

### Find and manage customers

1. From the entry point, follow the **CustomersRoot** link.
2. Use the **CreateQuery** action to get a filtered, sorted, paginated list of customers.
3. Open a customer from the result list.
4. On the customer, use **CustomerMove** to update their address, or **MarkAsFavorite** to flag them
   as preferred (offered only when they are not already a favorite).

### Onboard a new customer

1. From the entry point, follow the **CustomersRoot** link.
2. Use the **CreateCustomer** action with the new customer's details.
3. Follow the link in the response to the freshly created customer resource.

### Browse cars and buy one

1. From the entry point, follow the **CarsRoot** link to browse the catalogue.
2. Open an individual **Car** to see its details.
3. Purchasing is initiated from a customer via the **BuyCar** action — a car is always bought *by a
   customer*, which is why the action lives on the customer resource, not the car.

## Conventions

- Responses are Siren (`application/vnd.siren+json`); actions describe their parameters with JSON Schema.
- Actions appear in a response only when they are currently executable — absence means "not available
  in this state right now", not "does not exist". This is how the API communicates what a business
  process allows at each step.
- Navigate by following links; do not hand-build URLs.
- The machine-readable schema of all types, links, and actions is at `/hypermedia-schema`; the access
  groups used for schema filtering are at `/schema/access-groups`.
