using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        var applicationModel = ApplicationModel.Create([assembly]);

        var hmoType = applicationModel.HmoTypes.Should().ContainSingle().Which;
        hmoType.Key.Name.Should().Be(nameof(ExampleHto));
        hmoType.Value.GetHmoMethods.Should().ContainSingle().Which.RouteTemplateFull.Should().Be("Test/Get");

        var actionParameter = applicationModel.ActionParameterTypes.Should().ContainSingle().Which;
        actionParameter.Key.Name.Should().Be(nameof(ExampleHto.BasicParameter));
        actionParameter.Value.GetActionParameterInfoMethod.RouteTemplateFull.Should().Be("Test/Info");

        var controllerType = applicationModel.ControllerTypes.Should().ContainSingle().Which;
        controllerType.RouteTemplate.Should().Be("Test");
        controllerType.Methods.Should().HaveCount(3);
        controllerType.Methods.Should().ContainSingle(m => m.RouteTemplateFull == "Test/Get");
        var postMethod = controllerType.Methods.Should().ContainSingle(m => m.RouteTemplateFull == "Test/Post").Which;
        postMethod.Should().BeOfType<ApplicationModel.ActionMethod>().Which.ActionType.Name.Should()
            .Be(nameof(ExampleHto.BasicOp));
        var getInfoMethod = controllerType.Methods.Should().ContainSingle(m => m.RouteTemplateFull == "Test/Info").Which;
        getInfoMethod.Should().BeOfType<ApplicationModel.GetActionParameterInfoMethod>().Which.ActionParameterType.Name
            .Should().Be(nameof(ExampleHto.BasicParameter));
    }
    
    [TestMethod]
    public void CreateModel_HypermediaAttributes()
    {
        var assembly = CreateAssembly([
            CreateFile(
                $$"""
                [Route("Test")]
                public class Controller : ControllerBase
                {
                    [HttpGet("Get"), HypermediaEndpoint(typeof({{nameof(ExampleHto)}}))]
                    public IActionResult Get() => this.Ok();
                    
                    [HttpPost("Post"), HypermediaEndpoint(typeof({{nameof(ExampleHto)}}), "{{nameof(ExampleHto.DoSomething)}}")]
                    public IActionResult Post() => this.Ok();
                    
                    [HttpGet("Info"), HypermediaParameterInfo(typeof({{nameof(ExampleHto)}}.{{nameof(ExampleHto.BasicParameter)}}))]
                    public IActionResult Info() => this.Ok();
                }
                """),
            GetExampleHtoCode(),
        ]);
        var applicationModel = ApplicationModel.Create([assembly]);

        var hmoType = applicationModel.HmoTypes.Should().ContainSingle().Which;
        hmoType.Key.Name.Should().Be(nameof(ExampleHto));
        hmoType.Value.GetHmoMethods.Should().ContainSingle().Which.RouteTemplateFull.Should().Be("Test/Get");

        var actionParameter = applicationModel.ActionParameterTypes.Should().ContainSingle().Which;
        actionParameter.Key.Name.Should().Be(nameof(ExampleHto.BasicParameter));
        actionParameter.Value.GetActionParameterInfoMethod.RouteTemplateFull.Should().Be("Test/Info");

        var controllerType = applicationModel.ControllerTypes.Should().ContainSingle().Which;
        controllerType.RouteTemplate.Should().Be("Test");
        controllerType.Methods.Should().HaveCount(3);
        controllerType.Methods.Should().ContainSingle(m => m.RouteTemplateFull == "Test/Get");
        var postMethod = controllerType.Methods.Should().ContainSingle(m => m.RouteTemplateFull == "Test/Post").Which;
        postMethod.Should().BeOfType<ApplicationModel.ActionMethod>().Which.ActionType.Name.Should()
            .Be(nameof(ExampleHto.BasicOp));
        var getInfoMethod = controllerType.Methods.Should().ContainSingle(m => m.RouteTemplateFull == "Test/Info").Which;
        getInfoMethod.Should().BeOfType<ApplicationModel.GetActionParameterInfoMethod>().Which.ActionParameterType.Name
            .Should().Be(nameof(ExampleHto.BasicParameter));
    }
}