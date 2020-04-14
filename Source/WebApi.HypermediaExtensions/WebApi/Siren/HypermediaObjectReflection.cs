using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Util;

namespace WebApi.HypermediaExtensions.WebApi.Siren
{
    public class HypermediaObjectReflection
    {
        public Type HypermediaObjectType { get; }

        public List<HypermediaProperty> HypermediaProperties { get; set; }

        public HypermediaObjectReflection(object hypermediaObjectType)
        {
            HypermediaObjectType = hypermediaObjectType.GetType();
            HypermediaProperties = ExtractHypermediaProperties();
        }

        public HypermediaObjectAttribute GetHypermediaObjectAttribute()
        {
            return HypermediaObjectType.GetTypeInfo().GetCustomAttribute<HypermediaObjectAttribute>();
        }

        public List<HypermediaProperty> GetLinks()
        {
            return HypermediaProperties.Where(hp =>
                hp.LeadingHypermediaAttribute.HasValue & hp.LeadingHypermediaAttribute.Value is Link).ToList();
        }

        public List<HypermediaProperty> GetProperties()
        {
            return HypermediaProperties.Where(hp => !hp.LeadingHypermediaAttribute.HasValue || hp.LeadingHypermediaAttribute.Value is HypermediaPropertyAttribute).ToList();
        }

        public List<HypermediaProperty> GetActions()
        {
            return HypermediaProperties.Where(hp =>
                hp.LeadingHypermediaAttribute.HasValue & hp.LeadingHypermediaAttribute.Value is HypermediaActionAttribute).ToList();
        }

        private List<HypermediaProperty> ExtractHypermediaProperties()
        {
            return HypermediaObjectType.GetProperties().Select(p =>
            {
                return ExtractHypermediaProperty(p);
            }).ToList();
        }

        private static HypermediaProperty ExtractHypermediaProperty(PropertyInfo p)
        {
            var customAttributes = p.GetCustomAttributes().ToList();
            if (!customAttributes.Any()) return new HypermediaProperty(p);
            var leadingHypermediaAttribute = customAttributes.SingleOrDefault(a => a is LeadingHypermediaAttribute);
            return new HypermediaProperty(p, leadingHypermediaAttribute);
        }
    }

    public class HypermediaProperty
    {
        public HypermediaProperty(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }

        public HypermediaProperty(PropertyInfo propertyInfo, Attribute leadingHypermediaAttribute)
        {
            PropertyInfo = propertyInfo;
            LeadingHypermediaAttribute = leadingHypermediaAttribute;
        }
        public PropertyInfo PropertyInfo { get; set; }
        public Optional<Attribute> LeadingHypermediaAttribute { get; set; }


    }
}