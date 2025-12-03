using System;
using FluentAssertions;
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
                ADateTimeOffset = new DateTimeOffset(2001,11,23, 18, 10, 5, new TimeSpan(0, 2, 0, 0)),
                ADateOnly = DateOnly.FromDateTime(new DateTime(2025, 12, 31)),
                ATimeOnly = TimeOnly.FromTimeSpan(new TimeSpan(0, 3, 1, 2)),
                AUri = new Uri("http://www.example.com?a=1"),
            };

            var result = queryStringBuilder.CreateQueryString(queryObject);
            var valueDictionary = QueryStringBuilderTestHelper.CreateValueDictionaryFromQueryString(result);

            result[0].Should().Be('?');
            valueDictionary.Count.Should().Be(15);
            valueDictionary["ABool"].Should().Be(queryObject.ABool.ToString());
            valueDictionary["AString"].Should().Be(queryObject.AString);
            valueDictionary["AInt"].Should().Be(queryObject.AInt.ToInvariantString());
            valueDictionary["ALong"].Should().Be(queryObject.ALong.ToInvariantString());
            valueDictionary["AFloat"].Should().Be(Uri.EscapeDataString(queryObject.AFloat.ToInvariantString()));
            valueDictionary["ADouble"].Should().Be(Uri.EscapeDataString(queryObject.ADouble.ToInvariantString()));
            valueDictionary["AEnum"].Should().Be(queryObject.AEnum.ToString());
            valueDictionary["ADateTime"].Should().Be(Uri.EscapeDataString(queryObject.ADateTime.ToInvariantString()));
            valueDictionary["ATimeSpan"].Should().Be(Uri.EscapeDataString(queryObject.ATimeSpan.ToString()));
            valueDictionary["ADecimal"].Should().Be(Uri.EscapeDataString(queryObject.ADecimal.ToInvariantString()));
            valueDictionary["ANullableInt"].Should().Be(queryObject.ANullableInt.ToString());
            valueDictionary["ADateTimeOffset"].Should().Be(Uri.EscapeDataString(queryObject.ADateTimeOffset.ToInvariantString()));
            valueDictionary["ADateOnly"].Should().Be(Uri.EscapeDataString(queryObject.ADateOnly.ToInvariantString()));
            valueDictionary["ATimeOnly"].Should().Be(Uri.EscapeDataString(queryObject.ATimeOnly.ToInvariantString()));
            valueDictionary["AUri"].Should().Be(Uri.EscapeDataString(queryObject.AUri.ToInvariantString()));
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
            public DateOnly ADateOnly { get; set; }
            public TimeOnly ATimeOnly { get; set; }
            public Uri AUri { get; set; }            
        }

        private class QueryWithNesting
        {
            public string AString { get; set; }
            public QueryWithTypes First { get; set; }
            public QueryWithNesting Second { get; set; }
        }
    }
}
