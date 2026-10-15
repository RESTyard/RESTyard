namespace Extensions.Test.SerializerTests;

public abstract class JsonObjectSerializerTestBase : SerializerTestBase
{
    protected JsonObjectSerializerTestBase()
    {
        ExpectedResult = """
                         {
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
                         """;
    }
}