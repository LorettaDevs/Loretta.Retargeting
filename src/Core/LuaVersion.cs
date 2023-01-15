// See https://aka.ms/new-console-template for more information

using Loretta.CodeAnalysis.Lua;

namespace Loretta.Retargeting.Core
{
    /// <summary>
    /// The set of supported lua versions.
    /// </summary>
    public enum LuaVersion : byte
    {
        Lua51,
        Lua52,
        Lua53,
        Lua54,
        LuaJIT20,
        LuaJIT21,
        FiveM,
        GMod,
        GLua = GMod,
        Luau,
        RobloxLua = Luau
    }

    public static class LuaVersionExtensions
    {
        public static LuaSyntaxOptions GetSyntaxOptions(this LuaVersion version) =>
            version switch
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
            };
    }
}
