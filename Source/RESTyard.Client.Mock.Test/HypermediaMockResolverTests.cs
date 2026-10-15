using AwesomeAssertions;
using RESTyard.Client.Extensions;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Mock.Test;

public class EntryPointHco : HypermediaClientObject
{
    public int Number { get; set; }
    public MandatoryHypermediaLink<LinkedHco> Linked { get; set; }
    public IHypermediaClientAction ActionWithoutParameters { get; set; }
    public IHypermediaClientAction<int> ActionWithParameters { get; set; }
    public IHypermediaClientFunction<LinkedHco> FunctionWithoutParameters { get; set; }
    public IHypermediaClientFunction<LinkedHco, string> FunctionWithParameters { get; set; }
}

public class LinkedHco : HypermediaClientObject
{
    public string Text { get; set; }
}

public class HypermediaMockResolverTests
{
    [Fact]
    public async Task BasicEntityTest()
    {
        // Given
        var mockResolver = new HypermediaResolverMock(new Uri("https://some.link"));

        // When
        mockResolver
            .Entity<EntryPointHco>("/EntryPoint")
            .SetupProperties(e => e.Number = 5);

        // Then
        var epResult = await mockResolver.ResolveLinkAsync<EntryPointHco>(
            new Uri("https://some.link/EntryPoint"),
            cancellationToken: TestContext.Current.CancellationToken);
        var ep = epResult.Should().BeOk().Which;
        ep.Number.Should().Be(5);
    }
}

public class HypermediaMockResolverTests2
{
    private const string EntryPointPath = "/api/EntryPoint";
    private readonly HypermediaResolverMock mockResolver;
    private readonly HypermediaResolverMock.HypermediaResolverMockEntityBuilder<EntryPointHco> entryPointBuilder;

    public HypermediaMockResolverTests2()
    {
        this.mockResolver = new();
        this.entryPointBuilder = this.mockResolver.Entity<EntryPointHco>(EntryPointPath);
    }

    private async Task<HypermediaResult<EntryPointHco>> GetEntryPoint()
    {
        var result = await this.mockResolver.ResolveLinkAsync<EntryPointHco>(
            this.entryPointBuilder.ToLink().Uri!,
            cancellationToken: TestContext.Current.CancellationToken);
        result.Should().BeOk();
        return result;
    }

    [Fact]
    public async Task LinkedOkTest()
    {
        // When
        this.entryPointBuilder
            .Setup(
                x => x.Linked,
                linked => linked
                    .SetupProperties(x => x.Text = "Hi"));
        
        // Then
        var linkedResult = await this.GetEntryPoint()
            .NavigateAsync(ep => ep.Linked, cancellationToken: TestContext.Current.CancellationToken);
        var linked = linkedResult.Should().BeOk().Which;
        linked.Text.Should().Be("Hi");
    }

    [Fact]
    public async Task LinkedErrorTest()
    {
        // When
        entryPointBuilder
            .Setup(
                x => x.Linked,
                linked => linked
                    .Respond(HypermediaProblem.StatusCode(401)));
        
        // Then
        var linkedResult = await this.GetEntryPoint()
            .NavigateAsync(ep => ep.Linked, cancellationToken: TestContext.Current.CancellationToken);
        var linked = linkedResult.Should().BeError().Which;
        linked.Should().BeStatusCode().Which.Status.Should().Be(401);
    }
    
    [Fact]
    public async Task ActionOkTest()
    {
        // When
        entryPointBuilder
            .Setup(
                x => x.ActionWithoutParameters,
                action => action.RespondOk());
        
        // Then
        var actionResult = await this.GetEntryPoint()
            .Bind(ep => ep.ActionWithoutParameters.ExecuteAsync(this.mockResolver, TestContext.Current.CancellationToken));
        actionResult.Should().BeOk();
    }

    [Fact]
    public async Task ActionErrorTest()
    {
        // When
        entryPointBuilder
            .Setup(
                x => x.ActionWithoutParameters,
                action => action.Respond(
                    HypermediaProblem.StatusCode(403)));
        
        // Then
        var actionResult = await this.GetEntryPoint()
            .Bind(ep => ep.ActionWithoutParameters.ExecuteAsync(this.mockResolver, TestContext.Current.CancellationToken));
        actionResult.Should().BeError()
            .Which.Should().BeStatusCode()
            .Which.Status.Should().Be(403);
    }

    [Fact]
    public async Task ActionMissingTest()
    {
        // When
        entryPointBuilder
            .Setup(
                x => x.ActionWithoutParameters,
                _ => { });
        
        // Then
        var actionResult = await this.GetEntryPoint()
            .Bind(ep => ep.ActionWithoutParameters.ExecuteAsync(this.mockResolver, TestContext.Current.CancellationToken));
        actionResult.Should().BeError()
            .Which.Should().BeStatusCode()
            .Which.Status.Should().Be(404);
    }

    [Fact]
    public async Task ParameterActionOkTest()
    {
        // When
        entryPointBuilder
            .Setup(
                x => x.ActionWithParameters,
                action => action.RespondOk(5));
        
        // Then
        var actionResult = await this.GetEntryPoint()
            .Bind(ep => ep.ActionWithParameters.ExecuteAsync(5, this.mockResolver, TestContext.Current.CancellationToken));
        actionResult.Should().BeOk();
    }

    [Fact]
    public async Task ParameterActionErrorTest()
    {
        // When
        entryPointBuilder
            .Setup(
                x => x.ActionWithParameters,
                action => action.Respond(
                    7,
                    HypermediaProblem.StatusCode(403)));
        
        // Then
        var actionResult = await this.GetEntryPoint()
            .Bind(ep => ep.ActionWithParameters.ExecuteAsync(7, this.mockResolver, TestContext.Current.CancellationToken));
        actionResult.Should().BeError()
            .Which.Should().BeStatusCode()
            .Which.Status.Should().Be(403);
    }

    [Fact]
    public async Task ParameterActionMismatchTest()
    {
        // When
        entryPointBuilder
            .Setup(
                x => x.ActionWithParameters,
                action => action.RespondOk(7));
        
        // Then
        var actionResult = await this.GetEntryPoint()
            .Bind(ep => ep.ActionWithParameters.ExecuteAsync(8, this.mockResolver, TestContext.Current.CancellationToken));
        actionResult.Should().BeError()
            .Which.Should().BeStatusCode()
            .Which.Status.Should().Be(404);
    }
    
    [Fact]
    public async Task FunctionOkTest()
    {
        // When
        entryPointBuilder
            .Setup(
                x => x.FunctionWithoutParameters,
                action => action.Respond(
                    x => x.SetupProperties(
                        linked => linked.Text = "WithoutParameters")));
        
        // Then
        var functionResult = await this.GetEntryPoint()
            .Bind(ep => ep.FunctionWithoutParameters.ExecuteAndResolveAsync(this.mockResolver, TestContext.Current.CancellationToken));
        functionResult.Should().BeOk().Which.Text.Should().Be("WithoutParameters");

    }

    [Fact]
    public async Task FunctionErrorTest()
    {
        // When
        entryPointBuilder
            .Setup(
                x => x.FunctionWithoutParameters,
                function => function.Respond(
                    HypermediaProblem.StatusCode(403)));
        
        // Then
        var functionResult = await this.GetEntryPoint()
            .Bind(ep => ep.FunctionWithoutParameters.ExecuteAsync(this.mockResolver, TestContext.Current.CancellationToken));
        functionResult.Should().BeError()
            .Which.Should().BeStatusCode()
            .Which.Status.Should().Be(403);
    }

    [Fact]
    public async Task FunctionOkWithLinkErrorTest()
    {
        // When
        this.entryPointBuilder
            .Setup(
                x => x.FunctionWithoutParameters,
                function => function
                    .Respond(linked => linked
                        .Respond(HypermediaProblem.StatusCode(500))));
        
        // Then
        var functionResult = await this.GetEntryPoint()
            .Bind(ep => ep.FunctionWithoutParameters.ExecuteAsync(this.mockResolver, TestContext.Current.CancellationToken));
        var link = functionResult.Should().BeOk().Which;
        var linkResult = await link.ResolveAsync(cancellationToken: TestContext.Current.CancellationToken);
        linkResult.Should().BeError()
            .Which.Should().BeStatusCode()
            .Which.Status.Should().Be(500);
    }

    [Fact]
    public async Task FunctionMissingTest()
    {
        // When
        entryPointBuilder
            .Setup(
                x => x.FunctionWithoutParameters,
                _ => { });
        
        // Then
        var functionResult = await this.GetEntryPoint()
            .Bind(ep => ep.FunctionWithoutParameters.ExecuteAsync(this.mockResolver, TestContext.Current.CancellationToken));
        functionResult.Should().BeError()
            .Which.Should().BeStatusCode()
            .Which.Status.Should().Be(404);
    }

    [Fact]
    public async Task ParameterFunctionOkTest()
    {
        // When
        entryPointBuilder
            .Setup(
                x => x.FunctionWithParameters,
                action => action.Respond(
                    "5",
                    linked => linked.SetupProperties(
                        x => x.Text = "5")));
        
        // Then
        var functionResult = await this.GetEntryPoint()
            .Bind(ep => ep.FunctionWithParameters.ExecuteAndResolveAsync("5", this.mockResolver, TestContext.Current.CancellationToken));
        functionResult.Should().BeOk().Which.Text.Should().Be("5");
    }

    [Fact]
    public async Task ParameterFunctionErrorTest()
    {
        // When
        entryPointBuilder
            .Setup(
                x => x.FunctionWithParameters,
                function => function.Respond(
                    "7",
                    HypermediaProblem.StatusCode(403)));
        
        // Then
        var actionResult = await this.GetEntryPoint()
            .Bind(ep => ep.FunctionWithParameters.ExecuteAsync("7", this.mockResolver, TestContext.Current.CancellationToken));
        actionResult.Should().BeError()
            .Which.Should().BeStatusCode()
            .Which.Status.Should().Be(403);
    }

    [Fact]
    public async Task ParameterFunctionMismatchTest()
    {
        // When
        entryPointBuilder
            .Setup(
                x => x.FunctionWithParameters,
                action => action.Respond(
                    "7",
                    linked => linked
                        .SetupProperties(x => x.Text = "7")));
        
        // Then
        var actionResult = await this.GetEntryPoint()
            .Bind(ep => ep.FunctionWithParameters.ExecuteAsync("8", this.mockResolver, TestContext.Current.CancellationToken));
        actionResult.Should().BeError()
            .Which.Should().BeStatusCode()
            .Which.Status.Should().Be(404);
    }
}
