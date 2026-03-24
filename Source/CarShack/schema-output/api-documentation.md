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

**Definitions**

- [Country](#definition-country)
- [Pagination](#definition-pagination)
- [SortParameter<CustomerSortProperties>](#definition-sortparameterofcustomersortproperties)
- [CustomerFilter](#definition-customerfilter)
- [AddressTo](#definition-addressto)

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

### Actions

<a id="customersroot-createcustomer"></a>

#### CreateCustomer

Request creation of a new Customer.

**Returns:** [Customer](#customer)

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| Name | string | no |  |

<a id="customersroot-createquery"></a>

#### CreateQuery

Query the Customers collection.

**Returns:** [CustomerQueryResult](#customerqueryresult)

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| Pagination | [Pagination](#definition-pagination) | no |  |
| SortBy | [SortParameter<CustomerSortProperties>](#definition-sortparameterofcustomersortproperties) | no |  |
| Filter | [CustomerFilter](#definition-customerfilter) | no |  |

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

### Actions

<a id="carsroot-uploadcarimage"></a>

#### UploadCarImage

Upload image for car

**Returns:** [CarImage](#carimage)

**File upload** (`multipart/form-data`)

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| Text | string | no |  |
| Flag | boolean | no |  |

<a id="carsroot-uploadinsurancescan"></a>

#### UploadInsuranceScan

Upload scan of insurance for the car

**Returns:** [CarInsurance](#carinsurance)

**File upload** (`multipart/form-data`)

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
- [CustomersRoot → CreateQuery](#customersroot-createquery) (action result)
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
| Address | [AddressTo](#definition-addressto) | no |  |
| IsFavorite | boolean | no |  |

<a id="customer-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| PurchaseHistory | [CustomerPurchaseHistory](#customerpurchasehistory) |  |
| self | [Customer](#customer) |  |

### Actions

<a id="customer-customermove"></a>

#### CustomerMove

A Customer moved to a new location.

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| Address | [AddressTo](#definition-addressto) | no |  |

<a id="customer-customerremove"></a>

#### CustomerRemove

Remove a Customer.

<a id="customer-markasfavorite"></a>

#### MarkAsFavorite

Marks a Customer as a favorite buyer.

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| Customer | string | no | Format: `uri` |

<a id="customer-buycar"></a>

#### BuyCar

Buy a car.

**Returns:** [Car](#car)

**Parameters:**

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
- [CustomersRoot → CreateCustomer](#customersroot-createcustomer) (action result)
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
| PopularCountries | [Country](#definition-country)[] | no |  |
| MostPopularIn | [Country](#definition-country) | no |  |
| LastInspection | string | no | Format: `date` |

<a id="derivedcar-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| DerivedLink *(optional)* | [Customer](#customer) |  |
| self | [DerivedCar](#derivedcar) |  |

### Actions

<a id="derivedcar-derivedoperation"></a>

#### DerivedOperation

Derived Operation

<a id="derivedcar-updateinspection"></a>

#### UpdateInspection



**Parameters:**

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
| PopularCountries | [Country](#definition-country)[] | no |  |
| MostPopularIn | [Country](#definition-country) | no |  |
| LastInspection | string | no | Format: `date` |

<a id="car-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| self | [Car](#car) |  |

### Actions

<a id="car-updateinspection"></a>

#### UpdateInspection



**Returns:** [Car](#car)

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| NewInspection | string | no | Format: `date` |

**Referenced by:**

- [CarsRoot](#carsroot-links) (link: SuperCar)
- [Car → UpdateInspection](#car-updateinspection) (action result)
- [Customer → BuyCar](#customer-buycar) (action result)

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

**Referenced by:**

- [CarsRoot → UploadCarImage](#carsroot-uploadcarimage) (action result)

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

**Referenced by:**

- [CarsRoot → UploadInsuranceScan](#carsroot-uploadinsurancescan) (action result)

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
| PopularCountries | [Country](#definition-country)[] | no |  |
| MostPopularIn | [Country](#definition-country) | no |  |
| LastInspection | string | no | Format: `date` |

<a id="nextlevelderivedcar-links"></a>

### Links

| Relation | Target | Description |
|---|---|---|
| self | [NextLevelDerivedCar](#nextlevelderivedcar) |  |
| DerivedLink *(optional)* | [Customer](#customer) |  |

### Actions

<a id="nextlevelderivedcar-derivedoperation"></a>

#### DerivedOperation

Derived Operation

<a id="nextlevelderivedcar-updateinspection"></a>

#### UpdateInspection



**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| NewInspection | string | no | Format: `date` |

<a id="nextlevelderivedcar-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Description |
|---|---|---|---|
| item | [Customer](#customer) | yes |  |

## Definitions

<a id="definition-country"></a>

### Definition: Country

**Referenced by:**

- [Car](#car)
- [DerivedCar](#derivedcar)
- [NextLevelDerivedCar](#nextlevelderivedcar)

| Property | Type | Required | Description |
|---|---|---|---|
| Name | string | no |  |
| EstimatedPopulation | integer | no |  |
| LanguageCode | string | no |  |

<a id="definition-pagination"></a>

### Definition: Pagination

**Referenced by:**

- [CustomersRoot → CreateQuery](#customersroot-createquery)

| Property | Type | Required | Description |
|---|---|---|---|
| PageSize | integer | no |  |
| PageOffset | integer | no |  |

<a id="definition-sortparameterofcustomersortproperties"></a>

### Definition: SortParameter<CustomerSortProperties>

**Referenced by:**

- [CustomersRoot → CreateQuery](#customersroot-createquery)

| Property | Type | Required | Description |
|---|---|---|---|
| PropertyName | object | no | Values: `Age`, `Name`, `null` |
| SortType | object | no | Values: `None`, `Ascending`, `Descending` |

<a id="definition-customerfilter"></a>

### Definition: CustomerFilter

**Referenced by:**

- [CustomersRoot → CreateQuery](#customersroot-createquery)

| Property | Type | Required | Description |
|---|---|---|---|
| MinAge | integer | no |  |

<a id="definition-addressto"></a>

### Definition: AddressTo

**Referenced by:**

- [Customer](#customer)
- [Customer → CustomerMove](#customer-customermove)

| Property | Type | Required | Description |
|---|---|---|---|
| Street | string | no |  |
| Number | string | no |  |
| City | string | no |  |
| ZipCode | string | no |  |