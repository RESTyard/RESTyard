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
    /// <summary>
    /// Host the Angular Application HypermediaUI in the given subpath (or root) by serving the contents from memory, and redirecting relevant alias routes to index.html.
    /// </summary>
    /// <param name="builder">The app builder</param>
    /// <param name="subpath">The path under which to serve the HypermediaUI. Can contain slashes, leading or trailing slashes are normalized and can be omitted.</param>
    /// <param name="config">The HypermediaUiConfig to serve with the HypermediaUI. Can also be configured as an IOption with the IServiceProvider, which this method will attempt to read if it is not passed explicitly.</param>
    /// <typeparam name="TAppBuilder"></typeparam>
    /// <returns>The app builder</returns>
    public static IApplicationBuilder UseHypermediaUI(
        this IApplicationBuilder builder,
        string subpath,
        HypermediaUiConfig? config = null)
    {
        var files = ExtractAngularFilesFromArchive();

        config ??= builder.ApplicationServices.GetService<IOptions<HypermediaUiConfig>>()?.Value;
        var timeProvider = builder.ApplicationServices.GetService<TimeProvider>() ?? TimeProvider.System;
        
        var hypermediaFileProvider = new HypermediaFileProvider(
            timeProvider.GetLocalNow(),
            subpath.Trim('/'),
            files,
            config);
        
        return builder.UseStaticFiles(
            new StaticFileOptions()
            {
                FileProvider = hypermediaFileProvider,
                ContentTypeProvider = hypermediaFileProvider,
            });
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