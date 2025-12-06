using System.Reflection;

namespace Signals.Manifest.BuildTask;

/// <summary>
/// Provides utility methods to build <see cref="SignalsManifest"/> instances for signal modules.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Generates default plugin manifests from an assembly and a module type.</item>
/// <item>Infers plugin ID, name, version, and entry point automatically.</item>
/// <item>Used in MSBuild tasks or tooling to auto-generate plugin manifests without manually writing JSON.</item>
/// <item>Fills optional fields like <see cref="SignalsManifest.Author"/>, <see cref="SignalsManifest.Description"/>, and <see cref="SignalsManifest.ApiVersion"/> with default values.</item>
/// </list>
/// </remarks>
public static class SignalsManifestBuilder
{
    public static SignalsManifest CreateDefault(Type moduleType, string assemblyPath)
    {
        var asm = Assembly.LoadFrom(assemblyPath);
        var assemblyName = asm.GetName();

        var id = moduleType.Namespace ?? moduleType.Name;
        var name = moduleType.Name;

        var version =
            assemblyName.Version?.ToString() ??
            asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
            "1.0.0";

        return new SignalsManifest
        {
            Id = id,
            Name = name,
            Version = version,
            EntryPoint = moduleType.FullName!,
            Dll = Path.GetFileName(assemblyPath),
            Author = "Unknown",
            Description = $"AUTO-GENERATED manifest for {name}",
            ApiVersion = "1.0",
            Dependencies = []
        };
    }
}