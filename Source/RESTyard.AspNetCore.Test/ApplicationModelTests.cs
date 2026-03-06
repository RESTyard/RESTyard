using System.Linq;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;

namespace RESTyard.AspNetCore.Test;

[TestClass]
public class ApplicationModelTests : AssemblyBasedTestBase
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
                    [HttpGetHypermediaActionParameterInfo("Info", typeof({{nameof(ExampleHto)}}.{{nameof(ExampleHto.BasicParameter)}}))]
                    public IActionResult Info() => this.Ok();
                }
                """),
            GetExampleHtoCode(),
        ]);
        var applicationModel = ApplicationModel.Create([assembly]);

        var actionParameter = applicationModel.ActionParameterTypes.Should().ContainSingle().Which;
        actionParameter.Key.Name.Should().Be(nameof(ExampleHto.BasicParameter));
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
                     [HttpGet("Info"), HypermediaActionParameterInfoEndpoint<{{nameof(ExampleHto)}}.{{nameof(ExampleHto.BasicParameter)}}>]
                     public IActionResult Info() => this.Ok();
                 }
                 """),
             GetExampleHtoCode(),
        ]);
        var applicationModel = ApplicationModel.Create([assembly]);

        var actionParameter = applicationModel.ActionParameterTypes.Should().ContainSingle().Which;
        actionParameter.Key.Name.Should().Be(nameof(ExampleHto.BasicParameter));
    }
}