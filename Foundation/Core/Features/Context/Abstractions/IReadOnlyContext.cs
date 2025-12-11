namespace Core.Features.Context.Abstractions;

/// <summary>
/// Represents a read-only variant of <see cref="IContext"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Extends <see cref="IContext"/> without adding any mutable members.</item>
/// <item>Used to expose context information safely to consumers that must not modify it.</item>
/// <item>Provides a clear semantic boundary between mutable and immutable context access.</item>
/// </list>
/// </remarks>
public interface IReadOnlyContext : IContext 
{

}