using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Results;
using Mutators.Examples.IamRoles.State;

namespace Mutators.Examples.IamRoles.Mutations;

/// <summary>
/// Mutation that revokes role from user in the current <see cref="UserPermissionsState"/>.
/// </summary>
internal sealed record RevokeUserRoleMutation(
    string UserId,
    string Role,
    MutationContext Context
) : IMutation<UserPermissionsState>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "RevokeUserRole",
        Category = "Security",
        RiskLevel = MutationRiskLevel.High,
        Description = "Revokes a role from a user"
    };

    public ValidationResult Validate(UserPermissionsState state)
    {
        var result = new ValidationResult();

        if (!state.RolesByUser.TryGetValue(UserId, out var roles) ||
            !roles.Contains(Role))
            result.AddError("Role", "User does not have this role");

        return result;
    }

    public MutationResult<UserPermissionsState> Apply(UserPermissionsState state)
    {
        var rolesByUser = state.RolesByUser
            .ToDictionary(kv => kv.Key, kv => new HashSet<string>(kv.Value));

        rolesByUser[UserId].Remove(Role);

        var newState = state with { RolesByUser = rolesByUser };

        var changes = ChangeSet.Single(
            StateChange.Removed($"RolesByUser.{UserId}", Role)
        );

        return MutationResult<UserPermissionsState>.Success(newState, changes);
    }

    public MutationResult<UserPermissionsState> Simulate(UserPermissionsState state)
        => Apply(state);
}
