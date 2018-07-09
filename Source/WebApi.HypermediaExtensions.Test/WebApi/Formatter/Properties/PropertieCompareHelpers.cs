using System;
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

            Assert.IsFalse(propertiesObject["AString"].HasValues);
            Assert.IsFalse(propertiesObject["ANullableInt"].HasValues);
        }

        public static void CompareNotNullProperties(JObject propertiesObject, PropertyHypermediaObject ho)
        {
            Assert.AreEqual(propertiesObject["ABool"].ToString(), ho.ABool.ToString());

            Assert.AreEqual(propertiesObject["AInt"].ToString(), ho.AInt.ToInvariantString());
            Assert.AreEqual(propertiesObject["ALong"].ToString(), ho.ALong.ToInvariantString());
            Assert.AreEqual(((float)propertiesObject["AFloat"]).ToInvariantString(), ho.AFloat.ToInvariantString());
            Assert.AreEqual(((double)propertiesObject["ADouble"]).ToInvariantString(), ho.ADouble.ToInvariantString());

            Assert.AreEqual(propertiesObject["AEnum"].ToString(), EnumHelper.GetEnumMemberValue(ho.AEnum));
            Assert.AreEqual(propertiesObject["AEnumWithNames"].ToString(), EnumHelper.GetEnumMemberValue(ho.AEnumWithNames));

            Assert.AreEqual(((IFormattable)propertiesObject["ADateTime"]).ToStringZNotation(),
                ho.ADateTime.ToStringZNotation());
            Assert.AreEqual(((IFormattable)propertiesObject["ADateTimeOffset"]).ToStringZNotation(),
                ho.ADateTimeOffset.ToStringZNotation());
            Assert.AreEqual(propertiesObject["ATimeSpan"].ToString(), ho.ATimeSpan.ToInvariantString());
            Assert.AreEqual(propertiesObject["ADecimal"].ToString(), ho.ADecimal.ToInvariantString());
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
                if (htoProperty.GetValue(ho) == null) { 
                    Assert.AreEqual(JTokenType.Null, property.Value.Type);
                }
                else
                {
                    Assert.AreEqual(JTokenType.Array, property.Value.Type);
                    // todo check content
                }
            }
        }
    }
}