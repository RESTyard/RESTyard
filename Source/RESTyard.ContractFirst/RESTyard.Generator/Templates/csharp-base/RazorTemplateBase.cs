using FunicularSwitch;
using Microsoft.AspNetCore.Components;

namespace RESTyard.Generator.Templates.csharp_base;

public class RazorTemplateBase : ComponentBase
{
    [Parameter] public HypermediaType Schema { get; set; } = null!;

    public static void Warning(string message) => Console.WriteLine($"[WARNING] {message}");

    public string MapNullableType(bool nullable, string type)
    {
        return nullable ? $"{type}?" : type;
    }

    public string MapOption(bool hasSome, string type)
    {
        return hasSome ? $"Option<{type}>" : type;
    }

    public string Capitalize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return $"{text[..1].ToUpper()}{text[1..]}";
    }

    public string Uncapitalize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return $"{text[..1].ToLower()}{text[1..]}";
    }

    public void CheckProperties(string typeName, IEnumerable<PropertyType> properties)
    {
        foreach (var prop in properties.Where(p => p is { mandatory: true, hidden: true }))
        {
            Warning($"Mandatory property should not be hidden. ({typeName}.{prop.name})");
        }
    }

    public List<PropertyType> GetKeyProperties(DocumentType document)
    {
        return EnumerateParents(document, includeSelf: true)
            .SelectMany(GetOwnKeyProperties)
            .ToList();
    }

    protected static IEnumerable<PropertyType> GetOwnKeyProperties(DocumentType d)
    {
        return d.Properties.Where(p => p.isKey);
    }

    public string TransformMandatoryArgument(string argument)
    {
        return argument.Split(' ').Last();
    }

    public string TransformOptionalArgument(string argument)
    {
        return argument.Split(' ').Skip(1).First();
    }

    public List<string> GatherArguments(DocumentType document)
    {
        var result = new List<string>();
        foreach (var property in document.Properties)
        {
            result.Add($"{MapNullableType(!property.mandatory, property.type)} {Uncapitalize(property.name)}");
        }

        foreach (var operation in document.Operations)
        {
            result.Add($"{operation.name}Op {Uncapitalize(operation.name)}");
        }

        foreach (var entity in document.Entities)
        {
            var arg = $"IEnumerable<HypermediaObjectReferenceBase> {Uncapitalize(entity.collectionName)}";
            if (!string.IsNullOrEmpty(entity.document))
            {
                arg = $"IEnumerable<{entity.document}Hto> {Uncapitalize(entity.collectionName)}";
            }
            result.Add(arg);
        }

        foreach (var link in document.Links)
        {
            if (!string.IsNullOrEmpty(link.document))
            {
                var hasQuery = !string.IsNullOrEmpty(link.query);
                var linkDocument = Schema.Documents.First(d => d.name == link.document);
                var hasKey = HasKeyProperties(linkDocument);

                if (hasQuery && hasKey)
                {
                    var tupleType = MapOption(!link.mandatory, $"({link.query} Query, {linkDocument.name}Hto.Key Key)");
                    result.Add($"{tupleType} {Uncapitalize(link.rel)}Reference");
                }
                else if (hasQuery)
                {
                    var queryType = MapOption(!link.mandatory, link.query);
                    result.Add($"{queryType} {Uncapitalize(link.rel)}Query");
                }
                else if (hasKey)
                {
                    var keyType = MapOption(!link.mandatory, $"{linkDocument.name}Hto.Key");
                    result.Add($"{keyType} {Uncapitalize(link.rel)}Key");
                }
            }
            else
            {
                var referenceType = MapOption(!link.mandatory, "HypermediaObjectReferenceBase");
                result.Add($"{referenceType} {Uncapitalize(link.rel)}");
            }
        }

        if (document.isQueryResult)
        {
            result.Add($"IHypermediaQuery query");
        }

        return result;
    }

    public List<string> GatherParentArguments(DocumentType document)
    {
        return EnumerateParents(document)
            .Reverse()
            .SelectMany(GatherArguments)
            .ToList();
    }

    public bool HasKeyProperties(DocumentType document)
    {
        return EnumerateParents(document, includeSelf: true)
            .Any(HasOwnKeyProperties);
    }

    public static bool HasOwnKeyProperties(DocumentType document)
    {
        return document.Properties.Any(p => p.isKey);
    }

    public List<string> GatherMandatoryParameterArguments(ParameterType parameterType, bool showHidden)
    {
        CheckProperties(parameterType.typeName, parameterType.Property);
        var result = new List<string>();
        foreach (var property in parameterType.Property.Where(p => p.mandatory && (!p.hidden || showHidden)))
        {
            result.Add($"{property.type} {property.name}");
        }

        return result;
    }

    public List<string> GatherOptionalParameterArguments(ParameterType parameterType, bool showHidden)
    {
        var result = new List<string>();
        foreach (var property in parameterType.Property.Where(p => !p.mandatory && (!p.hidden || showHidden)))
        {
            result.Add($"{MapNullableType(true, property.type)} {property.name} = default");
        }

        return result;
    }

    public List<string> GatherMandatoryParameterParentArguments(ParameterType parameterType, bool showHidden)
    {
        return EnumerateParents(parameterType)
            .Reverse()
            .SelectMany(parent => GatherMandatoryParameterArguments(parent, showHidden: showHidden))
            .ToList();
    }

    public List<string> GatherOptionalParameterParentArguments(ParameterType parameterType, bool showHidden)
    {
        return EnumerateParents(parameterType)
            .Reverse()
            .SelectMany(parent => GatherOptionalParameterArguments(parent, showHidden: showHidden))
            .ToList();
    }

    public IEnumerable<DocumentType> EnumerateParents(DocumentType document, bool includeSelf = false)
    {
        var currentDocument = document;
        if (includeSelf)
        {
            yield return currentDocument;
        }
        while (!string.IsNullOrEmpty(currentDocument.parentDocument))
        {
            currentDocument = this.Schema.Documents.First(d => d.name == currentDocument.parentDocument);
            yield return currentDocument;
        }
    }

    public IEnumerable<ParameterType> EnumerateParents(ParameterType parameterType)
    {
        var currentParameters = parameterType;
        while (!string.IsNullOrEmpty(currentParameters.parentType))
        {
            currentParameters = this.Schema.TransferParameters.Parameters.First(p => p.typeName == currentParameters.parentType);
            yield return currentParameters;
        }
    }
}