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

**Access Groups**

- [customer](#access-group-customer)
- [fleet-manager](#access-group-fleet-manager)

## API Map

```mermaid
graph LR
    Truck["Truck"]
    Entrypoint["Entrypoint"]
    CarsRoot["CarsRoot [fleet-manager]"]
    Car["Car"]
    CarImage["CarImage"]
    CarInsurance["CarInsurance"]
    DerivedCar["DerivedCar"]
    NextLevelDerivedCar["NextLevelDerivedCar"]
    CustomersRoot["CustomersRoot"]
    CustomerPurchase["CustomerPurchase"]
    CustomerPurchaseHistory["CustomerPurchaseHistory"]
    Customer["Customer [customer]"]
    CustomerQueryResult["CustomerQueryResult"]
    _external[/"External"/]

    Entrypoint -- "schema" --> _external
    Entrypoint -- "schema-customer" --> _external
    Entrypoint -- "access-groups" --> _external
    Entrypoint -- "api-guide" --> _external
    Entrypoint -- "CustomersRoot" --> CustomersRoot
    Entrypoint -- "CarsRoot" --> CarsRoot
    CarsRoot -- "NiceCar" --> DerivedCar
    CarsRoot -- "SuperCar" --> Car
    CarsRoot -. "action: UploadCarImage" .-> CarImage
    CarsRoot -. "action: UploadInsuranceScan" .-> CarInsurance
    Car -. "action: UpdateInspection" .-> Car
    DerivedCar -- "DerivedLink" --> Customer
    DerivedCar -- "item" --> Customer
    DerivedCar -. "action: UpdateInspection" .-> Car
    NextLevelDerivedCar -- "DerivedLink" --> Customer
    NextLevelDerivedCar -- "item" --> Customer
    NextLevelDerivedCar -. "action: UpdateInspection" .-> Car
    CustomersRoot -- "all" --> CustomerQueryResult
    CustomersRoot -- "BestCustomer" --> Customer
    CustomersRoot -- "GreatSite" --> _external
    CustomersRoot -- "OkaySite" --> _external
    CustomersRoot -. "action: CreateCustomer" .-> Customer
    CustomersRoot -. "action: CreateQuery" .-> CustomerQueryResult
    CustomerPurchaseHistory -- "Purchases" --> CustomerPurchase
    Customer -- "PurchaseHistory" --> CustomerPurchaseHistory
    Customer -. "action: BuyCar" .-> Car
    CustomerQueryResult -- "Next" --> CustomerQueryResult
    CustomerQueryResult -- "Previous" --> CustomerQueryResult
    CustomerQueryResult -- "Last" --> CustomerQueryResult
    CustomerQueryResult -- "All" --> CustomerQueryResult
    CustomerQueryResult -- "Customers" --> Customer
```

## Entrypoint

#### Classes

- `Entrypoint`

<a id="entrypoint-links"></a>

### Links

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| schema | *external* (`application/vnd.siren+json`) |  |  |
| schema-customer | *external* (`application/vnd.siren+json`) |  |  |
| access-groups | *external* (`application/vnd.siren+json`) |  |  |
| api-guide | *external* (`application/vnd.siren+json`) |  |  |
| CustomersRoot | [CustomersRoot](#customersroot) |  |  |
| CarsRoot | [CarsRoot](#carsroot) |  |  |
| self | [Entrypoint](#entrypoint) |  |  |

## CustomersRoot

#### Classes

- `CustomersRoot`

<a id="customersroot-links"></a>

### Links

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| all | [CustomerQueryResult](#customerqueryresult) |  |  |
| BestCustomer | [Customer](#customer) |  |  |
| GreatSite | *external* (`application/vnd.siren+json`) |  |  |
| OkaySite *(optional)* | *external* (`application/vnd.siren+json`) |  |  |
| self | [CustomersRoot](#customersroot) |  |  |

### Actions

<a id="customersroot-createcustomer"></a>

#### CreateCustomer

Request creation of a new Customer.

**Returns:** [Customer](#customer)

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| Name | string | yes |  |
| Age | integer | no |  |

<a id="customersroot-createquery"></a>

#### CreateQuery

Query the Customers collection.

**Returns:** [CustomerQueryResult](#customerqueryresult)

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| Pagination | [Pagination](#definition-pagination) | yes |  |
| SortBy | [SortParameter<CustomerSortProperties>](#definition-sortparameterofcustomersortproperties) | yes |  |
| Filter | [CustomerFilter](#definition-customerfilter) | yes |  |

**Referenced by:**

- [Entrypoint](#entrypoint-links) (link: CustomersRoot)

## CarsRoot

#### Classes

- `CarsRoot`

**Access Groups:** [fleet-manager](#access-group-fleet-manager)

<a id="carsroot-links"></a>

### Links

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| NiceCar | [DerivedCar](#derivedcar) |  |  |
| SuperCar | [Car](#car) |  |  |
| self | [CarsRoot](#carsroot) |  |  |

### Actions

<a id="carsroot-uploadcarimage"></a>

#### UploadCarImage

Upload image for car

**Returns:** [CarImage](#carimage)

**File upload** (`multipart/form-data`)

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| Text | string | yes |  |
| Flag | boolean | yes |  |

<a id="carsroot-uploadinsurancescan"></a>

#### UploadInsuranceScan

Upload scan of insurance for the car

**Returns:** [CarInsurance](#carinsurance)

**File upload** (`multipart/form-data`)

**Referenced by:**

- [Entrypoint](#entrypoint-links) (link: CarsRoot)

## CustomerQueryResult

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

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| Next *(optional)* | [CustomerQueryResult](#customerqueryresult) |  |  |
| Previous *(optional)* | [CustomerQueryResult](#customerqueryresult) |  |  |
| Last *(optional)* | [CustomerQueryResult](#customerqueryresult) |  |  |
| All *(optional)* | [CustomerQueryResult](#customerqueryresult) |  |  |
| self | [CustomerQueryResult](#customerqueryresult) |  |  |

<a id="customerqueryresult-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Access Groups | Description |
|---|---|---|---|---|
| Customers | [Customer](#customer) | yes |  |  |

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

**Access Groups:** [customer](#access-group-customer)

<a id="customer-properties"></a>

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| Age | integer | no |  |
| FullName | string | no |  |
| Address | [AddressTo](#definition-addressto) | no |  |
| IsFavorite | boolean | yes |  |

<a id="customer-links"></a>

### Links

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| PurchaseHistory | [CustomerPurchaseHistory](#customerpurchasehistory) |  |  |
| self | [Customer](#customer) |  |  |

### Actions

<a id="customer-customermove"></a>

#### CustomerMove

A Customer moved to a new location.

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| Address | [AddressTo](#definition-addressto) | yes |  |

<a id="customer-customerremove"></a>

#### CustomerRemove

Remove a Customer.

<a id="customer-markasfavorite"></a>

#### MarkAsFavorite

Marks a Customer as a favorite buyer.

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| Customer | string | yes | Format: `uri` |

<a id="customer-buycar"></a>

#### BuyCar

Buy a car.

**Returns:** [Car](#car)

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| Brand | string | yes |  |
| CarId | integer | yes |  |
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

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| DerivedLink *(optional)* | [Customer](#customer) |  |  |
| self | [DerivedCar](#derivedcar) |  |  |

### Actions

<a id="derivedcar-derivedoperation"></a>

#### DerivedOperation

Derived Operation

<a id="derivedcar-updateinspection"></a>

#### UpdateInspection



**Returns:** [Car](#car)

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| NewInspection | string | yes | Format: `date` |

<a id="derivedcar-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Access Groups | Description |
|---|---|---|---|---|
| item | [Customer](#customer) | yes |  |  |

**Referenced by:**

- [CarsRoot](#carsroot-links) (link: NiceCar)

## Car

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

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| self | [Car](#car) |  |  |

### Actions

<a id="car-updateinspection"></a>

#### UpdateInspection



**Returns:** [Car](#car)

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| NewInspection | string | yes | Format: `date` |

**Referenced by:**

- [CarsRoot](#carsroot-links) (link: SuperCar)
- [Car → UpdateInspection](#car-updateinspection) (action result)
- [DerivedCar → UpdateInspection](#derivedcar-updateinspection) (action result)
- [NextLevelDerivedCar → UpdateInspection](#nextlevelderivedcar-updateinspection) (action result)
- [Customer → BuyCar](#customer-buycar) (action result)

## CustomerPurchaseHistory

#### Classes

- `CustomerPurchaseHistory`

<a id="customerpurchasehistory-links"></a>

### Links

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| self | [CustomerPurchaseHistory](#customerpurchasehistory) |  |  |

<a id="customerpurchasehistory-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Access Groups | Description |
|---|---|---|---|---|
| Purchases | [CustomerPurchase](#customerpurchase) | yes |  |  |

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
| CardNumber | string | yes |  |
| CardType | string | yes |  |

**Referenced by:**

- [CustomerPurchaseHistory](#customerpurchasehistory-embedded) (embedded: Purchases)

## Truck

#### Title

Truck

#### Description

A truck offered by the car shack.

Has no route; links to it resolve to the default route.

#### Classes

- `Truck`

<a id="truck-properties"></a>

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| Brand | string | yes |  |
| Id | integer | yes |  |

## CarImage

#### Classes

- `CarImage`

<a id="carimage-links"></a>

### Links

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| self | [CarImage](#carimage) |  |  |

**Referenced by:**

- [CarsRoot → UploadCarImage](#carsroot-uploadcarimage) (action result)

## CarInsurance

#### Classes

- `CarInsurance`

<a id="carinsurance-links"></a>

### Links

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| self | [CarInsurance](#carinsurance) |  |  |

**Referenced by:**

- [CarsRoot → UploadInsuranceScan](#carsroot-uploadinsurancescan) (action result)

## NextLevelDerivedCar

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

| Relation | Target | Access Groups | Description |
|---|---|---|---|
| self | [NextLevelDerivedCar](#nextlevelderivedcar) |  |  |
| DerivedLink *(optional)* | [Customer](#customer) |  |  |

### Actions

<a id="nextlevelderivedcar-derivedoperation"></a>

#### DerivedOperation

Derived Operation

<a id="nextlevelderivedcar-updateinspection"></a>

#### UpdateInspection



**Returns:** [Car](#car)

**Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| NewInspection | string | yes | Format: `date` |

<a id="nextlevelderivedcar-embedded"></a>

### Embedded Entities

| Relation | Target | Collection | Access Groups | Description |
|---|---|---|---|---|
| item | [Customer](#customer) | yes |  |  |

## Definitions

<a id="definition-country"></a>

### Definition: Country

**Referenced by:**

- [Car](#car)
- [DerivedCar](#derivedcar)
- [NextLevelDerivedCar](#nextlevelderivedcar)

| Property | Type | Required | Description |
|---|---|---|---|
| Name | string | yes |  |
| EstimatedPopulation | integer | yes |  |
| LanguageCode | string | yes |  |

<a id="definition-pagination"></a>

### Definition: Pagination

**Referenced by:**

- [CustomersRoot → CreateQuery](#customersroot-createquery)

| Property | Type | Required | Description |
|---|---|---|---|
| PageSize | integer | yes |  |
| PageOffset | integer | yes |  |

<a id="definition-sortparameterofcustomersortproperties"></a>

### Definition: SortParameter<CustomerSortProperties>

**Referenced by:**

- [CustomersRoot → CreateQuery](#customersroot-createquery)

| Property | Type | Required | Description |
|---|---|---|---|
| PropertyName | object | no | Values: `Age`, `Name`, `null` |
| SortType | object | yes | Values: `None`, `Ascending`, `Descending` |

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
| Street | string | yes |  |
| Number | string | yes |  |
| City | string | yes |  |
| ZipCode | string | yes |  |

## Access Groups

<a id="access-group-customer"></a>

### customer

- [Customer](#customer) (entity)

<a id="access-group-fleet-manager"></a>

### fleet-manager

- [CarsRoot](#carsroot) (entity)