using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.HtoSourceGenerators.TestHtos;

public enum Color
{
    [EnumMember(Value = "red")]
    Red,
    [EnumMember(Value = "green")]
    Green,
    [EnumMember(Value = "blue")]
    Blue
}

public class NestedAddress
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

/// <summary>
/// Covers SirenConverter property serialization paths (ValueToJToken):
/// - string, int, bool, double, decimal (value types)
/// - nullable int (nullable value type)
/// - enum (with EnumMember), nullable enum
/// - DateOnly, TimeOnly, DateTime, DateTimeOffset
/// - Uri (container type for string)
/// - nested object (class → recursive serialization)
/// - IEnumerable (list of strings)
/// - FormatterIgnoreHypermediaProperty (excluded from output)
/// </summary>
[HypermediaObject(Title = "All Property Types", Classes = ["AllTypes"])]
public class HtoWithVariousProperties : HypermediaObject
{
    public string Text { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool IsActive { get; set; }
    public double Ratio { get; set; }
    public decimal Price { get; set; }
    public int? NullableInt { get; set; }
    public Color FavoriteColor { get; set; }
    public Color? NullableColor { get; set; }
    public DateOnly BirthDate { get; set; }
    public TimeOnly AlarmTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public Uri? Website { get; set; }
    public NestedAddress? Address { get; set; }
    public List<string> Tags { get; set; } = [];

    [FormatterIgnoreHypermediaProperty]
    public string InternalNote { get; set; } = string.Empty;

    [Relations(["self"])]
    public ILink<HtoWithVariousProperties> Self { get; set; }

    public HtoWithVariousProperties()
    {
        Self = Link.To(this);
    }
}
