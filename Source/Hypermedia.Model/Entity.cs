using System.Collections.Generic;
using System.Collections.Immutable;
using FunicularSwitch;

namespace Bluehands.Hypermedia.Model
{
    public class Entity
    {
        public string Name => Key.Name;
        public string Namespace => Key.Namespace;

        public EntityKey Key { get; }
        public string Title { get; }
        public ImmutableArray<string> Classes { get; }
        public ImmutableArray<Property> Properties { get; }
        public ImmutableArray<Link> Links { get; }
        public ImmutableArray<SubEntity> Entities { get; }


        public Entity(string name, string title, string ns, IEnumerable<string> classes,
            IEnumerable<Property> properties, IEnumerable<Link> links, IEnumerable<SubEntity> entities)
        {
            Key = new EntityKey(name, ns);
            Title = title;
            Classes = classes.ToImmutableArray();
            Properties = properties.ToImmutableArray();
            Links = links.ToImmutableArray();
            Entities = entities.ToImmutableArray();
        }
    }

    public class EntityKey
    {
        public string Name { get; }
        public string Namespace { get; }

        public EntityKey(string name, string ns = null)
        {
            Name = name;
            Namespace = ns;
        }

        protected bool Equals(EntityKey other) => Name == other.Name && Namespace == other.Namespace;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EntityKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
            }
        }
    }

    public class EntityBuilder
    {
        public ImmutableArray<string> Classes { get; }
        public Option<string> Name { get; }
        public Option<string> Namespace { get; }
        public Option<string> Title { get; }

        public EntityBuilder(ImmutableArray<string> classes, Option<string> name, Option<string> @namespace, Option<string> title)
        {
            Classes = classes;
            Name = name;
            Namespace = @namespace;
            Title = title;
        }

        public static EntityBuilder Create(string name) => new EntityBuilder(ImmutableArray<string>.Empty, name, Option<string>.None, Option<string>.None);

        public EntityBuilder WithName(string name) => new EntityBuilder(Classes, name, Namespace, Title);

        public EntityBuilder WithTitle(string title) => new EntityBuilder(Classes, Name, Namespace, title);

        public EntityBuilder WithNamespace(string @namespace) => new EntityBuilder(Classes, Name, @namespace, Title);

        public Entity Build()
        {
            return new Entity(
                name: Name.GetValueOrDefault(Classes[0]),
                title: Title.GetValueOrDefault(),
                ns: Namespace.GetValueOrDefault(),
                classes: Classes,
                properties: ImmutableArray<Property>.Empty,
                links: ImmutableArray<Link>.Empty,
                entities: ImmutableArray<SubEntity>.Empty);
        }
    }
}
