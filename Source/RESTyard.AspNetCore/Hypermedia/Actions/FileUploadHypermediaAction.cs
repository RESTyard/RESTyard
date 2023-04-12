using System;

namespace RESTyard.AspNetCore.Hypermedia.Actions;

/// <summary>
/// A HypermediaAction which represents a file upload.
/// For each concrete type a corresponding attributed route must exist.
/// </summary>
public abstract class FileUploadHypermediaAction : HypermediaActionBase
{
    public FileUploadConfiguration FileUploadConfiguration { get; set; }
    
    protected FileUploadHypermediaAction(Func<bool> canExecute, FileUploadConfiguration fileUploadConfiguration = null) : base(canExecute)
    {
        FileUploadConfiguration = fileUploadConfiguration ?? new FileUploadConfiguration();
    }

    public override object GetPrefilledParameter()
    {
        return null;
    }

    public override Type ParameterType()
    {
        return typeof(FileUploadConfiguration);
    }
}