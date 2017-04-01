using System;
using System.Collections;
using System.Reflection;

namespace WebApiHypermediaExtensionsCore.Query
{
    internal class SerializeInfo
    {
        public bool IsClass { get; }
        public bool IsString { get; }
        public bool IsIEnumerable { get; }
        public bool IsStructWithNesting { get; }

        public SerializeInfo(object obj)
        {
            var itemType = obj.GetType();
            var itemTypeInfo = itemType.GetTypeInfo();

            IsClass = itemTypeInfo.IsClass;
            IsString = itemType == typeof(string);
            IsIEnumerable = typeof(IEnumerable).IsAssignableFrom(itemType);
            IsStructWithNesting = CheckStructWithNesting(itemType, itemTypeInfo);
        }

        public SerializeInfo(Type type, TypeInfo propertyInfo)
        {
            IsClass = propertyInfo.IsClass;
            IsString = type == typeof(string);
            IsIEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
            IsStructWithNesting = CheckStructWithNesting(type, propertyInfo);
        }

        private static bool CheckStructWithNesting(Type type, TypeInfo propertyTypeTypeInfo)
        {
            return propertyTypeTypeInfo.IsValueType
                   && !propertyTypeTypeInfo.IsEnum
                   && !propertyTypeTypeInfo.IsPrimitive
                   && type != typeof(decimal)
                   && Nullable.GetUnderlyingType(type) == null // check for nullable
                   && type != typeof(DateTime)
                   && type != typeof(DateTimeOffset)
                   && type != typeof(TimeSpan);
        }
    }
}