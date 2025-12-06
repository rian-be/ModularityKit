using System.Text.Json;

namespace Signals.Manifest.BuildTask;

/// <summary>
/// Provides utility methods for file and directory operations in the manifest build process.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Ensures target directories exist before writing files.</item>
/// <item>Generates full output file paths from a directory and file name.</item>
/// <item>Serializes objects to JSON and writes them to files with indented formatting.</item>
/// <item>Used internally in manifest generation tasks.</item>
/// </list>
/// </remarks>
public static class FileUtils
{
    public static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public static string GetOutputFilePath(string outputPath, string fileName)
        => Path.Combine(outputPath, fileName);

    public static void WriteJson<T>(string path, T obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}