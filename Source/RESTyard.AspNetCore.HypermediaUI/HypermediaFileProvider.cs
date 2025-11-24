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

    private readonly string subpath;
    private readonly HypermediaDirectoryContents files;
    private readonly FileExtensionContentTypeProvider contentTypeProvider = new FileExtensionContentTypeProvider();

    public HypermediaFileProvider(
        DateTimeOffset created,
        string subpath,
        IEnumerable<(string Name, string FullName, byte[] Content)> files,
        HypermediaUiConfig? config)
    {
        this.subpath = subpath;
        var prefix = this.subpath == "" ? "" : $"{this.subpath}/";
        this.files = new HypermediaDirectoryContents(created);
        (string Name, string FullName, byte[] Content) index = ("", "", []);
        foreach (var tuple in files)
        {
            var content = tuple.Content;
            if (tuple.Name == "index.html")
            {
                if (this.subpath != "")
                {
                    var indexHtml = Encoding.UTF8.GetString(content);
                    indexHtml = ChangeBasePath(indexHtml, this.subpath);
                    content = Encoding.UTF8.GetBytes(indexHtml);
                }
                index = (tuple.Name, tuple.FullName, content);
            }
            
            if (tuple.Name == "app.config.json" && config is not null)
            {
                var appConfigSerialized = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(config, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                this.files.Add(new HypermediaFileInfo(tuple.Name, $"{prefix}{tuple.FullName}", appConfigSerialized, created));
            }
            else
            {
                this.files.Add(new HypermediaFileInfo(tuple.Name, $"{prefix}{tuple.FullName}", content, created));
            }
        }

        if (index.Name != "")
        {
            List<string> builtinRedirects = ["", "hui", "auth-redirect"];
            foreach (var redirect in builtinRedirects.Concat(config?.ConfiguredEntryPoints.Select(e => e.Alias) ?? []))
            {
                if (redirect == "")
                {
                    this.files.Add(new HypermediaFileInfo(index.Name, $"{this.subpath}", index.Content, created));
                }

                this.files.Add(new HypermediaFileInfo(index.Name, $"{this.subpath}/{redirect}", index.Content, created));
            }
        }
    }

    private string ChangeBasePath(string indexHtml, string basePath)
    {
        return indexHtml
            .Replace("base href=\"/\"", "base href=\"\"")
            .Replace("href=\"", $"href=\"/{basePath}/")
            .Replace("src=\"", $"src=\"/{basePath}/");
    }

    private IEnumerable<HypermediaFileInfo> Files => this.files;
    
    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        return NotFoundDirectoryContents.Singleton;
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var comparePath = subpath == "/" ? subpath : subpath.TrimEnd('/');
        IFileInfo? match = this.Files.FirstOrDefault(f => f.RequestPath == comparePath);
        return match ?? new NotFoundFileInfo(subpath);
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