using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
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
using Xunit.Abstractions;

namespace RESTyard.Integration.Test;

public class IntegrationTests : IAsyncLifetime
{
    private static readonly Uri ApiEntryPoint = new Uri($"{CarShackWaf.BaseUrl}/EntryPoint");
    
    private readonly CarShackWaf carShackFactory;
    private readonly IHttpHypermediaResolverFactory apiResolverFactory;

    public IntegrationTests(ITestOutputHelper outputHelper)
    {
        this.carShackFactory = new(outputHelper);

        this.apiResolverFactory = DefaultHypermediaClientBuilder
            .CreateBuilder()
            .WithSirenHypermediaReader()
            .WithSystemTextJsonStringParser()
            .WithSystemTextJsonProblemReader()
            .WithSingleSystemTextJsonObjectParameterSerializer()
            .CreateHttpHypermediaResolverFactory();
    }

    protected HttpClient Client { get; private set; } = null!;
    protected IHypermediaResolver Resolver { get; private set; } = null!;
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

        var newAddress = new AddressTo(
            Street: "New Street",
            Number: "5",
            City: "New City",
            ZipCode: "54321");
        var actionResult = await customer.CustomerMove!.ExecuteAsync(new NewAddress(Address: newAddress), this.Resolver);

        var refreshResult = await customer.Self.ResolveAsync();
        actionResult.Should().BeOk();
        refreshResult.Should().BeOk()
            .Which.Address.Should().Be(newAddress);
    }

    [Fact]
    public async Task CallAction_MarkAsFavorite()
    {
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        var customersRootResult = await apiRoot
            .NavigateAsync(l => l.CustomersRoot);
        var customersRoot = customersRootResult.Should().BeOk().Which;
        (await customersRoot.CreateCustomer!.ExecuteAsync(new CreateCustomerParameters("Name"), this.Resolver)).Should().BeOk();
        var customersAll = await customersRoot.All.ResolveAsync();

        var customer = customersAll.Should().BeOk().Which.Customers.First(c => !c.IsFavorite);
        if (!customer.MarkAsFavorite!.CanExecute)
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
        var customersRootResult = await apiRoot.NavigateAsync(l => l.CustomersRoot);
        var customersRoot = customersRootResult.Should().BeOk().Which;
        
        var query = new CustomerQuery
        {
            Filter = new CustomerFilter { MinAge = 22 },
            SortBy = new SortOptions { PropertyName = "Age", SortType = "Ascending" },
            Pagination = new Pagination { PageOffset = 2, PageSize = 3 }
        };

        var resultResource = await customersRoot.CreateQuery!.ExecuteAsync(query, this.Resolver);
        var link = resultResource
            .Should()
            .BeOk()
            .Which;
        var result = await link.ResolveAsync();
        var queryResult = result.Should().BeOk().Which;
        queryResult.Customers.Should().HaveCount(3);
        queryResult.All?.Uri.Should().NotBeNull();
        queryResult.Next?.Uri.Should().NotBeNull();
        queryResult.Previous?.Uri.Should().NotBeNull();
        queryResult.TotalEntities.Should().Be(20);
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
            .NavigateAsync(l => l.Next!);
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
                    new(() => Task.FromResult<Stream>(new MemoryStream([5, 6, 7, 8])), "Scan", "Scan.pdf"),
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
                    new(() => Task.FromResult<Stream>(new MemoryStream([1, 2, 3, 4])), "Bytes", "Bytes.txt"),
                },
                new(
                    "Text",
                    true)),
            this.Resolver);
        var imageLink = uploadResult.Should().BeOk().Which;

        var imageResult = await this.Client.GetByteArrayAsync(imageLink.Uri);
        imageResult.Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4 });
    }

    [Fact]
    public async Task BuyCar()
    {
        // Given
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        var customersResult = await apiRoot
            .NavigateAsync(l => l.CustomersRoot);
        var carsResult = await apiRoot
            .NavigateAsync(l => l.CarsRoot);

        var customersRoot = customersResult.Should().BeOk().Which;
        var carsRoot = carsResult.Should().BeOk().Which;
        var niceCar = (await carsRoot.NiceCar.ResolveAsync()).Should().BeOk().Which;
        var createCustomerResult = await customersRoot.CreateCustomer!
            .ExecuteAsync(new CreateCustomerParameters("Jasper"), this.Resolver)
            .Bind(l => l.ResolveAsync());
        var customer = createCustomerResult.Should().BeOk().Which;
        
        // When
        var buyResult = await customer.BuyCar!
            .ExecuteAsync(new BuyCarParameters(niceCar.Brand!, niceCar.Id!.Value, Price: 100), this.Resolver)
            .Bind(l => l.ResolveAsync());
        
        // Then
        var boughtCar = buyResult.Should().BeOk().Which;
        boughtCar.Id.Should().Be(niceCar.Id);
    }

    [Fact]
    public async Task ActionParameterEndpoint_ExplicitAndImplicit()
    {
        // Given
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        var customersRootResult = await apiRoot.NavigateAsync(e => e.CustomersRoot);
        var customersRoot = customersRootResult.Should().BeOk().Which;
        var createCustomerResult = await customersRoot.CreateCustomer!
            .ExecuteAsync(new CreateCustomerParameters("Test"), this.Resolver)
            .Bind(l => l.ResolveAsync());
        var customer = createCustomerResult.Should().BeOk().Which;

        // Then
        var newAddressDescription = customer.CustomerMove!.ParameterDescriptions.Should().ContainSingle().Which;
        var newAddressParameterClass = newAddressDescription.Classes.Should().ContainSingle().Which;
        newAddressParameterClass.Should().Be($"{CarShackWaf.BaseUrl}/Customers/NewAddressType");
        
        // Then
        var createCustomerDescription =
            customersRoot.CreateCustomer!.ParameterDescriptions.Should().ContainSingle().Which;
        var createCustomerParameterClass = createCustomerDescription.Classes.Should().ContainSingle().Which;
        createCustomerParameterClass.Should()
            .Be($"{CarShackWaf.BaseUrl}/ActionParameterTypes/CreateCustomerParameters");
    }

    [Fact]
    public async Task DateOnly_InParameter_Works()
    {
        // Given
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        var carsRootResult = await apiRoot.NavigateAsync(e => e.CarsRoot);
        var carsRoot = carsRootResult.Should().BeOk().Which;
        var anyCarResult = await carsRoot.NiceCar.ResolveAsync();
        var anyCar = anyCarResult.Should().BeOk().Which;

        // Then
        anyCar.LastInspection.Should().HaveDay(2);
        var result = await anyCar.UpdateInspection!
            .ExecuteAsync(new UpdateCarInspection(new DateOnly(2026, 08, 31)), this.Resolver)
            .Bind(l => l.ResolveAsync());
        
        // Then
        result.Should().BeOk();
    }

    [Fact]
    public async Task HypermediaUI_AngularServeTests()
    {
        // Given
        
        // When
        var root = await this.Client.GetAsync($"{CarShackWaf.BaseUrl}/hui/from-appsettings/index.html");
        
        // Then
        root.Should().BeSuccessful();
        var index = await root.Content.ReadAsStringAsync();
        var regex = new Regex("(?:href|src)=\"(?<uri>.*?)\"");
        var matches = regex.Matches(index);
        matches.Should().AllSatisfy(m => m.Success.Should().BeTrue());
        foreach (var capture in matches.SelectMany(m => m.Groups["uri"].Captures))
        {
            var uri = capture.Value;
            var subContent = await this.Client.GetAsync($"{CarShackWaf.BaseUrl}/{uri.TrimStart('/')}");
            subContent.Should().BeSuccessful(because: uri);
        }
    }

    [Fact]
    public async Task HypermediaUI_RewriteTests()
    {
        // Given
        var indexByName = await this.Client.GetStringAsync($"{CarShackWaf.BaseUrl}/hui/from-appsettings/index.html");
        List<string> builtinRedirects = ["", "hui", "auth-redirect"];
        List<string> aliasRedirects = ["CarShack"];

        foreach (var redirect in builtinRedirects.Concat(aliasRedirects))
        {
            // When
            var result = await this.Client.GetAsync($"{CarShackWaf.BaseUrl}/hui/from-appsettings/{redirect}");
            
            // Then
            result.Should().BeSuccessful(because: redirect);
            (await result.Content.ReadAsStringAsync()).Should().Be(indexByName);
        }
        
        // When
        var specialCaseIndexNoTrailingSlash = await this.Client.GetAsync($"{CarShackWaf.BaseUrl}/hui/from-appsettings");
        
        // Then
        specialCaseIndexNoTrailingSlash.Should().BeSuccessful();
        (await specialCaseIndexNoTrailingSlash.Content.ReadAsStringAsync()).Should().Be(indexByName);
    }

    [Fact]
    public async Task HypermediaUI_SecondInstanceTest()
    {
        // When
        var index = await this.Client.GetAsync($"{CarShackWaf.BaseUrl}/hui/explicit-config/index.html");
        
        // Then
        index.Should().BeSuccessful();
        (await index.Content.ReadAsStringAsync()).Should().NotBeNullOrEmpty();
    }
}