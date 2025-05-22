using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;

namespace RESTyard.AspNetCore.Test.WebApi;

[TestClass]
public class HypermediaApiExplorerTests : AssemblyBasedTestBase
{
    [TestMethod]
    public void CreateModel_LegacyAttributes()
    {
        var assembly = CreateAssembly([
            CreateFile(
                $$"""
                [Route("Test")]
                [ApiController]
                public class Controller : ControllerBase
                {
                    [HttpGetHypermediaObject("Get", typeof({{nameof(ExampleHto)}}))]
                    public IActionResult Get() => this.Ok();
                    
                    [HttpPostHypermediaAction("Post", typeof({{nameof(ExampleHto)}}.{{nameof(ExampleHto.BasicOp)}}))]
                    public IActionResult Post() => this.Ok();
                    
                    [HttpGetHypermediaActionParameterInfo("Info", typeof({{nameof(ExampleHto)}}.{{nameof(ExampleHto.BasicParameter)}}))]
                    public IActionResult Info() => this.Ok();
                }
                """),
            GetExampleHtoCode(),
        ]);
        var apiExplorer = CreateApiExplorer(assembly);

        var getTemplate = apiExplorer.GetFullRouteTemplateFor(GetType<ExampleHto>(assembly));
        getTemplate.Should().ContainSingle().Which.Should().Be("Test/Get");

        var endpoints = apiExplorer.GetHypermediaEndpoints();
        endpoints.Should().HaveCount(3);

        var getEndpoint = endpoints.Should().ContainSingle(a =>
            a.ActionDescriptor.EndpointMetadata.OfType<IHypermediaObjectEndpointMetadata>().Any()).Which;
        getEndpoint.RelativePath.Should().Be("Test/Get");
        var hmoMetadata = getEndpoint.ActionDescriptor.EndpointMetadata.OfType<IHypermediaObjectEndpointMetadata>().Should()
            .ContainSingle().Which;
        hmoMetadata.RouteType.Should().Be(GetType<ExampleHto>(assembly));

        var postEndpoint = endpoints.Should().ContainSingle(a =>
            a.ActionDescriptor.EndpointMetadata.OfType<IHypermediaActionEndpointMetadata>().Any()).Which;
        postEndpoint.RelativePath.Should().Be("Test/Post");
        var actionMetadata = postEndpoint.ActionDescriptor.EndpointMetadata.OfType<IHypermediaActionEndpointMetadata>()
            .Should().ContainSingle().Which;
        actionMetadata.ActionType.Should().Be(GetType<ExampleHto.BasicOp>(assembly));

        var parameterInfoEndpoint = endpoints.Should().ContainSingle(a =>
            a.ActionDescriptor.EndpointMetadata.OfType<IHypermediaActionParameterInfoEndpointMetadata>().Any()).Which;
        parameterInfoEndpoint.RelativePath.Should().Be("Test/Info");
        var parameterInfoMetadata = parameterInfoEndpoint.ActionDescriptor.EndpointMetadata
            .OfType<IHypermediaActionParameterInfoEndpointMetadata>().Should().ContainSingle().Which;
        parameterInfoMetadata.RouteType.Should().Be(GetType<ExampleHto.BasicParameter>(assembly));
    }
    
    [TestMethod]
    public void CreateModel_HypermediaAttributes()
    {
        var assembly = CreateAssembly([
             CreateFile(
                 $$"""
                 [Route("Test")]
                 [ApiController]
                 public class Controller : ControllerBase
                 {
                     [HttpGet("Get"), HypermediaObjectEndpoint<{{nameof(ExampleHto)}}>]
                     public IActionResult Get() => this.Ok();
                     
                     [HttpPost("Post"), HypermediaActionEndpoint<{{nameof(ExampleHto)}}>("{{nameof(ExampleHto.DoSomething)}}")]
                     public IActionResult Post() => this.Ok();
                     
                     [HttpGet("Info"), HypermediaActionParameterInfoEndpoint<{{nameof(ExampleHto)}}.{{nameof(ExampleHto.BasicParameter)}}>]
                     public IActionResult Info() => this.Ok();
                 }
                 """),
             GetExampleHtoCode(),
        ]);
        var apiExplorer = CreateApiExplorer(assembly);

        var getTemplate = apiExplorer.GetFullRouteTemplateFor(GetType<ExampleHto>(assembly));
        getTemplate.Should().ContainSingle().Which.Should().Be("Test/Get");

        var endpoints = apiExplorer.GetHypermediaEndpoints();
        endpoints.Should().HaveCount(3);
        
        var getEndpoint = endpoints.Should().ContainSingle(a =>
            a.ActionDescriptor.EndpointMetadata.OfType<IHypermediaObjectEndpointMetadata>().Any()).Which;
        getEndpoint.RelativePath.Should().Be("Test/Get");
        var hmoMetadata = getEndpoint.ActionDescriptor.EndpointMetadata.OfType<IHypermediaObjectEndpointMetadata>().Should()
            .ContainSingle().Which;
        hmoMetadata.RouteType.Should().Be(GetType<ExampleHto>(assembly));

        var postEndpoint = endpoints.Should().ContainSingle(a =>
            a.ActionDescriptor.EndpointMetadata.OfType<IHypermediaActionEndpointMetadata>().Any()).Which;
        postEndpoint.RelativePath.Should().Be("Test/Post");
        var actionMetadata = postEndpoint.ActionDescriptor.EndpointMetadata.OfType<IHypermediaActionEndpointMetadata>()
            .Should().ContainSingle().Which;
        actionMetadata.ActionType.Should().Be(GetType<ExampleHto.BasicOp>(assembly));

        var parameterInfoEndpoint = endpoints.Should().ContainSingle(a =>
            a.ActionDescriptor.EndpointMetadata.OfType<IHypermediaActionParameterInfoEndpointMetadata>().Any()).Which;
        parameterInfoEndpoint.RelativePath.Should().Be("Test/Info");
        var parameterInfoMetadata = parameterInfoEndpoint.ActionDescriptor.EndpointMetadata
            .OfType<IHypermediaActionParameterInfoEndpointMetadata>().Should().ContainSingle().Which;
        parameterInfoMetadata.RouteType.Should().Be(GetType<ExampleHto.BasicParameter>(assembly));
    }

    [TestMethod]
    public void CreateModel_WithHmosDerivingFromOtherHmos()
    {
        var derived1name = "DerivedHmo1";
        var derived2name = "DerivedHmo2";
        var baseName = "BaseHmo";
        var assembly = CreateAssembly([
            CreateFile(
                $$"""
                [ApiController]
                public class MyController : ControllerBase
                {
                    [HttpGet("Route/To/Hmo1/{key}"), HypermediaObjectEndpoint<{{derived1name}}>]
                    public {{derived1name}} GetDerivedHmo1(string key)
                    {
                        return new {{derived1name}}(key);
                    }
                
                    [HttpGet("Route/To/Hmo2/{key}"), HypermediaObjectEndpoint<{{derived2name}}>]
                    public {{derived2name}} GetDerivedHmo2(string key)
                    {
                        return new {{derived2name}}(key);
                    }
                }
                """, false),
            CreateFile(
                $$"""
                [HypermediaObject(Classes = ["{{baseName}}"])]
                public abstract class {{baseName}} : IHypermediaObject
                {
                    [RESTyard.AspNetCore.WebApi.RouteResolver.Key]
                    public string Key { get; }
                
                    protected {{baseName}}(string key)
                    {
                        Key = key;
                    }
                }
                
                [HypermediaObject(Classes = ["{{derived1name}}"])]
                public class {{derived1name}} : {{baseName}}
                {
                    public {{derived1name}}(string key) : base(key)
                    {
                    }
                }
                
                [HypermediaObject(Classes = ["{{derived2name}}"])]
                public class {{derived2name}} : {{baseName}}
                {
                    public {{derived2name}}(string key) : base(key)
                    {
                    }
                }
                """, false),
        ]);
        var apiExplorer = CreateApiExplorer(assembly);

        var templates = apiExplorer.GetFullRouteTemplateFor(assembly.GetType($"{TestAssemblyNamespace}.{baseName}")!);
        templates.Should().HaveCount(2);
        templates.Should().BeEquivalentTo([
            "Route/To/Hmo1/{key}",
            "Route/To/Hmo2/{key}",
        ]);

        var endpoints = apiExplorer.GetHypermediaEndpoints();
        endpoints.Should().HaveCount(2);

        var ep1 = endpoints.Should().ContainSingle(a => a.RelativePath!.Contains("Hmo1")).Which;
        ep1.ActionDescriptor.EndpointMetadata.OfType<IHypermediaObjectEndpointMetadata>().Should().ContainSingle().Which
            .RouteType.Should().Be(assembly.GetType($"{TestAssemblyNamespace}.{derived1name}"));
        
        var ep2 = endpoints.Should().ContainSingle(a => a.RelativePath!.Contains("Hmo2")).Which;
        ep2.ActionDescriptor.EndpointMetadata.OfType<IHypermediaObjectEndpointMetadata>().Should().ContainSingle().Which
            .RouteType.Should().Be(assembly.GetType($"{TestAssemblyNamespace}.{derived2name}"));
    }
}