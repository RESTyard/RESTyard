using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace RESTyard.AspNetCore.Analyzers.Tests;

public class LegacyAttributeAnalyzerTest : VerifyAnalyzer
{
    public LegacyAttributeAnalyzerTest() : base()
    {
        
    }
    
    [Fact]
    public async Task LegacyGetAttributesTest()
    {
        var code =
            """
            using Microsoft.AspNetCore.Mvc;
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using RESTyard.AspNetCore.WebApi.RouteResolver;

            public class SomeController : Controller
            {
                [HttpGetHypermediaObject("Hi", typeof(SomeHto))]
                public IActionResult Get() => this.Ok();
                
                [HttpGetHypermediaObject(typeof(SomeHto))]
                public IActionResult Get2() => this.Ok();
                
                [HttpGetHypermediaObject("Hi3", typeof(SomeHto), typeof(SomeHtoRouteKeyProducer))]
                public IActionResult Get3() => this.Ok();
                
                [HttpGetHypermediaObject(typeof(SomeHto), typeof(SomeHtoRouteKeyProducer))]
                public IActionResult Get4() => this.Ok();
                
            }

            [HypermediaObject(Classes = new[] {"SomeHto"})]
            public class SomeHto : IHypermediaObject
            {
            }
            
            public class SomeHtoRouteKeyProducer : IKeyProducer
            {
                public object CreateFromHypermediaObject(IHypermediaObject hypermediaObject)
                {
                    return null!;
                }
                
                public object CreateFromKeyObject(object? keyObject)
                {
                    return null!;
                }
            }
            """;

        await Verify(
            code,
            new LegacyAttributeAnalyzer(),
            new LegacyAttributeCodeFixProvider(),
            diagnostics => diagnostics.Should().HaveCount(4)
                .And.AllSatisfy(d => d.Id.Should().Be("RY0010")),
            verifyCodeAction: (d, c) => c.Title.Should().ContainAll("HttpGetHypermediaObject", "HttpGet,", "HypermediaObjectEndpoint"));
    }
    
    [Fact]
    public async Task LegacyActionAttributesTest()
    {
        var code =
            """
            using Microsoft.AspNetCore.Mvc;
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using RESTyard.AspNetCore.WebApi.RouteResolver;
            using RESTyard.MediaTypes;

            public class SomeController : Controller
            {
                [HttpPostHypermediaAction("Hi", typeof(SomeHto.SomeOp))]
                public IActionResult Get() => this.Ok();
                
                [HttpPutHypermediaAction(typeof(SomeHto.SomeOp), AcceptedMediaType = DefaultMediaTypes.MultipartFormData)]
                public IActionResult Get2() => this.Ok();
                
                [HttpPatchHypermediaAction("Hi3", typeof(SomeHto.SomeOp), typeof(SomeHtoRouteKeyProducer))]
                public IActionResult Get3() => this.Ok();
                
                [HttpDeleteHypermediaAction(typeof(SomeHto.SomeOp), typeof(SomeHtoRouteKeyProducer))]
                public IActionResult Get4() => this.Ok();
                
            }

            [HypermediaObject(Classes = new[] {"SomeHto"})]
            public class SomeHto : IHypermediaObject
            {
                [HypermediaAction(Name = "SomeOp", Title = "Some Title.")]
                public SomeOp Operation { get; set; }
            
                public class SomeOp : HypermediaAction
                {
                    public SomeOp() : base(() => true) {}
                }
            }
            
            public class SomeHtoRouteKeyProducer : IKeyProducer
            {
                public object CreateFromHypermediaObject(IHypermediaObject hypermediaObject)
                {
                    return null!;
                }
                
                public object CreateFromKeyObject(object? keyObject)
                {
                    return null!;
                }
            }
            """;

        await Verify(
            code,
            new LegacyAttributeAnalyzer(),
            new LegacyAttributeCodeFixProvider(),
            diagnostics => diagnostics.Should().HaveCount(4)
                .And.AllSatisfy(d => d.Id.Should().BeOneOf("RY0011", "RY0012", "RY0013", "RY0014")),
            verifyCodeAction: (d, c) =>
            {
                c.Title.Should().Contain("HypermediaActionEndpoint");
                c.Title.Should().ContainAll(d.Id switch
                {
                    "RY0011" => ["HttpPostHypermediaAction", "HttpPost,"],
                    "RY0012" => ["HttpPutHypermediaAction", "HttpPut,"],
                    "RY0013" => ["HttpPatchHypermediaAction", "HttpPatch,"],
                    "RY0014" => ["HttpDeleteHypermediaAction", "HttpDelete,"],
                    _ => throw new ArgumentOutOfRangeException(),
                });
            });
    }
    
    [Fact]
    public async Task LegacyGetParameterInfoAttributesTest()
    {
        var code =
            """
            using Microsoft.AspNetCore.Mvc;
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using RESTyard.AspNetCore.WebApi.RouteResolver;

            public class SomeController : Controller
            {
                [HttpGetHypermediaActionParameterInfo("WithRoute", typeof(SomeParameter))]
                public IActionResult Get() => this.Ok();
                
                [HttpGetHypermediaActionParameterInfo(typeof(SomeHto.NestedParameter))]
                public IActionResult Get2() => this.Ok();
            }

            public record SomeParameter : IHypermediaActionParameter;
            public class SomeHto
            {
                public class NestedParameter : IHypermediaActionParameter
                {
                }
            }
            """;

        await Verify(
            code,
            new LegacyAttributeAnalyzer(),
            new LegacyAttributeCodeFixProvider(),
            diagnostics => diagnostics.Should().HaveCount(2)
                .And.AllSatisfy(d => d.Id.Should().Be("RY0015")),
            verifyCodeAction: (d, c) => c.Title.Should().ContainAll("HttpGetHypermediaActionParameterInfo", "HttpGet,", "HypermediaActionParameterInfoEndpoint"));
    }
}