using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.Test.Helpers;

namespace RESTyard.AspNetCore.Test.QueryStringBuilderTests
{
    [TestClass]
    public class QueryStringBuilderIEnumerableTest
    {
        private QueryStringBuilder queryStringBuilder;

        [TestInitialize]
        public void Init()
        {
            queryStringBuilder = new QueryStringBuilder();
        }

        [TestMethod]
        public void TestNullList()
        {
            var listHolder = new IntListHolder();

            var result = queryStringBuilder.CreateQueryString(listHolder);
            var valueList = QueryStringBuilderTestHelper.CreateValueListFromQueryString(result);

            Assert.IsTrue(string.IsNullOrEmpty(result));
            Assert.IsTrue(valueList.Count == 0);
        }

        [TestMethod]
        public void TestEmptyList()
        {
            var listHolder = new IntListHolder
            {
                List = new List<int>()
            };

            var result = queryStringBuilder.CreateQueryString(listHolder);
            var valueList = QueryStringBuilderTestHelper.CreateValueListFromQueryString(result);

            Assert.IsTrue(string.IsNullOrEmpty(result));
            Assert.IsTrue(valueList.Count == 0);

        }

        [TestMethod]
        public void TestIntList()
        {
            var listHolder = new IntListHolder()
            {
                List = new List<int>()
                {
                    1,2,-3
                }
            };

            var result = queryStringBuilder.CreateQueryString(listHolder);
            var valueList = QueryStringBuilderTestHelper.CreateValueListFromQueryString(result);

            Assert.IsTrue(result[0] == '?');
            Assert.IsTrue(valueList.Count == 3);

            var sourceIndex = 0;
            foreach (var item in valueList)
            {
                Assert.IsTrue(string.Equals(item[0], "List"));
                Assert.IsTrue(string.Equals(item[1], Uri.EscapeDataString(listHolder.List[sourceIndex].ToString(CultureInfo.InvariantCulture))));
                sourceIndex++;
            }
        }

        [TestMethod]
        public void TestStringList()
        {
            var listHolder = new StringListHolder()
            {
                List = new List<string>
                {
                    "First", "Second", "Third"
                }
            };

            var result = queryStringBuilder.CreateQueryString(listHolder);
            var valueList = QueryStringBuilderTestHelper.CreateValueListFromQueryString(result);

            Assert.IsTrue(result[0] == '?');
            Assert.IsTrue(valueList.Count == 3);

            var sourceIndex = 0;
            foreach (var item in valueList)
            {
                Assert.IsTrue(string.Equals(item[0], "List"));
                Assert.IsTrue(string.Equals(item[1], Uri.EscapeDataString(listHolder.List[sourceIndex])));
                sourceIndex++;
            }
        }

        [TestMethod]
        public void TestNestedList()
        {
            var nester = new Nester
            {
                Nested = new IntListHolder
                {
                    List = new List<int> { 1, 2, 3 }
                }
            };

            var result = queryStringBuilder.CreateQueryString(nester);
            var valueList = QueryStringBuilderTestHelper.CreateValueListFromQueryString(result);

            Assert.IsTrue(result[0] == '?');
            Assert.IsTrue(valueList.Count == 3);

            var sourceIndex = 0;
            foreach (var item in valueList)
            {
                Assert.IsTrue(string.Equals(item[0], "Nested.List"));
                Assert.IsTrue(string.Equals(item[1], Uri.EscapeDataString(nester.Nested.List[sourceIndex].ToInvariantString())));
                sourceIndex++;
            }
        }

        [TestMethod]
        public void TestClassList()
        {
            var listHolder = new ChildListHolder()
            {
                List = new List<Child>
                {
                    new Child{ Value = 1},
                    new Child{ Value = 2},
                    new Child{ Value = 3},
                }
            };

            var result = queryStringBuilder.CreateQueryString(listHolder);
            var valueList = QueryStringBuilderTestHelper.CreateValueListFromQueryString(result);

            Assert.IsTrue(result[0] == '?');
            Assert.IsTrue(valueList.Count == 3);

            var sourceIndex = 0;
            foreach (var item in valueList)
            {
                Assert.IsTrue(string.Equals(item[0], $"List[{sourceIndex}].Value"));
                Assert.IsTrue(string.Equals(item[1], Uri.EscapeDataString(listHolder.List[sourceIndex].Value.ToInvariantString())));
                sourceIndex++;
            }
        }

        [TestMethod]
        public void TestIntDictionary()
        {
            var dictionaryHolder = new DictionaryHolder()
            {
                Dictionary = new Dictionary<string, int>
                {
                    { "First", 1},
                    { "Second", 2},
                    { "Third", 3}
                }
            };

            var result = queryStringBuilder.CreateQueryString(dictionaryHolder);

            var valueList = QueryStringBuilderTestHelper.CreateValueListFromQueryString(result);

            Assert.IsTrue(result[0] == '?');
            Assert.IsTrue(valueList.Count == dictionaryHolder.Dictionary.Count * 2);

            var dictionaryList = dictionaryHolder.Dictionary.ToList();
            var dictionaryListIndex = 0;
            foreach (var item in dictionaryList)
            {
                // check key
                Assert.IsTrue(string.Equals(valueList[dictionaryListIndex * 2][0], $"Dictionary[{dictionaryListIndex}].Key"));

                // check value
                Assert.IsTrue(string.Equals(valueList[dictionaryListIndex * 2 + 1][1], Uri.EscapeDataString(item.Value.ToInvariantString())));

                dictionaryListIndex++;
            }
        }
    }

    public class DictionaryHolder
    {
        public Dictionary<string, int> Dictionary { get; set; }
    }

    public class ChildListHolder
    {
        public List<Child> List { get; set; }
    }

    public class IntListHolder
    {
        public List<int> List { get; set; }
    }

    public class StringListHolder
    {
        public List<string> List { get; set; }
    }


    public class Nester
    {
        public IntListHolder Nested { get; set; }
    }

    public class Child
    {
        public int Value { get; set; }
    }
}
