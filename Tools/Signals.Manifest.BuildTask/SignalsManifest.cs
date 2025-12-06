namespace Signals.Manifest.BuildTask;

/// <summary>
/// Represents the metadata of a signal plugin used during build tasks.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Contains all necessary information to generate a plugin manifest JSON.</item>
/// <item>Used internally in MSBuild tasks for signals to track plugin ID, version, entry point, and dependencies.</item>
/// <item>Includes optional metadata such as author, description, and API version.</item>
/// <item>Dependencies are represented as an array of plugin IDs.</item>
/// </list>
/// </remarks>
public sealed class SignalsManifest
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Version { get; set; }
    public required string EntryPoint { get; set; }
    public required string Dll { get; set; }

    public string? Author { get; set; }
    public string? Description { get; set; }
    public string? ApiVersion { get; set; }

    public string[] Dependencies { get; set; } = [];
}