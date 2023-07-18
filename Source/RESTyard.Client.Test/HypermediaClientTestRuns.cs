using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.Client.Authentication;
using RESTyard.Client.Builder;
using RESTyard.Client.Extensions;
using RESTyard.Client.Extensions.SystemNetHttp;
using RESTyard.Client.Extensions.SystemTextJson;
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
            this.Resolver = HypermediaResolverBuilder
                .CreateBuilder()
                .ConfigureObjectRegister(ConfigureHypermediaObjectRegister)
                .WithSingleSystemTextJsonObjectParameterSerializer()
                .WithSystemTextJsonStringParser()
                .WithSystemTextJsonProblemReader()
                .WithSirenHypermediaReader()
                .CreateHttpHypermediaResolver(httpClient);
        }

        [TestMethod]
        public async Task EnterEntryPoint()
        {
            var apiRoot = await this.Resolver.ResolveLinkAsync<EntryPointHco>(ApiEntryPoint);
            Assert.IsTrue(apiRoot.IsOk);
            Assert.IsNotNull(apiRoot.GetValueOrDefault());
        }

        [TestMethod]
        public async Task CallAction_CustomerMove()
        {
            var apiRoot = await this.Resolver.ResolveLinkAsync<EntryPointHco>(ApiEntryPoint);
            var customersAll = await apiRoot.NavigateAsync(l => l.Customers).NavigateAsync(l => l.All);
            Assert.IsTrue(customersAll.IsOk);
            var customer = customersAll.GetValueOrThrow().Customers.First();

            var newAddress = "New Address";
            var actionResult = await customer.CustomerMove.ExecuteAsync(new NewAddress(Address: newAddress), this.Resolver);
            
            var refreshResult = await customer.Self.ResolveAsync();
            Assert.IsTrue(actionResult.IsOk);
            Assert.IsTrue(refreshResult.IsOk);
            customer = refreshResult.GetValueOrThrow();
            Assert.AreEqual(newAddress, customer.Address);
        }

        [TestMethod]
        public async Task CallAction_MarkAsFavorite()
        {
            var apiRoot = await this.Resolver.ResolveLinkAsync<EntryPointHco>(ApiEntryPoint);
            var customersAll = await apiRoot
                .NavigateAsync(l => l.Customers)
                .NavigateAsync(l => l.All);

            var customer = customersAll.GetValueOrThrow().Customers.First();
            if (!customer.MarkAsFavorite.CanExecute)
            {
                Assert.Inconclusive("Action can not be run on server, not offered.");
            }

            var actionResult = await customer.MarkAsFavorite.ExecuteAsync(new FavoriteCustomer{ Customer = customer.Self.Uri.ToString() }, this.Resolver); 

            customer = (await customer.Self.ResolveAsync()).GetValueOrThrow();
            Assert.IsTrue(actionResult.IsOk);
            Assert.IsTrue(customer.IsFavorite);
        }

        [TestMethod]
        public async Task CallAction_CreateQuery()
        {
            var apiRoot = await this.Resolver.ResolveLinkAsync<EntryPointHco>(ApiEntryPoint);
            var customersRoot = await apiRoot.NavigateAsync(l => l.Customers);

            var query = new CustomersQuery
            {
                Filter = new CustomerFilter { MinAge = 22 },
                SortBy = new SortOptions { PropertyName = "Age", SortType  = "Ascending" },
                Pagination = new Pagination { PageOffset = 2, PageSize = 3 }
            };

            var resultResource = await customersRoot.GetValueOrThrow().CreateQuery.ExecuteAsync(query, this.Resolver);
            var queryResultPage = await resultResource.GetValueOrThrow().ResolveAsync();
            Assert.IsNotNull(queryResultPage);
        }

        [TestMethod]
        public async Task EnterEntryPointAndNavigate()
        {
            var apiRoot = await this.Resolver.ResolveLinkAsync<EntryPointHco>(ApiEntryPoint);
            var customers = (await apiRoot.GetValueOrThrow().Customers.ResolveAsync()).GetValueOrThrow();
            var all = await customers.All.ResolveAsync();

            var allFluent = await this.Resolver.ResolveLinkAsync<EntryPointHco>(ApiEntryPoint).NavigateAsync(l => l.Customers).NavigateAsync(l => l.All);
            var allFluent2 = await apiRoot.NavigateAsync(l => l.Customers).NavigateAsync(l => l.All);
            var optionalFluent = await apiRoot.NavigateAsync(l => l.Customers).NavigateAsync(l => l.All).NavigateAsync(l => l.Next);
        }

        private static void ConfigureHypermediaObjectRegister(IHypermediaObjectRegister register)
        {
            register.Register<EntryPointHco>();
            register.Register<CustomerHco>();
            register.Register<CustomersRootHco>();
            register.Register<CustomerQueryResultHco>();
        }
    }
}
