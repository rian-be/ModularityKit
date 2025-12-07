using Signals.Runtime.Interfaces;
using Signals.Runtime.Manifest;

namespace Signals.Runtime.Loader;

/// <summary>
/// Resolves plugin dependencies and sorts plugin manifests in a valid load order using topological sorting.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="IModuleDependencyResolver"/> to ensure that plugins are loaded respecting their dependencies.</item>
/// <item>Detects missing dependencies and throws <see cref="InvalidOperationException"/> if a required plugin is not present.</item>
/// <item>Detects cyclic dependencies and throws <see cref="InvalidOperationException"/> if cycles are found.</item>
/// <item>Produces an array of <see cref="ModuleManifest"/> sorted so that each plugin appears after its dependencies.</item>
/// <item>Supports case-insensitive plugin IDs.</item>
/// </list>
/// </remarks>

public sealed class TopologicalDependencyResolver : IModuleDependencyResolver
{
    /// <inheritdoc />
    public ModuleManifest[] SortByDependencies(IEnumerable<ModuleManifest> manifests)
    {
        var dict = manifests.ToDictionary(m => m.Id, StringComparer.OrdinalIgnoreCase);
        var visited = new Dictionary<string, int>();
        var result = new List<ModuleManifest>();

        void Visit(string id)
        {
            if (!dict.TryGetValue(id, out var value))
                throw new InvalidOperationException($"Missing dependency: {id}");

            if (visited.TryGetValue(id, out var state))
            {
                if (state == 1) throw new InvalidOperationException("Cyclic dependency detected");
                if (state == 2) return; // already visited
            }

            visited[id] = 1;

            foreach (var depId in value.Dependencies)
                Visit(depId);

            visited[id] = 2;
            result.Add(value);
        }

        foreach (var m in dict.Values)
            if (!visited.ContainsKey(m.Id))
                Visit(m.Id);

        return result.ToArray();
    }
}