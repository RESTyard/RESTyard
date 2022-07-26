using System;
using System.Collections.Generic;
using System.Linq;

namespace RESTyard.WebApi.Extensions.Test.QueryStringBuilderTests
{
    static internal class QueryStringBuilderTestHelper
    {
        public static Dictionary<string, string> CreateValueDictionaryFromQueryString(string result)
        {
            if (string.IsNullOrEmpty(result))
            {
                return new Dictionary<string, string>();
            }

            var splittedQueryString = result.Substring(1).Split('&').Select(s => s.Split(new[] { '=' })).ToList();
            var valueDictionary = new Dictionary<string, string>();
            foreach (var substring in splittedQueryString)
            {
                valueDictionary.Add(substring[0], substring[1]);
            }
            return valueDictionary;
        }

        public static List<string[]> CreateValueListFromQueryString(string result)
        {
            if (string.IsNullOrEmpty(result))
            {
                return new List<string[]>();
            }

            var splittedQueryString = result.Substring(1).Split('&').Select(s => s.Split(new[] { '=' })).ToList();

            return splittedQueryString;
        }
    }

}