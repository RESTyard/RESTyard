using System;
using System.ComponentModel;
using System.Globalization;
using RESTyard.AspNetCore.Exceptions;

namespace RESTyard.AspNetCore.Util.Enum
{

    // Enables WebApi to accept enum parameters which value is related to the EnumMemberAttribute.
    // With out a TypeConverter enums are converted by the enum name not the enum value.
    public class AttributedEnumTypeConverter<T> : TypeConverter where T : struct
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is not string stringValue)
            {
                throw new AttributedEnumTypeConverterException("Tried to convert value to enum which is not a string.");
            }

            try
            {
                return EnumHelper.GetEnumByAttributeValue<T>(stringValue);
            }
            catch (ArgumentException e)
            {
                throw new AttributedEnumTypeConverterException($"Could not convert value '{value}'", e);
            }
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType != typeof(string))
            {
                throw new AttributedEnumTypeConverterException("Tried to convert an enum to other type than string.");
            }

            var toString = value?.ToString();
            if (toString is null)
            {
                throw new ArgumentNullException(nameof(value), "Cannot convert null");
            }

            var enumValue = (T)System.Enum.Parse(typeof(T), toString);

            try
            {
                return EnumHelper.GetEnumMemberValue(enumValue);
            }
            catch (Exception e)
            {

                throw new AttributedEnumTypeConverterException($"Could not convert '{value}'", e);
            }
        }
    }
}