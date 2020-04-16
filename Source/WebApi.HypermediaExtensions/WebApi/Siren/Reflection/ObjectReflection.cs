using System;
using System.Collections.Generic;
using System.Reflection;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using FunicularSwitch;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Util;

namespace WebApi.HypermediaExtensions.WebApi.Siren.Reflection
{
    public class ObjectReflection
    {
        public Type HypermediaObjectType { get; private set; }

        public HypermediaObjectAttribute HypermediaObjectAttribute { get; private set; }

        public List<ReflectedProperty> Links { get; private set; }

        public List<ReflectedProperty> Properties { get; private set; }

        public List<ReflectedProperty> Actions { get; private set; }

        public List<ReflectedProperty> Entities { get; private set; }

        public ObjectReflection(Type hypermediaObjectType, HypermediaObjectAttribute hypermediaObjectAttribute, List<ReflectedProperty> links, List<ReflectedProperty> properties, List<ReflectedProperty> actions, List<ReflectedProperty> entities)
        {
            HypermediaObjectType = hypermediaObjectType;
            HypermediaObjectAttribute = hypermediaObjectAttribute;
            Links = links;
            Properties = properties;
            Actions = actions;
            Entities = entities;
        }

       
    }
}