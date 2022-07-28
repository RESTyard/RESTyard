using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.Client.Extensions.NewtonsoftJson;

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