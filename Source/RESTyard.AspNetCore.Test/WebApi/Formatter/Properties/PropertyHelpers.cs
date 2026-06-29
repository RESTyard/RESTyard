using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.AspNetCore.Test.Helpers;
using RESTyard.AspNetCore.Util.Enum;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter.Properties
{
    public class PropertyHelpers
    {
        public static JsonObject GetPropertiesJObject(JsonObject siren)
        {
            Assert.IsTrue(siren["properties"] is JsonObject);
            var propertiesObject = siren["properties"]!.AsObject();
            return propertiesObject;
        }

        public static void CompareHypermediaPropertiesAndJson(JsonObject propertiesObject, PropertyHypermediaObject ho)
        {
            var propertyInfos = ho.GetType().GetProperties()
                .Where(p => p.Name != "Entities" && p.Name != "Links")
                .ToList();
            Assert.AreEqual(propertiesObject.Count, propertyInfos.Count);

            CompareNotNullProperties(propertiesObject, ho);

            Assert.AreEqual(ho.AnUri?.ToString(), propertiesObject[nameof(PropertyHypermediaObject.AnUri)]?.GetValue<string>());
            Assert.AreEqual(ho.AType?.FullName, propertiesObject[nameof(PropertyHypermediaObject.AType)]?.GetValue<string>());
            Assert.AreEqual(ho.AString, propertiesObject[nameof(PropertyHypermediaObject.AString)]?.GetValue<string>());
            Assert.AreEqual(ho.ANullableInt, propertiesObject[nameof(PropertyHypermediaObject.ANullableInt)]?.GetValue<int>());
            Assert.AreEqual(ho.ANullableEnum?.ToString(), propertiesObject[nameof(PropertyHypermediaObject.ANullableEnum)]?.GetValue<string>());

        }

        public static void CompareNotNullProperties(JsonObject propertiesObject, PropertyHypermediaObject ho)
        {
            Assert.AreEqual(ho.ABool, propertiesObject[nameof(PropertyHypermediaObject.ABool)]!.GetValue<bool>());

            Assert.AreEqual(ho.AnInt, propertiesObject[nameof(PropertyHypermediaObject.AnInt)]!.GetValue<int>());
            Assert.AreEqual(ho.ALong, propertiesObject[nameof(PropertyHypermediaObject.ALong)]!.GetValue<long>());
            Assert.AreEqual(ho.AFloat, propertiesObject[nameof(PropertyHypermediaObject.AFloat)]!.GetValue<float>());
            Assert.AreEqual(ho.ADouble, propertiesObject[nameof(PropertyHypermediaObject.ADouble)]!.GetValue<double>());

            Assert.AreEqual(EnumHelper.GetEnumMemberValue(ho.AnEnum), propertiesObject[nameof(PropertyHypermediaObject.AnEnum)]!.GetValue<string>());
            Assert.AreEqual(EnumHelper.GetEnumMemberValue(ho.AnEnumWithNames), propertiesObject[nameof(PropertyHypermediaObject.AnEnumWithNames)]!.GetValue<string>());

            Assert.AreEqual(ho.ADateTime, propertiesObject[nameof(PropertyHypermediaObject.ADateTime)]!.GetValue<DateTime>());
            Assert.AreEqual(ho.ADateTimeOffset, propertiesObject[nameof(PropertyHypermediaObject.ADateTimeOffset)]!.GetValue<DateTimeOffset>());
            // System.Text.Json serializes TimeSpan as a string and GetValue<TimeSpan>() cannot convert it back, so compare the string form.
            Assert.AreEqual(ho.ATimeSpan.ToInvariantString(), propertiesObject[nameof(PropertyHypermediaObject.ATimeSpan)]!.GetValue<string>());
            Assert.AreEqual(ho.ADecimal, propertiesObject[nameof(PropertyHypermediaObject.ADecimal)]!.GetValue<decimal>());
        }

        public static void CompareHypermediaPropertiesAndJsonNoNullProperties(JsonObject propertiesObject, PropertyHypermediaObject ho)
        {
            var propertyInfos = typeof(PropertyHypermediaObject).GetProperties()
                .Where(p => p.Name != "Entities" && p.Name != "Links")
                .ToList();
            Assert.AreEqual(propertiesObject.Count, propertyInfos.Count - 5);

            CompareNotNullProperties(propertiesObject, ho);

            Assert.IsNull(propertiesObject[nameof(PropertyHypermediaObject.AnUri)]);
            Assert.IsNull(propertiesObject[nameof(PropertyHypermediaObject.AType)]);
            Assert.IsNull(propertiesObject[nameof(PropertyHypermediaObject.AString)]);
            Assert.IsNull(propertiesObject[nameof(PropertyHypermediaObject.ANullableInt)]);
            Assert.IsNull(propertiesObject[nameof(PropertyHypermediaObject.ANullableEnum)]);
        }

        public static void CompareHypermediaListPropertiesAndJson(JsonObject propertiesObject, HypermediaObjectWithListProperties ho)
        {
            var propertyInfos = ho.GetType().GetProperties()
                .Where(p => p.Name != "Entities" && p.Name != "Links")
                .ToList();
            Assert.AreEqual(propertiesObject.Count, propertyInfos.Count);


            foreach (var property in propertiesObject)
            {
                var htoProperty = propertyInfos.Single(p => p.Name == property.Key);
                var hoValue = (IEnumerable)htoProperty.GetValue(ho);
                if (hoValue == null) {
                    Assert.IsNull(property.Value);
                }
                else
                {
                    Assert.IsTrue(property.Value is JsonArray);
                    var jarray = property.Value!.AsArray();


                    var index = 0;
                    foreach (var value in hoValue)
                    {
                        if (value == null)
                        {
                            Assert.IsNull(jarray[index]);
                        }
                        else
                        {
                            var valueType = value.GetType();
                            var valueTypeInfo = valueType.GetTypeInfo();
                            if (IsNestedList(valueTypeInfo, valueType))
                            {
                                Assert.AreEqual(value.ToString(), jarray[index]!.ToString());
                            }
                        }

                        index++;
                    }

                    // no extra items
                    Assert.AreEqual(index, jarray.Count);
                }
            }
        }

        private static bool IsNestedList(TypeInfo valueTypeInfo, Type valueType)
        {
            return !valueTypeInfo.IsClass || valueType == typeof(string);
        }
    }
}
