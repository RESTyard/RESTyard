using System.Net;
using System.Net.Http.Headers;
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
        queryResult.TotalEntities.Should().BeGreaterThanOrEqualTo(18);
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
    public async Task FunctionWithResolveLocationFlag()
    {
        // Given
        var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
        var customersRootResult = await apiRoot.NavigateAsync(e => e.CustomersRoot);
        var customerResult = await customersRootResult.Bind(r => r.CreateCustomer!.ExecuteAndResolveAsync(new CreateCustomerParameters("InlineResult"), this.Resolver));
        var bestCustomer = customerResult.Should().BeOk().Which;
        
        // When
        var executeAndResolveResult = await bestCustomer.BuyCar!
            .ExecuteAndResolveAsync(new BuyCarParameters("VW", 1, 100), this.Resolver);
        
        // Then
        var car = executeAndResolveResult.Should().BeOk().Which;
        car.Brand.Should().Be("VW");
        car.Id.Should().Be(1);
    }

    [Fact]
    public async Task ManualFunctionWithResolveLocationOn()
    {
        // Given
        var createResult = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint)
            .NavigateAsync(e => e.CustomersRoot)
            .Bind(c => c.CreateCustomer!.ExecuteAndResolveAsync(
                new CreateCustomerParameters("ManualWithInline"), this.Resolver));
        var customer = createResult.Should().BeOk().Which;
        var url = customer.BuyCar!.Uri;
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(
            /* lang=json */
            """
            {
                "Brand": "VW",
                "CarId": 5,
                "Price": 100
            }
            """);
        request.Headers.Add("X-RestyardInlineFunctionResult", "true");
        
        // When
        var result = await this.Client.SendAsync(request);
        
        // Then
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Headers.Should().ContainKey("X-RestyardInlinedFunctionResult")
            .WhoseValue.Should().ContainSingle()
            .Which.Should().Be("true");
        result.Headers.Location.Should().NotBeNull();
        result.Headers.Location.LocalPath.Should().Be("/Cars/VW/5");
        var content = await result.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ManualFunctionWithResolveLocationOff()
    {
        // Given
        var createResult = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint)
            .NavigateAsync(e => e.CustomersRoot)
            .Bind(c => c.CreateCustomer!.ExecuteAndResolveAsync(
                new CreateCustomerParameters("ManualWithoutInline"), this.Resolver));
        var customer = createResult.Should().BeOk().Which;
        var url = customer.BuyCar!.Uri;
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(
            /* lang=json */
            """
            {
                "Brand": "VW",
                "CarId": 6,
                "Price": 100
            }
            """);
        
        // When
        var result = await this.Client.SendAsync(request);
        
        // Then
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Headers.Should().NotContainKey("X-RestyardInlinedFunctionResult");
        result.Headers.Location.Should().NotBeNull();
        result.Headers.Location.LocalPath.Should().Be("/Cars/VW/6");
    }
}