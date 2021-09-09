using Bluehands.Hypermedia.Client.Extensions.NewtonsoftJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Extensions.Test.StringParserTests
{
    [TestClass]
    public class NewtonsoftJsonParserTest : JsonParserTestBase
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            Parser = new NewtonsoftJsonStringParser();
        }
    }
}