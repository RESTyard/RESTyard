using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using RESTyard.Client.Authentication;
using RESTyard.Client.Extensions;
using RESTyard.Client.Extensions.SystemNetHttp;
using RESTyard.Client.Extensions.SystemTextJson;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Client.Reader;
using RESTyard.Client.Resolver;
using RESTyard.Integration.Test.Fixtures;
using RESTyard.Integration.Test.Hco;
using Xunit.Abstractions;

namespace RESTyard.Integration.Test;

/// <summary>
/// Verifies that custom <see cref="JsonConverter"/>s are applied when deserializing hypermedia action
/// parameters, both on the controller <c>[FromBody]</c> path and on the file-upload form binder path
/// (both use the MVC JsonOptions). Each converter rejects the value it handles, so a request that
/// reaches deserialization on that path is rejected — proving the converter participated.
/// </summary>
public class ParameterConverterTests : IAsyncLifetime
{
    private static readonly Uri ApiEntryPoint = new Uri($"{CarShackWaf.BaseUrl}/EntryPoint");

    private readonly CarShackWaf carShackFactory;
    private readonly WebApplicationFactory<CarShack.Program> factoryWithConverters;
    private readonly IHttpHypermediaResolverFactory apiResolverFactory;

    public ParameterConverterTests(ITestOutputHelper outputHelper)
    {
        this.carShackFactory = new(outputHelper);

        // Derive a factory that registers the custom converters on top of the base CarShack host configuration.
        // Both paths read the MVC JsonOptions (AddJsonOptions).
        this.factoryWithConverters = this.carShackFactory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new RejectingDateOnlyConverter());
                    options.JsonSerializerOptions.Converters.Add(new RejectingStringConverter());
                })));

        this.apiResolverFactory = DefaultHypermediaClientBuilder
            .CreateBuilder()
            .WithSirenHypermediaReader()
            .WithSystemTextJsonStringParser()
            .WithSystemTextJsonProblemReader()
            .WithSystemTextJsonObjectParameterSerializer()
            .CreateHttpHypermediaResolverFactory();
    }

    protected HttpClient Client { get; private set; } = null!;
    protected IHypermediaResolver Resolver { get; private set; } = null!;

    public Task InitializeAsync()
    {
        this.Client = this.factoryWithConverters.CreateClient();
        this.Client.DefaultRequestHeaders.Authorization =
            new UsernamePasswordCredentials("User", "Password")
                .CreateBasicAuthHeaderValue();
        this.Client.DefaultRequestHeaders.AcceptLanguage
            .Add(new StringWithQualityHeaderValue("en", 1.0));
        this.Resolver = this.apiResolverFactory.Create(this.Client);
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CustomConverter_IsApplied_OnController_FromBody_Path()
    {
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        var carsRoot = (await apiRoot.NavigateAsync(e => e.CarsRoot)).Should().BeOk().Which;
        var anyCar = (await carsRoot.NiceCar.ResolveAsync()).Should().BeOk().Which;

        // UpdateCarInspection has a DateOnly property; the rejecting DateOnly converter (registered on the
        // MVC JsonOptions) must run during body deserialization.
        var result = await anyCar.UpdateInspection!
            .ExecuteAsync(new UpdateCarInspection(new DateOnly(2026, 08, 31)), this.Resolver);

        result.Match(_ => false, _ => true)
            .Should().BeTrue("the DateOnly converter from the MVC JsonOptions should reject the body on the controller [FromBody] path");
    }

    [Fact]
    public async Task CustomConverter_IsApplied_OnFileUpload_FormBinder_Path()
    {
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        var cars = (await apiRoot.NavigateAsync(l => l.CarsRoot)).Should().BeOk().Which;

        // UploadCarImageParameters has a string property; the rejecting string converter must run when the
        // form binder deserializes the parameter object using the MVC JsonOptions from request services.
        var result = await cars.UploadCarImage!.ExecuteAsync(
            new HypermediaFileUploadActionParameter<UploadCarImageParameters>(
                FileDefinitions: new List<FileDefinition>
                {
                    new(_ => Task.FromResult<Stream>(new MemoryStream([1, 2, 3, 4])), "Bytes", "Bytes.txt"),
                },
                new("Text", true)),
            this.Resolver);

        result.Match(_ => false, _ => true)
            .Should().BeTrue("the string converter from the MVC JsonOptions should reject the parameter object on the form binder path");
    }

    private sealed class RejectingDateOnlyConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new JsonException("Rejected by RejectingDateOnlyConverter");

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }

    private sealed class RejectingStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new JsonException("Rejected by RejectingStringConverter");

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            => writer.WriteStringValue(value);
    }
}
