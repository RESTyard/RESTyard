using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RESTyard.AspNetCore.HypermediaUI;

public static class HypermediaUiExtensions
{
    public static TAppBuilder UseHypermediaUI<TAppBuilder>(
        this TAppBuilder builder,
        string subpath,
        HypermediaUiConfig? config = null)
        where TAppBuilder : IApplicationBuilder
    {
        var files = ExtractAngularFilesFromArchive();

        config ??= builder.ApplicationServices.GetService<IOptions<HypermediaUiConfig>>()?.Value;
        var timeProvider = builder.ApplicationServices.GetService<TimeProvider>() ?? TimeProvider.System;
        
        var hypermediaFileProvider = new HypermediaFileProvider(
            timeProvider.GetLocalNow(),
            subpath.Trim('/'),
            files,
            config);
        
        builder.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = hypermediaFileProvider,
            ContentTypeProvider = hypermediaFileProvider,
        });
        
        return builder;
    }

    private static IReadOnlyList<(string Name, string FullName, byte[] Content)> ExtractAngularFilesFromArchive()
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
            files.Add(
                (
                    Name: entry.Name,
                    FullName: entry.FullName.Substring(prefix.Length),
                    Content: ms.ToArray()
                ));
        }

        return files;
    }
}