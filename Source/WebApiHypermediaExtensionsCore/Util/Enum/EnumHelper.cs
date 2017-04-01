using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace WebApiHypermediaExtensionsCore.Util.Enum
{
    public static class EnumHelper
    {

        // Returns the string representation of an enum taken from the EnumMember attribute.
        // "enumValue": The enum value for which the string representation should be looked up.
        /// <summary>
        /// Converts an enum value to the string representation the EnumMember attribute.
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="enumValue">Value of the enum</param>
        /// <returns>Attributed value if found otherwise enumValue as string</returns>
        public static string GetEnumMemberValue<T>(T enumValue) where T : struct
        {
            var enumType = typeof(T);
            return GetEnumMemberValue(enumType, enumValue);
        }

        // Returns the string representation of an enum taken from the EnumMember attribute.
        // "enumValue": The enum value for which the string representation should be looked up.
        /// <summary>
        /// Converts an enum value to the string representation the EnumMember attribute.
        /// </summary>
        /// <param name="enumType">Type of the enum</param>
        /// <param name="enumValue">Value of the enum</param>
        /// <returns>Attributed value if found otherwise enumValue as string</returns>
        public static string GetEnumMemberValue(Type enumType, object enumValue)
        {
            
            var enumvalueAsString = enumValue.ToString();
            var fieldInfo = enumType.GetField(enumvalueAsString);
            if (fieldInfo == null)
            {
                throw new ArgumentException($"Enum '{enumType.Name}' has no value '{enumValue}'.");
            }

            var attributes = fieldInfo.GetCustomAttributes(typeof(EnumMemberAttribute), false).ToList();
            if (attributes.Any())
            {
                return ((EnumMemberAttribute)attributes.First()).Value;
            }

            return enumvalueAsString;
        }

        /// <summary>
        /// Maps a string to an enum value based on the EnumMemberAttribute. This attribute is required for all enum values.
        /// </summary>
        /// <typeparam name="T">The enum which is searched</typeparam>
        /// <param name="value">The string value which is compared to EnumMemberAttribute value.</param>
        /// <returns></returns>
        public static object GetEnumByAttributeValue<T>(string value) where T : struct
        {
            var enumType = typeof(T);
            if (!enumType.GetTypeInfo().IsEnum)
            {
                throw new ArgumentException($"Given value '{value}' is not a enum value.");
            }

            var enumValues = System.Enum.GetValues(typeof(T)).Cast<T>();
            foreach (var enumValue in enumValues)
            {
                var enumMemberValue = GetEnumMemberValue(enumValue);

                if (value.Equals(enumMemberValue))
                {
                    return enumValue;
                }
            }

            throw new ArgumentException($"Enum value '{value}' could not be mapped to EnumMemberAttribute of enum '{enumType.Name}'.");
        }
    }
}