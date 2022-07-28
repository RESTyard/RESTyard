using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.Client.Extensions.SystemTextJson;

namespace Extensions.Test.StringParserTests
{
    [TestClass]
    public class SystemTextJsonParserTest : JsonParserTestBase
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            Parser = new SystemTextJsonStringParser();
        }
    }
}