using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;

namespace RESTyard.AspNetCore.Test;

public class AssemblyBasedTestBase
{
    protected const string TestAssemblyNamespace = "AttributedRoutesRegisterTest";

    protected static Assembly CreateAssembly(IReadOnlyCollection<string> files)
    {
        var assemblyDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var references = new[]
        {
            typeof(object),
            typeof(Enumerable),
            typeof(IHypermediaObject),
            typeof(ControllerBase),
            typeof(IActionResult),
        }
        .Select(t => MetadataReference.CreateFromFile(t.Assembly.Location))
        .Concat([
            MetadataReference.CreateFromFile(Path.Combine(assemblyDirectory, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyDirectory, "System.Collections.dll")),
        ]);
        var compilation = CSharpCompilation.Create(
            assemblyName: "AdHoc",
            syntaxTrees: files.Select(s => CSharpSyntaxTree.ParseText(s)),
            references: references,
            options: new(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));
        var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        result.Success
            .Should()
            .BeTrue(
                because: string.Join(
                    Environment.NewLine,
                    result.Diagnostics
                        .OrderByDescending(d => d.Severity)
                        .Select(d => d.ToString())));
        var assembly = Assembly.Load(ms.ToArray());
        return assembly;
    }

    protected static string CreateFile(string content, bool includeExampleHtoNamespace = true)
    {
        var exampleHtoUsing = includeExampleHtoNamespace ? $"using {typeof(ExampleHto).Namespace};" : "";
        return $"""
                using System;
                using System.Collections.Generic;
                using Microsoft.AspNetCore.Mvc;
                using RESTyard.AspNetCore.Hypermedia;
                using RESTyard.AspNetCore.Hypermedia.Actions;
                using RESTyard.AspNetCore.Hypermedia.Attributes;
                using RESTyard.AspNetCore.Query;
                using RESTyard.AspNetCore.WebApi.AttributedRoutes;
                using RESTyard.AspNetCore.WebApi.RouteResolver;
                {exampleHtoUsing}
                namespace {TestAssemblyNamespace};
                {content}
                """;
    }

    protected static string GetExampleHtoCode() => File.ReadAllText("ExampleHto.cs");

    protected static Type GetType<T>(Assembly assembly)
        => GetTypeByFullName(assembly, typeof(T).FullName!);

    protected static Type GetTypeByName(Assembly assembly, string name)
        => GetTypeByFullName(assembly, $"{TestAssemblyNamespace}.{name}");
    
    private static Type GetTypeByFullName(Assembly assembly, string fullName)
    {
        var result = assembly.GetType(fullName);
        result.Should().NotBeNull(because: fullName);
        return result!;
    }
    
    protected static IHypermediaApiExplorer CreateApiExplorer(Assembly assembly)
    {
        var app = CreateWebApplication(assembly);
        var apiExplorer = app.Services.GetRequiredService<IHypermediaApiExplorer>();
        return apiExplorer;
    }

    protected static WebApplication CreateWebApplication(Assembly assembly)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Host.UseDefaultServiceProvider(o =>
        {
            o.ValidateOnBuild = true;
            o.ValidateScopes = true;
        });
        builder.Services.AddHypermediaExtensions();
        builder.Services.AddControllers().AddApplicationPart(assembly);
        var app = builder.Build();
        return app;
    }
}