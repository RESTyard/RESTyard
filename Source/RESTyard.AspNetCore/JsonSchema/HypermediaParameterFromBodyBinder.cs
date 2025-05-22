using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Util;

namespace RESTyard.AspNetCore.JsonSchema
{
    class HypermediaParameterFromBodyBinderProvider : IModelBinderProvider
    {
        readonly bool explicitUsage;

        public HypermediaParameterFromBodyBinderProvider(bool explicitUsage = false)
        {
            this.explicitUsage = explicitUsage;
        }

        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            var modelType = context.Metadata.ModelType;
            if (ParameterIsHypermediaActionType(modelType) 
                && (ThisBinderIsSelectedOnMethod(context) || this.UseThisBinderImplicit(context)))
            {
                return new HypermediaParameterFromBodyBinder(modelType);
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

        public HypermediaParameterFromBodyBinder(Type modelType)
        {
            this.modelType = modelType;
            serializer = new JsonDeserializer(modelType);
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var isDevelopmentEnvironment = IsDevelopmentEnvironment(bindingContext);

            var modelTypeName = modelType.BeautifulName();
            if (bindingContext.ModelType != modelType)
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
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid Json: {(isDevelopmentEnvironment ? e : e.Message)}.");
                    return;
                }
            }

            JObject? jObject;
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
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Deserialization failed: {(isDevelopmentEnvironment ? e : e.Message)}");
                return;
            }
        }

        private static bool IsDevelopmentEnvironment(ModelBindingContext bindingContext)
        {
            var isDevelopmentEnvironment = false;
            var hostEnvironment = bindingContext.HttpContext.RequestServices.GetService<IHostEnvironment>();
            if (hostEnvironment != null)
            {
                isDevelopmentEnvironment = hostEnvironment.IsDevelopment();
            }

            return isDevelopmentEnvironment;
        }

        static bool TryUnwrapArray(JArray wrapperArray, string modelTypeName, [NotNullWhen(true)] out JObject? jObject)
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