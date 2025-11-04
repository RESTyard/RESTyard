using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace RESTyard.AspNetCore.HypermediaUI;

public class HypermediaFileProvider : IFileProvider, IContentTypeProvider
{
    private class HypermediaDirectoryContents : List<HypermediaFileInfo>, IDirectoryContents, IFileInfo
    {
        public HypermediaDirectoryContents(DateTimeOffset created)
        {
            this.LastModified = created;
        }
        
        public Stream CreateReadStream()
        {
            throw new NotImplementedException();
        }

        public bool Exists => true;
        public bool IsDirectory => true;
        public DateTimeOffset LastModified { get; }
        public long Length => 0;
        public string Name => "";
        public string? PhysicalPath => "";

        IEnumerator<IFileInfo> IEnumerable<IFileInfo>.GetEnumerator() => this.GetEnumerator();
    }

    private class HypermediaFileInfo : IFileInfo
    {
        private readonly byte[] content;

        public HypermediaFileInfo(string filename, string fullPath, byte[] content)
        {
            this.Name = filename;
            this.RequestPath = $"/{fullPath}";
            this.content = content;
            this.Length = this.content.Length;
        }
        
        public Stream CreateReadStream()
        {
            var result = new MemoryStream(this.content);
            return result;
        }

        public bool Exists => true;
        public bool IsDirectory => false;
        public DateTimeOffset LastModified { get; init; }
        public long Length { get; }
        public string Name { get; }
        public string? PhysicalPath { get; } = null;
        public string RequestPath { get; }
    }
    
    private readonly HypermediaDirectoryContents files;
    private readonly FileExtensionContentTypeProvider contentTypeProvider = new FileExtensionContentTypeProvider();

    public HypermediaFileProvider(
        DateTimeOffset created,
        IEnumerable<(string Name, string FullName, byte[] Content)> files,
        HypermediaConfig config)
    {
        this.files = new HypermediaDirectoryContents(created);
        foreach (var tuple in files)
        {
            if (tuple.Name == "app.config.json")
            {
                this.files.Add(new HypermediaFileInfo(tuple.Name, tuple.FullName, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(config)))
                {
                    LastModified = created,
                });
            }
            else
            {
                this.files.Add(new HypermediaFileInfo(tuple.Name, tuple.FullName, tuple.Content)
                {
                    LastModified = created,
                });
            }
        }
    }
    
    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        if (subpath == "")
        {
            return this.files;
        }
        else
        {
            return NotFoundDirectoryContents.Singleton;
        }
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var match = ((IEnumerable<HypermediaFileInfo>)this.files).FirstOrDefault(f => f.RequestPath == subpath);
        if (match is not null)
        {
            return match;
        }
        else
        {
            return ((IEnumerable<HypermediaFileInfo>)this.files).First(f => f.Name == "index.html");
        }
    }

    public IChangeToken Watch(string filter)
    {
        return NullChangeToken.Singleton;
    }

    public bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType)
    {
        if (!this.contentTypeProvider.TryGetContentType(subpath, out contentType))
        {
            contentType = "text/html";
        }
        return true;
    }
}