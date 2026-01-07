using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using FunicularSwitch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.Test.WebApi.RouteResolver;

[TestClass]
public class KeyFromUriServiceTests
{
    private IHypermediaApiExplorer CreateExplorer(string[] templates)
    {
        var mock = new Mock<IHypermediaApiExplorer>();
        mock.Setup(e => e.GetFullRouteTemplateFor(It.IsAny<Type>()))
            .Returns(templates);
        return mock.Object;
    }

    [TestMethod]
    public void Test()
    {
        // Given
        var apiExplorer = CreateExplorer(["Test/{intKey:int}/{key}"]);
        var service = new KeyFromUriService(apiExplorer);
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
        var apiExplorer = CreateExplorer([]);
        var service = new KeyFromUriService(apiExplorer);
        var uri = new Uri("https://some.uri");
        
        // When
        var values = service.GetKeyFromUri<IHypermediaObject, string>(uri);
        
        // Then
        values.Should().BeError().Which.Should().Contain("route");
    }

    [TestMethod]
    public void TypeWithoutGetMethod_ReturnsError()
    {
        // Given
        var apiExplorer = CreateExplorer([]);
        var service = new KeyFromUriService(apiExplorer);
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
        var apiExplorer = CreateExplorer(["Test/{intKey:int}/{key}"]);
        var service = new KeyFromUriService(apiExplorer);
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
        var apiExplorer = CreateExplorer(["Test/{intKey:int}/{key}"]);
        var service = new KeyFromUriService(apiExplorer);
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
        var apiExplorer = CreateExplorer(["Test/{intKey:int}/{key}"]);
        var service = new KeyFromUriService(apiExplorer);
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
        var apiExplorer = CreateExplorer(["Test/{intKey:int}/{key}"]);
        var service = new KeyFromUriService(apiExplorer);
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
        var apiExplorer = CreateExplorer(["Test/{intKey:int}/{key}"]);
        var service = new KeyFromUriService(apiExplorer);
        var uri = new Uri("https://api.local:1234/Test/15/some-key?someOther=42");
        
        // When
        var values = service.GetKeyFromUri<TestHto, TestHto.TestHtoKeyWhichThrows>(uri);
        
        // Then
        values.Should().BeError().Which.Should().Contain("invoke");
    }
    
    [TestMethod]
    public void KeyHasMultipleConstructors_TheConstructorWithParametersIsChosen()
    {
        // Given
        var apiExplorer = CreateExplorer(["Test/{intKey:int}/{key}"]);
        var service = new KeyFromUriService(apiExplorer);
        var uri = new Uri("https://api.local:1234/Test/15/some-key?someOther=42");
        
        // When
        var values = service.GetKeyFromUri<TestHto, ClassWithMultipleConstructors>(uri);
        
        // Then
        var result = values.Should().BeOk().Which;
        result.Key.Should().Be("some-key");
        result.IntKey.Should().Be(15);
        result.SomeOther.Should().Be(42);
    }

    [TestMethod]
    public void GuidAsKeyType_IsResolvedProperly()
    {
        // Given
        var apiExplorer = CreateExplorer(["Test/AllTypes/{intKey}/{guidKey}"]);
        var service = new KeyFromUriService(apiExplorer);
        var guid = "821516ed-8f2b-4a23-9e30-5e9781a6490a";
        var uri = new Uri($"https://api.local:1234/Test/AllTypes/15/{guid}");
        
        // When
        var values = service.GetKeyFromUri<AllKeyTypesHto, AllKeyTypesHto.KeyRecord>(uri);
        
        // Then
        var result = values.Should().BeOk().Which;
        result.IntKey.Should().Be(15);
        result.GuidKey.Should().Be(new Guid(guid));
    }

    [HypermediaObject(Classes = [nameof(TestHto)])]
    public class TestHto : IHypermediaObject
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

    [HypermediaObject(Classes = ["AllKeyTypes"])]
    public class AllKeyTypesHto : IHypermediaObject
    {
        [Key]
        public int IntKey { get; set; }
        
        [Key]
        public Guid GuidKey { get; set; }

        public record KeyRecord(int IntKey, Guid GuidKey);
    }

    [HypermediaObject(Classes = [nameof(HtoWithoutGet)])]
    public class HtoWithoutGet : IHypermediaObject
    {
        
    }

    public class ClassWithoutPublicConstructor
    {
        private ClassWithoutPublicConstructor()
        {
                
        }
    }

    public class ClassWithMultipleConstructors
    {
        public string Key { get; }
        public int IntKey { get; }
        public double SomeOther { get; }

        public ClassWithMultipleConstructors() : this("Hi", 1, 0.5d)
        {
        }

        public ClassWithMultipleConstructors(string key, int intKey, double someOther)
        {
            Key = key;
            IntKey = intKey;
            SomeOther = someOther;
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

        [HttpGetHypermediaObject("AllTypes/{intKey}/{guidKey}", typeof(AllKeyTypesHto))]
        public IActionResult GetAllKeyTypesHto(int intKey, Guid guidKey)
        {
            return this.Ok();
        }
    }
}