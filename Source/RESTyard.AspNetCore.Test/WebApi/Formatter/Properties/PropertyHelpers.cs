using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Test.Helpers;
using RESTyard.AspNetCore.Util.Enum;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter.Properties
{
    public class PropertyHelpers
    {
        public static JObject GetPropertiesJObject(JObject siren)
        {
            Assert.IsTrue(siren["properties"].Type == JTokenType.Object);
            var propertiesObject = (JObject)siren["properties"];
            return propertiesObject;
        }

        public static void CompareHypermediaPropertiesAndJson(JObject propertiesObject, PropertyHypermediaObject ho)
        {
            var propertyInfos = ho.GetType().GetProperties()
                .Where(p => p.Name != "Entities" && p.Name != "Links")
                .ToList();
            Assert.AreEqual(propertiesObject.Properties().Count(), propertyInfos.Count);

            CompareNotNullProperties(propertiesObject, ho);

            Assert.AreEqual(ho.AnUri?.ToString(), propertiesObject[nameof(PropertyHypermediaObject.AnUri)].Value<string>());
            Assert.AreEqual(ho.AType?.FullName, propertiesObject[nameof(PropertyHypermediaObject.AType)].Value<string>());
            Assert.AreEqual(ho.AString, propertiesObject[nameof(PropertyHypermediaObject.AString)].Value<string>());
            Assert.AreEqual(ho.ANullableInt, propertiesObject[nameof(PropertyHypermediaObject.ANullableInt)]);
            Assert.AreEqual(ho.ANullableEnum?.ToString(), propertiesObject[nameof(PropertyHypermediaObject.ANullableEnum)].Value<string>());

        }

        public static void CompareNotNullProperties(JObject propertiesObject, PropertyHypermediaObject ho)
        {
            Assert.AreEqual(ho.ABool.ToString(), propertiesObject[nameof(PropertyHypermediaObject.ABool)].ToString());

            Assert.AreEqual(ho.AnInt.ToInvariantString(), propertiesObject[nameof(PropertyHypermediaObject.AnInt)].ToString());
            Assert.AreEqual(ho.ALong.ToInvariantString(), propertiesObject[nameof(PropertyHypermediaObject.ALong)].ToString());
            Assert.AreEqual(ho.AFloat.ToInvariantString(), ((float)propertiesObject[nameof(PropertyHypermediaObject.AFloat)]).ToInvariantString());
            Assert.AreEqual(ho.ADouble.ToInvariantString(), ((double)propertiesObject[nameof(PropertyHypermediaObject.ADouble)]).ToInvariantString());

            Assert.AreEqual(EnumHelper.GetEnumMemberValue(ho.AnEnum), propertiesObject[nameof(PropertyHypermediaObject.AnEnum)].ToString());
            Assert.AreEqual(EnumHelper.GetEnumMemberValue(ho.AnEnumWithNames), propertiesObject[nameof(PropertyHypermediaObject.AnEnumWithNames)].ToString());

            Assert.AreEqual(ho.ADateTime.ToStringZNotation(), ((IFormattable)propertiesObject[nameof(PropertyHypermediaObject.ADateTime)]).ToStringZNotation());
            Assert.AreEqual(ho.ADateTimeOffset.ToStringZNotation(), ((IFormattable)propertiesObject[nameof(PropertyHypermediaObject.ADateTimeOffset)]).ToStringZNotation());
            Assert.AreEqual(ho.ATimeSpan.ToInvariantString(), propertiesObject[nameof(PropertyHypermediaObject.ATimeSpan)].ToString());
            Assert.AreEqual(ho.ADecimal.ToInvariantString(), propertiesObject[nameof(PropertyHypermediaObject.ADecimal)].ToString());
        }

        public static void CompareHypermediaPropertiesAndJsonNoNullProperties(JObject propertiesObject, PropertyHypermediaObject ho)
        {
            var propertyInfos = typeof(PropertyHypermediaObject).GetProperties()
                .Where(p => p.Name != "Entities" && p.Name != "Links")
                .ToList();
            Assert.AreEqual(propertiesObject.Properties().Count(), propertyInfos.Count - 5);

            CompareNotNullProperties(propertiesObject, ho);

            Assert.IsNull(propertiesObject[nameof(PropertyHypermediaObject.AnUri)]);
            Assert.IsNull(propertiesObject[nameof(PropertyHypermediaObject.AType)]);
            Assert.IsNull(propertiesObject[nameof(PropertyHypermediaObject.AString)]);
            Assert.IsNull(propertiesObject[nameof(PropertyHypermediaObject.ANullableInt)]);
            Assert.IsNull(propertiesObject[nameof(PropertyHypermediaObject.ANullableEnum)]);
        }

        public static void CompareHypermediaListPropertiesAndJson(JObject propertiesObject, HypermediaObjectWithListProperties ho)
        {
            var propertyInfos = ho.GetType().GetProperties()
                .Where(p => p.Name != "Entities" && p.Name != "Links")
                .ToList();
            Assert.AreEqual(propertiesObject.Properties().Count(), propertyInfos.Count);


            foreach (var property in propertiesObject.Properties())
            {
                var htoProperty = propertyInfos.Single(p => p.Name == property.Name);
                var hoValue = (IEnumerable)htoProperty.GetValue(ho);
                if (hoValue == null) { 
                    Assert.AreEqual(JTokenType.Null, property.Value.Type);
                }
                else
                {
                    Assert.AreEqual(JTokenType.Array, property.Value.Type);
                    var jarray = (JArray)property.Value;


                    var index = 0;
                    foreach (var value in hoValue)
                    {
                        if (value == null)
                        {
                            Assert.IsTrue(jarray[index].Type == JTokenType.Null);
                        }
                        else
                        {
                            var valueType = value.GetType();
                            var valueTypeInfo = valueType.GetTypeInfo();
                            if (IsNestedList(valueTypeInfo, valueType))
                            {
                                Assert.AreEqual(value.ToString(), jarray[index].Value<object>().ToString());
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