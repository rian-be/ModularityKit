using System.Runtime.Loader;
using Microsoft.Build.Utilities;

namespace Signals.Manifest.BuildTask;

/// <summary>
/// Loads assemblies in an isolated, collectible context to discover <c>ISignalModule</c> types.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Creates a temporary <see cref="AssemblyLoadContext"/> for each assembly to avoid locking files.</item>
/// <item>Scans the assembly for types implementing <c>ISignalModule</c>.</item>
/// <item>Ensures only one <c>ISignalModule</c> per assembly; logs an error if multiple implementations exist.</item>
/// <item>Unloads the assembly context after scanning to free resources and allow file overwrites.</item>
/// <item>Intended for use in MSBuild tasks generating or validating signal manifests.</item>
/// </list>
/// </remarks>
public sealed class SignalsAssemblyLoader(TaskLoggingHelper log)
{
    public Type? LoadModuleType(string assemblyPath)
    {
        var alc = new AssemblyLoadContext("SignalsLoader", isCollectible: true);
        try
        {
            var asm = alc.LoadFromAssemblyPath(assemblyPath);

            var modules = asm.GetTypes()
                .Where(t => !t.IsAbstract && t.GetInterfaces().Any(i => i.Name == "ISignalModule"))
                .ToArray();

            switch (modules.Length)
            {
                case 0:
                    return null;
                case > 1:
                    log.LogError("Multiple ISignalModule implementations found. Only one is allowed per signal.");
                    return null;
                default:
                    return modules[0];
            }
        }
        finally
        {
            alc.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}