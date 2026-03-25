# API Documentation

**Entry Point:** [A](#a)

## Table of Contents

- [A](#a)
- [B](#b)
- [C](#c)

## API Map

```mermaid
graph LR
    A["A"]
    B["B"]
    C["C"]

    A -- "next" --> B
    B -- "next" --> C
    C -- "back" --> A
```

## A

#### Classes

- `A`

<a id="a-links"></a>

### Links

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| next *(optional)* | [B](#b) |  |  |

**Referenced by:**

- [C](#c-links) (link: back)

## B

#### Classes

- `B`

<a id="b-links"></a>

### Links

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| next *(optional)* | [C](#c) |  |  |

**Referenced by:**

- [A](#a-links) (link: next)

## C

#### Classes

- `C`

<a id="c-links"></a>

### Links

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| back *(optional)* | [A](#a) |  |  |

**Referenced by:**

- [B](#b-links) (link: next)