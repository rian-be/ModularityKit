namespace ModularityKit.Mutators.Abstractions.Changes;

/// <summary>
/// Represents the type of state change in a <see cref="StateChange"/>.
/// Used for tracking and auditing modifications in the system.
/// </summary>
public enum ChangeType
{
    /// <summary>
    /// The value was modified (old value updated to new value).
    /// </summary>
    Modified = 0,

    /// <summary>
    /// A new value was added.
    /// </summary>
    Added = 1,

    /// <summary>
    /// An existing value was removed.
    /// </summary>
    Removed = 2,

    /// <summary>
    /// The value was completely replaced (old value replaced by new value).
    /// </summary>
    Replaced = 3,

    /// <summary>
    /// An element was moved within a collection.
    /// </summary>
    Moved = 4
}
