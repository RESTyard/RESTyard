using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;

namespace RESTyard.AspNetCore.Analyzers.Tests;

public class LinkQueryResultByKeyAnalyzerTest : VerifyAnalyzer
{
    public LinkQueryResultByKeyAnalyzerTest() : base()
    {
    }

    [Fact]
    public async Task WarningForHypermediaQueryResultWithLinkByKey()
    {
        var code =
            """
            using System.Collections.Generic;
            using System.Linq;
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.Query;

            public class TestClass()
            {
                public void TestMethod()
                {
                    var key = Link.ByKey(new SomeHto.Key());
                }
            }

            [HypermediaObject(Classes = ["SomeHto"])]
            public class SomeHto : IHypermediaQueryResult
            {
                public IHypermediaQuery Query { get; }
                
                public record Key() : HypermediaObjectKeyBase<SomeHto>
                {
                    protected override IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration()
                        => Enumerable.Empty<KeyValuePair<string, object?>>();
                }
            }
            """;
        await Verify(
            code,
            new LinkQueryResultByKeyAnalyzer(),
            new LinkQueryResultByKeyCodeFixProvider(),
            diagnostics => diagnostics.Should().ContainSingle().Which.Id.Should().Be("RY0020"));
    }

    [Fact]
    public async Task WarningForHypermediaQueryResultWithLinkByKey_WithoutKey()
    {
        var code =
            """
            using System.Collections.Generic;
            using System.Linq;
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.Query;

            public class TestClass()
            {
                public void TestMethod()
                {
                    var key = Link.ByKey<SomeHto>(null);
                }
            }

            [HypermediaObject(Classes = ["SomeHto"])]
            public class SomeHto : IHypermediaQueryResult
            {
                public IHypermediaQuery Query { get; }
                
                public record Key() : HypermediaObjectKeyBase<SomeHto>
                {
                    protected override IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration()
                        => Enumerable.Empty<KeyValuePair<string, object?>>();
                }
            }
            """;
        await Verify(
            code,
            new LinkQueryResultByKeyAnalyzer(),
            new LinkQueryResultByKeyCodeFixProvider(),
            diagnostics => diagnostics.Should().ContainSingle().Which.Id.Should().Be("RY0020"));
    }
}