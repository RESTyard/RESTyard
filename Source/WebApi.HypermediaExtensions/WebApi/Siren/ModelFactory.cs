using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Hypermedia.Links;
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
                SirenTitle = this.CreateSirenTitle(reflection.HypermediaObjectAttribute),
                SirenClasses = this.CreateSirenClasses(reflection.HypermediaObjectAttribute, reflection.HypermediaObjectType),
                SirenProperties = this.CreateSirenProperties(reflection.Properties),
                SirenLinks = this.CreateSirenLinks(reflection.Links),
                SirenActions = this.CreateSirenActions(reflection.Actions)
            };
        }

        private SirenTitle CreateSirenTitle(HypermediaObjectAttribute hypermediaObjectAttribute)
        {
            return new SirenTitle(hypermediaObjectAttribute?.Title);
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

        private List<SirenProperty> CreateSirenProperties(IEnumerable<ReflectedHypermediaProperty> hypermediaProperty)
        {
            return hypermediaProperty.Select(hp => 
                !hp.LeadingHypermediaAttribute.HasValue 
                    ? new SirenProperty(hp.PropertyInfo) 
                    : new SirenProperty(hp.PropertyInfo, (hp.LeadingHypermediaAttribute.Value as HypermediaPropertyAttribute)?.Name))
                .ToList();
        }

        private List<SirenLink> CreateSirenLinks(IEnumerable<ReflectedHypermediaProperty> hypermediaProperty)
        {
            return hypermediaProperty.Select(hp => 
                new SirenLink(
                    (hp.LeadingHypermediaAttribute.Value as Link)?.Relations.Select(r => new SirenRelation(r)).ToList(),
                    hp.PropertyInfo, 
                    hp.PropertyInfo.GetType().IsAssignableFrom(typeof(HypermediaObjectReferenceBase)),
                    null)) // attribute needs title too
                .ToList();
        }

        private List<SirenAction> CreateSirenActions(IEnumerable<ReflectedHypermediaProperty> hypermediaProperty)
        {
            return hypermediaProperty.Select(hp =>
            {
                if (!hp.LeadingHypermediaAttribute.HasValue)
                {
                    return new SirenAction(hp.PropertyInfo);
                }
                var actionAttribute = hp.LeadingHypermediaAttribute.Value as HypermediaActionAttribute;
                return new SirenAction(
                    hp.PropertyInfo,
                    new SirenTitle(actionAttribute?.Title),
                    actionAttribute?.Name);
            }).ToList();
        }
    }
}