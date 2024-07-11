using System;

namespace RESTyard.AspNetCore.Hypermedia.Actions;

/// <summary>
/// A HypermediaAction which represents a file upload.
/// For each concrete type a corresponding attributed route must exist.
/// </summary>
public abstract class FileUploadHypermediaAction : HypermediaActionBase, IFileUploadConfiguration
{
    public FileUploadConfiguration FileUploadConfiguration { get; set; }
    
    protected FileUploadHypermediaAction(Func<bool> canExecute, FileUploadConfiguration? fileUploadConfiguration = null) : base(canExecute)
    {
        FileUploadConfiguration = fileUploadConfiguration ?? new FileUploadConfiguration();
    }
    
    protected FileUploadHypermediaAction(FileUploadConfiguration? fileUploadConfiguration = null) : base(() => true)
    {
        FileUploadConfiguration = fileUploadConfiguration ?? new FileUploadConfiguration();
    }

    public override object? GetPrefilledParameter()
    {
        return null;
    }

    protected override Type? ParameterType => null;
}

/// <summary>
/// A HypermediaAction which represents a file upload.
/// For each concrete type a corresponding attributed route must exist.
/// </summary>
public abstract class FileUploadHypermediaAction<TParameter> : HypermediaActionBase, IFileUploadConfiguration
    where TParameter : class
{
    public FileUploadConfiguration FileUploadConfiguration { get; set; }
    public TParameter? PrefilledValues { get; protected set; }

    protected FileUploadHypermediaAction(Func<bool> canExecute, FileUploadConfiguration? fileUploadConfiguration = null, TParameter? prefilledValues = null) : base(canExecute)
    {
        this.FileUploadConfiguration = fileUploadConfiguration ?? new FileUploadConfiguration();
        this.PrefilledValues = prefilledValues;
    }

    protected FileUploadHypermediaAction(FileUploadConfiguration? fileUploadConfiguration = null, TParameter? prefilledValues = null) : this(() => true, fileUploadConfiguration, prefilledValues)
    {
    }

    public override object? GetPrefilledParameter()
    {
        return this.PrefilledValues;
    }

    protected override Type? ParameterType => typeof(TParameter);
}