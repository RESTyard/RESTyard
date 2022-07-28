using System;

namespace Extensions.Test.SerializerTests
{
    public class SingleJsonObjectSerializerTestBase : SerializerTestBase
    {
        public virtual void Initializer()
        {
            ExpectedResult = @"[
  {
    ""customer"": {
      ""FullName"": ""TestCustomer"",
      ""Age"": 4,
      ""Address"": ""TestWeg"",
      ""IsFavorite"": true,
      ""Self"": null,
      ""MarkAsFavorite"": null,
      ""CustomerMove"": null,
      ""Relations"": [],
      ""Title"": """"
    }
  }
]";
        }
    }
}