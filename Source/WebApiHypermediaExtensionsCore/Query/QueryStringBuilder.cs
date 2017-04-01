using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace WebApiHypermediaExtensionsCore.Query
{
    public class QueryStringBuilder : IQueryStringBuilder
    {
        public string CreateQueryString(object sourceObject, string objectPrefix = "")
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
            if (sourceObject == null)
            {
                return string.Empty;
            }

            var properties = sourceObject.GetType().GetProperties()
                .Where(x => x.CanRead)
                .Where(x => x.GetValue(sourceObject, null) != null);

            var result = string.Empty;
            foreach (var propertyInfo in properties)
            {
                var propretyValue = propertyInfo.GetValue(sourceObject);
                var propertyType = propertyInfo.PropertyType;
                var propertyTypeTypeInfo = propertyType.GetTypeInfo();
                var serializeInfo = new SerializeInfo(propertyType, propertyTypeTypeInfo);

                if (serializeInfo.IsString)
                {
                    result += CreateKeyValue($"{objectPrefix}{propertyInfo.Name}", propretyValue);
                    continue;
                }

                if (serializeInfo.IsIEnumerable)
                {
                    result += SerializeIEnumerable(objectPrefix, (IEnumerable)propretyValue, propertyInfo.Name);
                    continue;
                }

                if (serializeInfo.IsClass || serializeInfo.IsStructWithNesting)
                {
                    result += CreateQueryStingInternal(propretyValue, $"{objectPrefix}{propertyInfo.Name}.");
                    continue;
                }

                if (IsDefaultValue(propretyValue, propertyType))
                {
                    continue;
                }

                result += CreateKeyValue($"{objectPrefix}{propertyInfo.Name}", propretyValue);
            }

            return result;
        }

        private string SerializeIEnumerable(string objectPrefix, IEnumerable enumerable, string propertyPath)
        {
            var result = string.Empty;

            SerializeInfo serializeInfo = null;
            var enumIndex = 0;
            foreach (var item in enumerable)
            {
                if (serializeInfo == null)
                {
                    serializeInfo = new SerializeInfo(item);
                }

                if (!serializeInfo.IsString && (serializeInfo.IsClass || serializeInfo.IsIEnumerable || serializeInfo.IsStructWithNesting))
                {
                    result += CreateQueryStingInternal(item, $"{objectPrefix}{propertyPath}[{enumIndex}].");
                }
                else
                {
                    result += CreateKeyValue($"{objectPrefix}{propertyPath}", item);
                }

                enumIndex++;
            }
            return result;
        }

        private static string CreateKeyValue(string propertyPath, object item)
        {
            var result = propertyPath
                         + "="
                         + Uri.EscapeDataString(ToInvariantString(item))
                         + "&";
            return result;
        }

        private static bool IsDefaultValue(object propretyValue, Type propertyType)
        {
            bool isDefault;

            var defaultValue = Activator.CreateInstance(propertyType);
            if (defaultValue == null)
            {
                isDefault = propretyValue == null;
            }
            else
            {
                isDefault = defaultValue.Equals(propretyValue);
            }
            return isDefault;
        }

        private static string ToInvariantString(object obj)
        {
            return obj is IConvertible ? ((IConvertible)obj).ToString(CultureInfo.InvariantCulture)
                 : obj is IFormattable ? ((IFormattable)obj).ToString(null, CultureInfo.InvariantCulture)
                 : obj.ToString();
        }
    }
}
