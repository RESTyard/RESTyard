using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Extensions;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.Test.Hypermedia
{
    [TestClass]
    public class EntitiesListExtensionsTest
    {
        [TestMethod]
        public void FilterByClass_NoEntities_ReturnsNoEntities()
        {
            var ho = new TestHypermediaObject();

            var result = ho.Entities.FilterByClass<SubEntity1HypermediaObject>();
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void FilterByClass_SubEntities_ReturnsEntities()
        {
            var ho = new TestHypermediaObject();
            var entity1 = new SubEntity1HypermediaObject();
            var entity2 = new SubEntity1HypermediaObject();

            ho.Entities.Add("ARel", entity1);
            ho.Entities.Add("ARel", entity2);

            var result = ho.Entities.FilterByClass<SubEntity1HypermediaObject>().ToList();
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(e => e.Reference.GetInstance() == entity1));
            Assert.IsTrue(result.Any(e => e.Reference.GetInstance() == entity2));
        }

        [TestMethod]
        public void FilterByClass_SubEntities_ReturnsOnlyMatchingEntities()
        {
            var ho = new TestHypermediaObject();
            var entity1 = new SubEntity1HypermediaObject();
            var entity2 = new SubEntity2HypermediaObject();

            ho.Entities.Add("ARel", entity1);
            ho.Entities.Add("ARel", entity2);

            var result = ho.Entities.FilterByClass<SubEntity1HypermediaObject>().ToList();
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Any(e => e.Reference.GetInstance() == entity1));
        }

        [TestMethod]
        public void FilterByClass_DerivedSubEntities_ReturnsOnlyMatchingEntities()
        {
            var ho = new TestHypermediaObject();
            var entity1 = new SubEntity1HypermediaObject();
            var entity2 = new SubEntity2HypermediaObject();
            var entity3 = new DerivedSubEntityHypermediaObject();

            ho.Entities.Add("ARel", entity1);
            ho.Entities.Add("ARel", entity2);
            ho.Entities.Add("ARel", entity3);

            var result = ho.Entities.FilterByClass<SubEntity1HypermediaObject>().ToList();
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(e => e.Reference.GetInstance() == entity1));
            Assert.IsTrue(result.Any(e => e.Reference.GetInstance() == entity3));
        }

        [TestMethod]
        public void FilterByRelation_NoSubEntities_ReturnsZero()
        {
            var ho = new TestHypermediaObject();

            var result = ho.Entities.FilterByRelation("ARel").ToList();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FilterByRelation_OneEntity_ReturnsEntity()
        {
            const string relation = "ARel";

            var ho = new TestHypermediaObject();
            var entity1 = new SubEntity1HypermediaObject();
            ho.Entities.Add(relation, entity1);

            var result = ho.Entities.FilterByRelation(relation).ToList();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void FilterByRelation_DifferentEntities_ReturnsMatching()
        {
            const string relation = "ARel";

            var ho = new TestHypermediaObject();
            var entity1 = new SubEntity1HypermediaObject();
            ho.Entities.Add(relation, entity1);
            ho.Entities.Add(relation, entity1);
            ho.Entities.Add("AnotherRel", entity1);

            var result = ho.Entities.FilterByRelation(relation).ToList();
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void FilterByRelation_RelationList_ReturnsMatching()
        {
            const string relation = "ARel";
            var relList = new List<string> {relation, "Another"};

            var ho = new TestHypermediaObject();
            var entity1 = new SubEntity1HypermediaObject();
            ho.Entities.Add(relList, entity1);

            var result = ho.Entities.FilterByRelation(relation).ToList();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void FilterByRelations_RelationList_ReturnsMatching()
        {
            const string relationA = "ARel";
            const string relationB = "BRel";
            const string relationC = "CRel";
            var relList = new List<string> { relationA, relationB, relationC };

            var ho = new TestHypermediaObject();
            var entity1 = new SubEntity1HypermediaObject();
            var entity2 = new SubEntity1HypermediaObject();
            ho.Entities.Add(relList, entity1);
            ho.Entities.Add(relationA, entity2);

            var filterRelList = new List<string> { relationA, relationB };
            var result = ho.Entities.FilterByRelations(filterRelList).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Any(e => e.Reference.GetInstance() == entity1));
        }

        [TestMethod]
        public void GetInstanceByClass_MixedEntities_ReturnsMatching()
        {
            const string relationA = "ARel";
            
            var ho = new TestHypermediaObject();
            var entity1 = new SubEntity1HypermediaObject();
            var entity2 = new SubEntity2HypermediaObject();


            ho.Entities.Add(relationA, entity1);
            ho.Entities.Add(relationA, entity2);


            var result = ho.Entities.GetInstanceByClass<SubEntity1HypermediaObject>().ToList();

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Contains(entity1));
        }

        [TestMethod]
        public void GetInstanceByClass_EntityWithNullAsInstance_ReturnsMatching()
        {
            const string relationA = "ARel";

            var ho = new TestHypermediaObject();
            var entity1 = new SubEntity1HypermediaObject();
            var entity2 = new SubEntity2HypermediaObject();


            ho.Entities.Add(relationA, entity1);
            // get instance will return null
            ho.Entities.Add(relationA, new HypermediaObjectKeyReference(typeof(TestHypermediaObject), "AKey"));


            var result = ho.Entities.GetInstanceByClass<SubEntity1HypermediaObject>().ToList();

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Contains(entity1));
        }

        public class TestHypermediaObject : HypermediaObject
        {
        }

        public class SubEntity1HypermediaObject : HypermediaObject
        {
        }

        public class SubEntity2HypermediaObject : HypermediaObject
        {
        }

        public class DerivedSubEntityHypermediaObject : SubEntity1HypermediaObject
        {
        }
    }
}
