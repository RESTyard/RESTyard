using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;

namespace RESTyard.AspNetCore.Analyzers.Tests;

public class HypermediaObjectTitleAnalyzerTests : VerifyAnalyzer
{
    public HypermediaObjectTitleAnalyzerTests() : base()
    {
    }

    [Fact]
    public async Task MovesLegacyTitleToSirenTitlePropertyAndBuilds()
    {
        var code =
            /* lang=c# */
            """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [HypermediaObject(Title = "A document", Classes = ["Document"])]
            public class Document : IHypermediaObject
            {
            }
            """;

        await Verify(
            code,
            new HypermediaObjectTitleAnalyzer(),
            new HypermediaObjectTitleCodeFixProvider(),
            diagnostics => diagnostics.Should().ContainSingle().Which.Id.Should().Be(HypermediaObjectTitleAnalyzer.DiagnosticId),
            sourceMustBuild: false,
            fixedDocumentMustBuild: true);
    }
}
