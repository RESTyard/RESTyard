using Bluehands.Hypermedia.Client.Extensions.NewtonsoftJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Extensions.Test.ProblemStringReaderTests
{
    [TestClass]
    public class NewtonsoftJsonProblemStringReaderTest : JsonProblemStringReaderTestBase
    {
        [TestInitialize]
        public override void Initializer()
        {
            base.Initializer();
            ProblemReader = new NewtonsoftJsonProblemStringReader();
        }
    }
}