namespace Extensions.Test.SerializerTests;

public abstract class SingleJsonObjectSerializerTestBase : SerializerTestBase
{
    protected SingleJsonObjectSerializerTestBase()
    {
        ExpectedResult = """
                         [
                           {
                             "customer": {
                               "FullName": "TestCustomer",
                               "Age": 4,
                               "Address": "TestWeg",
                               "IsFavorite": true,
                               "Self": null,
                               "MarkAsFavorite": null,
                               "CustomerMove": null,
                               "Relations": [],
                               "Title": ""
                             }
                           }
                         ]
                         """;
    }
}