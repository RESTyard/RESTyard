using System;
using System.Collections.Generic;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Test.Helpers;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter.Properties
{
    [HypermediaObject(Classes = [nameof(EmptyHypermediaObject)])]
    public class EmptyHypermediaObject : IHypermediaObject
    {
        [Relations(["Embedded"])]
        public List<IEmbeddedEntity<SirenBuilderEntitiesTest.EmbeddedSubEntity>> Embedded { get; } = [];
        
        [Relations(["RelationA", "RelationB"])]
        public List<IEmbeddedEntity<SirenBuilderEntitiesTest.EmbeddedSubEntity>> Multiple { get; } = [];
    }

    [HypermediaObject(Title = "A Title", Classes = ["CustomClass1", "CustomClass2"])]
    public class AttributedEmptyHypermediaObject : IHypermediaObject
    {
    }

    [HypermediaObject(Classes = [nameof(PropertyDuplicateHypermediaObject)])]
    public class PropertyDuplicateHypermediaObject : IHypermediaObject
    {
        [HypermediaProperty(Name = "DuplicateRename")]
        public bool Property1 { get; set; }

        [HypermediaProperty(Name = "DuplicateRename")]
        public bool Property2 { get; set; }
    }

    [HypermediaObject(Classes = [nameof(PropertyNestedClassHypermediaObject)])]
    public class PropertyNestedClassHypermediaObject : IHypermediaObject
    {
        public AttributedPropertyHypermediaObject AChild { get; set; }
    }

    [HypermediaObject(Classes = [nameof(AttributedPropertyHypermediaObject)])]
    public class AttributedPropertyHypermediaObject : IHypermediaObject
    {
        [HypermediaProperty(Name = "Property1Renamed")]
        public bool Property1 { get; set; }

        [HypermediaProperty(Name = "Property2Renamed")]
        public bool Property2 { get; set; }

        [FormatterIgnoreHypermediaProperty]
        public bool IgnoredProperty { get; set; }

        public bool NotRenamed { get; set; }
    }

    [HypermediaObject(Classes = [nameof(PropertyHypermediaObject)])]
    public class PropertyHypermediaObject : IHypermediaObject
    {
        public bool ABool { get; set; }
        public string AString { get; set; }
        public int AnInt { get; set; }
        public long ALong { get; set; }
        public float AFloat { get; set; }
        public double ADouble { get; set; }

        public TestEnum AnEnum { get; set; }
        public TestEnum? ANullableEnum { get; set; }
        public TestEnumWithNames AnEnumWithNames { get; set; }

        public DateTime ADateTime { get; set; }
        public DateTimeOffset ADateTimeOffset { get; set; }
        public TimeSpan ATimeSpan { get; set; }
        public decimal ADecimal { get; set; }
        public int? ANullableInt { get; set; }
        public Uri AnUri { get; set; }
        
        public Type AType { get; set; }
    }

    [HypermediaObject(Classes = [nameof(HypermediaObjectWithListProperties)])]
    public class HypermediaObjectWithListProperties : IHypermediaObject
    {
        public IEnumerable<int> AValueList { get; set; }

        public IEnumerable<int?> ANullableList { get; set; }

        public IEnumerable<string> AReferenceList { get; set; }

        public int[] AValueArray { get; set; } // arrays need special treatment

        public IEnumerable<Nested> AObjectList { get; set; }

        public IEnumerable<IEnumerable<int>> ListOfLists { get; set; }
        
        public IEnumerable<object> ListOfDownCastObjects { get; set; }
    }

    public class Nested
    {
        public Nested(int i)
        {
            AInt = i;
        }

        public int AInt { get; set; }
    }

}