using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.Client.Authentication;
using RESTyard.Client.Builder;
using RESTyard.Client.Extensions;
using RESTyard.Client.Extensions.SystemNetHttp;
using RESTyard.Client.Extensions.SystemTextJson;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Client.Reader;
using RESTyard.Client.Resolver;
using RESTyard.Client.Test.Hypermedia;

namespace RESTyard.Client.Test
{
    /// <summary>
    /// Tests used during prototyping.
    /// </summary>
    [TestClass, TestCategory("SkipInCI")]
    public class HypermediaClientTestRuns
    {
        private static readonly Uri ApiEntryPoint = new Uri("http://localhost:5000/entrypoint");

        private IHypermediaResolver Resolver { get; set; }


        [TestInitialize]
        public void Initialize()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new UsernamePasswordCredentials("User", "Password").CreateBasicAuthHeaderValue();
            httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en", 1.0));
            this.Resolver = DefaultHypermediaClientBuilder
                .CreateBuilder()
                .WithSingleSystemTextJsonObjectParameterSerializer()
                .WithSystemTextJsonStringParser()
                .WithSystemTextJsonProblemReader()
                .WithSirenHypermediaReader()
                .CreateHttpHypermediaResolver(httpClient);
        }

        [TestMethod]
        public async Task EnterEntryPoint()
        {
            var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
            apiRoot.Should().BeOk()
                .Which.Should().NotBeNull();
        }

        [TestMethod]
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

        [TestMethod]
        public async Task CallAction_MarkAsFavorite()
        {
            var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
            var customersAll = await apiRoot
                .NavigateAsync(l => l.CustomersRoot)
                .NavigateAsync(l => l.All);

            var customer = customersAll.Should().BeOk().Which.Customers.First();
            if (!customer.MarkAsFavorite.CanExecute)
            {
                Assert.Inconclusive("Action can not be run on server, not offered.");
            }

            var actionResult = await customer.MarkAsFavorite.ExecuteAsync(MarkAsFavoriteParameters.FromCustomer(customer), this.Resolver); 

            var customerResult = await customer.Self.ResolveAsync();
            actionResult.Should().BeOk();
            customerResult.Should().BeOk().Which.IsFavorite.Should().BeTrue();
        }

        [TestMethod]
        public async Task CallAction_CreateQuery()
        {
            var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
            var customersRoot = await apiRoot.NavigateAsync(l => l.CustomersRoot);

            var query = new CustomerQuery
            {
                Filter = new CustomerFilter { MinAge = 22 },
                SortBy = new SortOptions { PropertyName = "Age", SortType  = "Ascending" },
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

        [TestMethod]
        public async Task EnterEntryPointAndNavigate()
        {
            var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
            var customers = (await apiRoot.Should().BeOk().Which.CustomersRoot.ResolveAsync()).Should().BeOk().Which;
            var all = await customers.All.ResolveAsync();

            var allFluent = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint).NavigateAsync(l => l.CustomersRoot).NavigateAsync(l => l.All);
            var allFluent2 = await apiRoot.NavigateAsync(l => l.CustomersRoot).NavigateAsync(l => l.All);
            var optionalFluent = await apiRoot.NavigateAsync(l => l.CustomersRoot).NavigateAsync(l => l.All).NavigateAsync(l => l.Next);
        }

        [TestMethod]
        public async Task FileUpload()
        {
            var apiRoot = await this.Resolver.ResolveLinkAsync<HypermediaEntrypointHco>(ApiEntryPoint);
            var carsResult = await apiRoot
                .NavigateAsync(l => l.CarsRoot);

            var cars = carsResult.Should().BeOk().Which;
            var uploadResult = await cars.UploadCarImage!.ExecuteAsync(
                new HypermediaFileUploadActionParameter<UploadCarImageParameters>(
                    FileDefinitions: new List<FileDefinition>()
                    {
                        new(async () => new MemoryStream(new byte[] { 1, 2, 3, 4}), "Bytes", "Bytes.txt"),
                    },
                    new(
                        "Text",
                        true)),
                this.Resolver);
            uploadResult.Should().BeOk();
        }
    }
}
