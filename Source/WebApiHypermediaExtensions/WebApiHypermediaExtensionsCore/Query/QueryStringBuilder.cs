using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace WebApiHypermediaExtensionsCore.Query
{
    public class QueryStringBuilder : IQueryStringBuilder
    {
        public  string CreateQueryString(object sourceObject, string objectPrefix = "")
        {
            var queryString = CreateQueryStingInternal(sourceObject, objectPrefix);

            var trimedQueryString = queryString.TrimEnd('&');
            if (string.IsNullOrEmpty(trimedQueryString))
            {
                return string.Empty;
            }

            return "?" + trimedQueryString;
        }

        private string CreateQueryStingInternal(object sourceObject, string objectPrefix)
        {
            var properties = sourceObject.GetType().GetProperties()
                .Where(x => x.CanRead)
                .Where(x => x.GetValue(sourceObject, null) != null);

            var result = string.Empty;
            foreach (var propertyInfo in properties)
            {
                var propertyType = propertyInfo.PropertyType;
                var propertyTypeTypeInfo = propertyType.GetTypeInfo();
                var propretyValue = propertyInfo.GetValue(sourceObject);

                var isClass = propertyTypeTypeInfo.IsClass;
                if (isClass)
                {
                    result += CreateQueryStingInternal(propretyValue, objectPrefix + propertyInfo.Name + ".");
                }

                var isPrimitive = propertyTypeTypeInfo.IsPrimitive;
                var isValueType = propertyTypeTypeInfo.IsValueType;
                var isEnum = propertyTypeTypeInfo.IsEnum;
                var isString = propertyType == typeof(string);
                var isDateTime = propertyType == typeof(DateTime);
                var isTimeSpan = propertyType == typeof(TimeSpan);
                var isDecimal = propertyType == typeof(decimal);
                if (isPrimitive || isEnum || isString || isValueType || isDateTime || isDecimal || isTimeSpan)
                {
                    var defaultValue = Activator.CreateInstance(propertyType);
                    if ( (defaultValue == null && propretyValue != null) || !defaultValue.Equals(propretyValue)) // do not write default values
                    {
                        result += Uri.EscapeDataString(objectPrefix + propertyInfo.Name) + "=" +
                              Uri.EscapeDataString(propretyValue.ToString()) + "&";
                    }
                    
                }

                var isIEnumerable = typeof(IEnumerable).IsAssignableFrom(propertyType);
                if (isIEnumerable)
                {
                    throw new ArgumentException($"{typeof(QueryStringBuilder)} can not process IEnumarables");
                }
            }

            return result;
        }
    }
}
