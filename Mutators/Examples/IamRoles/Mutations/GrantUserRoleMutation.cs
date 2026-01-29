using ModularityKit.Mutators.Abstractions.Changes;
using ModularityKit.Mutators.Abstractions.Context;
using ModularityKit.Mutators.Abstractions.Engine;
using ModularityKit.Mutators.Abstractions.Intent;
using ModularityKit.Mutators.Abstractions.Results;
using Mutators.Examples.IamRoles.State;

namespace Mutators.Examples.IamRoles.Mutations;

/// <summary>
/// Mutation that grants role to user in current <see cref="UserPermissionsState"/>.
/// </summary>
internal sealed record GrantUserRoleMutation(
    string UserId,
    string Role,
    MutationContext Context
) : IMutation<UserPermissionsState>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "GrantUserRole",
        Category = "Security",
        RiskLevel = MutationRiskLevel.Critical,
        Description = "Grants role to a user"
    };

    public ValidationResult Validate(UserPermissionsState state)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(UserId))
            result.AddError("UserId", "UserId cannot be empty");

        if (string.IsNullOrWhiteSpace(Role))
            result.AddError("Role", "Role cannot be empty");

        if (state.RolesByUser.TryGetValue(UserId, out var roles) &&
            roles.Contains(Role))
            result.AddError("Role", "User already has this role");

        return result;
    }

    public MutationResult<UserPermissionsState> Apply(UserPermissionsState state)
    {
        var rolesByUser = state.RolesByUser
            .ToDictionary(kv => kv.Key, kv => new HashSet<string>(kv.Value));

        if (!rolesByUser.TryGetValue(UserId, out var roles))
        {
            roles = [];
            rolesByUser[UserId] = roles;
        }

        roles.Add(Role);

        var newState = state with { RolesByUser = rolesByUser };

        var changes = ChangeSet.Single(
            StateChange.Added($"RolesByUser.{UserId}", Role)
        );

        return MutationResult<UserPermissionsState>.Success(newState, changes);
    }

    public MutationResult<UserPermissionsState> Simulate(UserPermissionsState state)
        => Apply(state);
}
