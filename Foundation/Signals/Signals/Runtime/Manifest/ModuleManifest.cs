using System.Text.Json.Serialization;

namespace Signals.Runtime.Manifest;

/// <summary>
/// Represents the manifest of a module, containing metadata, entry point, and dependency information.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Used by module loaders to identify, load, and activate modules dynamically.</item>
/// <item>Contains metadata such as <see cref="Id"/>, <see cref="Name"/>, <see cref="Version"/>, <see cref="Author"/>, and <see cref="Description"/>.</item>
/// <item><see cref="EntryPoint"/> specifies the fully-qualified type name of the class implementing <see cref="Signals.Core.Modules.ISignalModule"/>.</item>
/// <item><see cref="ApiVersion"/> can be used to enforce compatibility with the host system.</item>
/// <item><see cref="Dependencies"/> lists other modules that must be loaded first to satisfy runtime requirements.</item>
/// <item>Supports serialization/deserialization via JSON for manifest files.</item>
/// </list>
/// </remarks>
public sealed record ModuleManifest
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("entryPoint")]
    public required string EntryPoint { get; init; }

    [JsonPropertyName("dll")]
    public required string Dll { get; init; }

    [JsonPropertyName("author")]
    public string? Author { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("apiVersion")]
    public string? ApiVersion { get; init; }

    [JsonPropertyName("dependencies")]
    public string[] Dependencies { get; init; } = [];
}