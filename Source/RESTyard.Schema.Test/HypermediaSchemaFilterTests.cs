using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using RESTyard.Schema;
using RESTyard.Schema.Model;
using Xunit;

namespace RESTyard.Schema.Test;

public class HypermediaSchemaFilterTests
{
    private static HypermediaApiSchema CreateTestSchema()
    {
        return new HypermediaApiSchema
        {
            SchemaVersion = "1.0.0",
            Title = "Test API",
            EntryPointName = "Entrypoint",
            DeclaredAccessGroups = ["admin", "read", "write"],
            EntityTypes =
            [
                new EntityTypeSchema
                {
                    Name = "Entrypoint",
                    Classes = ["Entrypoint"],
                    Links =
                    [
                        new LinkDescription { Relations = ["customers"], TargetName = "CustomersRoot" },
                        new LinkDescription { Relations = ["admin"], TargetName = "AdminDashboard", AccessGroups = ["admin"] },
                    ],
                },
                new EntityTypeSchema
                {
                    Name = "CustomersRoot",
                    Classes = ["CustomersRoot"],
                    Actions =
                    [
                        new ActionDescription { Name = "CreateQuery" },
                        new ActionDescription { Name = "DeleteAll", AccessGroups = ["admin"] },
                    ],
                    Links =
                    [
                        new LinkDescription { Relations = ["self"], TargetName = "CustomersRoot" },
                    ],
                    EmbeddedEntities =
                    [
                        new EmbeddedEntityDescription { Relations = ["item"], TargetName = "Customer" },
                        new EmbeddedEntityDescription { Relations = ["audit"], TargetName = "AuditLog", AccessGroups = ["admin"] },
                    ],
                },
                new EntityTypeSchema
                {
                    Name = "Customer",
                    Classes = ["Customer"],
                    AccessGroups = ["read"],
                    Actions =
                    [
                        new ActionDescription { Name = "Update", AccessGroups = ["write"] },
                    ],
                },
                new EntityTypeSchema
                {
                    Name = "AdminDashboard",
                    Classes = ["AdminDashboard"],
                    AccessGroups = ["admin"],
                },
                new EntityTypeSchema
                {
                    Name = "AuditLog",
                    Classes = ["AuditLog"],
                    AccessGroups = ["admin"],
                },
            ],
        };
    }

    // --- Include mode (ForAccessGroups) ---

    [Fact]
    public void ForAccessGroups_keeps_public_elements()
    {
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ForAccessGroups(schema, new HashSet<string>(["read"]));

        var entrypoint = filtered.EntityTypes.Should().Contain(e => e.Name == "Entrypoint").Which;
        entrypoint.Links.Should().ContainSingle(l => l.Relations.Contains("customers"));
        entrypoint.Links.Should().NotContain(l => l.Relations.Contains("admin"));
    }

    [Fact]
    public void ForAccessGroups_removes_entity_with_unsatisfied_groups()
    {
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ForAccessGroups(schema, new HashSet<string>(["read"]));

        filtered.EntityTypes.Should().NotContain(e => e.Name == "AdminDashboard");
        filtered.EntityTypes.Should().NotContain(e => e.Name == "AuditLog");
    }

    [Fact]
    public void ForAccessGroups_keeps_entity_with_satisfied_groups()
    {
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ForAccessGroups(schema, new HashSet<string>(["read", "admin"]));

        filtered.EntityTypes.Should().Contain(e => e.Name == "AdminDashboard");
        filtered.EntityTypes.Should().Contain(e => e.Name == "Customer");
    }

    [Fact]
    public void ForAccessGroups_filters_actions_within_entity()
    {
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ForAccessGroups(schema, new HashSet<string>(["read"]));

        var customersRoot = filtered.EntityTypes.Should().Contain(e => e.Name == "CustomersRoot").Which;
        customersRoot.Actions.Should().ContainSingle(a => a.Name == "CreateQuery");
        customersRoot.Actions.Should().NotContain(a => a.Name == "DeleteAll");
    }

    [Fact]
    public void ForAccessGroups_filters_embedded_entities()
    {
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ForAccessGroups(schema, new HashSet<string>(["read"]));

        var customersRoot = filtered.EntityTypes.Should().Contain(e => e.Name == "CustomersRoot").Which;
        customersRoot.EmbeddedEntities.Should().ContainSingle(e => e.Relations.Contains("item"));
        customersRoot.EmbeddedEntities.Should().NotContain(e => e.Relations.Contains("audit"));
    }

    [Fact]
    public void ForAccessGroups_strips_DeclaredAccessGroups()
    {
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ForAccessGroups(schema, new HashSet<string>(["read", "admin"]));

        filtered.DeclaredAccessGroups.Should().BeNull();
    }

    [Fact]
    public void ForAccessGroups_with_all_groups_keeps_everything()
    {
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ForAccessGroups(schema, new HashSet<string>(["admin", "read", "write"]));

        filtered.EntityTypes.Should().HaveCount(schema.EntityTypes.Count);
    }

    [Fact]
    public void ForAccessGroups_removes_unreachable_entities()
    {
        // AuditLog is only referenced via the "audit" embedded entity on CustomersRoot.
        // When filtering with "read", the "audit" embedded is removed, making AuditLog unreachable.
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ForAccessGroups(schema, new HashSet<string>(["read"]));

        filtered.EntityTypes.Should().NotContain(e => e.Name == "AuditLog");
    }

    // --- Exclude mode (ExcludeAccessGroups) ---

    [Fact]
    public void ExcludeAccessGroups_removes_admin_elements()
    {
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ExcludeAccessGroups(schema, new HashSet<string>(["admin"]));

        filtered.EntityTypes.Should().NotContain(e => e.Name == "AdminDashboard");
        filtered.EntityTypes.Should().NotContain(e => e.Name == "AuditLog");
    }

    [Fact]
    public void ExcludeAccessGroups_keeps_public_elements()
    {
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ExcludeAccessGroups(schema, new HashSet<string>(["admin"]));

        var entrypoint = filtered.EntityTypes.Should().Contain(e => e.Name == "Entrypoint").Which;
        entrypoint.Links.Should().ContainSingle(l => l.Relations.Contains("customers"));
    }

    [Fact]
    public void ExcludeAccessGroups_filters_actions_and_embedded()
    {
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ExcludeAccessGroups(schema, new HashSet<string>(["admin"]));

        var customersRoot = filtered.EntityTypes.Should().Contain(e => e.Name == "CustomersRoot").Which;
        customersRoot.Actions.Should().NotContain(a => a.Name == "DeleteAll");
        customersRoot.Actions.Should().ContainSingle(a => a.Name == "CreateQuery");
        customersRoot.EmbeddedEntities.Should().NotContain(e => e.Relations.Contains("audit"));
    }

    [Fact]
    public void ExcludeAccessGroups_strips_DeclaredAccessGroups()
    {
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ExcludeAccessGroups(schema, new HashSet<string>(["admin"]));

        filtered.DeclaredAccessGroups.Should().BeNull();
    }

    [Fact]
    public void ExcludeAccessGroups_with_empty_set_keeps_everything()
    {
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ExcludeAccessGroups(schema, new HashSet<string>());

        filtered.EntityTypes.Should().HaveCount(schema.EntityTypes.Count);
    }

    // --- Preserves metadata ---

    [Fact]
    public void Filter_preserves_schema_metadata()
    {
        var schema = CreateTestSchema();
        var filtered = HypermediaSchemaFilter.ForAccessGroups(schema, new HashSet<string>(["read"]));

        filtered.SchemaVersion.Should().Be("1.0.0");
        filtered.Title.Should().Be("Test API");
        filtered.EntryPointName.Should().Be("Entrypoint");
    }
}
