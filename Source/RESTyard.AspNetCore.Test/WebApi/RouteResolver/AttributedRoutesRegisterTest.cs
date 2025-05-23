using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.Test.WebApi.RouteResolver;

[TestClass]
public class AttributedRoutesRegisterTest : AssemblyBasedTestBase
{
    private static AttributedRoutesRegister CreateRegister(Assembly assembly) => new AttributedRoutesRegister(
        new HypermediaExtensionsOptions() { ControllerAndHypermediaAssemblies = [assembly] },
        null!);

    [TestMethod]
    public void LegacyGetHypermediaObjectAttribute()
    {
        var assembly = CreateAssembly([
            CreateFile(
                $$"""
                [Route("Test")]
                public class Controller : ControllerBase
                {
                    [HttpGetHypermediaObject("Get", typeof({{nameof(ExampleHto)}}))]
                    public IActionResult Get() => this.Ok();
                }
                """),
            GetExampleHtoCode(),
        ]);
        var register = CreateRegister(assembly);
        var type = GetType<ExampleHto>(assembly);
        register.TryGetRoute(type, out var info).Should().BeTrue();
        info.HttpMethod.Should().Be(HttpMethod.GET);
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
                public class Controller : ControllerBase
                {
                    [HttpGet("Get")]
                    [HypermediaObjectEndpoint<{{nameof(ExampleHto)}}>]
                    public IActionResult Get() => this.Ok();
                }
                """),
            GetExampleHtoCode(),
        ]);
        var register = CreateRegister(assembly);
        var type = GetType<ExampleHto>(assembly);
        register.TryGetRoute(type, out var info).Should().BeTrue();
        info.HttpMethod.Should().Be(HttpMethod.GET);
        info.AcceptableMediaType.Should().BeNull();
        info.Name.Should().Contain(nameof(ExampleHto));
    }

    [TestMethod]
    [DataRow(HttpMethod.POST)]
    [DataRow(HttpMethod.PUT)]
    [DataRow(HttpMethod.PATCH)]
    [DataRow(HttpMethod.DELETE)]
    public void TestLegacyHypermediaAction(HttpMethod method)
    {
        var methodString = method switch
        {
            HttpMethod.POST => "Post",
            HttpMethod.PUT => "Put",
            HttpMethod.PATCH => "Patch",
            HttpMethod.DELETE => "Delete",
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, method.ToString()),
        };
        var assembly = CreateAssembly([
            CreateFile(
                $$"""
                  [Route("Test")]
                  public class Controller : ControllerBase
                  {
                      [Http{{methodString}}HypermediaAction("{{methodString}}", typeof({{nameof(ExampleHto)}}.{{nameof(ExampleHto.BasicOp)}}))]
                      public IActionResult {{methodString}}() => this.Ok();
                  }
                  """),
            GetExampleHtoCode(),
        ]);
        var register = CreateRegister(assembly);
        var type = GetType<ExampleHto.BasicOp>(assembly);
        register.TryGetRoute(type, out var info).Should().BeTrue();
        info.HttpMethod.Should().Be(method);
        info.AcceptableMediaType.Should().BeNull();
        info.Name.Should().Contain(nameof(ExampleHto.BasicOp));
    }

    [TestMethod]
    [DataRow(HttpMethod.POST)]
    [DataRow(HttpMethod.PUT)]
    [DataRow(HttpMethod.PATCH)]
    [DataRow(HttpMethod.DELETE)]
    public void TestHypermediaAction(HttpMethod method)
    {
        var methodString = method switch
        {
            HttpMethod.POST => "Post",
            HttpMethod.PUT => "Put",
            HttpMethod.PATCH => "Patch",
            HttpMethod.DELETE => "Delete",
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, method.ToString()),
        };
        var assembly = CreateAssembly([
            CreateFile(
                $$"""
                  [Route("Test")]
                  public class Controller : ControllerBase
                  {
                      [Http{{methodString}}("{{methodString}}")]
                      [HypermediaActionEndpoint<{{nameof(ExampleHto)}}>("{{nameof(ExampleHto.DoSomething)}}")]
                      public IActionResult {{methodString}}() => this.Ok();
                  }
                  """),
            GetExampleHtoCode(),
        ]);
        var register = CreateRegister(assembly);
        var type = GetType<ExampleHto.BasicOp>(assembly);
        register.TryGetRoute(type, out var info).Should().BeTrue();
        info.HttpMethod.Should().Be(method);
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
                  public class Controller : ControllerBase
                  {
                      [HttpGetHypermediaActionParameterInfo("Info", typeof({{nameof(ExampleHto)}}.{{nameof(ExampleHto.BasicParameter)}}))]
                      public IActionResult Info() => this.Ok();
                  }
                  """),
            GetExampleHtoCode(),
        ]);
        var register = CreateRegister(assembly);
        var type = GetType<ExampleHto.BasicParameter>(assembly);
        register.TryGetRoute(type, out var info).Should().BeTrue();
        info.HttpMethod.Should().Be(HttpMethod.GET);
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
                  public class Controller : ControllerBase
                  {
                      [HttpGet("Info")]
                      [HypermediaActionParameterInfoEndpoint<{{nameof(ExampleHto)}}.{{nameof(ExampleHto.BasicParameter)}}>]
                      public IActionResult Info() => this.Ok();
                  }
                  """),
            GetExampleHtoCode(),
        ]);
        var register = CreateRegister(assembly);
        var type = GetType<ExampleHto.BasicParameter>(assembly);
        register.TryGetRoute(type, out var info).Should().BeTrue();
        info.HttpMethod.Should().Be(HttpMethod.GET);
        info.AcceptableMediaType.Should().BeNull();
        info.Name.Should().Contain(nameof(ExampleHto.BasicParameter));
    }
}