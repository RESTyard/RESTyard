using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace RESTyard.AspNetCore.Query
{
    public class QueryStringBuilder : IQueryStringBuilder
    {
        public string CreateQueryString(object? sourceObject, string objectPrefix = "")
        {
            var queryString = CreateQueryStingInternal(sourceObject, objectPrefix);

            var trimedQueryString = queryString.TrimEnd('&');
            if (string.IsNullOrEmpty(trimedQueryString))
            {
                return string.Empty;
            }

            return "?" + trimedQueryString;
        }

        private string CreateQueryStingInternal(object? sourceObject, string objectPrefix)
        {
            if (sourceObject == null)
            {
                return string.Empty;
            }

            var properties = sourceObject.GetType().GetTypeInfo().GetProperties()
                .Where(x => x.CanRead)
                .Where(x => x.GetValue(sourceObject, null) != null);

            var result = string.Empty;
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.GetMethod?.IsStatic ?? false)
                {
                    continue;
                }
                var propertyValue = propertyInfo.GetValue(sourceObject);
                var propertyType = propertyInfo.PropertyType;
                var propertyTypeTypeInfo = propertyType.GetTypeInfo();
                var serializeInfo = new SerializeInfo(propertyType, propertyTypeTypeInfo);

                if (propertyValue is null)
                {
                    continue;
                }

                if (serializeInfo.IsString)
                {
                    result += CreateKeyValue($"{objectPrefix}{propertyInfo.Name}", propertyValue);
                    continue;
                }

                if (serializeInfo.IsIEnumerable)
                {
                    result += SerializeIEnumerable(objectPrefix, (IEnumerable)propertyValue, propertyInfo.Name);
                    continue;
                }

                if (serializeInfo.IsClass || serializeInfo.IsStructWithNesting)
                {
                    result += CreateQueryStingInternal(propertyValue, $"{objectPrefix}{propertyInfo.Name}.");
                    continue;
                }

                if (IsDefaultValue(propertyValue, propertyType))
                {
                    continue;
                }

                result += CreateKeyValue($"{objectPrefix}{propertyInfo.Name}", propertyValue);
            }

            return result;
        }

        private string SerializeIEnumerable(string objectPrefix, IEnumerable enumerable, string propertyPath)
        {
            var result = string.Empty;

            SerializeInfo? serializeInfo = null;
            var enumIndex = 0;
            foreach (var item in enumerable)
            {
                if (item is null)
                {
                    continue;
                }
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

        private static bool IsDefaultValue(object? propertyValue, Type propertyType)
        {
            bool isDefault;

            var defaultValue = Activator.CreateInstance(propertyType);
            if (defaultValue == null)
            {
                isDefault = propertyValue == null;
            }
            else
            {
                isDefault = defaultValue.Equals(propertyValue);
            }
            return isDefault;
        }

        private static string ToInvariantString(object? obj)
        {
            return obj is IConvertible convertible ? convertible.ToString(CultureInfo.InvariantCulture)
                 : obj is IFormattable formattable ? formattable.ToString(null, CultureInfo.InvariantCulture)
                 : obj?.ToString() ?? string.Empty;
        }
    }
}
