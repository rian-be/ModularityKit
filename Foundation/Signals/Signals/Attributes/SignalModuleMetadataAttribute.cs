namespace Signals.Attributes;

/// <summary>
/// Provides descriptive metadata for a signal module.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Used to attach human-readable information to a signal module.</item>
/// <item>Intended for discovery, diagnostics, and manifest generation.</item>
/// <item>Applied only to module classes implementing <c>ISignalModule</c>.</item>
/// <item>Not inherited by derived types to avoid accidental metadata propagation.</item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SignalModuleMetadataAttribute(
    string name,
    string author,
    string version,
    string description = "")
    : Attribute
{
    public string Name { get; } = name;
    public string Author { get; } = author;
    public string Version { get; } = version;
    public string Description { get; } = description;
}