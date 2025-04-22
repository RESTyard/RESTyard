using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace RESTyard.HtoSourceGenerators.Siren;

/// <summary>
///  Container for https://github.com/kevinswiber/siren.
/// </summary>
/// <typeparam name="TProperties"></typeparam>

public class Entity<TProperties>
{   
    public List<string>? @class { get; set; }
    public string? title { get; set; }
    public TProperties? properties { get; set; }
    public List<ISubEntity>? entities { get; set; }
    public List<Link>? links { get; set; }
    public List<Action>? actions { set; get; }
}

public interface ISubEntity
{
    public List<string>? @class { get; set; }
    public string? title { get; set; }
    
    public List<string> rel { get; set; }
}

public class EmbeddedEntity<TProperties> : Entity<TProperties>, ISubEntity
{
    [Required]
    public List<string> rel { get; set; } = [];
}

public class EmbeddedLinkEntity : ISubEntity
{
    public List<string>? @class { get; set; }
    public string? title { get; set; }
    
    [Required]
    public List<string> rel { get; set; } = [];
    
    public string? @type { get; set; }
    [Required] public string href { get; set; } = string.Empty;
}


public class Link
{
    [Required]
    public List<string> rel { get; set; } = [];

    [Required]
    public string href { get; set; } = string.Empty;

    public List<string>? @class { get; set; }

    public string? title { get; set; }

    public string? type { get; set; }
}

public class Action
{
    [Required]
    public string name { get; set; } = string.Empty;

    [Required]
    public string href { get; set; } = string.Empty;

    public List<string>? @class { get; set; }

    public string? method { get; set; }

    public string? title { get; set; }

    public string? type { get; set; }

    public List<Field>? fields { get; set; }
}

public class Field
{
    [Required]
    public string name { get; set; } = string.Empty;

    public string? type { get; set; }

    public object? value { get; set; }

    public List<string>? @class { get; set; }

    public string? title { get; set; }
    
    /// <summary>
    /// Accepted media types for file upload
    /// </summary>
    public List<string>? accept { get; set; }
    
    /// <summary>
    /// Max filesize for file upload
    /// </summary>
    public long maxFileSizeBytes { get; set; }
    
    /// <summary>
    /// Does file upload allow multiple files 
    /// </summary>
    public bool allowMultiple { get; set; }
}

public class EmptyProperties { }
