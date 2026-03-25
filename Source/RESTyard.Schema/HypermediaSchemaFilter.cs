using System;
using System.Collections.Generic;
using System.Linq;
using RESTyard.Schema.Model;

namespace RESTyard.Schema;

/// <summary>
/// Filters a <see cref="HypermediaApiSchema"/> by access groups.
/// Both methods return a new schema instance — the original is not modified.
/// </summary>
public static class HypermediaSchemaFilter
{
    /// <summary>
    /// Include mode: keeps elements whose <c>RequiredAccessGroups</c> are a subset of
    /// <paramref name="grantedAccessGroups"/>, plus elements with no access group restriction (null).
    /// Entity types whose <c>RequiredAccessGroups</c> are not satisfied are removed entirely.
    /// Entity types that become unreachable (no remaining links, actions, embedded entities,
    /// and not referenced by any remaining element) are also removed.
    /// <c>DeclaredAccessGroups</c> is stripped from the output.
    /// </summary>
    public static HypermediaApiSchema ForAccessGroups(
        HypermediaApiSchema fullSchema,
        ISet<string> grantedAccessGroups)
    {
        var filteredEntityTypes = new List<EntityTypeSchema>();

        foreach (var entity in fullSchema.EntityTypes)
        {
            if (!IsGranted(entity.RequiredAccessGroups, grantedAccessGroups))
            {
                continue;
            }

            var filteredEntity = FilterEntityElements(entity, grantedAccessGroups);
            filteredEntityTypes.Add(filteredEntity);
        }

        var reachable = RemoveUnreachableEntityTypes(filteredEntityTypes, fullSchema.EntryPointName);

        return new HypermediaApiSchema
        {
            SchemaVersion = fullSchema.SchemaVersion,
            ApiVersion = fullSchema.ApiVersion,
            Title = fullSchema.Title,
            Description = fullSchema.Description,
            ExternalDocsUrl = fullSchema.ExternalDocsUrl,
            EntryPointName = fullSchema.EntryPointName,
            EntityTypes = reachable,
            Definitions = fullSchema.Definitions,
            DeclaredAccessGroups = null,
        };
    }

    /// <summary>
    /// Exclude mode: removes elements whose <c>RequiredAccessGroups</c> intersect with
    /// <paramref name="excludedAccessGroups"/>. Elements with no access group restriction (null) are kept.
    /// Entity types that become unreachable are also removed.
    /// <c>DeclaredAccessGroups</c> is stripped from the output.
    /// </summary>
    public static HypermediaApiSchema ExcludeAccessGroups(
        HypermediaApiSchema fullSchema,
        ISet<string> excludedAccessGroups)
    {
        var filteredEntityTypes = new List<EntityTypeSchema>();

        foreach (var entity in fullSchema.EntityTypes)
        {
            if (IsExcluded(entity.RequiredAccessGroups, excludedAccessGroups))
            {
                continue;
            }

            var filteredEntity = ExcludeEntityElements(entity, excludedAccessGroups);
            filteredEntityTypes.Add(filteredEntity);
        }

        var reachable = RemoveUnreachableEntityTypes(filteredEntityTypes, fullSchema.EntryPointName);

        return new HypermediaApiSchema
        {
            SchemaVersion = fullSchema.SchemaVersion,
            ApiVersion = fullSchema.ApiVersion,
            Title = fullSchema.Title,
            Description = fullSchema.Description,
            ExternalDocsUrl = fullSchema.ExternalDocsUrl,
            EntryPointName = fullSchema.EntryPointName,
            EntityTypes = reachable,
            Definitions = fullSchema.Definitions,
            DeclaredAccessGroups = null,
        };
    }

    /// <summary>
    /// Returns true when the element's required access groups are all contained in the granted set,
    /// or when the element has no access group restriction.
    /// </summary>
    private static bool IsGranted(IReadOnlyList<string>? requiredAccessGroups, ISet<string> grantedAccessGroups)
    {
        if (requiredAccessGroups == null || requiredAccessGroups.Count == 0)
        {
            return true;
        }

        return requiredAccessGroups.All(grantedAccessGroups.Contains);
    }

    /// <summary>
    /// Returns true when the element's required access groups intersect with the excluded set.
    /// </summary>
    private static bool IsExcluded(IReadOnlyList<string>? requiredAccessGroups, ISet<string> excludedAccessGroups)
    {
        if (requiredAccessGroups == null || requiredAccessGroups.Count == 0)
        {
            return false;
        }

        return requiredAccessGroups.Any(excludedAccessGroups.Contains);
    }

    private static EntityTypeSchema FilterEntityElements(
        EntityTypeSchema entity, ISet<string> grantedAccessGroups)
    {
        return new EntityTypeSchema
        {
            Name = entity.Name,
            Classes = entity.Classes,
            Title = entity.Title,
            Description = entity.Description,
            PropertiesSchema = entity.PropertiesSchema,
            RequiredAccessGroups = entity.RequiredAccessGroups,
            IsDeprecated = entity.IsDeprecated,
            DeprecationMessage = entity.DeprecationMessage,
            Links = entity.Links
                .Where(l => IsGranted(l.RequiredAccessGroups, grantedAccessGroups))
                .ToList(),
            Actions = entity.Actions
                .Where(a => IsGranted(a.RequiredAccessGroups, grantedAccessGroups))
                .ToList(),
            EmbeddedEntities = entity.EmbeddedEntities
                .Where(e => IsGranted(e.RequiredAccessGroups, grantedAccessGroups))
                .ToList(),
        };
    }

    private static EntityTypeSchema ExcludeEntityElements(
        EntityTypeSchema entity, ISet<string> excludedAccessGroups)
    {
        return new EntityTypeSchema
        {
            Name = entity.Name,
            Classes = entity.Classes,
            Title = entity.Title,
            Description = entity.Description,
            PropertiesSchema = entity.PropertiesSchema,
            RequiredAccessGroups = entity.RequiredAccessGroups,
            IsDeprecated = entity.IsDeprecated,
            DeprecationMessage = entity.DeprecationMessage,
            Links = entity.Links
                .Where(l => !IsExcluded(l.RequiredAccessGroups, excludedAccessGroups))
                .ToList(),
            Actions = entity.Actions
                .Where(a => !IsExcluded(a.RequiredAccessGroups, excludedAccessGroups))
                .ToList(),
            EmbeddedEntities = entity.EmbeddedEntities
                .Where(e => !IsExcluded(e.RequiredAccessGroups, excludedAccessGroups))
                .ToList(),
        };
    }

    /// <summary>
    /// Removes entity types that are not reachable from the entry point or any remaining link/embedded reference.
    /// An entity is reachable if it is the entry point, or if any remaining entity links to it or embeds it.
    /// </summary>
    private static List<EntityTypeSchema> RemoveUnreachableEntityTypes(
        List<EntityTypeSchema> entityTypes, string entryPointName)
    {
        var referencedNames = new HashSet<string>(StringComparer.Ordinal);

        if (!string.IsNullOrEmpty(entryPointName))
        {
            referencedNames.Add(entryPointName);
        }

        foreach (var entity in entityTypes)
        {
            foreach (var link in entity.Links)
            {
                referencedNames.Add(link.TargetName);
            }

            foreach (var embedded in entity.EmbeddedEntities)
            {
                referencedNames.Add(embedded.TargetName);
            }

            foreach (var action in entity.Actions)
            {
                if (action.ResultName != null)
                {
                    referencedNames.Add(action.ResultName);
                }
            }
        }

        return entityTypes
            .Where(e => referencedNames.Contains(e.Name))
            .ToList();
    }
}
