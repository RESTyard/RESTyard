using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RESTyard.WebApi.Extensions.WebApi
{
    /// <summary>
    /// Siren requires that a Post to an Action is an array of objects.
    /// To avoid wrapping parameter objects in arrays this helper will extract a single object form a Siren conform Post.
    /// </summary>
    public class SingleParameterBinder : ModelBinderAttribute
    {
        public SingleParameterBinder(Type modelType)
        {
            var generic = typeof(SingleParameterBinderInternal<>);
            Type[] typeArgs = { modelType };
            var singleParameterBinderInternalType = generic.MakeGenericType(typeArgs);
            this.BinderType = singleParameterBinderInternalType;
        }

        private class SingleParameterBinderInternal<T> : IModelBinder
        {
            public async Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (bindingContext.ModelType != typeof(T))
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"ModelBinder does not match model type '{typeof(T)}' != '{bindingContext.ModelType}'");
                    return;
                }

                var bodyStream = bindingContext.ActionContext.HttpContext.Request.Body;
                object rawDeserialized;

                var serializer = new JsonSerializer();
                using (var sr = new StreamReader(bodyStream))
                using (var stringReader = new StringReader(await sr.ReadToEndAsync()))
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    try
                    {
                        rawDeserialized = serializer.Deserialize(jsonTextReader);
                    }
                    catch (Exception)
                    {
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid Json.");
                        return;
                    }
                }

                if (!(rawDeserialized is JArray wrapperArray))
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Action field not wrapped in an Array.");
                    return;
                }

                if (wrapperArray.Count != 1)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Action field must contain one element.");
                    return;
                }


                var parameterObject = wrapperArray[0][typeof(T).Name];
                if (parameterObject == null)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Could not find property called '{typeof(T).Name}'");
                    return;
                }

                T result;
                try
                {
                    result = parameterObject.ToObject<T>();
                }
                catch (JsonReaderException)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Could not convert parameter object.");
                    return;
                }

                bindingContext.Result = ModelBindingResult.Success(result);
                return;
            }
        }
    }
}
