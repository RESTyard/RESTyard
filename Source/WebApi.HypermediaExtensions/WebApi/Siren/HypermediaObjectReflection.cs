using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Util;

namespace WebApi.HypermediaExtensions.WebApi.Siren
{
    public class HypermediaObjectReflection
    {
        public Type HypermediaObjectType { get; }

        public HypermediaObjectAttribute HypermediaObjectAttribute { get; private set; }

        public List<ReflectedHypermediaProperty> Links { get; private set; }

        public List<ReflectedHypermediaProperty> Properties { get; private set; }

        public List<ReflectedHypermediaProperty> Actions { get; private set; }

        public List<ReflectedHypermediaProperty> Entities { get; private set; }

        public HypermediaObjectReflection(object hypermediaObjectType)
        {
            HypermediaObjectType = hypermediaObjectType.GetType();
            HypermediaObjectAttribute = GetHypermediaObjectAttribute();

            var hypermediaProperties = ExtractHypermediaProperties();

            Links = GetLinks(hypermediaProperties);
            Properties = GetProperties(hypermediaProperties);
            Actions = GetActions(hypermediaProperties);
        }

        private HypermediaObjectAttribute GetHypermediaObjectAttribute()
        {
            return HypermediaObjectType.GetTypeInfo().GetCustomAttribute<HypermediaObjectAttribute>();
        }

        private List<ReflectedHypermediaProperty> GetLinks(List<ReflectedHypermediaProperty> hypermediaProperties)
        {
            var links = hypermediaProperties.Where(hp =>
                hp.LeadingHypermediaAttribute.HasValue && hp.LeadingHypermediaAttribute.Value is Link).ToList();
            links.Select(l =>
                (l.LeadingHypermediaAttribute.Value as Link).BaseTypes.Contains(l.PropertyInfo.PropertyType));
            return links;
        }

        private List<ReflectedHypermediaProperty> GetProperties(List<ReflectedHypermediaProperty> hypermediaProperties)
        {
            return hypermediaProperties.Where(hp => !hp.LeadingHypermediaAttribute.HasValue || hp.LeadingHypermediaAttribute.Value is HypermediaPropertyAttribute).ToList();
        }

        private List<ReflectedHypermediaProperty> GetActions(List<ReflectedHypermediaProperty> hypermediaProperties)
        {
            return hypermediaProperties.Where(hp =>
                hp.LeadingHypermediaAttribute.HasValue && hp.LeadingHypermediaAttribute.Value is HypermediaActionAttribute).ToList();
        }

        private List<ReflectedHypermediaProperty> ExtractHypermediaProperties()
        {
            return HypermediaObjectType.GetProperties().Select(p =>
            {
                return ExtractHypermediaProperty(p);
            }).ToList();
        }

        private static ReflectedHypermediaProperty ExtractHypermediaProperty(PropertyInfo p)
        {
            var customAttributes = p.GetCustomAttributes().ToList();
            if (!customAttributes.Any()) return new ReflectedHypermediaProperty(p);
            var leadingHypermediaAttribute = customAttributes.SingleOrDefault(a => a is LeadingHypermediaAttribute);
            return new ReflectedHypermediaProperty(p, leadingHypermediaAttribute);
        }
    }

    public class ReflectedHypermediaProperty
    {
        public ReflectedHypermediaProperty(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            IsOrdinaryProperty = true;
        }

        public ReflectedHypermediaProperty(PropertyInfo propertyInfo, Attribute leadingHypermediaAttribute)
        {
            PropertyInfo = propertyInfo;
            LeadingHypermediaAttribute = leadingHypermediaAttribute;
            IsOrdinaryProperty = false;
        }

        public bool IsOrdinaryProperty { get; private set; }
        public PropertyInfo PropertyInfo { get; set; }
        public Optional<Attribute> LeadingHypermediaAttribute { get; set; }


    }
}