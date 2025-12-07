using System.Text.Json.Serialization;

namespace Signals.Manifest.BuildTask.Models;

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