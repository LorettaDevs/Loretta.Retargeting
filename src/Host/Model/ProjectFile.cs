using System.Text.Json.Serialization;
using Loretta.Retargeting.Core;

namespace Loretta.Retargeting.Host.Model;

public sealed class ProjectFile
{
    [JsonConstructor]
    public ProjectFile(
        LuaVersion inputVersion,
        LuaVersion outputVersion,
        string outputPath,
        IEnumerable<string> files)
    {
        InputVersion = inputVersion;
        OutputVersion = outputVersion;
        Files = files ?? throw new ArgumentNullException(nameof(files));
        OutputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
    }

    public LuaVersion InputVersion { get; }
    public LuaVersion OutputVersion { get; }
    public string OutputPath { get; }
    public IEnumerable<string> Files { get; }
}
