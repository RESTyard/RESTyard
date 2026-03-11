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

`Root`

### Links

| Relation | Target | Description |
|---|---|---|
| self | [Root](#root) |  |
| optionalLink *(optional)* | [Target](#target) |  |
| mandatoryLink | [Target](#target) |  |

### Actions

| Action | Description |
|---|---|
| OptionalAction *(optional)* |  |
| MandatoryAction |  |

### Embedded Entities

| Relation | Target | Collection | Description |
|---|---|---|---|
| optionalEmbed *(optional)* | [Target](#target) | no |  |
| mandatoryEmbed | [Target](#target) | no |  |

## Target

#### Classes

`Target`