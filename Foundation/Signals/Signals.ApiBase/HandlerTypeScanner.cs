using System.Reflection;

namespace Signals.ApiBase;

/// <summary>
/// Scans an assembly for handler types.
/// </summary>
public sealed class HandlerTypeScanner(Assembly assembly)
{
    public Type[] GetHandlers()
        => assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .ToArray();
}