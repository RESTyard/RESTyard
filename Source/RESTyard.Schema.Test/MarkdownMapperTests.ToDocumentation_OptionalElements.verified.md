# API Documentation

**Entry Point:** [Root](#root)

## Table of Contents

- [Root](#root)
- [Target](#target)

## API Map

```mermaid
graph LR
    Root["Root"]
    Target["Target"]

    Root -- "optionalLink" --> Target
    Root -- "mandatoryLink" --> Target
    Root -- "optionalEmbed" --> Target
    Root -- "mandatoryEmbed" --> Target
```

## Root

#### Classes

- `Root`

<a id="root-links"></a>

### Links

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| self | [Root](#root) |  |  |
| optionalLink *(optional)* | [Target](#target) |  |  |
| mandatoryLink | [Target](#target) |  |  |

### Actions

<a id="root-optionalaction"></a>

#### OptionalAction *(optional)*

<a id="root-mandatoryaction"></a>

#### MandatoryAction

<a id="root-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Access Groups | Description |
|---|---|---|---|---|
| optionalEmbed *(optional)* | [Target](#target) | no |  |  |
| mandatoryEmbed | [Target](#target) | no |  |  |

## Target

#### Classes

- `Target`

**Referenced by:**

- [Root](#root-links) (link: optionalLink)
- [Root](#root-links) (link: mandatoryLink)
- [Root](#root-embedded) (embedded: optionalEmbed)
- [Root](#root-embedded) (embedded: mandatoryEmbed)