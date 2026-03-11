# Car Shop API

**Entry Point:** [EntryPoint](#entrypoint)

## EntryPoint

#### Classes

- `EntryPoint`

<a id="entrypoint-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| self *(optional)* | [EntryPoint](#entrypoint) |  |
| customers *(optional)* | [CustomersRoot](#customersroot) |  |
| cars *(optional)* | [CarsRoot](#carsroot) |  |

## CustomersRoot

#### Classes

- `CustomersRoot`

<a id="customersroot-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Description |
|---|---|---|---|
| item *(optional)* | [Customer](#customer) | yes |  |

**Referenced by:**

- [EntryPoint](#entrypoint-links) (link: customers)

## CarsRoot

#### Classes

- `CarsRoot`

<a id="carsroot-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Description |
|---|---|---|---|
| item *(optional)* | [Car](#car) | yes |  |

**Referenced by:**

- [EntryPoint](#entrypoint-links) (link: cars)

## Customer

#### Classes

- `Customer`

<a id="customer-properties"></a>

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| name | string | no |  |
| age | integer | no |  |

<a id="customer-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| self *(optional)* | [Customer](#customer) |  |

<a id="customer-actions"></a>

### Actions

| Action | Description | Links to |
|---|---|---|
| MarkAsFavorite *(optional)* |  |  |
| BuyCar *(optional)* |  |  |

  | Parameter | Type | Required | Description |
  |---|---|---|---|
  | carId | string | no |  |

**Referenced by:**

- [CustomersRoot](#customersroot-embedded) (embedded: item)

## Car

#### Classes

- `Car`

**Referenced by:**

- [CarsRoot](#carsroot-embedded) (embedded: item)