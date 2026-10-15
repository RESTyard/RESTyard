using System;
using System.Threading;
using System.Threading.Tasks;

namespace RESTyard.AspNetCore.Test.Hypermedia
{
    public abstract class TestSpecification
    {
        protected TestSpecification()
        {
            Given();
            When();
        }

        public virtual void Given(){}
        public abstract void When();
    }

    public abstract class AsyncTestSpecification : IAsyncLifetime
    {
        public async ValueTask InitializeAsync()
        {
            Given();
            await GivenAsync().ConfigureAwait(false);
            await When().ConfigureAwait(false);
        }

        protected virtual Task GivenAsync() { return Task.FromResult(42); }

        protected virtual void Given() { }
        protected virtual Task When() { return Task.FromResult(42); }

        public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;

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