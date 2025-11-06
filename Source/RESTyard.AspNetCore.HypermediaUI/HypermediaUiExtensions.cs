using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Builder;

namespace RESTyard.AspNetCore.HypermediaUI;

public static class HypermediaUiExtensions
{
    public static TAppBuilder UseHypermediaUI<TAppBuilder>(this TAppBuilder builder, HypermediaConfig? config = null)
        where TAppBuilder : IApplicationBuilder
    {
        var resourceStream = typeof(HypermediaUiExtensions).Assembly.GetManifestResourceStream("RESTyard.AspNetCore.HypermediaUI.Content.release.zip");
        var zipArchive = new ZipArchive(resourceStream!);
        List<(string Name, string FullName, byte[] Content)> files = [];
        const string prefix = "dist/prod/browser/";
        foreach (var entry in zipArchive.Entries.Where(e => e.Name != "" && e.FullName.StartsWith(prefix)))
        {
            using var ms = new MemoryStream();
            using var stream = entry.Open();
            stream.CopyTo(ms);
            files.Add((entry.Name, entry.FullName.Substring(prefix.Length), ms.ToArray()));
        }
        var hypermediaFileProvider = new HypermediaFileProvider(
            DateTimeOffset.Now,
            files,
            config);
        builder.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = hypermediaFileProvider,
            ContentTypeProvider = hypermediaFileProvider,
        });
        return builder;
    }
}