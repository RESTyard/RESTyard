#nullable enable
using RESTyard.Generator.Test.Output;

namespace server._csharp._v4;
public static class PermissionScopes
{
    public const string BASIC = "Basic";
}

public partial class BaseHto
{
    private static readonly string[] allowedScopes = new string[]
    {
        PermissionScopes.BASIC,
    };
    public static bool IsInScope(IEnumerable<string> currentScopes) => !allowedScopes.Except(currentScopes).Any();
}

public partial class ChildHto
{
}

public partial class DerivedHto
{
}

public partial class SecondLevelDerivedHto
{
}

public partial class NoSelfLinkHto
{
}

public partial class QueryHto
{
}