using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace Signals.Manifest.BuildTask;

/// <summary>
/// MSBuild task that generates a signal plugin manifest JSON for a given assembly.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Scans the specified assembly for a single <c>ISignalModule</c> implementation using <see cref="SignalsAssemblyLoader"/>.</item>
/// <item>Automatically creates a <see cref="SignalsManifest"/> using <see cref="SignalsManifestBuilder.CreateDefault"/>.</item>
/// <item>Writes the manifest JSON to the specified output path if it does not already exist.</item>
/// <item>Logs warnings for fields that may need manual review, such as ID, Name, Description, Author, and Dependencies.</item>
/// <item>Validates input paths and reports errors using the MSBuild logging system.</item>
/// <item>Safe to run even if no <c>ISignalModule</c> is found; it will simply skip generation.</item>
/// </list>
/// </remarks>
public sealed class GenerateSignalManifestTask : Task
{
    [Required]
    public string AssemblyPath { get; set; } = null!;

    [Required]
    public string OutputPath { get; set; } = null!;

    
    public override bool Execute()
    {
        try
        {
            ValidateInput();

            Log.LogMessage(MessageImportance.High, $"Generating signal manifest for: {AssemblyPath}");

            var loader = new SignalsAssemblyLoader(Log);
            var moduleType = loader.LoadModuleType(AssemblyPath);
            if (moduleType == null)
            {
                Log.LogMessage(MessageImportance.Low, "No ISignalModule found. Manifest not generated.");
                return true;
            }

            var manifest = SignalsManifestBuilder.CreateDefault(moduleType, AssemblyPath);

            var outputFile = FileUtils.GetOutputFilePath(OutputPath, manifest.Id + ".signal.json");

            if (File.Exists(outputFile))
            {
                Log.LogMessage(MessageImportance.High, $"Manifest already exists – skipping: {outputFile}");
                return true;
            }

            FileUtils.EnsureDirectory(OutputPath);
            FileUtils.WriteJson(outputFile, manifest);

            Log.LogWarning($"""
                            Signal manifest GENERATED automatically:
                            {outputFile}

                            ⚠ REVIEW REQUIRED:
                            - Id
                            - Name
                            - Description
                            - Author
                            - Dependencies
                            """);

            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, true);
            return false;
        }
    }

    /// <summary>
    /// Validates the input properties <see cref="AssemblyPath"/> and <see cref="OutputPath"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if required properties are empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the assembly DLL does not exist.</exception>
    private void ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(AssemblyPath))
            throw new InvalidOperationException("AssemblyPath is empty.");

        if (!File.Exists(AssemblyPath))
            throw new FileNotFoundException("Assembly not found.", AssemblyPath);

        if (string.IsNullOrWhiteSpace(OutputPath))
            throw new InvalidOperationException("OutputPath is empty.");
    }
}
