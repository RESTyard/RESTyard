# CarShack API Guide

This is the authored manual for the CarShack demo API. It complements the machine-readable
schema (`/hypermedia-schema`) with the knowledge that cannot be derived from types:
workflows, conventions, and semantics.

## Getting started

Start at the entry point: `GET /EntryPoint`. Every other resource is reachable from there by
following links — do not construct URLs manually.

## Key workflows

### Browse and edit customers

1. Follow the `CustomersRoot` link from the entry point.
2. Use the `CreateQuery` action to get a filtered, paginated customer list.
3. On a customer, use `CustomerMove` to change the address, or `MarkAsFavorite`
   (only offered when the customer is not already a favorite).

### Cars

Follow the `CarsRoot` link from the entry point. Car resources are read-only in this demo.

## Conventions

- Responses are Siren (`application/vnd.siren+json`); actions describe their parameters
  with JSON Schema.
- Actions appear in a response only when they are currently executable — absence means
  "not available in this state", not "does not exist".
- The machine-readable schema of all types, links, and actions is served at
  `/hypermedia-schema`; the access groups used for schema filtering at `/schema/access-groups`.
