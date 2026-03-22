# HTOs

```mermaid
classDiagram
    class Truck {
        +string Brand
        +integer Id
    }
    class Entrypoint {
    }
    class CarsRoot {
        +UploadCarImage(params)
        +UploadInsuranceScan()
    }
    class Car {
        +integer Id
        +string Brand
        +number[] PriceDevelopment
        +country[] PopularCountries
        +country MostPopularIn
        +string LastInspection
        +UpdateInspection(params)
    }
    class CarImage {
    }
    class CarInsurance {
    }
    class DerivedCar {
        +string DerivedProperty
        +integer Id
        +string Brand
        +number[] PriceDevelopment
        +country[] PopularCountries
        +country MostPopularIn
        +string LastInspection
        +DerivedOperation()
        +UpdateInspection(params)
    }
    class NextLevelDerivedCar {
        +string NextLevelDerivedProperty
        +string DerivedProperty
        +integer Id
        +string Brand
        +number[] PriceDevelopment
        +country[] PopularCountries
        +country MostPopularIn
        +string LastInspection
        +DerivedOperation()
        +UpdateInspection(params)
    }
    class CustomersRoot {
        +CreateCustomer(params)
        +CreateQuery(params)
    }
    class CustomerPurchase {
        +integer Amount
        +string CardNumber
        +string CardType
    }
    class CustomerPurchaseHistory {
    }
    class Customer {
        +integer Age
        +string FullName
        +object Address
        +boolean IsFavorite
        +CustomerMove(params)
        +CustomerRemove()
        +MarkAsFavorite(params)
        +BuyCar(params)
    }
    class CustomerQueryResult {
        +integer TotalEntities
        +integer CurrentEntitiesCount
    }
    Entrypoint --> CustomersRoot : CustomersRoot
    Entrypoint --> CarsRoot : CarsRoot
    CarsRoot --> DerivedCar : NiceCar
    CarsRoot --> Car : SuperCar
    DerivedCar --> Customer : DerivedLink
    DerivedCar --> Customer : item
    NextLevelDerivedCar --> Customer : DerivedLink
    NextLevelDerivedCar --> Customer : item
    CustomersRoot --> CustomerQueryResult : all
    CustomersRoot --> Customer : BestCustomer
    CustomerPurchaseHistory --> CustomerPurchase : Purchases
    Customer --> CustomerPurchaseHistory : PurchaseHistory
    CustomerQueryResult --> CustomerQueryResult : Next
    CustomerQueryResult --> CustomerQueryResult : Previous
    CustomerQueryResult --> CustomerQueryResult : Last
    CustomerQueryResult --> CustomerQueryResult : All
    CustomerQueryResult --> Customer : Customers
```
