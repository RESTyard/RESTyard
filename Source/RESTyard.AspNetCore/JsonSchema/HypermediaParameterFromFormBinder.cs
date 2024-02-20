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
        if (ParameterIsHypermediaActionType(modelType)
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

    private static bool ParameterIsHypermediaActionType(Type modelType)
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
    private readonly Option<Type> genericModelType;
    private readonly Option<JsonDeserializer> serializer;

    public HypermediaParameterFromFormBinder(Type modelType, Func<Type, ImmutableArray<string>> getRouteTemplatesForType)
    {
        this.wrapperModelType = modelType;
        this.genericModelType = modelType.GenericTypeArguments.FirstOrDefault().ToOption();
        this.serializer = this.genericModelType.Map(gmt => new JsonDeserializer(gmt, getRouteTemplatesForType));
    }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var modelTypeName = wrapperModelType.BeautifulName();
        if (bindingContext.ModelType != wrapperModelType)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"ModelBinder does not match model type '{modelTypeName}' != '{bindingContext.ModelType}'");
            return;
        }

        var requestMethod = bindingContext.ActionContext.HttpContext.Request.Method;
        if (requestMethod != HttpMethods.Post && requestMethod != HttpMethods.Patch && requestMethod != HttpMethods.Put)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid http method {requestMethod} expected Post, Put or Patch");
            return;
        }

        var request = bindingContext.ActionContext.HttpContext.Request;
        if (!request.HasFormContentType
            || !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader)
            || string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "File upload malformed. File upload must be form-data and have boundary");
            return;
        }

        var (deserializeParameter, gmt, s) = this.genericModelType.Match(
            some: gmt => this.serializer.Match(
                some: s => (true, gmt, s),
                none: () => (false, gmt, null!)),
            none: () => (false, null!, null!));
        JObject? jObject = null;
        if (deserializeParameter)
        {
            if (request.Form.TryGetValue(nameof(HypermediaFileUploadActionParameter<Unit>.ParameterObject), out var parameters))
            {
                var rawDeserialized = JsonConvert.DeserializeObject(parameters);

                if (rawDeserialized is JArray wrapperArray)
                {
                    if (!TryUnwrapArray(wrapperArray, nameof(HypermediaFileUploadActionParameter<Unit>.ParameterObject), out jObject))
                    {
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid Json. Expected an object or and array containing one element with one object property '{modelTypeName}'");
                        return;
                    }
                }
                else
                {
                    jObject = (JObject)rawDeserialized;
                }
            }
            else
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Method indicates additional parameters, but no {nameof(StringContent)} with key {nameof(HypermediaFileUploadActionParameter<Unit>.ParameterObject)} found in the form");
                return;
            }
        }

        try
        {
            HypermediaFileUploadActionParameter resultObject;
            if (deserializeParameter)
            {
                var deserialized = s.Deserialize(jObject);
                var resultType = typeof(HypermediaFileUploadActionParameter<>).MakeGenericType(gmt);
                resultObject = (HypermediaFileUploadActionParameter)Activator.CreateInstance(resultType);
                var parameterProperty =
                    resultType.GetProperty(nameof(HypermediaFileUploadActionParameter<Unit>.ParameterObject));
                parameterProperty.SetValue(resultObject, deserialized);
            }
            else
            {
                resultObject = new HypermediaFileUploadActionParameter();
            }
            resultObject.Files = request.Form.Files;
            bindingContext.Result = ModelBindingResult.Success(resultObject);
            return;
        }
        catch (Exception e)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Deserialization failed: {e}");
            return;
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