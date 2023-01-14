using System.Text.Json.Serialization;
using Loretta.CodeAnalysis.Lua;
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
        Files = files ?? throw new ArgumentNullException(nameof(files));
        InputVersion = inputVersion;
        OutputVersion = outputVersion;
        OutputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));

        InputParseOptions = GetParseOptions(inputVersion);
        OutputParseOptions = GetParseOptions(outputVersion);
    }

    public LuaVersion InputVersion { get; }
    public LuaVersion OutputVersion { get; }
    public string OutputPath { get; }
    public IEnumerable<string> Files { get; }

    [JsonIgnore]
    public LuaParseOptions InputParseOptions { get; }
    [JsonIgnore]
    public LuaParseOptions OutputParseOptions { get; }

    private static LuaParseOptions GetParseOptions(LuaVersion version)
    {
        return new LuaParseOptions(version switch
        {
            LuaVersion.Lua51 => LuaSyntaxOptions.Lua51,
            LuaVersion.Lua52 => LuaSyntaxOptions.Lua52,
            LuaVersion.Lua53 => LuaSyntaxOptions.Lua53,
            LuaVersion.Lua54 => LuaSyntaxOptions.Lua54,
            LuaVersion.LuaJIT20 => LuaSyntaxOptions.LuaJIT20,
            LuaVersion.LuaJIT21 => LuaSyntaxOptions.LuaJIT21,
            LuaVersion.GMod => LuaSyntaxOptions.GMod,
            LuaVersion.FiveM => LuaSyntaxOptions.FiveM,
            LuaVersion.Luau => LuaSyntaxOptions.Luau,
            _ => throw new ArgumentException($"Version {version} is not a valid lua version."),
        });
    }
}
