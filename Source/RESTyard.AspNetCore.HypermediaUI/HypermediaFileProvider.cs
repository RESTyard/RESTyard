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

        public HypermediaFileInfo(string filename, string fullPath, byte[] content, DateTimeOffset lastModified)
        {
            this.Name = filename;
            this.RequestPath = $"/{fullPath}";
            this.content = content;
            this.Length = this.content.Length;
            this.LastModified = lastModified;
        }
        
        public Stream CreateReadStream()
        {
            var result = new MemoryStream(this.content);
            return result;
        }

        public bool Exists => true;
        public bool IsDirectory => false;
        public DateTimeOffset LastModified { get; }
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
        HypermediaConfig? config)
    {
        this.files = new HypermediaDirectoryContents(created);
        (string Name, string FullName, byte[] Content) index = ("", "", []);
        foreach (var tuple in files)
        {
            if (tuple.Name == "index.html")
            {
                index = tuple;
            }
            
            if (tuple.Name == "app.config.json" && config is not null)
            {
                var appConfigSerialized = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(config, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                this.files.Add(new HypermediaFileInfo(tuple.Name, tuple.FullName, appConfigSerialized, created));
            }
            else
            {
                this.files.Add(new HypermediaFileInfo(tuple.Name, tuple.FullName, tuple.Content, created));
            }
        }

        if (index.Name != "")
        {
            List<string> builtinRedirects = ["", "hui", "auth-redirect"];
            foreach (var redirect in builtinRedirects.Concat(config?.ConfiguredEntryPoints.Select(e => e.Alias) ?? []))
            {
                this.files.Add(new HypermediaFileInfo(index.Name, redirect, index.Content, created));
            }
        }
    }

    private IEnumerable<HypermediaFileInfo> Files => this.files;
    
    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        return NotFoundDirectoryContents.Singleton;
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var match = this.Files.FirstOrDefault(f => f.RequestPath == subpath);
        if (match is not null)
        {
            return match;
        }
        else
        {
            return new NotFoundFileInfo(subpath);
        }
    }

    public IChangeToken Watch(string filter)
    {
        return NullChangeToken.Singleton;
    }

    public bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType)
    {
        var fileInfo = this.GetFileInfo(subpath);
        if (fileInfo is HypermediaFileInfo hfi)
        {
            return this.contentTypeProvider.TryGetContentType(fileInfo.Name, out contentType);
        }

        contentType = null;
        return false;
    }
}