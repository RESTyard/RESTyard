using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NJsonSchema.Infrastructure;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Util;

namespace WebApi.HypermediaExtensions.WebApi.Siren
{
    using Model;
    public class ModelFactory
    {
        public ModelFactory()
        {
            
        }

        public Siren Create(object hypermediaObject)
        {
            var reflection = new HypermediaObjectReflection(hypermediaObject);
            return new Siren() { 
                SirenTitle = this.CreateSirenTitle(reflection.GetHypermediaObjectAttribute()),
                SirenClasses = this.CreateSirenClasses(reflection.GetHypermediaObjectAttribute(), reflection.HypermediaObjectType),
                SirenProperties = this.CreateSirenProperties(reflection.GetProperties())
            };
        }

        private SirenTitle CreateSirenTitle(HypermediaObjectAttribute hypermediaObjectAttribute)
        {
            return !string.IsNullOrEmpty(hypermediaObjectAttribute?.Title) ? new SirenTitle(hypermediaObjectAttribute.Title) : null;
        }

        private List<SirenClass> CreateSirenClasses(HypermediaObjectAttribute hypermediaObjectAttribute, Type hypermediaObjectType)
        {
            var sirenClasses = new List<SirenClass>();
            if (hypermediaObjectAttribute != null && hypermediaObjectAttribute.Classes.Any())
            {
                sirenClasses.AddRange(hypermediaObjectAttribute.Classes.Select(c => new SirenClass(c)));
            }
            else
            {
                sirenClasses.Add(new SirenClass(hypermediaObjectType.BeautifulName()));
            }
            return sirenClasses;
        }

        private List<SirenProperty> CreateSirenProperties(IEnumerable<HypermediaProperty> hypermediaProperty)
        {
            return hypermediaProperty.Select(hp => 
                !hp.LeadingHypermediaAttribute.HasValue 
                    ? new SirenProperty(hp.PropertyInfo) 
                    : new SirenProperty(hp.PropertyInfo, (hp.LeadingHypermediaAttribute.Value as HypermediaPropertyAttribute)?.Name))
                .ToList();
        }
    }
}