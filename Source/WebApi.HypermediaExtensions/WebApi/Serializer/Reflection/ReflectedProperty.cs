using System;
using System.Collections.Generic;
using System.Reflection;
using FunicularSwitch;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Reflection
{
    public class ReflectedProperty
    {

        public ReflectedProperty(PropertyInfo propertyInfo, List<Attribute> hypermediaAttributes)
        {
            PropertyInfo = propertyInfo;
            HypermediaAttributes = hypermediaAttributes;
            PrimaryHypermediaAttribute = Option<Attribute>.None;
        }

        public ReflectedProperty(PropertyInfo propertyInfo, Attribute primaryHypermediaAttribute, List<Attribute> hypermediaAttributes)
        {
            PropertyInfo = propertyInfo;
            PrimaryHypermediaAttribute = primaryHypermediaAttribute;
            HypermediaAttributes = hypermediaAttributes;
        }

        public PropertyInfo PropertyInfo { get; private set; }
        public Option<Attribute> PrimaryHypermediaAttribute { get; private set; }
        public List<Attribute> HypermediaAttributes { get; private set; }


    }
}