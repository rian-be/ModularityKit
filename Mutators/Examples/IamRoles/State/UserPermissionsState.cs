namespace Mutators.Examples.IamRoles.State;

/// <summary>
/// Represents the current user permissions state, mapping users to their roles.
/// </summary>
public sealed record UserPermissionsState
{
    public required IReadOnlyDictionary<string, HashSet<string>> RolesByUser { get; init; }
}