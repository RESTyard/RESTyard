using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;

namespace RESTyard.Client.Analyzers.Tests;

public class ExecuteAndResolveAnalyzerTests : VerifyAnalyzer
{
    public ExecuteAndResolveAnalyzerTests() : base()
    {
    }
    
    [Fact]
    public async Task ReplacesExecuteAndBindResolve()
    {
        var code =
            """
            using System.Threading.Tasks;
            using FunicularSwitch;
            using RESTyard.Client.Extensions;
            using RESTyard.Client.Hypermedia;
            using RESTyard.Client.Hypermedia.Commands;
            using RESTyard.Client.Resolver;

            public class Result : HypermediaClientObject { }

            public class Test
            {
                public Task<HypermediaResult<Result>> Execute(IHypermediaClientFunction<Result, object> hco, IHypermediaResolver resolver)
                    => hco.ExecuteAsync(new object(), resolver).Bind(l => l.ResolveAsync());
            }
            """;

        await Verify(
            code,
            new ExecuteAndResolveAnalyzer(),
            new ExecuteAndResolveCodeFixProvider(),
            diagnostics => diagnostics.Should().ContainSingle().Which.Id.Should().Be(ExecuteAndResolveAnalyzer.DiagnosticId));
    }

    [Fact]
    public async Task DoesNotReportOtherBindCalls()
    {
        var code =
            """
            using System.Threading.Tasks;
            using FunicularSwitch;
            using RESTyard.Client.Extensions;
            using RESTyard.Client.Hypermedia;
            using RESTyard.Client.Hypermedia.Commands;
            using RESTyard.Client.Resolver;

            public class Result : HypermediaClientObject { }

            public class Test
            {
                public Task<HypermediaResult<Result>> Execute(IHypermediaClientFunction<Result, object> hco, IHypermediaResolver resolver)
                    => hco.ExecuteAsync(new object(), resolver).Bind(l => l.ResolveAsync(default));
            }
            """;

        await Verify(
            code,
            new ExecuteAndResolveAnalyzer(),
            new ExecuteAndResolveCodeFixProvider(),
            diagnostics => diagnostics.Should().BeEmpty());
    }

    [Fact]
    public async Task ReplacesExecuteAndBindResolveWithSameCancellationToken()
    {
        var code =
            """
            using System.Threading;
            using System.Threading.Tasks;
            using FunicularSwitch;
            using RESTyard.Client.Extensions;
            using RESTyard.Client.Hypermedia;
            using RESTyard.Client.Hypermedia.Commands;
            using RESTyard.Client.Resolver;

            public class Result : HypermediaClientObject { }

            public class Test
            {
                public Task<HypermediaResult<Result>> Execute(IHypermediaClientFunction<Result, object> hco, IHypermediaResolver resolver, CancellationToken cancellationToken)
                    => hco.ExecuteAsync(new object(), resolver, cancellationToken).Bind(l => l.ResolveAsync(cancellationToken));
            }
            """;

        await Verify(
            code,
            new ExecuteAndResolveAnalyzer(),
            new ExecuteAndResolveCodeFixProvider(),
            diagnostics => diagnostics.Should().ContainSingle().Which.Id.Should().Be(ExecuteAndResolveAnalyzer.DiagnosticId));
    }

    [Fact]
    public async Task DoesNotReportDifferentCancellationTokens()
    {
        var code =
            """
            using System.Threading;
            using System.Threading.Tasks;
            using FunicularSwitch;
            using RESTyard.Client.Extensions;
            using RESTyard.Client.Hypermedia;
            using RESTyard.Client.Hypermedia.Commands;
            using RESTyard.Client.Resolver;

            public class Result : HypermediaClientObject { }

            public class Test
            {
                public Task<HypermediaResult<Result>> Execute(IHypermediaClientFunction<Result, object> hco, IHypermediaResolver resolver, CancellationToken executeCancellationToken, CancellationToken resolveCancellationToken)
                    => hco.ExecuteAsync(new object(), resolver, executeCancellationToken).Bind(l => l.ResolveAsync(resolveCancellationToken));
            }
            """;

        await Verify(
            code,
            new ExecuteAndResolveAnalyzer(),
            new ExecuteAndResolveCodeFixProvider(),
            diagnostics => diagnostics.Should().BeEmpty());
    }
}
