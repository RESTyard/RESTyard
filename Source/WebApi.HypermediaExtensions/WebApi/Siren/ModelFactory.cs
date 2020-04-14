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
                SirenTitle = this.CreateSirenTitle(reflection.GetHypermediaObjectAttribute()),
                SirenClasses = this.CreateSirenClasses(reflection.GetHypermediaObjectAttribute(), reflection.HypermediaObjectType),
                SirenProperties = this.CreateSirenProperties(reflection.GetProperties()),
                SirenLinks = this.CreateSirenLinks(reflection.GetLinks()),
                SirenActions = this.CreateSirenActions(reflection.GetActions())
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

        private List<SirenProperty> CreateSirenProperties(IEnumerable<HypermediaProperty> hypermediaProperty)
        {
            return hypermediaProperty.Select(hp => 
                !hp.LeadingHypermediaAttribute.HasValue 
                    ? new SirenProperty(hp.PropertyInfo) 
                    : new SirenProperty(hp.PropertyInfo, (hp.LeadingHypermediaAttribute.Value as HypermediaPropertyAttribute)?.Name))
                .ToList();
        }

        private List<SirenLink> CreateSirenLinks(IEnumerable<HypermediaProperty> hypermediaProperty)
        {
            return hypermediaProperty.Select(hp => 
                new SirenLink(
                    (hp.LeadingHypermediaAttribute.Value as Link)?.Relations.Select(r => new SirenRelation(r)).ToList(),
                    hp.PropertyInfo, 
                    hp.PropertyInfo.GetType().IsAssignableFrom(typeof(HypermediaObjectReferenceBase)),
                    new SirenTitle())) // attribute needs title too
                .ToList();
        }

        private List<SirenAction> CreateSirenActions(IEnumerable<HypermediaProperty> hypermediaProperty)
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