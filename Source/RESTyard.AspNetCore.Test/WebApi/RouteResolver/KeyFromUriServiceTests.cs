using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.Test.WebApi.RouteResolver;

[TestClass]
public class KeyFromUriServiceTests
{
    [TestMethod]
    public void Test()
    {
        var applicationModel = ApplicationModel.Create([typeof(TestHto).Assembly]);
        var service = new KeyFromUriService(applicationModel);

        var uri = new Uri("https://api.local:1234/Test/15/some-key?someOther=42");
        var values = service.GetKeyFromUri<TestHto, TestHto.TestHtoKey>(uri);
        values.Should().Be(new TestHto.TestHtoKey("some-key", 15, 42));
    }

    public class TestHto : HypermediaObject
    {
        public TestHto(string key)
        {
            Key = key;
        }

        [Key]
        public string Key { get; set; }

        [Key]
        public int IntKey { get; set; }

        [Key]
        public double SomeOther { get; set; }

        public record TestHtoKey(string Key, int IntKey, double SomeOther);
    }

    [Route("Test")]
    public class Controller : ControllerBase
    {
        [HttpGetHypermediaObject("{intKey:int}/{key}", typeof(TestHto))]
        public async Task<IActionResult> Get(string key, int intKey)
        {
            await Task.Delay(5);
            return this.Problem("not implemented");
        }
    }
}