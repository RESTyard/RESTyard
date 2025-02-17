using Microsoft.AspNetCore.Mvc.ModelBinding;
using RESTyard.AspNetCore.Hypermedia.Actions;
using System.Collections.Immutable;
using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FunicularSwitch;
using Microsoft.Net.Http.Headers;
using RESTyard.AspNetCore.Util;

namespace RESTyard.AspNetCore.JsonSchema;

public class HypermediaParameterFromFormBinderProvider : IModelBinderProvider
{
    private readonly Func<Type, ImmutableArray<string>> getRouteTemplateForType;
    private readonly bool explicitUsage;

    public HypermediaParameterFromFormBinderProvider(Func<Type, ImmutableArray<string>> getRouteTemplateForType, bool explicitUsage = false)
    {
        this.getRouteTemplateForType = getRouteTemplateForType;
        this.explicitUsage = explicitUsage;
    }

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var modelType = context.Metadata.ModelType;
        if (ParameterIsHypermediaFileUploadActionType(modelType)
            && (ThisBinderIsSelectedOnMethod(context) || this.UseThisBinderImplicit(context)))
        {
            return new HypermediaParameterFromFormBinder(modelType, getRouteTemplateForType);
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

    public HypermediaParameterFromFormBinder(Type modelType, Func<Type, ImmutableArray<string>> getRouteTemplatesForType)
    {
        this.wrapperModelType = modelType;
        this.parameterModelInfo = modelType.GenericTypeArguments
            .FirstOrDefault()
            .ToOption()
            .Map(parameterModelType => (
                parameterModelType,
                new JsonDeserializer(parameterModelType, getRouteTemplatesForType)));
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
        this.CheckModelType(bindingContext)
            .Bind(this.CheckRequestMethod)
            .Map(bc => bc.HttpContext.Request)
            .Bind(this.CheckFormDataAndBoundary)
            .Bind(request => ExtractParameterObject(request).Map(jObject => (request, jObject)))
            .Bind(tuple => CreateResultObject(tuple.request, tuple.jObject))
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

        Result<JObject?> ExtractParameterObject(HttpRequest request)
        {
            return this.parameterModelInfo.Match(
                some =>
                {
                    var typeName = some.ParameterModelType.BeautifulName();
                    if (request.Form.TryGetValue(typeName, out var parameters))
                    {
                        var rawDeserialized = JsonConvert.DeserializeObject(parameters);

                        if (rawDeserialized is JArray wrapperArray)
                        {
                            if (!TryUnwrapArray(wrapperArray, typeName, out var jObject))
                            {
                                return Result.Error<JObject?>(
                                    $"Invalid Json. Expected an object or and array containing one element with one object property '{typeName}'");
                            }

                            return Result.Ok<JObject?>(jObject);
                        }
                        else
                        {
                            return Result.Ok((JObject?)rawDeserialized);
                        }
                    }
                    else
                    {
                        return Result.Error<JObject?>(
                            $"Method indicates additional parameters, but no {nameof(StringContent)} with key {nameof(HypermediaFileUploadActionParameter<Unit>.ParameterObject)} found in the form");
                    }
                },
                none: () => Result.Ok<JObject?>(null));
        }
        
        Result<HypermediaFileUploadActionParameter> CreateResultObject(HttpRequest request, JObject? jObject)
        {
            return Result.Try(
                () =>
                {
                    var resultObject = this.parameterModelInfo.Match(
                        some =>
                        {
                            var deserialized = some.ModelDeserializer.Deserialize(jObject);
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

    private static bool TryUnwrapArray(JArray wrapperArray, string modelTypeName, [NotNullWhen(true)] out JObject? jObject)
    {
        if (wrapperArray.Count != 1)
        {
            jObject = null;
            return false;
        }

        jObject = wrapperArray[0][modelTypeName] as JObject;
        if (jObject == null)
        {
            jObject = null;
            return false;
        }

        return true;
    }
}