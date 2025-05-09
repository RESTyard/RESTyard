using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.AspNetCore.JsonSchema;

namespace RESTyard.AspNetCore.Test.JsonSchema;

[TestClass]
public class KeyFromUriExtensionTests
{
    [TestMethod]
    // Regression test for https://github.com/RESTyard/RESTyard/issues/83
    public void GetKeyFromUriProperties_WithStaticNesting_DoesNotThrowStackOverflowException()
    {
        // ARRANGE
        var action = () => KeyFromUriExtension.GetKeyFromUriProperties(typeof(StaticRecursionRecord));
        
        // ACT
        action.Should().NotThrow<StackOverflowException>();
    }

    public record StaticRecursionRecord(string Text)
    {
        public static StaticRecursionRecord Empty => new("");
    }

    [TestMethod]
    // Regression test for https://github.com/RESTyard/RESTyard/issues/83
    public void GetKeyFromUriProperties_WithInstanceNesting_DoesNotThrowStackOverflowException()
    {
        // ARRANGE
        var action = () => KeyFromUriExtension.GetKeyFromUriProperties(typeof(RecursionRecord));
        
        // ACT / ASSERT
        action.Should().NotThrow<StackOverflowException>();
    }

    public record RecursionRecord(string Text, RecursionRecord? Child);
}