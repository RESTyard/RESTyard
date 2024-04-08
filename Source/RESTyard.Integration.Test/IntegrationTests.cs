using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using RESTyard.Client.Authentication;
using RESTyard.Client.Extensions;
using RESTyard.Client.Extensions.SystemNetHttp;
using RESTyard.Client.Extensions.SystemTextJson;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Client.Reader;
using RESTyard.Client.Resolver;
using RESTyard.Integration.Test.Fixtures;
using RESTyard.Integration.Test.Hco;

namespace RESTyard.Integration.Test;

public class IntegrationTests : IClassFixture<CarShackWaf>, IAsyncLifetime
{
    private static readonly Uri ApiEntryPoint = new Uri($"{CarShackWaf.BaseUrl}/EntryPoint");
    
    private readonly CarShackWaf carShackFactory;
    private readonly IHttpHypermediaResolverFactory apiResolverFactory;

    public IntegrationTests(CarShackWaf carShackFactory)
    {
        this.carShackFactory = carShackFactory;

        this.apiResolverFactory = DefaultHypermediaClientBuilder
            .CreateBuilder()
            .WithSirenHypermediaReader()
            .WithSystemTextJsonStringParser()
            .WithSystemTextJsonProblemReader()
            .WithSingleSystemTextJsonObjectParameterSerializer()
            .CreateHttpHypermediaResolverFactory();
    }

    protected HttpClient Client { get; private set; }
    protected IHypermediaResolver Resolver { get; private set; }
    protected HttpClient CreateClient() => this.carShackFactory.CreateClient();

    public Task InitializeAsync()
    {
        this.Client = this.CreateClient();
        this.Client.DefaultRequestHeaders.Authorization =
            new UsernamePasswordCredentials("User", "Password")
                .CreateBasicAuthHeaderValue();
        this.Client.DefaultRequestHeaders.AcceptLanguage
            .Add(new StringWithQualityHeaderValue("en", 1.0));
        this.Resolver = this.apiResolverFactory.Create(this.Client);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task EntryPointTest()
    {
        // ACT
        var healthResponse = await this.Client.GetAsync(ApiEntryPoint);
        
        // ASSERT
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EnterEntryPoint()
    {
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        apiRoot.Should().BeOk()
            .Which.Should().NotBeNull();
    }

    [Fact]
    public async Task CallAction_CustomerMove()
    {
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        var customersAll = await apiRoot.NavigateAsync(l => l.CustomersRoot).NavigateAsync(l => l.All);

        var customer = customersAll.Should().BeOk().Which.Customers.First();

        var newAddress = "New Address";
        var actionResult = await customer.CustomerMove.ExecuteAsync(new NewAddress(Address: newAddress), this.Resolver);

        var refreshResult = await customer.Self.ResolveAsync();
        actionResult.Should().BeOk();
        refreshResult.Should().BeOk()
            .Which.Address.Should().Be(newAddress);
    }

    [Fact]
    public async Task CallAction_MarkAsFavorite()
    {
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        var customersAll = await apiRoot
            .NavigateAsync(l => l.CustomersRoot)
            .NavigateAsync(l => l.All);

        var customer = customersAll.Should().BeOk().Which.Customers.First();
        if (!customer.MarkAsFavorite.CanExecute)
        {
            Assert.Fail("Action can not be run on server, not offered.");
        }

        var actionResult =
            await customer.MarkAsFavorite.ExecuteAsync(MarkAsFavoriteParameters.FromCustomer(customer), this.Resolver);

        var customerResult = await customer.Self.ResolveAsync();
        actionResult.Should().BeOk();
        customerResult.Should().BeOk().Which.IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task CallAction_CreateQuery()
    {
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        var customersRoot = await apiRoot.NavigateAsync(l => l.CustomersRoot);

        var query = new CustomerQuery
        {
            Filter = new CustomerFilter { MinAge = 22 },
            SortBy = new SortOptions { PropertyName = "Age", SortType = "Ascending" },
            Pagination = new Pagination { PageOffset = 2, PageSize = 3 }
        };

        var resultResource = await customersRoot.Should().BeOk().Which.CreateQuery!.ExecuteAsync(query, this.Resolver);
        resultResource
            .Should()
            .BeOk()
            .Which
            .Should()
            .NotBeNull();
    }

    [Fact]
    public async Task EnterEntryPointAndNavigate()
    {
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        var customers = (await apiRoot.Should().BeOk().Which.CustomersRoot.ResolveAsync()).Should().BeOk().Which;
        var all = await customers.All.ResolveAsync();

        var allFluent = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint)
            .NavigateAsync(l => l.CustomersRoot).NavigateAsync(l => l.All);
        var allFluent2 = await apiRoot.NavigateAsync(l => l.CustomersRoot).NavigateAsync(l => l.All);
        var optionalFluent = await apiRoot.NavigateAsync(l => l.CustomersRoot).NavigateAsync(l => l.All)
            .NavigateAsync(l => l.Next);
    }

    [Fact]
    public async Task FileUpload()
    {
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        var carsResult = await apiRoot
            .NavigateAsync(l => l.CarsRoot);

        var cars = carsResult.Should().BeOk().Which;
        var function = cars.UploadInsuranceScan!;
        function.Should().NotBeNull();
        function.Configuration.Should().NotBeNull();
        function.Configuration.Should().BeEquivalentTo(new HypermediaClientFileUploadConfiguration(
            MaxFileSizeBytes: 200,
            AllowMultiple: true,
            Accept: [".pdf"]));
        var uploadResult = await cars.UploadInsuranceScan!.ExecuteAsync(
            new HypermediaFileUploadActionParameter(
                FileDefinitions:
                [
                    new(async () => new MemoryStream([5, 6, 7, 8]), "Scan", "Scan.pdf"),
                ]),
            this.Resolver);
        var link = uploadResult.Should().BeOk().Which;
        var bytes = await this.Client.GetByteArrayAsync(link.Uri);
        bytes.Should().BeEquivalentTo([5, 6, 7, 8]);
    }

    [Fact]
    public async Task FileUploadWithParameter()
    {
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        var carsResult = await apiRoot
            .NavigateAsync(l => l.CarsRoot);

        var cars = carsResult.Should().BeOk().Which;
        var function = cars.UploadCarImage!;
        function.Should().NotBeNull();
        function.Configuration.Should().NotBeNull();
        function.Configuration.Should().BeEquivalentTo(new HypermediaClientFileUploadConfiguration(
            MaxFileSizeBytes: 1024 * 1024 * 4,
            AllowMultiple: false,
            Accept: [".jpg", "image/png", "image/*"]));
        var uploadResult = await cars.UploadCarImage!.ExecuteAsync(
            new HypermediaFileUploadActionParameter<UploadCarImageParameters>(
                FileDefinitions: new List<FileDefinition>()
                {
                    new(async () => new MemoryStream(new byte[] { 1, 2, 3, 4 }), "Bytes", "Bytes.txt"),
                },
                new(
                    "Text",
                    true)),
            this.Resolver);
        var imageLink = uploadResult.Should().BeOk().Which;

        var imageResult = await this.Client.GetByteArrayAsync(imageLink.Uri);
        imageResult.Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4 });
    }
}