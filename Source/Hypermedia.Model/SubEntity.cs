using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bluehands.Hypermedia.Model
{
    public abstract class SubEntity
    {
        public static SubEntity Embedded(string name, EntityKey entityKey, IEnumerable<string> relations) => new Embedded_(name, entityKey, relations);
        public static SubEntity Link(string name, EntityKey entityKey, IEnumerable<string> relations) => new Link_(name, entityKey, relations);

        public string Name { get; }
        public EntityKey EntityKey { get; }
        public ImmutableArray<string> Relations { get; }
        public class Embedded_ : SubEntity
        {
            public Embedded_(string name, EntityKey entityKey, IEnumerable<string> relations) : base(UnionCases.Embedded, name, entityKey, relations.ToImmutableArray())
            {
            }
        }

        public class Link_ : SubEntity
        {
            public Link_(string name, EntityKey entityKey, IEnumerable<string> relations) : base(UnionCases.Link, name, entityKey, relations.ToImmutableArray())
            {
            }
        }

        internal enum UnionCases
        {
            Embedded,
            Link
        }

        internal UnionCases UnionCase { get; }
        SubEntity(UnionCases unionCase, string name, EntityKey entityKey, ImmutableArray<string> relations)
        {
            UnionCase = unionCase;
            EntityKey = entityKey;
            Name = name;
            Relations = relations;
        }

        public override string ToString() => Enum.GetName(typeof(UnionCases), UnionCase) ?? UnionCase.ToString();
        bool Equals(SubEntity other) => UnionCase == other.UnionCase;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SubEntity)obj);
        }

        public override int GetHashCode() => (int)UnionCase;
    }

    public static class SubEntityExtension
    {
        public static T Match<T>(this SubEntity subEntity, Func<SubEntity.Embedded_, T> embedded, Func<SubEntity.Link_, T> link)
        {
            switch (subEntity.UnionCase)
            {
                case SubEntity.UnionCases.Embedded:
                    return embedded((SubEntity.Embedded_)subEntity);
                case SubEntity.UnionCases.Link:
                    return link((SubEntity.Link_)subEntity);
                default:
                    throw new ArgumentException($"Unknown type implementing SubEntity: {subEntity.GetType().Name}");
            }
        }

        public static async Task<T> Match<T>(this SubEntity subEntity, Func<SubEntity.Embedded_, Task<T>> embedded, Func<SubEntity.Link_, Task<T>> link)
        {
            switch (subEntity.UnionCase)
            {
                case SubEntity.UnionCases.Embedded:
                    return await embedded((SubEntity.Embedded_)subEntity).ConfigureAwait(false);
                case SubEntity.UnionCases.Link:
                    return await link((SubEntity.Link_)subEntity).ConfigureAwait(false);
                default:
                    throw new ArgumentException($"Unknown type implementing SubEntity: {subEntity.GetType().Name}");
            }
        }

        public static async Task<T> Match<T>(this Task<SubEntity> subEntity, Func<SubEntity.Embedded_, T> embedded, Func<SubEntity.Link_, T> link) => (await subEntity.ConfigureAwait(false)).Match(embedded, link);
        public static async Task<T> Match<T>(this Task<SubEntity> subEntity, Func<SubEntity.Embedded_, Task<T>> embedded, Func<SubEntity.Link_, Task<T>> link) => await(await subEntity.ConfigureAwait(false)).Match(embedded, link).ConfigureAwait(false);
    }
}
