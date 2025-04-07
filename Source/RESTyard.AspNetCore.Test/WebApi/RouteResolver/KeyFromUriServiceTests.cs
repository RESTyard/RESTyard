using System;
using System.Threading.Tasks;
using FluentAssertions;
using FunicularSwitch;
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
        // Given
        var applicationModel = ApplicationModel.Create([typeof(TestHto).Assembly]);
        var service = new KeyFromUriService(applicationModel);
        var uri = new Uri("https://api.local:1234/Test/15/some-key?someOther=42");
        
        // When
        var values = service.GetKeyFromUri<TestHto, TestHto.TestHtoKey>(uri);
        
        // Then
        values.Should().BeOk().Which.Should().Be(new TestHto.TestHtoKey("some-key", 15, 42));
    }

    [TestMethod]
    public void TypeNotRegistered_ReturnsError()
    {
        // Given
        var applicationModel = ApplicationModel.Create([typeof(TestHto).Assembly]);
        var service = new KeyFromUriService(applicationModel);
        var uri = new Uri("https://some.uri");
        
        // When
        var values = service.GetKeyFromUri<HypermediaObject, string>(uri);
        
        // Then
        values.Should().BeError().Which.Should().Contain("application model");
    }

    [TestMethod]
    public void TypeWithoutGetMethod_ReturnsError()
    {
        // Given
        var applicationModel = ApplicationModel.Create([typeof(TestHto).Assembly]);
        var service = new KeyFromUriService(applicationModel);
        var uri = new Uri("https://some.uri");
        
        // When
        var values = service.GetKeyFromUri<HtoWithoutGet, string>(uri);
        
        // Then
        values.Should().BeError().Which.Should().Contain("route");
    }

    [TestMethod]
    public void UriDoesNotMatch_ReturnsError()
    {
        // Given
        var applicationModel = ApplicationModel.Create([typeof(TestHto).Assembly]);
        var service = new KeyFromUriService(applicationModel);
        var uri = new Uri("https://api.local:1234/HiThere");
        
        // When
        var values = service.GetKeyFromUri<TestHto, TestHto.TestHtoKey>(uri);
        
        // Then
        values.Should().BeError().Which.Should().Contain("match");
    }
    
    [TestMethod]
    public void KeyHasNoPublicConstructor_ReturnsError()
    {
        // Given
        var applicationModel = ApplicationModel.Create([typeof(TestHto).Assembly]);
        var service = new KeyFromUriService(applicationModel);
        var uri = new Uri("https://api.local:1234/Test/15/some-key?someOther=42");
        
        // When
        var values = service.GetKeyFromUri<TestHto, ClassWithoutPublicConstructor>(uri);
        
        // Then
        values.Should().BeError().Which.Should().Contain("constructor");
    }
    
    [TestMethod]
    public void NamesOfRouteAndKeyMismatch_ReturnsError()
    {
        // Given
        var applicationModel = ApplicationModel.Create([typeof(TestHto).Assembly]);
        var service = new KeyFromUriService(applicationModel);
        var uri = new Uri("https://api.local:1234/Test/15/some-key?someOther=42");
        
        // When
        var values = service.GetKeyFromUri<TestHto, TestHto.TestHtoKeyBadNames>(uri);
        
        // Then
        values.Should().BeError().Which.Should().Contain("present");
    }
    
    [TestMethod]
    public void TypeOfRouteAndKeyMismatch_ReturnsError()
    {
        // Given
        var applicationModel = ApplicationModel.Create([typeof(TestHto).Assembly]);
        var service = new KeyFromUriService(applicationModel);
        var uri = new Uri("https://api.local:1234/Test/15/some-key?someOther=42");
        
        // When
        var values = service.GetKeyFromUri<TestHto, TestHto.TestHtoKeyBadTypes>(uri);
        
        // Then
        values.Should().BeError().Which.Should().Contain("convert");
    }
    
    [TestMethod]
    public void ExceptionInConstructor_ReturnsError()
    {
        // Given
        var applicationModel = ApplicationModel.Create([typeof(TestHto).Assembly]);
        var service = new KeyFromUriService(applicationModel);
        var uri = new Uri("https://api.local:1234/Test/15/some-key?someOther=42");
        
        // When
        var values = service.GetKeyFromUri<TestHto, TestHto.TestHtoKeyWhichThrows>(uri);
        
        // Then
        values.Should().BeError().Which.Should().Contain("invoke");
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

        public record TestHtoKeyBadNames(string One, int Two, double Three);

        public record TestHtoKeyBadTypes(string Key, TestHto IntKey, TestHtoKeyBadTypes SomeOther);

        public record TestHtoKeyWhichThrows
        {
            public TestHtoKeyWhichThrows(string key, int intKey, double someOther)
            {
                throw new Exception("BOOM");
            }
        }
    }

    public class HtoWithoutGet : HypermediaObject
    {
        
    }

    public class ClassWithoutPublicConstructor
    {
        private ClassWithoutPublicConstructor()
        {
                
        }
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