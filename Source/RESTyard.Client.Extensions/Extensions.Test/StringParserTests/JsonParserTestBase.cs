namespace Extensions.Test.StringParserTests;

public abstract class JsonParserTestBase : StringParserTestBase
{
    protected JsonParserTestBase()
    {
        TextToBeParsed =
            """
            {
              "class": [
                "CustomersQueryResult"
              ],
              "title": "Query result on Customers",
              "properties": {
                "TotalEnties": 20,
                "CurrentEntitiesCount": 3
              },
              "entities": [
                {
                  "class": [
                    "Customer"
                  ],
                  "title": "A Customer",
                  "rel": [
                    "item"
                  ],
                  "properties": {
                    "FullName": "Adam Wong",
                    "Age": 31,
                    "Address": "Lantern Lane 202 Saint Petersburg",
                    "IsFavorite": false
                  },
                  "entities": [],
                  "actions": [
                    {
                      "name": "CustomerMove",
                      "title": "A Customer moved to a new location.",
                      "method": "POST",
                      "href": "http://localhost:5000/Customers/10/Moves",
                      "type": "application/json",
                      "fields": [
                        {
                          "name": "NewAddress",
                          "type": "application/json",
                          "class": [
                            "http://localhost:5000/Customers/NewAddressType"
                          ]
                        }
                      ]
                    },
                    {
                      "name": "MarkAsFavoriteAction",
                      "title": "Marks a Customer as a favorite buyer.",
                      "method": "POST",
                      "href": "http://localhost:5000/Customers/MyFavoriteCustomers?key=10",
                      "type": "application/json",
                      "fields": [
                        {
                          "name": "FavoriteCustomer",
                          "type": "application/json",
                          "class": [
                            "http://localhost:5000/ActionParameterTypes/FavoriteCustomer"
                          ]
                        }
                      ]
                    },
                    {
                      "name": "BuyCarAction",
                      "title": "Buy a car.",
                      "method": "POST",
                      "href": "http://localhost:5000/Customers/10/BuysCar",
                      "type": "application/json",
                      "fields": [
                        {
                          "name": "HypermediaActionCustomerBuysCar.Parameter",
                          "type": "application/json",
                          "class": [
                            "http://localhost:5000/ActionParameterTypes/HypermediaActionCustomerBuysCar.Parameter"
                          ]
                        }
                      ]
                    }
                  ],
                  "links": [
                    {
                      "rel": [
                        "self"
                      ],
                      "href": "http://localhost:5000/Customers/10"
                    }
                  ]
                },
                {
                  "class": [
                    "Customer"
                  ],
                  "title": "A Customer",
                  "rel": [
                    "item"
                  ],
                  "properties": {
                    "FullName": "Duncan Miranda",
                    "Age": 40,
                    "Address": "Cambridge Road 11 Riyadh",
                    "IsFavorite": true
                  },
                  "entities": [],
                  "actions": [
                    {
                      "name": "CustomerMove",
                      "title": "A Customer moved to a new location.",
                      "method": "POST",
                      "href": "http://localhost:5000/Customers/13/Moves",
                      "type": "application/json",
                      "fields": [
                        {
                          "name": "NewAddress",
                          "type": "application/json",
                          "class": [
                            "http://localhost:5000/Customers/NewAddressType"
                          ]
                        }
                      ]
                    },
                    {
                      "name": "BuyCarAction",
                      "title": "Buy a car.",
                      "method": "POST",
                      "href": "http://localhost:5000/Customers/13/BuysCar",
                      "type": "application/json",
                      "fields": [
                        {
                          "name": "HypermediaActionCustomerBuysCar.Parameter",
                          "type": "application/json",
                          "class": [
                            "http://localhost:5000/ActionParameterTypes/HypermediaActionCustomerBuysCar.Parameter"
                          ]
                        }
                      ]
                    }
                  ],
                  "links": [
                    {
                      "rel": [
                        "self"
                      ],
                      "href": "http://localhost:5000/Customers/13"
                    }
                  ]
                },
                {
                  "class": [
                    "Customer"
                  ],
                  "title": "A Customer",
                  "rel": [
                    "item"
                  ],
                  "properties": {
                    "FullName": "Christine Kelly",
                    "Age": 41,
                    "Address": "Eagle Street 62 Cairo",
                    "IsFavorite": false
                  },
                  "entities": [],
                  "actions": [
                    {
                      "name": "CustomerMove",
                      "title": "A Customer moved to a new location.",
                      "method": "POST",
                      "href": "http://localhost:5000/Customers/6/Moves",
                      "type": "application/json",
                      "fields": [
                        {
                          "name": "NewAddress",
                          "type": "application/json",
                          "class": [
                            "http://localhost:5000/Customers/NewAddressType"
                          ]
                        }
                      ]
                    },
                    {
                      "name": "MarkAsFavoriteAction",
                      "title": "Marks a Customer as a favorite buyer.",
                      "method": "POST",
                      "href": "http://localhost:5000/Customers/MyFavoriteCustomers?key=6",
                      "type": "application/json",
                      "fields": [
                        {
                          "name": "FavoriteCustomer",
                          "type": "application/json",
                          "class": [
                            "http://localhost:5000/ActionParameterTypes/FavoriteCustomer"
                          ]
                        }
                      ]
                    },
                    {
                      "name": "BuyCarAction",
                      "title": "Buy a car.",
                      "method": "POST",
                      "href": "http://localhost:5000/Customers/6/BuysCar",
                      "type": "application/json",
                      "fields": [
                        {
                          "name": "HypermediaActionCustomerBuysCar.Parameter",
                          "type": "application/json",
                          "class": [
                            "http://localhost:5000/ActionParameterTypes/HypermediaActionCustomerBuysCar.Parameter"
                          ]
                        }
                      ]
                    }
                  ],
                  "links": [
                    {
                      "rel": [
                        "self"
                      ],
                      "href": "http://localhost:5000/Customers/6"
                    }
                  ]
                }
              ],
              "actions": [],
              "links": [
                {
                  "rel": [
                    "self"
                  ],
                  "href": "http://localhost:5000/Customers/Query?Pagination.PageSize=3&Pagination.PageOffset=2&SortBy.PropertyName=Age&SortBy.SortType=Ascending&Filter.MinAge=22"
                },
                {
                  "rel": [
                    "all"
                  ],
                  "href": "http://localhost:5000/Customers/Query?SortBy.PropertyName=Age&SortBy.SortType=Ascending&Filter.MinAge=22"
                },
                {
                  "rel": [
                    "first"
                  ],
                  "href": "http://localhost:5000/Customers/Query?Pagination.PageSize=3&SortBy.PropertyName=Age&SortBy.SortType=Ascending&Filter.MinAge=22"
                },
                {
                  "rel": [
                    "next"
                  ],
                  "href": "http://localhost:5000/Customers/Query?Pagination.PageSize=3&Pagination.PageOffset=5&SortBy.PropertyName=Age&SortBy.SortType=Ascending&Filter.MinAge=22"
                },
                {
                  "rel": [
                    "previous"
                  ],
                  "href": "http://localhost:5000/Customers/Query?Pagination.PageSize=3&SortBy.PropertyName=Age&SortBy.SortType=Ascending&Filter.MinAge=22"
                },
                {
                  "rel": [
                    "last"
                  ],
                  "href": "http://localhost:5000/Customers/Query?Pagination.PageSize=3&Pagination.PageOffset=17&SortBy.PropertyName=Age&SortBy.SortType=Ascending&Filter.MinAge=22"
                }
              ]
            }
            """;
    }
}