using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.Client.Reader;

namespace Extensions.Test.StringParserTests
{
    public class StringParserTestBase
    {
        protected IStringParser Parser { get; set; }
        protected string TextToBeParsed { get; set; }

        [TestMethod]
        public void StringParsingTest()
        {
            // TextToBeParsed is from HypermediaClientTestRuns.CallAction_CreateQuery

            var token = Parser.Parse(TextToBeParsed);
            token["class"].Should().NotBeEmpty().And.Subject.First().ValueAsString().Should().Be("CustomersQueryResult");
            token["title"].ValueAsString().Should().Be("Query result on Customers");
            token["entities"].Should().NotBeEmpty();
            var entity = token["entities"].First();
            entity["class"].ChildrenAsStrings().Should().ContainSingle().Which.Should().Be("Customer");
            entity["properties"]["Age"].ToObject(typeof(int)).Should().Be(31);
        }
    }
}