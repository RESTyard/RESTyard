using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Test.Helpers;
using RESTyard.AspNetCore.Util.Enum;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter.Properties
{
    public class PropertyHelpers
    {
        public static JsonObject GetPropertiesJObject(JsonObject siren)
            => siren["properties"].Should().BeOfType<JsonObject>().Which;

        public static void CompareHypermediaPropertiesAndJson(JsonObject propertiesObject, PropertyHypermediaObject ho)
        {
            var propertyInfos = ho.GetType().GetProperties()
                .Where(p => p.Name != "Entities" && p.Name != "Links" && p.Name != nameof(IHypermediaObject.HtoTitle))
                .ToList();
            propertiesObject.Should().HaveCount(propertyInfos.Count);

            CompareNotNullProperties(propertiesObject, ho);

            (propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.AnUri)).WhoseValue?.GetValue<string>()).Should().Be(ho.AnUri?.ToString());
            (propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.AType)).WhoseValue?.GetValue<string>()).Should().Be(ho.AType?.FullName);
            (propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.AString)).WhoseValue?.GetValue<string>()).Should().Be(ho.AString);
            (propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.ANullableInt)).WhoseValue?.GetValue<int>()).Should().Be(ho.ANullableInt);
            (propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.ANullableEnum)).WhoseValue?.GetValue<string>()).Should().Be(ho.ANullableEnum?.ToString());
        }

        public static void CompareNotNullProperties(JsonObject propertiesObject, PropertyHypermediaObject ho)
        {
            propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.ABool)).WhoseValue!.GetValue<bool>().Should().Be(ho.ABool);

            propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.AnInt)).WhoseValue!.GetValue<int>().Should().Be(ho.AnInt);
            propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.ALong)).WhoseValue!.GetValue<long>().Should().Be(ho.ALong);
            propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.AFloat)).WhoseValue!.GetValue<float>().Should().Be(ho.AFloat);
            propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.ADouble)).WhoseValue!.GetValue<double>().Should().Be(ho.ADouble);

            propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.AnEnum)).WhoseValue!.GetValue<string>().Should().Be(EnumHelper.GetEnumMemberValue(ho.AnEnum));
            propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.AnEnumWithNames)).WhoseValue!.GetValue<string>().Should().Be(EnumHelper.GetEnumMemberValue(ho.AnEnumWithNames));

            propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.ADateTime)).WhoseValue!.GetValue<DateTime>().Should().Be(ho.ADateTime);
            propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.ADateTimeOffset)).WhoseValue!.GetValue<DateTimeOffset>().Should().Be(ho.ADateTimeOffset);
            // System.Text.Json serializes TimeSpan as a string and GetValue<TimeSpan>() cannot convert it back, so compare the string form.
            propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.ATimeSpan)).WhoseValue!.GetValue<string>().Should().Be(ho.ATimeSpan.ToInvariantString());
            propertiesObject.Should().ContainKey(nameof(PropertyHypermediaObject.ADecimal)).WhoseValue!.GetValue<decimal>().Should().Be(ho.ADecimal);
        }

        public static void CompareHypermediaPropertiesAndJsonNoNullProperties(JsonObject propertiesObject, PropertyHypermediaObject ho)
        {
            var propertyInfos = typeof(PropertyHypermediaObject).GetProperties()
                .Where(p => p.Name != "Entities" && p.Name != "Links" && p.Name != nameof(IHypermediaObject.HtoTitle))
                .ToList();
            propertiesObject.Should().HaveCount(propertyInfos.Count - 5);

            CompareNotNullProperties(propertiesObject, ho);

            propertiesObject.Should().NotContainKey(nameof(PropertyHypermediaObject.AnUri));
            propertiesObject.Should().NotContainKey(nameof(PropertyHypermediaObject.AType));
            propertiesObject.Should().NotContainKey(nameof(PropertyHypermediaObject.AString));
            propertiesObject.Should().NotContainKey(nameof(PropertyHypermediaObject.ANullableInt));
            propertiesObject.Should().NotContainKey(nameof(PropertyHypermediaObject.ANullableEnum));
        }

        public static void CompareHypermediaListPropertiesAndJson(JsonObject propertiesObject, HypermediaObjectWithListProperties ho)
        {
            var propertyInfos = ho.GetType().GetProperties()
                .Where(p => p.Name != "Entities" && p.Name != "Links" && p.Name != nameof(IHypermediaObject.HtoTitle))
                .ToList();
            propertiesObject.Should().HaveCount(propertyInfos.Count);


            foreach (var property in propertiesObject)
            {
                var htoProperty = propertyInfos.Single(p => p.Name == property.Key);
                var hoValue = (IEnumerable)htoProperty.GetValue(ho);
                if (hoValue == null) {
                    property.Value.Should().BeNull();
                }
                else
                {
                    var jarray = property.Value.Should().BeOfType<JsonArray>().Which;

                    var index = 0;
                    foreach (var value in hoValue)
                    {
                        if (value == null)
                        {
                            jarray[index].Should().BeNull();
                        }
                        else
                        {
                            var valueType = value.GetType();
                            var valueTypeInfo = valueType.GetTypeInfo();
                            if (IsNestedList(valueTypeInfo, valueType))
                            {
                                jarray[index]!.ToString().Should().Be(value.ToString());
                            }
                        }

                        index++;
                    }

                    // no extra items
                    jarray.Should().HaveCount(index);
                }
            }
        }

        private static bool IsNestedList(TypeInfo valueTypeInfo, Type valueType)
        {
            return !valueTypeInfo.IsClass || valueType == typeof(string);
        }
    }
}
