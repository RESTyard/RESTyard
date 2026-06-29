using Microsoft.AspNetCore.Mvc.ModelBinding;
using RESTyard.AspNetCore.Hypermedia.Actions;
using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using FunicularSwitch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using RESTyard.AspNetCore.Util;

namespace RESTyard.AspNetCore.JsonSchema;

public class HypermediaParameterFromFormBinderProvider : IModelBinderProvider
{
    private readonly bool explicitUsage;

    public HypermediaParameterFromFormBinderProvider(bool explicitUsage = false)
    {
        this.explicitUsage = explicitUsage;
    }

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var modelType = context.Metadata.ModelType;
        if (ParameterIsHypermediaFileUploadActionType(modelType)
            && (ThisBinderIsSelectedOnMethod(context) || this.UseThisBinderImplicit(context)))
        {
            return new HypermediaParameterFromFormBinder(modelType);
        }

        return null;
    }

    private bool UseThisBinderImplicit(ModelBinderProviderContext context)
    {
        return !this.explicitUsage
               && context.BindingInfo.BinderType == null
               && DataIsInTheBodyOrNull(context);
    }

    private static bool DataIsInTheBodyOrNull(ModelBinderProviderContext context)
    {
        return (context.BindingInfo.BindingSource == null || context.BindingInfo.BindingSource == BindingSource.Body);
    }

    private static bool ThisBinderIsSelectedOnMethod(ModelBinderProviderContext context)
    {
        return context.BindingInfo.BinderType == typeof(HypermediaParameterFromFormBinder);
    }

    private static bool ParameterIsHypermediaFileUploadActionType(Type modelType)
    {
        return typeof(HypermediaFileUploadActionParameter).GetTypeInfo().IsAssignableFrom(modelType);
    }
}

public class HypermediaFileUploadActionParameter
{
    public IFormFileCollection Files { get; set; }
}

public class HypermediaFileUploadActionParameter<TParameters> : HypermediaFileUploadActionParameter
{
    public TParameters ParameterObject { get; set; }
}

public class HypermediaParameterFromFormBinder : IModelBinder
{
    private readonly Type wrapperModelType;
    private readonly Option<(Type ParameterModelType, JsonDeserializer ModelDeserializer)> parameterModelInfo;

    public HypermediaParameterFromFormBinder(Type modelType)
    {
        this.wrapperModelType = modelType;
        this.parameterModelInfo = modelType.GenericTypeArguments
            .FirstOrDefault()
            .ToOption()
            .Map(parameterModelType => (
                parameterModelType,
                new JsonDeserializer(parameterModelType)));
    }

    private Result<ModelBindingContext> CheckModelType(ModelBindingContext bindingContext)
    {
        if (bindingContext.ModelType == this.wrapperModelType)
        {
            return Result.Ok(bindingContext);
        }
        else
        {
            return Result.Error<ModelBindingContext>(
                $"ModelBinder does not match model type: '{this.wrapperModelType.BeautifulName()}' != '{bindingContext.ModelType}'");
        }
    }

    private Result<ModelBindingContext> CheckRequestMethod(ModelBindingContext bindingContext)
    {
        var requestMethod = bindingContext.ActionContext.HttpContext.Request.Method;
        if (requestMethod == HttpMethods.Post || requestMethod == HttpMethods.Patch || requestMethod == HttpMethods.Put)
        {
            return Result.Ok(bindingContext);
        }
        else
        {
            return Result.Error<ModelBindingContext>(
                $"Invalid http method {requestMethod} expected Post, Put or Patch");
        }
    }

    private Result<HttpRequest> CheckFormDataAndBoundary(HttpRequest request)
    {
        if (request.HasFormContentType
            && MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader)
            && !string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
        {
            return Result.Ok(request);
        }
        else
        {
            return Result.Error<HttpRequest>("File upload malformed. File upload must be form-data and have boundary");
        }
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var serializerOptions = ResolveSerializerOptions(bindingContext.HttpContext);

        this.CheckModelType(bindingContext)
            .Bind(this.CheckRequestMethod)
            .Map(bc => bc.HttpContext.Request)
            .Bind(this.CheckFormDataAndBoundary)
            .Bind(request => ExtractParameterObject(request).Map(node => (request, node)))
            .Bind(tuple => CreateResultObject(tuple.request, tuple.node, serializerOptions))
            .Match(
                ok =>
                {
                    bindingContext.Result = ModelBindingResult.Success(ok);
                },
                error =>
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, error);
                });
        return Task.CompletedTask;

        Result<JsonNode?> ExtractParameterObject(HttpRequest request)
        {
            return this.parameterModelInfo.Match(
                some =>
                {
                    var typeName = some.ParameterModelType.BeautifulName();
                    if (request.Form.TryGetValue(typeName, out var parameters))
                    {
                        JsonNode? parsed;
                        try
                        {
                            parsed = JsonNode.Parse(parameters.ToString());
                        }
                        catch (Exception e)
                        {
                            return Result.Error<JsonNode?>($"Invalid Json: {e.Message}");
                        }

                        // The plain JSON object is the supported wire format. The legacy Siren
                        // array-wrapper [{ "TypeName": {...} }] is still unwrapped here for
                        // backwards compatibility with clients using the (now obsolete) Single*
                        // parameter serializers.
                        if (parsed is JsonArray wrapperArray)
                        {
                            if (!TryUnwrapArray(wrapperArray, typeName, out var jObject))
                            {
                                return Result.Error<JsonNode?>(
                                    $"Invalid Json. Expected an object or an array containing one element with one object property '{typeName}'");
                            }

                            return Result.Ok<JsonNode?>(jObject);
                        }
                        else
                        {
                            return Result.Ok(parsed);
                        }
                    }
                    else
                    {
                        return Result.Error<JsonNode?>(
                            $"Method indicates additional parameters, but no {nameof(StringContent)} with key {nameof(HypermediaFileUploadActionParameter<Unit>.ParameterObject)} found in the form");
                    }
                },
                none: () => Result.Ok<JsonNode?>(null));
        }

        Result<HypermediaFileUploadActionParameter> CreateResultObject(HttpRequest request, JsonNode? node, JsonSerializerOptions options)
        {
            return Result.Try(
                () =>
                {
                    var resultObject = this.parameterModelInfo.Match(
                        some =>
                        {
                            var deserialized = some.ModelDeserializer.Deserialize(node, options);
                            var resultType =
                                typeof(HypermediaFileUploadActionParameter<>).MakeGenericType(some.ParameterModelType);
                            var result = (HypermediaFileUploadActionParameter)Activator.CreateInstance(resultType)!;
                            var parameterProperty =
                                resultType.GetProperty(
                                    nameof(HypermediaFileUploadActionParameter<Unit>.ParameterObject));
                            parameterProperty!.SetValue(result, deserialized);
                            return result;
                        },
                        none: () => new HypermediaFileUploadActionParameter());
                    resultObject.Files = request.Form.Files;
                    return resultObject;
                },
                e => $"Deserialization failed: {e}");
        }
    }

    private static JsonSerializerOptions ResolveSerializerOptions(HttpContext httpContext)
    {
        // Http.Json.JsonOptions (configured via ConfigureHttpJsonOptions) is the single source of
        // custom converters shared across MVC controllers and minimal APIs.
        var options = httpContext.RequestServices
            .GetService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>()
            ?.Value.SerializerOptions;
        return options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    private static bool TryUnwrapArray(JsonArray wrapperArray, string modelTypeName, [NotNullWhen(true)] out JsonNode? jObject)
    {
        jObject = null;
        if (wrapperArray.Count != 1)
        {
            return false;
        }

        if (wrapperArray[0] is not JsonObject container)
        {
            return false;
        }

        jObject = container[modelTypeName];
        return jObject is not null;
    }
}
