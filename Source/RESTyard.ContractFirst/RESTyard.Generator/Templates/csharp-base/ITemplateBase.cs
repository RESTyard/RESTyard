namespace RESTyard.Generator.Templates.csharp_base;

public interface ITemplateBase
{
    HypermediaType Schema { get; set; }
    string? Namespace { get; set; }
    string Includes { get; set; }
}