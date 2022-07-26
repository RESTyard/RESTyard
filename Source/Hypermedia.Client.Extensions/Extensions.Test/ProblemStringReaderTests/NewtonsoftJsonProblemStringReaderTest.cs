using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.Client.Extensions.NewtonsoftJson;

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