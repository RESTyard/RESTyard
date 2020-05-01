﻿using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.WebApi.Serializer;

namespace WebApi.Hypermedia.ModelFactory.Test.ObjectReflection.Links
{
    [TestClass]
    public class When_building_model_for_hto_with_object_link : ModelFactoryTestBase
    {
        public override void When()
        {
            this.Result = ModelFactory2.Build(typeof(TestHto));
        }

        [TestMethod]
        public void Then_hto_can_be_reflected()
        {
            Result.GetValueOrThrow();
        }

        [TestMethod]
        public void Then_result_contains_one_link()
        {
            Result.GetValueOrThrow().Links.Length.Should().Be(1);
        }

        [TestMethod]
        public void Then_result_link_has_relation()
        {
            Result.GetValueOrThrow().Links.First().Relations.Contains("MyRelation");
        }

        [TestMethod]
        public void Then_result_link_has_one_relation()
        {
            Result.GetValueOrThrow().Links.Length.Should().Be(1);
        }

        [HypermediaObject(NoDefaultSelfLink = true)]
        private class TestHto
        {
            [Link("MyRelation")]
            public ReferencedHto HtoLink { get; private set; }

        }

        [HypermediaObject(NoDefaultSelfLink = true)]
        private class ReferencedHto
        {
        }
    }
}