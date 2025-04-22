using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using RESTyard.HtoSourceGenerators.Attributes;

namespace RESTyard.HtoSourceGenerators.Test.Htos;

/// <summary>
/// This is the comment for this class
/// </summary>
[HypermediaObject(Title = "Only contains properties")]
[CarryOverAttribute(Content = "OnClass")]
public class PropertyHto
{
    /// <summary>
    /// A comment on a property
    /// </summary>
    public int MyInteger { get; set; }

    public int MyIntegerDefault { get; set; } = 5; // todo carry over default so open api can see it too
    
    [HypermediaProperty(Name = "MyRenamed" )]
    public int ToRename { get; set; }

    public string MyString { get; set; } = "my string";

    [IgnoreHypermediaProperty] public string Ignored { get; set; } = "ignored";

    [CarryOver(Content = "my content")] public List<string> ListOfStrings { get; set; } = [];

    public MyRecord MyRecord { get; set; } = new MyRecord(42);

    public MyClass MyClass { get; set; } = new MyClass(43);
    
    public JsonElement GenericJson { get; set; }
}

public record MyRecord(int MyRecordInt);

public class MyClass(int MyClassInt);

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public sealed class CarryOverAttribute : Attribute
{
    public string? Content { get; set; }
}