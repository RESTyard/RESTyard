using System;
using System.Reflection;
using FunicularSwitch;
using WebApi.HypermediaExtensions.Util.Extensions;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class Property
    {

        public Property(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            SerializationName = propertyInfo.Name;
            GetPropertyValue = propertyInfo.GetValueGetter<object>();
        }
        public Property(PropertyInfo propertyInfo, string serializationName)
        {
            PropertyInfo = propertyInfo;
            SerializationName = serializationName;
            GetPropertyValue = propertyInfo.GetValueGetter<object>();
        }

        protected Func<object, object> GetPropertyValue { get; }

        public string SerializationName { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }

        public Result<object> GetValue(object hypermediaObject)
        {
           var value = GetPropertyValue.Invoke(hypermediaObject);
           return value != null ? Result.Ok(value) : Result.Error<object>("");
        }

    }
}