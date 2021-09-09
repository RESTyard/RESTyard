using Bluehands.Hypermedia.Client.Extensions.SystemTextJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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