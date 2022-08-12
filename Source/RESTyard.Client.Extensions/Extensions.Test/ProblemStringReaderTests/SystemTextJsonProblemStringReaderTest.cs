using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.Client.Extensions.SystemTextJson;

namespace Extensions.Test.ProblemStringReaderTests
{
    [TestClass]
    public class SystemTextJsonProblemStringReaderTest : JsonProblemStringReaderTestBase
    {
        [TestInitialize]
        public override void Initializer()
        {
            base.Initializer();
            ProblemReader = new SystemTextJsonProblemStringReader();
        }
    }
}