using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.Test.WebApi.RouteResolver;

[TestClass]
public class AttributedRoutesRegisterTest : AssemblyBasedTestBase
{
    private static AttributedRoutesRegister CreateRegister(IHypermediaApiExplorer apiExplorer) => new AttributedRoutesRegister(
        apiExplorer,
        null!);

    [TestMethod]
    public void LegacyGetHypermediaObjectAttribute()
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
                }
                """),
            GetExampleHtoCode(),
        ]);
        var apiExplorer = CreateApiExplorer(assembly);
        var register = CreateRegister(apiExplorer);
        var type = GetType<ExampleHto>(assembly);
        register.TryGetRoute(type, out var info).Should().BeTrue();
        info.HttpMethod.Should().Be(HttpMethods.Get);
        info.AcceptableMediaType.Should().BeNull();
        info.Name.Should().Contain(nameof(ExampleHto));
    }
    
    [TestMethod]
    public void HypermediaEndpoint_Get()
    {
        var assembly = CreateAssembly([
            CreateFile(
                $$"""
                [Route("Test")]
                [ApiController]
                public class Controller : ControllerBase
                {
                    [HttpGet("Get")]
                    [HypermediaObjectEndpoint<{{nameof(ExampleHto)}}>]
                    public IActionResult Get() => this.Ok();
                }
                """),
            GetExampleHtoCode(),
        ]);
        var apiExplorer = CreateApiExplorer(assembly);
        var register = CreateRegister(apiExplorer);
        var type = GetType<ExampleHto>(assembly);
        register.TryGetRoute(type, out var info).Should().BeTrue();
        info.HttpMethod.Should().Be(HttpMethods.Get);
        info.AcceptableMediaType.Should().BeNull();
        info.Name.Should().Contain(nameof(ExampleHto));
    }

    [TestMethod]
    [DataRow("Post")]
    [DataRow("Put")]
    [DataRow("Patch")]
    [DataRow("Delete")]
    public void TestLegacyHypermediaAction(string method)
    {
        var assembly = CreateAssembly([
            CreateFile(
                $$"""
                  [Route("Test")]
                  [ApiController]
                  public class Controller : ControllerBase
                  {
                      [Http{{method}}HypermediaAction("{{method}}", typeof({{nameof(ExampleHto)}}.{{nameof(ExampleHto.BasicOp)}}))]
                      public IActionResult {{method}}() => this.Ok();
                  }
                  """),
            GetExampleHtoCode(),
        ]);
        var apiExplorer = CreateApiExplorer(assembly);
        var register = CreateRegister(apiExplorer);
        var type = GetType<ExampleHto.BasicOp>(assembly);
        register.TryGetRoute(type, out var info).Should().BeTrue();
        info.HttpMethod.Should().Be(method.ToUpper());
        info.AcceptableMediaType.Should().BeNull();
        info.Name.Should().Contain(nameof(ExampleHto.BasicOp));
    }

    [TestMethod]
    [DataRow("Post")]
    [DataRow("Put")]
    [DataRow("Patch")]
    [DataRow("Delete")]
    public void TestHypermediaAction(string method)
    {
        var assembly = CreateAssembly([
            CreateFile(
                $$"""
                  [Route("Test")]
                  [ApiController]
                  public class Controller : ControllerBase
                  {
                      [Http{{method}}("{{method}}")]
                      [HypermediaActionEndpoint<{{nameof(ExampleHto)}}>("{{nameof(ExampleHto.DoSomething)}}")]
                      public IActionResult {{method}}() => this.Ok();
                  }
                  """),
            GetExampleHtoCode(),
        ]);
        var apiExplorer = CreateApiExplorer(assembly);
        var register = CreateRegister(apiExplorer);
        var type = GetType<ExampleHto.BasicOp>(assembly);
        register.TryGetRoute(type, out var info).Should().BeTrue();
        info.HttpMethod.Should().Be(method.ToUpper());
        info.AcceptableMediaType.Should().BeNull();
        info.Name.Should().Contain(nameof(ExampleHto.BasicOp));
    }

    [TestMethod]
    public void LegacyHypermediaParameterInfoEndpoint_Get()
    {
        var assembly = CreateAssembly([
            CreateFile(
                $$"""
                  [Route("Test")]
                  [ApiController]
                  public class Controller : ControllerBase
                  {
                      [HttpGetHypermediaActionParameterInfo("Info", typeof({{nameof(ExampleHto)}}.{{nameof(ExampleHto.BasicParameter)}}))]
                      public IActionResult Info() => this.Ok();
                  }
                  """),
            GetExampleHtoCode(),
        ]);
        var apiExplorer = CreateApiExplorer(assembly);
        var register = CreateRegister(apiExplorer);
        var type = GetType<ExampleHto.BasicParameter>(assembly);
        register.TryGetRoute(type, out var info).Should().BeTrue();
        info.HttpMethod.Should().Be(HttpMethods.Get);
        info.AcceptableMediaType.Should().BeNull();
        info.Name.Should().Contain(nameof(ExampleHto.BasicParameter));
    }

    [TestMethod]
    public void HypermediaParameterInfoEndpoint_Get()
    {
        var assembly = CreateAssembly([
            CreateFile(
                $$"""
                  [Route("Test")]
                  [ApiController]
                  public class Controller : ControllerBase
                  {
                      [HttpGet("Info")]
                      [HypermediaActionParameterInfoEndpoint<{{nameof(ExampleHto)}}.{{nameof(ExampleHto.BasicParameter)}}>]
                      public IActionResult Info() => this.Ok();
                  }
                  """),
            GetExampleHtoCode(),
        ]);
        var apiExplorer = CreateApiExplorer(assembly);
        var register = CreateRegister(apiExplorer);
        var type = GetType<ExampleHto.BasicParameter>(assembly);
        register.TryGetRoute(type, out var info).Should().BeTrue();
        info.HttpMethod.Should().Be(HttpMethods.Get);
        info.AcceptableMediaType.Should().BeNull();
        info.Name.Should().Contain(nameof(ExampleHto.BasicParameter));
    }
}