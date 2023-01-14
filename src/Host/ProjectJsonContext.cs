using System.Text.Json.Serialization;
using Loretta.Retargeting.Host.Model;

namespace Loretta.Retargeting.Host
{
    [JsonSourceGenerationOptions(
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        GenerationMode = JsonSourceGenerationMode.Metadata,
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = true,
        IncludeFields = false,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        WriteIndented = true)]
    [JsonSerializable(typeof(ProjectFile))]
    internal partial class ProjectJsonContext : JsonSerializerContext
    {
    }
}
