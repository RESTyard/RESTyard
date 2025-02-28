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
        return document.Properties.Where(p => p.isKey).ToList();
    }

    public ParameterType FindParentParameters(string name)
    {
        return Schema.TransferParameters.Parameters.First(p => p.typeName == name);
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
        var result = new List<string>();
        var currentDocument = document;
        while (!string.IsNullOrEmpty(currentDocument.parentDocument))
        {
            currentDocument = Schema.Documents.First(d => d.name == currentDocument.parentDocument);
            result.InsertRange(0, GatherArguments(currentDocument));
        }

        return result;
    }

    public bool HasKeyProperties(DocumentType document)
    {
        var hasOwnKeyProperties = document.Properties?.Any(p => p.isKey) ?? false;
        return hasOwnKeyProperties || (string.IsNullOrEmpty(document.parentDocument) ? false : HasKeyProperties(Schema.Documents.First(d => d.name == document.parentDocument)));
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
        var result = new List<string>();
        var currentParameters = parameterType;
        while (!string.IsNullOrEmpty(currentParameters.parentType))
        {
            currentParameters = FindParentParameters(currentParameters.parentType);
            result.InsertRange(0, GatherMandatoryParameterArguments(currentParameters, showHidden: showHidden));
        }

        return result;
    }

    public List<string> GatherOptionalParameterParentArguments(ParameterType parameterType, bool showHidden)
    {
        var result = new List<string>();
        var currentParameters = parameterType;
        while (!string.IsNullOrEmpty(currentParameters.parentType))
        {
            currentParameters = FindParentParameters(currentParameters.parentType);
            result.InsertRange(0, GatherOptionalParameterArguments(currentParameters, showHidden: showHidden));
        }

        return result;
    }
}