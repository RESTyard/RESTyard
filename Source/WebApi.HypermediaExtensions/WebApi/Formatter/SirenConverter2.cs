using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bluehands.Hypermedia.Model;
using FunicularSwitch;
using Newtonsoft.Json.Linq;
using WebApi.HypermediaExtensions.WebApi.Formatter.Siren;
using Entity = WebApi.HypermediaExtensions.WebApi.Formatter.Siren.Entity;

namespace WebApi.HypermediaExtensions.WebApi.Formatter
{
    public static class SirenConverter2
    {
        public static JObject Serialize(object hypermediaObject, IImmutableDictionary<EntityKey,Bluehands.Hypermedia.Model.Entity> model)
        {
            var sirenEntity = FillSirenEntity<Entity>(hypermediaObject, model);
            return JObject.FromObject(sirenEntity.GetValueOrThrow());
        }

        static Result<T> FillSirenEntity<T>(object hypermediaObject, IImmutableDictionary<EntityKey, Bluehands.Hypermedia.Model.Entity> model) where T : Entity, new()

        {
            var modelEntity = model.TryGetValue(hypermediaObject.GetType().ToEntityKey())
                .GetValueOrThrow($"No model entity found for hmo type {hypermediaObject.GetType().Name}");

            object GetPropertyValue(string propertyName)
            {
                var propertyInfo = hypermediaObject.GetType().GetProperty(propertyName);
                if (propertyInfo == null)
                    throw new Exception("");
                return propertyInfo.GetValue(hypermediaObject);
            }

            var sirenEntity = new T();
            sirenEntity.Class = modelEntity.Classes;
            sirenEntity.Title = modelEntity.Title;
            sirenEntity.Properties = modelEntity.Properties
                .Select(p => (p.Name, value: GetPropertyValue(p.PropertyName)))
                .ToDictionary(t => t.Name, t => t.value);

            var entities = modelEntity.Entities.Select(e =>
            {
                return e.Match(embedded =>
                    {
                        var subEntity = FillSirenEntity<EmbeddedRepresentationSubEntity>(GetPropertyValue(embedded.Name), model);
                        return subEntity.Map(s => (ISubEntity)s);
                    },
                    link => Result.Ok<ISubEntity>(new EmbeddedLinkSubEntity
                    {
                        Rel = link.Relations,
                        Title = link.Name,
                        //Class = link.
                        //Href = 
                        //Type = 
                    }));
            }).Aggregate();

            return entities.Map(e =>
            {
                sirenEntity.Entities = e.ToImmutableArray();
                return sirenEntity;
            });
        }
    }

    static class DictionaryExtension
    {
        public static Option<T> TryGetValue<TKey, T>(this IReadOnlyDictionary<TKey, T> dictionary, TKey key) =>
            dictionary.TryGetValue(key, out var value) ? value : Option<T>.None;
    }
}