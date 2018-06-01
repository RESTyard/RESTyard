using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HypermediaClient.Hypermedia;
using HypermediaClient.Hypermedia.Attributes;
using HypermediaClient.Util;

namespace HypermediaClient
{
    public class HypermediaObjectRegister : IHypermediaObjectRegister
    {
        private readonly Dictionary<List<string>, Type> hypermediaObjectTypeDictionary = new Dictionary<List<string>, Type>(new StringCollectionComparer());

        public void Register(Type hypermediaObjectType)
        {
            if (!typeof(HypermediaClientObject).GetTypeInfo().IsAssignableFrom(hypermediaObjectType))
            {
                throw new Exception($"Can only register {typeof(HypermediaClientObject).Name}");
            }

            var attribute = hypermediaObjectType.GetTypeInfo().GetCustomAttribute<HypermediaClientObjectAttribute>();
            if (attribute == null)
            {
                hypermediaObjectTypeDictionary.Add(new List<string> { hypermediaObjectType.Name }, hypermediaObjectType);
            }
            else
            {
                hypermediaObjectTypeDictionary.Add(attribute.Classes.ToList(), hypermediaObjectType);
            }
            
        }

        public HypermediaClientObject CreateFromClasses(List<string> classes)
        {
            var hypermediaObjectType = GethypermediaType(classes);

            return (HypermediaClientObject)Activator.CreateInstance(hypermediaObjectType);
        }

        public Type GethypermediaType(List<string> classes)
        {
            Type hypermediaObjectType;
            if (!hypermediaObjectTypeDictionary.TryGetValue(classes, out hypermediaObjectType))
            {
                throw new Exception($"No Type registered for classes: {classes}");
            }
            return hypermediaObjectType;
        }
    }

    public interface IHypermediaObjectRegister
    {
        void Register(Type hypermediaObjectType);

        Type GethypermediaType(List<string> classes);

        HypermediaClientObject CreateFromClasses(List<string> classes);
    }
}