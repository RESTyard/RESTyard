# API Documentation

**Entry Point:** [Root](#root)

## Table of Contents

- [Root](#root)
- [Item](#item)

## API Map

```mermaid
graph LR
    Root["Root"]
    Item["Item"]

    Root -. "action: CreateItem" .-> Item
```

## Root

#### Classes

- `Root`

### Actions

<a id="root-createitem"></a>

#### CreateItem *(optional)*

Creates a new item.

**Returns:** [Item](#item)

## Item

#### Classes

- `Item`

**Referenced by:**

- [Root → CreateItem](#root-createitem) (action result)