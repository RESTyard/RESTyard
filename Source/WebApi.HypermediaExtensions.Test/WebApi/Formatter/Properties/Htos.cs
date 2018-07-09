using System;
using System.Collections.Generic;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Test.Helpers;

namespace WebApi.HypermediaExtensions.Test.WebApi.Formatter.Properties
{
    public class EmptyHypermediaObject : HypermediaObject
    {
    }

    [HypermediaObject(Title = "A Title", Classes = new[] { "CustomClass1", "CustomClass2" })]
    public class AttributedEmptyHypermediaObject : HypermediaObject
    {
    }

    public class PropertyDuplicateHypermediaObject : HypermediaObject
    {
        [HypermediaProperty(Name = "DuplicateRename")]
        public bool Property1 { get; set; }

        [HypermediaProperty(Name = "DuplicateRename")]
        public bool Property2 { get; set; }
    }

    public class PropertyNestedClassHypermediaObject : HypermediaObject
    {
        public ChildClass AChild { get; set; }
    }

    public class AttributedPropertyHypermediaObject : HypermediaObject
    {
        [HypermediaProperty(Name = "Property1Renamed")]
        public bool Property1 { get; set; }

        [HypermediaProperty(Name = "Property2Renamed")]
        public bool Property2 { get; set; }

        [FormatterIgnoreHypermediaProperty]
        public bool IgnoredProperty { get; set; }

        public bool NotRenamed { get; set; }
    }

    public class PropertyHypermediaObject : HypermediaObject
    {
        public bool ABool { get; set; }
        public string AString { get; set; }
        public int AInt { get; set; }
        public long ALong { get; set; }
        public float AFloat { get; set; }
        public double ADouble { get; set; }

        public TestEnum AEnum { get; set; }
        public TestEnumWithNames AEnumWithNames { get; set; }

        public DateTime ADateTime { get; set; }
        public DateTimeOffset ADateTimeOffset { get; set; }
        public TimeSpan ATimeSpan { get; set; }
        public decimal ADecimal { get; set; }
        public int? ANullableInt { get; set; }
    }

    public class HypermediaObjectWithListProperties : HypermediaObject
    {
        public IEnumerable<int> AValueList { get; set; }

        public IEnumerable<int?> ANullableList { get; set; }

        public IEnumerable<string> AReferenceList { get; set; }

        public int[] AValueArray { get; set; } // arrays need special treatment
    }

    public class ChildClass
    {
        public bool ABool { get; set; }
    }
}