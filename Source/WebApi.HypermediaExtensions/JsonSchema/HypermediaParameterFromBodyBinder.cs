using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Util;

namespace WebApi.HypermediaExtensions.JsonSchema
{
    class HypermediaParameterFromBodyBinderProvider : IModelBinderProvider
    {
        readonly Func<Type, ImmutableArray<string>> getRouteTemplateForType;
        readonly bool explicitUsage;

        public HypermediaParameterFromBodyBinderProvider(Func<Type, ImmutableArray<string>> getRouteTemplateForType, bool explicitUsage = false)
        {
            this.getRouteTemplateForType = getRouteTemplateForType;
            this.explicitUsage = explicitUsage;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            var modelType = context.Metadata.ModelType;
            if (ParameterIsHypermediaActionType(modelType) 
                && (ThisBinderIsSelectedOnMethod(context) || this.UseThisBinderImplicit(context)))
            {
                return new HypermediaParameterFromBodyBinder(modelType, getRouteTemplateForType);
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
            return context.BindingInfo.BinderType == typeof(HypermediaParameterFromBodyBinder);
        }

        private static bool ParameterIsHypermediaActionType(Type modelType)
        {
            return typeof(IHypermediaActionParameter).GetTypeInfo().IsAssignableFrom(modelType);
        }
    }

    class HypermediaParameterFromBodyBinder : IModelBinder
    {
        readonly Type modelType;
        readonly JsonDeserializer serializer;

        public HypermediaParameterFromBodyBinder(Type modelType, Func<Type, ImmutableArray<string>> getRouteTemplatesForType)
        {
            this.modelType = modelType;
            serializer = new JsonDeserializer(modelType, getRouteTemplatesForType);
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelTypeName = modelType.BeautifulName();
            if (bindingContext.ModelType != modelType)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"ModelBinder does not match model type '{modelTypeName}' != '{bindingContext.ModelType}'");
                return;
            }

            if (bindingContext.ActionContext.HttpContext.Request.Method != HttpMethods.Post && bindingContext.ActionContext.HttpContext.Request.Method != HttpMethods.Patch)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid http method {bindingContext.ActionContext.HttpContext.Request.Method} expected Post or Patch");
                return;
            }

            var bodyStream = bindingContext.ActionContext.HttpContext.Request.Body;
            object rawDeserialized;
            
            using (var sr = new StreamReader(bodyStream))
            using (var stringReader = new StringReader(await sr.ReadToEndAsync()))
            using (var jsonTextReader = new JsonTextReader(stringReader))
            {
                try
                {
                    rawDeserialized = new JsonSerializer().Deserialize(jsonTextReader);
                }
                catch (Exception e)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid Json: {e}.");
                    return;
                }
            }

            JObject jObject;
            if (rawDeserialized is JArray wrapperArray)
            {
                if (!TryUnwrapArray(wrapperArray, modelTypeName, out jObject))
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid Json. Expected an object or and array containing one element with one object property '{modelTypeName}'");
                    return;
                }
            }
            else
            {
                jObject = (JObject) rawDeserialized;
            }

            try
            {
                bindingContext.Result = ModelBindingResult.Success(serializer.Deserialize(jObject));
                return;
            }
            catch (Exception e)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Deserialization failed: {e}");
                return;
            }
        }

        static bool TryUnwrapArray(JArray wrapperArray, string modelTypeName, out JObject jObject)
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
}