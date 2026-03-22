# CarShack API

RESTyard demo API for managing cars and customers

**Version:** 1.2.3

**Entry Point:** [Entrypoint](#entrypoint)

## Table of Contents

- [Entrypoint](#entrypoint)
- [CustomersRoot](#customersroot)
- [CarsRoot](#carsroot)
- [CustomerQueryResult](#customerqueryresult)
- [Customer](#customer)
- [DerivedCar](#derivedcar)
- [Car](#car)
- [CustomerPurchaseHistory](#customerpurchasehistory)
- [CustomerPurchase](#customerpurchase)
- [Truck](#truck)
- [CarImage](#carimage)
- [CarInsurance](#carinsurance)
- [NextLevelDerivedCar](#nextlevelderivedcar)

## API Map

```mermaid
graph LR
    Truck["Truck"]
    Entrypoint["Entrypoint"]
    CarsRoot["CarsRoot"]
    Car["Car"]
    CarImage["CarImage"]
    CarInsurance["CarInsurance"]
    DerivedCar["DerivedCar"]
    NextLevelDerivedCar["NextLevelDerivedCar"]
    CustomersRoot["CustomersRoot"]
    CustomerPurchase["CustomerPurchase"]
    CustomerPurchaseHistory["CustomerPurchaseHistory"]
    Customer["Customer"]
    CustomerQueryResult["CustomerQueryResult"]

    Entrypoint -- "CustomersRoot" --> CustomersRoot
    Entrypoint -- "CarsRoot" --> CarsRoot
    CarsRoot -- "NiceCar" --> DerivedCar
    CarsRoot -- "SuperCar" --> Car
    DerivedCar -- "DerivedLink" --> Customer
    DerivedCar -- "item" --> Customer
    NextLevelDerivedCar -- "DerivedLink" --> Customer
    NextLevelDerivedCar -- "item" --> Customer
    CustomersRoot -- "all" --> CustomerQueryResult
    CustomersRoot -- "BestCustomer" --> Customer
    CustomerPurchaseHistory -- "Purchases" --> CustomerPurchase
    Customer -- "PurchaseHistory" --> CustomerPurchaseHistory
    CustomerQueryResult -- "Next" --> CustomerQueryResult
    CustomerQueryResult -- "Previous" --> CustomerQueryResult
    CustomerQueryResult -- "Last" --> CustomerQueryResult
    CustomerQueryResult -- "All" --> CustomerQueryResult
    CustomerQueryResult -- "Customers" --> Customer
```

## Entrypoint

#### Title

Entry to the Rest API

#### Classes

- `Entrypoint`

<a id="entrypoint-properties"></a>

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| MyType | [type](#definition-type) | no |  |

<a id="entrypoint-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| CustomersRoot | [CustomersRoot](#customersroot) |  |
| CarsRoot | [CarsRoot](#carsroot) |  |
| self | [Entrypoint](#entrypoint) |  |

## CustomersRoot

#### Title

The Customers API

#### Classes

- `CustomersRoot`

<a id="customersroot-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| all | [CustomerQueryResult](#customerqueryresult) |  |
| BestCustomer | [Customer](#customer) |  |
| self | [CustomersRoot](#customersroot) |  |

<a id="customersroot-actions"></a>

### Actions

| Action | Description | Links to |
|---|---|---|
| CreateCustomer |  |  |

  | Parameter | Type | Required | Description |
  |---|---|---|---|
  | Name | string | no |  |
| CreateQuery |  |  |

  | Parameter | Type | Required | Description |
  |---|---|---|---|
  | Pagination | object | no |  |
  | SortBy | object | no |  |
  | Filter | object | no |  |

**Referenced by:**

- [Entrypoint](#entrypoint-links) (link: CustomersRoot)

## CarsRoot

#### Title

The Cars API

#### Classes

- `CarsRoot`

<a id="carsroot-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| NiceCar | [DerivedCar](#derivedcar) |  |
| SuperCar | [Car](#car) |  |
| self | [CarsRoot](#carsroot) |  |

<a id="carsroot-actions"></a>

### Actions

| Action | Description | Links to |
|---|---|---|
| UploadCarImage |  |  |

  | Parameter | Type | Required | Description |
  |---|---|---|---|
  | Text | string | no |  |
  | Flag | boolean | no |  |
| UploadInsuranceScan |  |  |

**Referenced by:**

- [Entrypoint](#entrypoint-links) (link: CarsRoot)

## CustomerQueryResult

#### Title

Query result on Customer

#### Classes

- `CustomersQueryResult`

<a id="customerqueryresult-properties"></a>

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| TotalEntities | integer | no |  |
| CurrentEntitiesCount | integer | no |  |

<a id="customerqueryresult-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| Next *(optional)* | [CustomerQueryResult](#customerqueryresult) |  |
| Previous *(optional)* | [CustomerQueryResult](#customerqueryresult) |  |
| Last *(optional)* | [CustomerQueryResult](#customerqueryresult) |  |
| All *(optional)* | [CustomerQueryResult](#customerqueryresult) |  |
| self | [CustomerQueryResult](#customerqueryresult) |  |

<a id="customerqueryresult-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Description |
|---|---|---|---|
| Customers | [Customer](#customer) | yes |  |

**Referenced by:**

- [CustomersRoot](#customersroot-links) (link: all)
- [CustomerQueryResult](#customerqueryresult-links) (link: Next)
- [CustomerQueryResult](#customerqueryresult-links) (link: Previous)
- [CustomerQueryResult](#customerqueryresult-links) (link: Last)
- [CustomerQueryResult](#customerqueryresult-links) (link: All)

## Customer

#### Classes

- `Customer`

<a id="customer-properties"></a>

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| Age | integer | no |  |
| FullName | string | no |  |
| Address | object | no |  |
| IsFavorite | boolean | no |  |

<a id="customer-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| PurchaseHistory | [CustomerPurchaseHistory](#customerpurchasehistory) |  |
| self | [Customer](#customer) |  |

<a id="customer-actions"></a>

### Actions

| Action | Description | Links to |
|---|---|---|
| CustomerMove |  |  |

  | Parameter | Type | Required | Description |
  |---|---|---|---|
  | Address | object | no |  |
| CustomerRemove |  |  |
| MarkAsFavorite |  |  |

  | Parameter | Type | Required | Description |
  |---|---|---|---|
  | Customer | string | no | Format: `uri` |
| BuyCar |  |  |

  | Parameter | Type | Required | Description |
  |---|---|---|---|
  | Brand | string | no |  |
  | CarId | integer | no |  |
  | Price | number | no |  |
  | HiddenProperty | number | no |  |

**Referenced by:**

- [DerivedCar](#derivedcar-links) (link: DerivedLink)
- [DerivedCar](#derivedcar-embedded) (embedded: item)
- [NextLevelDerivedCar](#nextlevelderivedcar-links) (link: DerivedLink)
- [NextLevelDerivedCar](#nextlevelderivedcar-embedded) (embedded: item)
- [CustomersRoot](#customersroot-links) (link: BestCustomer)
- [CustomerQueryResult](#customerqueryresult-embedded) (embedded: Customers)

## DerivedCar

#### Title

Derived Car

#### Classes

- `DerivedCar`

<a id="derivedcar-properties"></a>

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| DerivedProperty | string | no |  |
| Id | integer | no |  |
| Brand | string | no |  |
| PriceDevelopment | number[] | no |  |
| PopularCountries | [country](#definition-country)[] | no |  |
| MostPopularIn | [country](#definition-country) | no |  |
| LastInspection | string | no | Format: `date` |

<a id="derivedcar-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| DerivedLink *(optional)* | [Customer](#customer) |  |
| self | [DerivedCar](#derivedcar) |  |

<a id="derivedcar-actions"></a>

### Actions

| Action | Description | Links to |
|---|---|---|
| DerivedOperation |  |  |
| UpdateInspection |  |  |

  | Parameter | Type | Required | Description |
  |---|---|---|---|
  | NewInspection | string | no | Format: `date` |

<a id="derivedcar-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Description |
|---|---|---|---|
| item | [Customer](#customer) | yes |  |

**Referenced by:**

- [CarsRoot](#carsroot-links) (link: NiceCar)

## Car

#### Title

A Car

#### Classes

- `Car`

<a id="car-properties"></a>

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| Id | integer | no |  |
| Brand | string | no |  |
| PriceDevelopment | number[] | no |  |
| PopularCountries | [country](#definition-country)[] | no |  |
| MostPopularIn | [country](#definition-country) | no |  |
| LastInspection | string | no | Format: `date` |

<a id="car-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| self | [Car](#car) |  |

<a id="car-actions"></a>

### Actions

| Action | Description | Links to |
|---|---|---|
| UpdateInspection |  |  |

  | Parameter | Type | Required | Description |
  |---|---|---|---|
  | NewInspection | string | no | Format: `date` |

**Referenced by:**

- [CarsRoot](#carsroot-links) (link: SuperCar)

## CustomerPurchaseHistory

#### Classes

- `CustomerPurchaseHistory`

<a id="customerpurchasehistory-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| self | [CustomerPurchaseHistory](#customerpurchasehistory) |  |

<a id="customerpurchasehistory-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Description |
|---|---|---|---|
| Purchases | [CustomerPurchase](#customerpurchase) | yes |  |

**Referenced by:**

- [Customer](#customer-links) (link: PurchaseHistory)

## CustomerPurchase

#### Classes

- `CustomerPurchase`

<a id="customerpurchase-properties"></a>

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| Amount | integer | no |  |
| CardNumber | string | no |  |
| CardType | string | no |  |

**Referenced by:**

- [CustomerPurchaseHistory](#customerpurchasehistory-embedded) (embedded: Purchases)

## Truck

#### Title

A truck

#### Classes

- `Truck`

<a id="truck-properties"></a>

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| Brand | string | no |  |
| Id | integer | no |  |

## CarImage

#### Title

Image for a car

#### Classes

- `CarImage`

<a id="carimage-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| self | [CarImage](#carimage) |  |

## CarInsurance

#### Title

Insurance scan for a car

#### Classes

- `CarInsurance`

<a id="carinsurance-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| self | [CarInsurance](#carinsurance) |  |

## NextLevelDerivedCar

#### Title

Derives from Derived Car

#### Classes

- `NextLevelDerivedCar`

<a id="nextlevelderivedcar-properties"></a>

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| NextLevelDerivedProperty | string | no |  |
| DerivedProperty | string | no |  |
| Id | integer | no |  |
| Brand | string | no |  |
| PriceDevelopment | number[] | no |  |
| PopularCountries | [country](#definition-country)[] | no |  |
| MostPopularIn | [country](#definition-country) | no |  |
| LastInspection | string | no | Format: `date` |

<a id="nextlevelderivedcar-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| self | [NextLevelDerivedCar](#nextlevelderivedcar) |  |
| DerivedLink *(optional)* | [Customer](#customer) |  |

<a id="nextlevelderivedcar-actions"></a>

### Actions

| Action | Description | Links to |
|---|---|---|
| DerivedOperation |  |  |
| UpdateInspection |  |  |

  | Parameter | Type | Required | Description |
  |---|---|---|---|
  | NewInspection | string | no | Format: `date` |

<a id="nextlevelderivedcar-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Description |
|---|---|---|---|
| item | [Customer](#customer) | yes |  |