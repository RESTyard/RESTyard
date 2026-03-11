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

| Relation | Target | Description |
|---|---|---|
| self | [Root](#root) |  |
| optionalLink *(optional)* | [Target](#target) |  |
| mandatoryLink | [Target](#target) |  |

<a id="root-actions"></a>

### Actions

| Action | Description | Links to |
|---|---|---|
| OptionalAction *(optional)* |  |  |
| MandatoryAction |  |  |

<a id="root-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Description |
|---|---|---|---|
| optionalEmbed *(optional)* | [Target](#target) | no |  |
| mandatoryEmbed | [Target](#target) | no |  |

## Target

#### Classes

- `Target`

**Referenced by:**

- [Root](#root-links) (link: optionalLink)
- [Root](#root-links) (link: mandatoryLink)
- [Root](#root-embedded) (embedded: optionalEmbed)
- [Root](#root-embedded) (embedded: mandatoryEmbed)