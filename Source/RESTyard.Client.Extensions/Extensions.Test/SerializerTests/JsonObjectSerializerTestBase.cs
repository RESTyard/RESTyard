using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Extensions.Test.SerializerTests
{
    public class JsonObjectSerializerTestBase : SerializerTestBase
    {
        public virtual void Initialize()
        {
            ExpectedResult = @"{
  ""FullName"": ""TestCustomer"",
  ""Age"": 4,
  ""Address"": ""TestWeg"",
  ""IsFavorite"": true,
  ""Self"": null,
  ""MarkAsFavorite"": null,
  ""CustomerMove"": null,
  ""Relations"": [],
  ""Title"": """"
}";
        }
    }
}