using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bluehands.Hypermedia.Model
{
    public class Entity
    {
        public string Name => Key.Name;
        public string Namespace => Key.Namespace;

        public EntityKey Key { get; }
        public string Title { get; }
        public ImmutableArray<string> Classes { get; }
        public ImmutableArray<KeyProperty> KeyProperties { get; }
        public ImmutableArray<Property> Properties { get; }
        public ImmutableArray<Link> Links { get; }
        public ImmutableArray<SubEntity> Entities { get; }


        public Entity(string name, string title, string ns, IEnumerable<string> classes,
            IEnumerable<Property> properties, IEnumerable<KeyProperty> keyProperties, IEnumerable<Link> links, IEnumerable<SubEntity> entities)
        {
            Key = new EntityKey(name, ns);
            Title = title;
            Classes = classes.ToImmutableArray();
            Properties = properties.ToImmutableArray();
            KeyProperties = keyProperties.ToImmutableArray();
            Links = links.ToImmutableArray();
            Entities = entities.ToImmutableArray();
        }

        public override string ToString() => $"{Name} - {Title}";
    }

    public static class EntityKeyExtension
    {
        public static EntityKey ToEntityKey(this Type type) => new EntityKey(type.Name, type.Namespace);
        
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

        public override string ToString() => $"{(Namespace != null ? $"{Namespace}." : "")}{Name}";
    }
}
