using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FunicularSwitch;
using FunicularSwitch.Extensions;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Mock;

internal interface IHypermediaLookupResult;

internal interface IHypermediaLookupOk<out T> : IHypermediaLookupResult
{
    T Value { get; }
}

internal interface IHypermediaLookupError : IHypermediaLookupResult
{
    HypermediaProblem Problem { get; }
}

internal static class HypermediaLookup
{
    public static IHypermediaLookupOk<T> Ok<T>(T value)
        => new HypermediaLookupOk<T>(value);

    public static IHypermediaLookupError Error(HypermediaProblem problem)
        => new HypermediaLookupError(problem);

    internal record HypermediaLookupOk<T>(T Value) : IHypermediaLookupOk<T>;

    internal record HypermediaLookupError(HypermediaProblem Problem) : IHypermediaLookupError;
}

public class HypermediaResolverMock : IHypermediaResolver
{
    private static readonly HypermediaResultError notFoundError =
        HypermediaResult.Error(HypermediaProblem.StatusCode((int)HttpStatusCode.NotFound));
    
    private readonly Dictionary<Uri, IHypermediaLookupResult> hcoLookup = new();

    private readonly Dictionary<Uri, IHypermediaLookupResult> actionLookup = new();
    private readonly Dictionary<(Uri, object?), IHypermediaLookupResult> actionParameterLookup = new(); 
    private readonly Dictionary<Uri, IHypermediaLookupResult> functionLookup = new();
    private readonly Dictionary<(Uri, object?), IHypermediaLookupResult> functionParameterLookup = new();

    public HypermediaResolverMock(Uri? baseUri = null)
    {
        this.BaseUri = baseUri ?? new Uri("https://hypermedia.mock");
    }
    
    public Uri BaseUri { get; }

    public void Dispose()
    {
    }

    public HypermediaResolverMockEntityBuilder<THco> Entity<THco>(string? relativeUri = null)
        where THco : HypermediaClientObject, new()
    {
        return new HypermediaResolverMockEntityBuilder<THco>(this, this.BaseUri, relativeUri);
    }

    private static HypermediaResult<T> FromLookupResult<T>(IHypermediaLookupResult lookupResult)
        => lookupResult switch
        {
            IHypermediaLookupOk<T> ok => HypermediaResult.Ok(ok.Value),
            IHypermediaLookupError error => HypermediaResult.Error(error.Problem),
            _ => notFoundError,
        };

    public Task<HypermediaResult<T>> ResolveLinkAsync<T>(Uri uriToResolve, bool forceResolve = false,
        CancellationToken cancellationToken = default) where T : HypermediaClientObject
    {
        return Task.FromResult(
            this.hcoLookup
                .TryGetValue(uriToResolve)
                .Map(FromLookupResult<T>)
                .GetValueOrDefault(notFoundError));
    }

    public Task<HypermediaResult<Unit>> ResolveActionAsync(Uri uri, string method,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            this.actionLookup
                .TryGetValue(uri)
                .Map(FromLookupResult<Unit>)
                .GetValueOrDefault(notFoundError));
    }

    public Task<HypermediaResult<Unit>> ResolveActionAsync(Uri uri, string method,
        IReadOnlyList<ParameterDescription> parameterDescriptions, object? parameterObject,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            this.actionParameterLookup
                .TryGetValue((uri, parameterObject))
                .Map(FromLookupResult<Unit>)
                .GetValueOrDefault(notFoundError));
    }

    public Task<HypermediaResult<LinkOrEntity<T>>> ResolveFunctionAsync<T>(Uri uri, string method,
        CancellationToken cancellationToken = default) where T : HypermediaClientObject
    {
        return Task.FromResult(
            this.functionLookup
                .TryGetValue(uri)
                .Map(FromLookupResult<MandatoryHypermediaLink<T>>)
                .GetValueOrDefault(notFoundError)
                .Map(LinkOrEntity<T>.Link));
    }

    public Task<HypermediaResult<LinkOrEntity<T>>> ResolveFunctionAsync<T>(Uri uri, string method,
        IReadOnlyList<ParameterDescription> parameterDescriptions, object? parameterObject,
        CancellationToken cancellationToken = default) where T : HypermediaClientObject
    {
        return Task.FromResult(
            this.functionParameterLookup
                .TryGetValue((uri, parameterObject))
                .Map(FromLookupResult<MandatoryHypermediaLink<T>>)
                .GetValueOrDefault(notFoundError)
                .Map(LinkOrEntity<T>.Link));
    }

    public class HypermediaResolverMockEntityBuilder<THco> where THco : HypermediaClientObject, new()
    {
        private static string UriName<T>() => Uri.EscapeDataString(typeof(T).Name);

        private static PropertyInfo ParseExpression(LambdaExpression expression)
        {
            if (expression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException($"Only PropertyExpression is supported");
            }

            if (memberExpression.Member is not PropertyInfo propertyInfo)
            {
                throw new ArgumentException($"Only PropertyExpression is supported");
            }

            if (propertyInfo.SetMethod is null)
            {
                throw new ArgumentException($"Needs setter on property");
            }

            return propertyInfo;
        }

        private readonly THco hco = new();
        private readonly HypermediaResolverMock resolver;
        private readonly Uri baseUri;
        private readonly Uri entityUri;

        public HypermediaResolverMockEntityBuilder(HypermediaResolverMock resolver,
            Uri baseUri,
            string? relativeUri)
        {
            this.resolver = resolver;
            this.baseUri = baseUri;
            this.entityUri = new Uri(baseUri, relativeUri ?? $"/api/{UriName<THco>()}");
            this.resolver.hcoLookup[this.entityUri] = HypermediaLookup.Ok(this.hco);
        }

        public HypermediaResolverMockEntityBuilder<THco> SetupProperties(
            Action<THco> setup)
        {
            setup(this.hco);
            return this;
        }

        public HypermediaResolverMockEntityBuilder<THco> Respond(
            HypermediaProblem problem)
        {
            this.resolver.hcoLookup[this.entityUri] = HypermediaLookup.Error(problem);
            return this;
        }

        public HypermediaResolverMockEntityBuilder<THco> Setup<TNested>(
            Expression<Func<THco, HypermediaLink<TNested>>> link,
            Action<HypermediaResolverMockEntityBuilder<TNested>> configure,
            string? relativeUri = null)
            where TNested : HypermediaClientObject, new()
        {
            var property = ParseExpression(link);
            var builder = this.resolver.Entity<TNested>(relativeUri);
            configure(builder);
            property.SetValue(this.hco, builder.ToLink());
            return this;
        }
        
        public HypermediaResolverMockEntityBuilder<THco> Setup(
            Expression<Func<THco, IHypermediaClientAction?>> action,
            Action<HypermediaResolverActionMockBuilder> configure)
        {
            var property = ParseExpression(action);
            var builder = new HypermediaResolverActionMockBuilder(
                this.resolver,
                new Uri(baseUri, $"/api/action/{UriName<THco>()}/{property.Name}"));
            configure(builder);
            property.SetValue(this.hco, builder);
            return this;
        }

        public HypermediaResolverMockEntityBuilder<THco> Setup<TParameters>(
            Expression<Func<THco, IHypermediaClientAction<TParameters>?>> action,
            Action<HypermediaResolverActionMockBuilder<TParameters>> configure)
        {
            var property = ParseExpression(action);
            var builder = new HypermediaResolverActionMockBuilder<TParameters>(
                this.resolver,
                new Uri(baseUri, $"/api/action/{UriName<THco>()}/{property.Name}"));
            configure(builder);
            property.SetValue(this.hco, builder);
            return this;
        }

        public HypermediaResolverMockEntityBuilder<THco> Setup<TResultHco>(
            Expression<Func<THco, IHypermediaClientFunction<TResultHco>?>> function,
            Action<HypermediaResolverFunctionMockBuilder<TResultHco>> configure)
            where TResultHco : HypermediaClientObject, new()
        {
            var property = ParseExpression(function);
            var builder = new HypermediaResolverFunctionMockBuilder<TResultHco>(
                this.resolver,
                new Uri(baseUri, $"/api/action/{UriName<THco>()}/{property.Name}"));
            configure(builder);
            property.SetValue(this.hco, builder);
            return this;
        }

        public HypermediaResolverMockEntityBuilder<THco> Setup<TResultHco, TParameters>(
            Expression<Func<THco, IHypermediaClientFunction<TResultHco, TParameters>?>> function,
            Action<HypermediaResolverFunctionMockBuilder<TResultHco, TParameters>> configure)
            where TResultHco : HypermediaClientObject, new()
        {
            var property = ParseExpression(function);
            var builder = new HypermediaResolverFunctionMockBuilder<TResultHco, TParameters>(
                this.resolver,
                new Uri(baseUri, $"/api/action/{UriName<THco>()}/{property.Name}"));
            configure(builder);
            property.SetValue(this.hco, builder);
            return this;
        }

        public MandatoryHypermediaLink<THco> ToLink() => new MandatoryHypermediaLink<THco>()
        {
            Resolver = resolver,
            Uri = this.entityUri,
        };
    }

    public abstract class CommandMockBuilderBase : IHypermediaClientCommand
    {
        protected HypermediaResolverMock Resolver { get; }
        protected Uri Uri { get; }

        protected CommandMockBuilderBase(
            HypermediaResolverMock resolver,
            Uri uri,
            Action<IHypermediaClientCommand> setup)
        {
            this.Resolver = resolver;
            this.Uri = uri;
            setup(this);
        }

        string IHypermediaClientCommand.Name { get; set; } = "";
        string IHypermediaClientCommand.Title { get; set; } = "";
        string IHypermediaClientCommand.Method { get; set; } = "POST";
        string IHypermediaClientCommand.Type { get; set; } = "";

        Uri IHypermediaClientCommand.Uri
        {
            get => this.Uri;
            set { }
        }

        bool IHypermediaClientCommand.CanExecute { get; set; } = true;
        bool IHypermediaClientCommand.HasResultLink { get; set; }
        bool IHypermediaClientCommand.HasParameters { get; set; }
        IReadOnlyList<ParameterDescription> IHypermediaClientCommand.ParameterDescriptions { get; set; } = [];

        IHypermediaResolver IHypermediaClientCommand.Resolver
        {
            get => this.Resolver;
            set { }
        }
    }

    public class HypermediaResolverActionMockBuilder(HypermediaResolverMock resolver, Uri uri)
        : CommandMockBuilderBase(
                resolver,
                uri,
                _ => { }),
            IHypermediaClientAction
    {
        public HypermediaResolverActionMockBuilder RespondOk()
        {
            this.Resolver.actionLookup[this.Uri] = HypermediaLookup.Ok(No.Thing);
            return this;
        }
        
        public HypermediaResolverActionMockBuilder Respond(HypermediaProblem problem)
        {
            this.Resolver.actionLookup[this.Uri] = HypermediaLookup.Error(problem);
            return this;
        }
    }

    public class HypermediaResolverActionMockBuilder<TParameters>(HypermediaResolverMock resolver, Uri uri)
        : CommandMockBuilderBase(
                resolver,
                uri,
                command => { command.HasParameters = true; }),
            IHypermediaClientAction<TParameters>
    {
        public HypermediaResolverActionMockBuilder<TParameters> RespondOk(TParameters parameters)
        {
            this.Resolver.actionParameterLookup[(this.Uri, parameters)] = HypermediaLookup.Ok(No.Thing);
            return this;
        }
        
        public HypermediaResolverActionMockBuilder<TParameters> Respond(TParameters parameters, HypermediaProblem problem)
        {
            this.Resolver.actionParameterLookup[(this.Uri, parameters)] = HypermediaLookup.Error(problem);
            return this;
        }
    }

    public class HypermediaResolverFunctionMockBuilder<THco>(HypermediaResolverMock resolver, Uri uri)
        : CommandMockBuilderBase(
                resolver,
                uri,
                command => { command.HasResultLink = true; }),
            IHypermediaClientFunction<THco>
        where THco : HypermediaClientObject, new()
    {
        public HypermediaResolverFunctionMockBuilder<THco> Respond(
            Action<HypermediaResolverMockEntityBuilder<THco>> configure,
            string? relativeUri = null)
        {
            var builder = this.Resolver.Entity<THco>(relativeUri);
            configure(builder);
            this.Resolver.functionLookup[this.Uri] = HypermediaLookup.Ok(builder.ToLink());
            return this;
        }

        public HypermediaResolverFunctionMockBuilder<THco> Respond(HypermediaProblem problem)
        {
            this.Resolver.functionLookup[this.Uri] = HypermediaLookup.Error(problem);
            return this;
        }
    }

    public class HypermediaResolverFunctionMockBuilder<THco, TParameters>(HypermediaResolverMock resolver, Uri uri)
        : CommandMockBuilderBase(
                resolver,
                uri,
                command =>
                {
                    command.HasParameters = true;
                    command.HasResultLink = true;
                }),
            IHypermediaClientFunction<THco, TParameters>
        where THco : HypermediaClientObject, new()
    {
        public HypermediaResolverFunctionMockBuilder<THco, TParameters> Respond(
            TParameters parameters,
            Action<HypermediaResolverMockEntityBuilder<THco>> configure,
            string? relativeUri = null)
        {
            var result = this.Resolver.Entity<THco>(relativeUri);
            configure(result);
            this.Resolver.functionParameterLookup[(this.Uri, parameters)] = HypermediaLookup.Ok(result.ToLink());
            return this;
        }

        public HypermediaResolverFunctionMockBuilder<THco, TParameters> Respond(
            TParameters parameters,
            HypermediaProblem problem)
        {
            this.Resolver.functionParameterLookup[(this.Uri, parameters)] = HypermediaLookup.Error(problem);
            return this;
        }
    }
}