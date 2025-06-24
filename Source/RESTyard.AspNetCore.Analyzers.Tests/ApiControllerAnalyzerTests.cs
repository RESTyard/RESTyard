using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace RESTyard.AspNetCore.Analyzers.Tests;

public class ApiControllerAnalyzerTests : VerifyAnalyzer
{
    public ApiControllerAnalyzerTests() : base()
    {
        
    }
    
    [Fact]
    public async Task WarningForHypermediaEndpointWithoutApiController()
    {
        var code =
            """
            using Microsoft.AspNetCore.Mvc;
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;

            public class SomeController : Controller
            {
                [HttpGet]
                [HypermediaObjectEndpoint<SomeHto>]
                public IActionResult Get() => this.Ok();
            }
            
            [HypermediaObject(Classes = ["SomeHto"])]
            public class SomeHto : IHypermediaObject
            {
            }
            """;
        await Verify(
            code,
            new ApiControllerAnalyzer(),
            new ApiControllerCodeFixProvider(),
            diagnostics => diagnostics.Should().ContainSingle().Which.Id.Should().Be("RY0001"));
    }
    
    [Fact]
    public async Task NoWarningForHypermediaEndpointWithPartialApiController()
    {
        var code =
            """
            using Microsoft.AspNetCore.Mvc;
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;

            public partial class SomeController : Controller
            {
                [HttpGet]
                [HypermediaObjectEndpoint<SomeHto>]
                public IActionResult Get() => this.Ok();
            }
            
            [ApiController]
            public partial class SomeController
            {
            }

            [HypermediaObject(Classes = ["SomeHto"])]
            public class SomeHto : IHypermediaObject
            {
            }
            """;
        await Verify(
            code,
            new ApiControllerAnalyzer(),
            new ApiControllerCodeFixProvider(),
            diagnostics => diagnostics.Should().BeEmpty());
    }
}