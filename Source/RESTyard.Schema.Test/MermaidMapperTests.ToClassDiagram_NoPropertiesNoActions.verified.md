```mermaid
classDiagram
    class EntryPoint {
    }
    class CustomersRoot {
    }
    class Customer {
    }
    class CarsRoot {
    }
    class Car {
    }
    EntryPoint --> CustomersRoot : customers
    EntryPoint --> CarsRoot : cars
    CustomersRoot --> Customer : item
    CarsRoot --> Car : item
```