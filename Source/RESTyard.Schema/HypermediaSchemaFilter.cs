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
    /// Include mode: keeps elements where any of the element's access groups is in
    /// <paramref name="grantedAccessGroups"/> (OR semantics), plus elements with no restriction (null).
    /// Entity types that don't match are removed entirely.
    /// Unreachable entity types are also removed.
    /// <c>DeclaredAccessGroups</c> is stripped from the output.
    /// </summary>
    public static HypermediaApiSchema ForAccessGroups(
        HypermediaApiSchema fullSchema,
        ISet<string> grantedAccessGroups)
    {
        var filteredEntityTypes = new List<EntityTypeSchema>();

        foreach (var entity in fullSchema.EntityTypes)
        {
            if (!IsGranted(entity.AccessGroups, grantedAccessGroups))
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
    /// Exclude mode: removes elements whose <c>AccessGroups</c> intersect with
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
            if (IsExcluded(entity.AccessGroups, excludedAccessGroups))
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
    /// Returns true when any of the element's access groups is contained in the granted set (OR semantics),
    /// or when the element has no access group restriction.
    /// </summary>
    private static bool IsGranted(IReadOnlyList<string>? accessGroups, ISet<string> grantedAccessGroups)
    {
        if (accessGroups == null || accessGroups.Count == 0)
        {
            return true;
        }

        return accessGroups.Any(grantedAccessGroups.Contains);
    }

    /// <summary>
    /// Returns true when the element's required access groups intersect with the excluded set.
    /// </summary>
    private static bool IsExcluded(IReadOnlyList<string>? accessGroups, ISet<string> excludedAccessGroups)
    {
        if (accessGroups == null || accessGroups.Count == 0)
        {
            return false;
        }

        return accessGroups.Any(excludedAccessGroups.Contains);
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
            AccessGroups = entity.AccessGroups,
            IsDeprecated = entity.IsDeprecated,
            DeprecationMessage = entity.DeprecationMessage,
            Links = entity.Links
                .Where(l => IsGranted(l.AccessGroups, grantedAccessGroups))
                .ToList(),
            Actions = entity.Actions
                .Where(a => IsGranted(a.AccessGroups, grantedAccessGroups))
                .ToList(),
            EmbeddedEntities = entity.EmbeddedEntities
                .Where(e => IsGranted(e.AccessGroups, grantedAccessGroups))
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
            AccessGroups = entity.AccessGroups,
            IsDeprecated = entity.IsDeprecated,
            DeprecationMessage = entity.DeprecationMessage,
            Links = entity.Links
                .Where(l => !IsExcluded(l.AccessGroups, excludedAccessGroups))
                .ToList(),
            Actions = entity.Actions
                .Where(a => !IsExcluded(a.AccessGroups, excludedAccessGroups))
                .ToList(),
            EmbeddedEntities = entity.EmbeddedEntities
                .Where(e => !IsExcluded(e.AccessGroups, excludedAccessGroups))
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
