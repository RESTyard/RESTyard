using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.Test.Helpers;

namespace RESTyard.AspNetCore.Test.QueryStringBuilderTests
{
    [TestClass]
    public class QueryStringBuilderTest
    {
        private QueryStringBuilder queryStringBuilder;

        [TestInitialize]
        public void Init()
        {
            queryStringBuilder = new QueryStringBuilder();
        }

        [TestMethod]
        public void CreateQueryStringBuilder()
        {
            Assert.IsNotNull(new QueryStringBuilder());
        }

        [TestMethod]
        public void SerializesTypesWithValues()
        {
            var queryObject = new QueryWithTypes
            {
                ABool = true,
                AString = "Tree",
                AInt = 5,
                ALong = 11L,
                AFloat = 0.5f,
                ADouble = 1.7,
                AEnum = TestEnum.Value1,
                ADateTime = new DateTime(2000, 5, 4, 3, 2, 1),
                ATimeSpan = new TimeSpan(5, 4, 3, 2, 1),
                ADecimal = 9,
                ANullableInt = 2,
                ADateTimeOffset = new DateTimeOffset(2001,11,23, 18, 10, 5, new TimeSpan(0, 2, 0, 0))
            };

            var result = queryStringBuilder.CreateQueryString(queryObject);
            var valueDictionary = QueryStringBuilderTestHelper.CreateValueDictionaryFromQueryString(result);

            Assert.IsTrue(result[0] == '?');
            Assert.IsTrue(valueDictionary.Count == 12);
            Assert.IsTrue(valueDictionary["ABool"] == queryObject.ABool.ToString());
            Assert.IsTrue(valueDictionary["AString"] == queryObject.AString);
            Assert.IsTrue(valueDictionary["AInt"] == queryObject.AInt.ToInvariantString());
            Assert.IsTrue(valueDictionary["ALong"] == queryObject.ALong.ToInvariantString());
            Assert.IsTrue(valueDictionary["AFloat"] == Uri.EscapeDataString(queryObject.AFloat.ToInvariantString()));
            Assert.IsTrue(valueDictionary["ADouble"] == Uri.EscapeDataString(queryObject.ADouble.ToInvariantString()));
            Assert.IsTrue(valueDictionary["AEnum"] == queryObject.AEnum.ToString());
            Assert.IsTrue(valueDictionary["ADateTime"] == Uri.EscapeDataString(queryObject.ADateTime.ToInvariantString()));
            Assert.IsTrue(valueDictionary["ATimeSpan"] == Uri.EscapeDataString(queryObject.ATimeSpan.ToString()));
            Assert.IsTrue(valueDictionary["ADecimal"] == Uri.EscapeDataString(queryObject.ADecimal.ToInvariantString()));
            Assert.IsTrue(valueDictionary["ANullableInt"] == queryObject.ANullableInt.ToString());
            Assert.IsTrue(valueDictionary["ADateTimeOffset"] == Uri.EscapeDataString(queryObject.ADateTimeOffset.ToInvariantString()));
        }

        [TestMethod]
        public void SerializesTypesOnlyDefaults()
        {
            var queryObject = new QueryWithTypes();

            var result = queryStringBuilder.CreateQueryString(queryObject);
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void SerializesNullToStringEmpty()
        {
            var result = queryStringBuilder.CreateQueryString(null);
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void SerializesTypesWithNesting()
        {
            var queryObject = new QueryWithNesting
            {
                AString = "Parent",
                First = new QueryWithTypes
                {
                    AString = "Nesting1"
                },
                Second = new QueryWithNesting
                {
                    AString = "Nesting2",
                    Second = new QueryWithNesting
                    {
                        AString = "Nesting3"
                    },

                }
            };

            var result = queryStringBuilder.CreateQueryString(queryObject);
            var valueDictionary = QueryStringBuilderTestHelper.CreateValueDictionaryFromQueryString(result);

            Assert.IsTrue(result[0] == '?');
            Assert.IsTrue(valueDictionary.Count == 4);
            Assert.IsTrue(valueDictionary["AString"] == queryObject.AString);
            Assert.IsTrue(valueDictionary["First.AString"] == queryObject.First.AString);
            Assert.IsTrue(valueDictionary["Second.AString"] == queryObject.Second.AString);
            Assert.IsTrue(valueDictionary["Second.Second.AString"] == queryObject.Second.Second.AString);
        }
        
        private class QueryWithTypes
        {
            public bool ABool { get; set; }
            public string AString { get; set; }
            public int AInt { get; set; }
            public long ALong { get; set; }
            public float AFloat { get; set; }
            public double ADouble { get; set; }
            public TestEnum AEnum { get; set; }
            public DateTime ADateTime { get; set; }
            public TimeSpan ATimeSpan { get; set; }
            public decimal ADecimal { get; set; }
            public int? ANullableInt { get; set; }
            public DateTimeOffset ADateTimeOffset { get; set; }
        }

        private class QueryWithNesting
        {
            public string AString { get; set; }
            public QueryWithTypes First { get; set; }
            public QueryWithNesting Second { get; set; }
        }
    }
}
