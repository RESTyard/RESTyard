using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;

namespace WebApi.Hypermedia.Serializer.Tests.ObjectReflection.Links
{
    [TestClass]
    public class When_reflecting_hto_with_link : ObjectReflectionServiceTestBase
    {
        public override void When()
        {
            this.Result = this.ObjectReflectionService.Reflect(typeof(ExternalLinkHto));
        }

        [TestMethod]
        public void Then_hto_can_be_reflected()
        {
            Result.GetValueOrThrow();
        }

        [TestMethod]
        public void Then_result_contains_one_link()
        {
            Result.GetValueOrThrow().Links.Count.Should().Be(1);
        }

        [TestMethod]
        public void Then_result_self_link_has_relation()
        {
            var linkAttribute = Result.GetValueOrThrow().Links.First().PrimaryHypermediaAttribute.GetValueOrThrow().As<Link>();
            linkAttribute.Relations.Single().Should().Be("MyExternal");
        }

        [HypermediaObject(NoDefaultSelfLink = true)]
        private class ExternalLinkHto : HypermediaExtensions.Hypermedia.HypermediaObject
        {
            [Link("MyExternal")]
            public Uri ExternalLink { get; private set; } = new Uri("www.example.com");

        }
    }
}