using System;
using System.Threading;
using Microsoft.Extensions.Primitives;

namespace MicrosoftExtensionsCaching
{
    internal sealed class CancellationChangeTokenWrapper : IDisposable
    {
        private readonly CancellationTokenSource cts;

        public CancellationChangeTokenWrapper()
        {
            this.cts = new CancellationTokenSource();
            this.Token = new CancellationChangeToken(this.cts.Token);
        }

        public IChangeToken Token { get; }

        public void Cancel()
        {
            this.cts.Cancel();
        }

        public void Dispose()
        {
            this.cts.Dispose();
        }
    }
}