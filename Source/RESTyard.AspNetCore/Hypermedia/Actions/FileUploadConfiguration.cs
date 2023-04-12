using System.Collections.Generic;

namespace RESTyard.AspNetCore.Hypermedia.Actions;

public record FileUploadConfiguration
{
    /// <summary>
    /// Max size of a single file in bytes the server will accept
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = -1;
    
    /// <summary>
    /// Max number of files accepted for upload. Default is 1 
    /// </summary>
    public int MaxFileCount { get; set; } = 1;
    
    /// <summary>
    /// List of extensions which the client can use to limit file selection. Must not contain '.'
    /// Default is empty indicating no limit. 
    /// </summary>
    public List<string> AllowedFileExtensions { get; set; } = new();
}