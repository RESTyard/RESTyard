```mermaid
graph LR
    EntryPoint["EntryPoint"]
    CustomersRoot["CustomersRoot"]
    Customer["Customer"]
    CarsRoot["CarsRoot"]
    Car["Car"]

    EntryPoint -- "customers" --> CustomersRoot
    EntryPoint -- "cars" --> CarsRoot
    CustomersRoot -- "item" --> Customer
    CarsRoot -- "item" --> Car
```