using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FunicularSwitch;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Client.Resolver;
using RESTyard.Client.Util;

namespace RESTyard.Client.Reader
{
    internal enum HypermediaPropertyType
    {
        Unknown,
        Property,
        Entity,
        EntityCollection,
        Link,
        Command
    }

    public class SirenHypermediaReader : IHypermediaReader
    {
        private readonly IHypermediaObjectRegister hypermediaObjectRegister;
        private readonly IHypermediaCommandFactory hypermediaCommandFactory;
        private readonly IStringParser stringParser;
        private readonly DistinctOrderedStringCollectionComparer distinctOrderedStringCollectionComparer = new DistinctOrderedStringCollectionComparer();

        public SirenHypermediaReader(
            IHypermediaObjectRegister hypermediaObjectRegister,
            IStringParser stringParser)
        {
            this.hypermediaObjectRegister = hypermediaObjectRegister;
            this.hypermediaCommandFactory = RegisterHypermediaCommandFactory.Create();
            this.stringParser = stringParser;
        }

        public HypermediaReaderResult<HypermediaClientObject> Read(
            string contentString,
            IHypermediaResolver resolver)
        {
            try
            {
                var rootObject = this.stringParser.Parse(contentString);
                if (rootObject is null)
                {
                    return HypermediaReaderResult.Error<HypermediaClientObject>(HypermediaReaderProblem.InvalidFormat("empty content"));
                }
                var result = this.ReadHypermediaObject(rootObject, resolver);
                return result;
            }
            catch (Exception e)
            {
                return HypermediaReaderResult.Error<HypermediaClientObject>(HypermediaReaderProblem.Exception(e));
            }
        }

        public async Task<HypermediaReaderResult<HypermediaClientObject>> ReadAsync(
            Stream contentStream,
            IHypermediaResolver resolver)
        {
            try
            {
                var rootObject = await this.stringParser.ParseAsync(contentStream);
                if (rootObject is null)
                {
                    return HypermediaReaderResult.Error<HypermediaClientObject>(HypermediaReaderProblem.InvalidFormat("empty content"));
                }
                var result = this.ReadHypermediaObject(rootObject, resolver);
                return result;
            }
            catch (Exception e)
            {
                return HypermediaReaderResult.Error<HypermediaClientObject>(HypermediaReaderProblem.Exception(e));
            }
        }

        public async Task<HypermediaReaderResult<(HypermediaClientObject, string)>> ReadAndSerializeAsync(
            Stream contentStream,
            IHypermediaResolver resolver)
        {
            try
            {
                var rootObject = await this.stringParser.ParseAsync(contentStream);
                if (rootObject is null)
                {
                    return HypermediaReaderResult.Error<(HypermediaClientObject, string)>(HypermediaReaderProblem.InvalidFormat("empty content"));
                }
                var result = this.ReadHypermediaObject(rootObject, resolver);
                return result.Bind<(HypermediaClientObject, string)>(
                    ok => (ok, rootObject.Serialize()));
            }
            catch (Exception e)
            {
                return HypermediaReaderResult.Error<(HypermediaClientObject, string)>(HypermediaReaderProblem.Exception(e));
            }
        }

        private HypermediaReaderResult<HypermediaClientObject> ReadHypermediaObject(
            IToken rootObject,
            IHypermediaResolver resolver)
        {
            return ReadClasses(rootObject)
                .Bind(classes =>
                {
                    return this.hypermediaObjectRegister.CreateFromClasses(classes)
                        .Match(
                            hypermediaObjectInstance =>
                            {
                                this.ReadTitle(hypermediaObjectInstance, rootObject);
                                this.ReadRelations(hypermediaObjectInstance, rootObject);
                                return this.FillHypermediaProperties(hypermediaObjectInstance, rootObject, resolver)
                                    .Map(_ => hypermediaObjectInstance);
                            },
                            error => HypermediaReaderResult.Error<HypermediaClientObject>(
                                HypermediaReaderProblem.InvalidClientClass(error)));

                });
        }

        private HypermediaReaderResult<Unit> FillHypermediaProperties(
            HypermediaClientObject hypermediaObjectInstance,
            IToken rootObject,
            IHypermediaResolver resolver)
        {
            var typeInfo = hypermediaObjectInstance.GetType().GetTypeInfo();
            var properties = typeInfo.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            HypermediaReaderResult<Unit> result = HypermediaReaderResult.Ok(No.Thing);
            bool HasSetter(PropertyInfo p) => p.CanWrite;
            bool IsIgnored(PropertyInfo p) =>
                p.GetCustomAttribute<ClientIgnoreHypermediaPropertyAttribute>() is not null;
            foreach (var propertyInfo in properties.Where(p => HasSetter(p) && !IsIgnored(p)))
            {
                var hypermediaPropertyType = GetHypermediaPropertyType(propertyInfo);
                switch (hypermediaPropertyType)
                {
                    case HypermediaPropertyType.Property:
                        result = FillProperty(hypermediaObjectInstance, propertyInfo, rootObject);
                        break;
                    case HypermediaPropertyType.Link:
                        result = this.FillLink(hypermediaObjectInstance, propertyInfo, rootObject, resolver);
                        break;
                    case HypermediaPropertyType.Entity:
                        result = this.FillEntity(hypermediaObjectInstance, propertyInfo, rootObject, resolver);
                        break;
                    case HypermediaPropertyType.EntityCollection:
                        result = this.FillEntities(hypermediaObjectInstance, propertyInfo, rootObject, resolver);
                        break;
                    case HypermediaPropertyType.Command:
                        result = this.FillCommand(hypermediaObjectInstance, propertyInfo, rootObject, resolver);
                        break;
                    default:
                        result = HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.Exception(new ArgumentOutOfRangeException(hypermediaPropertyType.ToString())));
                        break;
                }

                if (result.IsError)
                {
                    return result;
                }
            }

            return result;
        }
        
        //todo linked entities
        //todo no derived types considered
        private HypermediaReaderResult<Unit> FillEntities(
            HypermediaClientObject hypermediaObjectInstance,
            PropertyInfo propertyInfo,
            IToken rootObject,
            IHypermediaResolver resolver)
        {
            var relationsAttribute = propertyInfo.GetCustomAttribute<HypermediaRelationsAttribute>();
            var classes = this.GetClassesFromEntitiesListProperty(propertyInfo);

            var entityCollection = Activator.CreateInstance(propertyInfo.PropertyType);
            propertyInfo.SetValue(hypermediaObjectInstance, entityCollection);

            var entities = rootObject["entities"];
            if (entities is null)
            {
                return HypermediaReaderResult.Ok(No.Thing);
            }

            if (entityCollection is null)
            {
                return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.InvalidClientClass($"Cannot instantiate type '{propertyInfo.PropertyType}' for property '{propertyInfo.Name}' of '{hypermediaObjectInstance.GetType()}'"));
            }

            var matchingEntities = entities.Where(e =>
                this.EntityRelationsMatch(e, relationsAttribute?.Relations) && this.EntityClassMatch(e, classes, propertyInfo.Name));

            var genericAddFunction = entityCollection.GetType().GetTypeInfo().GetMethod("Add");
            if (genericAddFunction is null)
            {
                return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.InvalidClientClass($"Collection '{entityCollection.GetType()}' has no suitable method 'Add' to fill it with entities"));
            }

            foreach (var match in matchingEntities)
            {
                var entityResult = this.ReadHypermediaObject(match, resolver);
                entityResult.Match(entity => genericAddFunction.Invoke(entityCollection, new object[] { entity }));
                if (entityResult.IsError)
                {
                    return entityResult.Map(_ => No.Thing);
                }
            }

            return HypermediaReaderResult.Ok(No.Thing);
        }

        private HypermediaReaderResult<Unit> FillCommand(
            HypermediaClientObject hypermediaObjectInstance,
            PropertyInfo propertyInfo,
            IToken rootObject,
            IHypermediaResolver resolver)
        {
            var commandAttribute = propertyInfo.GetCustomAttribute<HypermediaCommandAttribute>();
            if (commandAttribute == null)
            {
                return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.InvalidClientClass(
                    $"Hypermedia command '{propertyInfo.Name}' of type '{hypermediaObjectInstance.GetType()}' requires a {nameof(HypermediaCommandAttribute)}"));
            }

            return this.CreateHypermediaClientCommand(propertyInfo.PropertyType)
                .Bind(commandInstance =>
                {
                    propertyInfo.SetValue(hypermediaObjectInstance, commandInstance);

                    var actions = rootObject["actions"];
                    var desiredAction = actions?.FirstOrDefault(e => this.IsDesiredAction(e, commandAttribute.Name));
                    if (actions == null || desiredAction == null)
                    {
                        if (IsMandatoryHypermediaProperty(propertyInfo))
                        {
                            return HypermediaReaderResult.Error<Unit>(
                                HypermediaReaderProblem.RequiredPropertyMissing(
                                    $"Mandatory hypermedia command '{propertyInfo.Name}' not found."));
                        }

                        return HypermediaReaderResult.Ok(No.Thing);
                    }

                    return this.FillCommandParameters(commandInstance, desiredAction, commandAttribute.Name, resolver);
                });
        }

        private HypermediaReaderResult<Unit> FillCommandParameters(
            IHypermediaClientCommand commandInstance,
            IToken action,
            string commandName,
            IHypermediaResolver resolver)
        {
            commandInstance.Name = commandName;
            commandInstance.CanExecute = true;
            commandInstance.Resolver = resolver;

            var title = action["title"]?.ValueAsString() ?? string.Empty;
            commandInstance.Title = title;


            var uri = action["href"]?.ValueAsString();
            if (uri == null)
            {
                return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.RequiredPropertyMissing($"Siren action without href: '{commandName}'"));
            }
            commandInstance.Uri = new Uri(uri);

            var method = action["method"]?.ValueAsString() ?? "GET";
            commandInstance.Method = method;

            var fields = action["fields"];
            if (!commandInstance.HasParameters && fields != null)
            {
                return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.InvalidClientClass($"hypermedia Command '{commandName}' has no parameter but hypermedia document indicates parameters."));
            }
            if (fields == null)
            {
                if (commandInstance.HasParameters)
                {
                    return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.InvalidFormat($"hypermedia Command '{commandName}' has no parameter but hypermedia document indicates parameters."));
                }

                return HypermediaReaderResult.Ok(No.Thing);
            }

            var parameterDescriptions = new List<ParameterDescription>();
            foreach (var field in fields)
            {
                var name = field["name"]?.ValueAsString();
                var type = field["type"]?.ValueAsString();
                if (name is null || type is null)
                {
                    return HypermediaReaderResult.Error<Unit>(
                        HypermediaReaderProblem.InvalidFormat(
                            $"name of type is empty for parameter of command '{commandName}'"));
                }
                var parameterDescription = new ParameterDescription(
                    Name: name,
                    Type: type,
                    Classes: field["class"]?.ChildrenAsStrings().ToList() ?? (IReadOnlyList<string>)Array.Empty<string>());
                // todo optional but not save, or check annotation on class

                parameterDescriptions.Add(parameterDescription);
            }
            commandInstance.ParameterDescriptions = parameterDescriptions.ToImmutableList();

            return HypermediaReaderResult.Ok(No.Thing);
        }

        private HypermediaReaderResult<IHypermediaClientCommand> CreateHypermediaClientCommand(Type commandType)
        {
            var commandInstance = this.hypermediaCommandFactory.Create(commandType);
            return commandInstance.Match(
                ok => HypermediaReaderResult.Ok(ok),
                error => HypermediaReaderResult.Error<IHypermediaClientCommand>(HypermediaReaderProblem.InvalidClientClass(error)));
        }

        private bool IsDesiredAction(IToken action, string commandName)
        {
            var name = action["name"]?.ValueAsString();
            if (name == null)
            {
                return false;
            }

            return name.Equals(commandName);
        }

        private HypermediaReaderResult<Unit> FillEntity(
            HypermediaClientObject hypermediaObjectInstance,
            PropertyInfo propertyInfo,
            IToken rootObject,
            IHypermediaResolver resolver)
        {
            var relationsAttribute = propertyInfo.GetCustomAttribute<HypermediaRelationsAttribute>();
            var classes = GetClassesFromEntityProperty(propertyInfo.PropertyType.GetTypeInfo());

            var entities = rootObject["entities"];
            if (entities == null)
            {
                if (IsMandatoryHypermediaProperty(propertyInfo))
                {
                    return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.InvalidClientClass($"{nameof(IHypermediaLink)} requires a {nameof(HypermediaRelationsAttribute)} Attribute."));
                }
                return HypermediaReaderResult.Ok(No.Thing);
            }

            var jEntity = entities.FirstOrDefault(e =>
                this.EntityRelationsMatch(e, relationsAttribute?.Relations) && this.EntityClassMatch(e, classes, propertyInfo.Name));

            if (jEntity == null)
            {
                if (IsMandatoryHypermediaProperty(propertyInfo)) { 
                    return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.InvalidClientClass($"{nameof(IHypermediaLink)} requires a {nameof(HypermediaRelationsAttribute)} Attribute."));
                }
                return HypermediaReaderResult.Ok(No.Thing);
            }

            var entityResult = this.ReadHypermediaObject(jEntity, resolver);
            entityResult.Match(entity => propertyInfo.SetValue(hypermediaObjectInstance, entity));
            return entityResult.Map(_ => No.Thing);
        }

        private IDistinctOrderedCollection<string> GetClassesFromEntitiesListProperty(PropertyInfo propertyInfo)
        {
            var genericListType = GetGenericFromICollection(propertyInfo.PropertyType);
            return GetClassesFromEntityProperty(genericListType.GetTypeInfo());
        }

        private static IDistinctOrderedCollection<string> GetClassesFromEntityProperty(TypeInfo targetTypeInfo)
        {
            var classAttribute = targetTypeInfo.GetCustomAttribute<HypermediaClientObjectAttribute>();

            IDistinctOrderedCollection<string> classes;
            if (classAttribute is null)
            {
                classes = new DistinctOrderedStringCollection(targetTypeInfo.Name);
            }
            else
            {
                classes = classAttribute.Classes;
            }

            return classes;
        }

        private bool EntityClassMatch(IToken entity, IDistinctOrderedCollection<string> expectedClasses, string propertyName)
        {
            if (expectedClasses == null || expectedClasses.Count == 0)
            {
                throw new Exception($"No class provided for entity property '{propertyName}'.");
            }

            var actualClasses = entity["class"];
            if (actualClasses == null)
            {
                return false;
            }

            return this.distinctOrderedStringCollectionComparer.Equals(new DistinctOrderedStringCollection(actualClasses.ChildrenAsStrings()), expectedClasses);
        }

        private bool EntityRelationsMatch(IToken entity, IDistinctOrderedCollection<string>? expectedRelations)
        {
            if (expectedRelations == null || expectedRelations.Count == 0)
            {
                return true;
            }

            var actualRelations = entity["rel"];
            if (actualRelations == null)
            {
                return false;
            }

            return this.distinctOrderedStringCollectionComparer.Equals(new DistinctOrderedStringCollection(actualRelations.ChildrenAsStrings()), expectedRelations);
        }


        private HypermediaReaderResult<Unit> FillLink(
            HypermediaClientObject hypermediaObjectInstance,
            PropertyInfo propertyInfo,
            IToken rootObject,
            IHypermediaResolver resolver)
        {
            var linkAttribute = propertyInfo.GetCustomAttribute<HypermediaRelationsAttribute>();
            if (linkAttribute == null)
            {
                return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.InvalidClientClass($"{nameof(IHypermediaLink)} requires a {nameof(HypermediaRelationsAttribute)} Attribute."));
            }

            var hypermediaLink = (IHypermediaLink)Activator.CreateInstance(propertyInfo.PropertyType)!;
            propertyInfo.SetValue(hypermediaObjectInstance, hypermediaLink);

            var links = rootObject["links"];
            if (links == null)
            {
                if (IsMandatoryHypermediaLink(propertyInfo))
                {
                    return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.RequiredPropertyMissing($"Mandatory link not found {propertyInfo.Name}"));
                }
                return HypermediaReaderResult.Ok(No.Thing);
            }

            var link = links
                .Where(l => l["rel"] is not null)
                .FirstOrDefault(l => this.distinctOrderedStringCollectionComparer.Equals(
                    new DistinctOrderedStringCollection(l["rel"]!.ChildrenAsStrings()),
                    linkAttribute.Relations));
            if (link == null)
            {
                if (IsMandatoryHypermediaLink(propertyInfo))
                {
                    return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.RequiredPropertyMissing($"Mandatory link not found {propertyInfo.Name}"));
                }
                return HypermediaReaderResult.Ok(No.Thing);
            }

            var href = link["href"]?.ValueAsString();
            if (href is null)
            {
                return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.RequiredPropertyMissing($"href not found on link {propertyInfo.Name}"));
            }
            hypermediaLink.Uri = new Uri(href);
            hypermediaLink.Relations = link["rel"]!.ChildrenAsStrings().ToList();
            hypermediaLink.Resolver = resolver;
            return HypermediaReaderResult.Ok(No.Thing);
        }

        // todo attribute with different property name
        private static HypermediaReaderResult<Unit> FillProperty(HypermediaClientObject hypermediaObjectInstance, PropertyInfo propertyInfo, IToken rootObject)
        {
            var properties = rootObject["properties"];
            if (properties == null)
            {
                if (IsMandatoryHypermediaProperty(propertyInfo)) {
                    return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.RequiredPropertyMissing($"Mandatory property {propertyInfo.Name} not found"));
                }

                return HypermediaReaderResult.Ok(No.Thing);
            }

            var propertyValue = properties[propertyInfo.Name];
            if (propertyValue == null)
            {
                if (IsMandatoryHypermediaProperty(propertyInfo))
                {
                    return HypermediaReaderResult.Error<Unit>(HypermediaReaderProblem.RequiredPropertyMissing($"Mandatory property {propertyInfo.Name} not found"));
                }

                return HypermediaReaderResult.Ok(No.Thing);
            }

            propertyInfo.SetValue(hypermediaObjectInstance, propertyValue.ToObject(propertyInfo.PropertyType));
            return HypermediaReaderResult.Ok(No.Thing);
        }

        private static bool IsMandatoryHypermediaProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<MandatoryAttribute>() != null;
        }

        private static bool IsMandatoryHypermediaLink(PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.GetTypeInfo().IsAssignableFrom(typeof(MandatoryHypermediaLink<>))
                || propertyInfo.GetCustomAttribute<MandatoryAttribute>() != null;
        }

        private static HypermediaPropertyType GetHypermediaPropertyType(PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;

            if (typeof(IHypermediaLink).GetTypeInfo().IsAssignableFrom(propertyType))
            {
                return HypermediaPropertyType.Link;
            }

            if (typeof(HypermediaClientObject).GetTypeInfo().IsAssignableFrom(propertyType))
            {
                return HypermediaPropertyType.Entity;
            }

            if (typeof(IHypermediaClientCommand).GetTypeInfo().IsAssignableFrom(propertyType))
            {
                return HypermediaPropertyType.Command;
            }

            var isCollection = IsGenericCollection(propertyType);
            if (isCollection)
            {
                var collectionType = GetGenericFromICollection(propertyType);
                if (typeof(HypermediaClientObject).GetTypeInfo().IsAssignableFrom(collectionType))
                {
                    return HypermediaPropertyType.EntityCollection;
                }
            }

            return HypermediaPropertyType.Property;
        }

        private static bool IsGenericCollection(Type propertyType)
        {
            return propertyType.GetTypeInfo().IsGenericType && propertyType.GetTypeInfo().GetInterface("ICollection") != null;
        }

        private static Type GetGenericFromICollection(Type type)
        {
            return type.GetTypeInfo().GetGenericArguments()[0];
        }

        private void ReadRelations(HypermediaClientObject hypermediaObjectInstance, IToken rootObject)
        {
            var relations = rootObject["rel"]?.ChildrenAsStrings();
            if (relations == null)
            {
                return;
            }

            hypermediaObjectInstance.Relations = relations.ToList();
        }

        private void ReadTitle(HypermediaClientObject hypermediaObjectInstance, IToken rootObject)
        {
            var title = rootObject["title"];
            if (title == null)
            {
                return;
            }

            hypermediaObjectInstance.Title = title.ValueAsString() ?? string.Empty;
        }      

        private static HypermediaReaderResult<IDistinctOrderedCollection<string>> ReadClasses(IToken rootObject)
        {
            const string ClassDescriptor = "class";
            // rel might be missing so provide better error message
            try
            {
                var classesToken = rootObject[ClassDescriptor];
                if (classesToken is null)
                {
                    return HypermediaReaderResult.Error<IDistinctOrderedCollection<string>>(HypermediaReaderProblem.RequiredPropertyMissing(ClassDescriptor));
                }
                var classes = classesToken.ChildrenAsStrings().ToList();
                if (!classes.Any())
                {
                    return HypermediaReaderResult.Error<IDistinctOrderedCollection<string>>(HypermediaReaderProblem.RequiredPropertyMissing(ClassDescriptor));
                }
                return new DistinctOrderedStringCollection(classes);
            }
            catch (Exception e)
            {
                return HypermediaReaderResult.Error<IDistinctOrderedCollection<string>>(HypermediaReaderProblem.Exception(e));
            }
        }
    }
}