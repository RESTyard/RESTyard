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
        public static string GetEnumMemberValue<T>(T enumValue) where T : struct
        {
            var enumType = typeof(T);


            var fieldInfo = enumType.GetField(enumValue.ToString());
            if (fieldInfo == null)
            {
                throw new ArgumentException($"Enum '{enumType.Name}' has no value '{enumValue}'.");
            }

            var attributes = fieldInfo.GetCustomAttributes(typeof(EnumMemberAttribute), false).ToList();
            if (!attributes.Any())
            {
                throw new ArgumentException($"Enum '{enumType.Name}' is missing a EnumMemberAttribute for value '{enumValue}'.");
            }

            return ((EnumMemberAttribute)attributes.First()).Value;
        }


        // Maps a string to an enum value based on the EnumMemberAttribute. This attribute is required for all enum values.
        // "value":The string value which is compared to EnumMemberAttribute value.</param>
        // "T":The enum which is searched
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