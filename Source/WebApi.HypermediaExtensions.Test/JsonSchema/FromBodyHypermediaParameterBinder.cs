using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApi.HypermediaExtensions.Util;

namespace WebApi.HypermediaExtensions.Test.JsonSchema
{
    public class FromBodyHypermediaParameterBinder : IModelBinder
    {
        readonly Type modelType;
        readonly JsonDeserializer serializer;

        public FromBodyHypermediaParameterBinder(Type modelType, Func<Type, string> getRouteTemplateForType)
        {
            this.modelType = modelType;
            serializer = new JsonDeserializer(modelType, getRouteTemplateForType);
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelTypeName = modelType.BeautifulName();
            if (bindingContext.ModelType != modelType)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"ModelBinder does not match model type '{modelTypeName}' != '{bindingContext.ModelType}'");
                return Task.FromResult(false);
            }

            var bodyStream = bindingContext.ActionContext.HttpContext.Request.Body;
            object rawDeserialized;
            
            using (var sr = new StreamReader(bodyStream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                try
                {
                    rawDeserialized = new JsonSerializer().Deserialize(jsonTextReader);
                }
                catch (Exception e)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid Json: {e}.");
                    return Task.FromResult(false);
                }
            }

            JObject jObject;
            if (rawDeserialized is JArray wrapperArray)
            {
                if (!TryUnwrapArray(wrapperArray, modelTypeName, out jObject))
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid Json. Expected an object or and array containing one element with one object property '{modelTypeName}'");
                    return Task.FromResult(false);
                }
            }
            else
            {
                jObject = (JObject) rawDeserialized;
            }

            try
            {
                bindingContext.Result = ModelBindingResult.Success(serializer.Deserialize(jObject));
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Deserialization failed: {e}");
                return Task.FromResult(false);
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