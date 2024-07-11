using System;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.Hypermedia.Actions;

/// <summary>
/// A HypermediaAction which represents a file upload.
/// For each concrete type a corresponding attributed route must exist.
/// </summary>
public abstract class ExternalFileUploadHypermediaAction : HypermediaExternalActionBase, IFileUploadConfiguration
{
    public FileUploadConfiguration FileUploadConfiguration { get; set; }
    
    protected ExternalFileUploadHypermediaAction(
        Func<bool> canExecute,
        Uri externalUri,
        HttpMethod httpMethod,
        string acceptedMediaType = DefaultMediaTypes.ApplicationJson,
        FileUploadConfiguration? fileUploadConfiguration = null) : base(canExecute, externalUri, httpMethod, acceptedMediaType)
    {
        FileUploadConfiguration = fileUploadConfiguration ?? new FileUploadConfiguration();
    }
    
    protected ExternalFileUploadHypermediaAction(
        Uri externalUri,
        HttpMethod httpMethod,
        string acceptedMediaType = DefaultMediaTypes.ApplicationJson,
        FileUploadConfiguration? fileUploadConfiguration = null) : base(() => true, externalUri, httpMethod, acceptedMediaType)
    {
        FileUploadConfiguration = fileUploadConfiguration ?? new FileUploadConfiguration();
    }

    public override object? GetPrefilledParameter()
    {
        return null;
    }

    protected override Type? ParameterType => null;
}