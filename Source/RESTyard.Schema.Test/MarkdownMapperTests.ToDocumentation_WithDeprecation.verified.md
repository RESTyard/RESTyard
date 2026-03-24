# API Documentation

**Entry Point:** [EntryPoint](#entrypoint)

## Table of Contents

- [EntryPoint](#entrypoint)
- [OldEntity](#oldentity)
- [NewEntity](#newentity)

## API Map

```mermaid
graph LR
    EntryPoint["EntryPoint"]
    OldEntity["OldEntity"]
    NewEntity["NewEntity"]

    EntryPoint -- "oldResource" --> OldEntity
    EntryPoint -- "newResource" --> NewEntity
    EntryPoint -- "legacyItems" --> OldEntity
```

## EntryPoint

#### Classes

- `EntryPoint`

<a id="entrypoint-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| **[Deprecated]** oldResource *(optional)* | [OldEntity](#oldentity) | Use newResource instead |
| newResource *(optional)* | [NewEntity](#newentity) |  |

### Actions

<a id="entrypoint-oldaction"></a>

#### **[Deprecated]** OldAction *(optional)*

Use NewAction instead

<a id="entrypoint-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Description |
|---|---|---|---|
| **[Deprecated]** legacyItems *(optional)* | [OldEntity](#oldentity) | no | Use newResource link instead |

## **[Deprecated]** OldEntity

> This entity is deprecated. Use NewEntity instead.

#### Classes

- `OldEntity`

**Referenced by:**

- [EntryPoint](#entrypoint-links) (link: oldResource)
- [EntryPoint](#entrypoint-embedded) (embedded: legacyItems)

## NewEntity

#### Classes

- `NewEntity`

**Referenced by:**

- [EntryPoint](#entrypoint-links) (link: newResource)