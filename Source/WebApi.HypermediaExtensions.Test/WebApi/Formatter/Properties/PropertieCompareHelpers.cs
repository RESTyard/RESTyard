using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using WebApi.HypermediaExtensions.Test.Helpers;
using WebApi.HypermediaExtensions.Util.Enum;

namespace WebApi.HypermediaExtensions.Test.WebApi.Formatter.Properties
{
    public class PropertieCompareHelpers
    {
        public static void CompareHypermediaPropertiesAndJson(JObject propertiesObject, PropertyHypermediaObject ho)
        {
            var propertyInfos = ho.GetType().GetProperties()
                .Where(p => p.Name != "Entities" && p.Name != "Links")
                .ToList();
            Assert.AreEqual(propertiesObject.Properties().Count(), propertyInfos.Count);

            CompareNotNullProperties(propertiesObject, ho);

            Assert.AreEqual(ho.AString, propertiesObject["AString"].Value<string>());
            Assert.AreEqual(ho.ANullableInt, propertiesObject["ANullableInt"]);
        }

        public static void CompareNotNullProperties(JObject propertiesObject, PropertyHypermediaObject ho)
        {
            Assert.AreEqual(ho.ABool.ToString(), propertiesObject["ABool"].ToString());

            Assert.AreEqual(ho.AInt.ToInvariantString(), propertiesObject["AInt"].ToString());
            Assert.AreEqual(ho.ALong.ToInvariantString(), propertiesObject["ALong"].ToString());
            Assert.AreEqual(ho.AFloat.ToInvariantString(), ((float)propertiesObject["AFloat"]).ToInvariantString());
            Assert.AreEqual(ho.ADouble.ToInvariantString(), ((double)propertiesObject["ADouble"]).ToInvariantString());

            Assert.AreEqual(EnumHelper.GetEnumMemberValue(ho.AEnum), propertiesObject["AEnum"].ToString());
            Assert.AreEqual(EnumHelper.GetEnumMemberValue(ho.AEnumWithNames), propertiesObject["AEnumWithNames"].ToString());

            Assert.AreEqual(ho.ADateTime.ToStringZNotation(), ((IFormattable)propertiesObject["ADateTime"]).ToStringZNotation());
            Assert.AreEqual(ho.ADateTimeOffset.ToStringZNotation(), ((IFormattable)propertiesObject["ADateTimeOffset"]).ToStringZNotation());
            Assert.AreEqual(ho.ATimeSpan.ToInvariantString(), propertiesObject["ATimeSpan"].ToString());
            Assert.AreEqual(ho.ADecimal.ToInvariantString(), propertiesObject["ADecimal"].ToString());
        }

        public static void CompareHypermediaPropertiesAndJsonNoNullProperties(JObject propertiesObject, PropertyHypermediaObject ho)
        {
            var propertyInfos = typeof(PropertyHypermediaObject).GetProperties()
                .Where(p => p.Name != "Entities" && p.Name != "Links")
                .ToList();
            Assert.AreEqual(propertiesObject.Properties().Count(), propertyInfos.Count - 2);

            CompareNotNullProperties(propertiesObject, ho);

            Assert.IsNull(propertiesObject["AString"]);
            Assert.IsNull(propertiesObject["ANullableInt"]);
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
                            Assert.AreEqual(value.ToString(), jarray[index].Value<object>().ToString());
                        }
                        index++;
                    }

                    // no extra items
                    Assert.AreEqual(index, jarray.Count);
                }
            }
        }
    }
}