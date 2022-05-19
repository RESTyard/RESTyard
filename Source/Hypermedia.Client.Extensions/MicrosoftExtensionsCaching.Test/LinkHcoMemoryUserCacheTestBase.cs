using System;
using System.Linq;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Resolver.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MicrosoftExtensionsCaching;
using Xunit;

namespace Extensions.Test.Caching
{
    public class LinkHcoMemoryUserCacheTestBase
    {
        protected Func<string, Uri, object> HcoEntryKeyBuilder = (user, uri) => (user, uri);
        protected Func<string, object> ControlEntryKeyBuilder = user => user;
        protected Action<ICacheEntry> ConfigureEntryExpiration = _ => { };
        protected Action<ICacheEntry> ConfigureControlExpiration = _ => { };
        protected Action<ICacheEntry> ConfigureRootExpiration = _ => { };
        protected const string CurrentUserIdentifier = "CurrentUser";
        protected const string SharedUserIdentifier = "SharedUser";
        protected const string RootControlTokenKey = "RootControlToken";

        protected Uri TestUri => new Uri("test://some_test_uri");

        protected HypermediaClientObject TestHco => new TestHco();

        protected IOptions<MemoryCacheOptions> MemoryCacheOptions { get; }

        protected IMemoryCache MemoryCache { get; }

        protected LinkHcoMemoryUserCache<string, string> UserCache { get; }

        protected LinkHcoMemoryUserCacheTestBase()
            : this(new MemoryCacheOptions())
        {
        }

        protected LinkHcoMemoryUserCacheTestBase(MemoryCacheOptions options)
        {
            this.MemoryCacheOptions = new OptionsWrapper<MemoryCacheOptions>(options);
            this.MemoryCache = new MemoryCache(this.MemoryCacheOptions);
            this.UserCache = new LinkHcoMemoryUserCache<string, string>(
                this.MemoryCache,
                CurrentUserIdentifier,
                SharedUserIdentifier,
                HcoEntryKeyBuilder,
                ControlEntryKeyBuilder,
                ConfigureEntryExpiration,
                ConfigureControlExpiration,
                ConfigureRootExpiration,
                RootControlTokenKey);
        }

        protected CacheEntry<string> CreateEntry(
            HypermediaClientObject hco,
            CacheScope scope)
        {
            return new CacheEntry<string>(
                hco,
                CacheMode.Default,
                scope,
                "",
                null);
        }
    }

    public class TestHco : HypermediaClientObject
    {
        public TestHco()
        {
            this.Title = "TestHco";
            this.Relations.Add("DummyRelation");
        }

        public TestHco(
            string title,
            params string[] relations)
        {
            this.Title = title;
            this.Relations = relations.ToList();
        }
    }
}