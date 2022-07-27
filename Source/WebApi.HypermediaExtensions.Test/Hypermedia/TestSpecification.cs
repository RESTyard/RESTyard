using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RESTyard.AspNetCore.Test.Hypermedia
{
    public abstract class TestSpecification
    {
        [TestInitialize]
        public void Init()
        {
            Given();
            When();
        }

        public virtual void Given(){}
        public abstract void When();
    }

    [TestClass]
    public abstract class AsyncTestSpecification
    {
        [TestInitialize]
        public async Task Initialize()
        {
            Given();
            await GivenAsync().ConfigureAwait(false);
            await When().ConfigureAwait(false);
        }

        protected virtual Task GivenAsync() { return Task.FromResult(42); }

        protected virtual void Given() { }
        protected virtual Task When() { return Task.FromResult(42); }

        [TestCleanup]
        public virtual void CleanUp() { }

        protected void Log(string message)
        {
            Logger.Log(message);
        }
    }

    public static class Logger
    {
        public static void Log(object message)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} ({Thread.CurrentThread.ManagedThreadId:000}): {message}");
        }
    }
}