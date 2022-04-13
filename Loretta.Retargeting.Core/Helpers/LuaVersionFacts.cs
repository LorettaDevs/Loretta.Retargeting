using Tsu;

namespace Loretta.Retargeting.Core
{
    internal static class LuaVersionFacts
    {
        public static Option<string> BitLibraryGlobalName(LuaVersion version)
        {
            return version switch
            {
                LuaVersion.Lua53 or LuaVersion.Lua54 or LuaVersion.FiveM or LuaVersion.Luau => "bit32",
                LuaVersion.LuaJIT20 or LuaVersion.LuaJIT21 or LuaVersion.GMod => "bit",
                _ => Option.None<string>()
            };
        }

        public static bool HasBitLibrary(LuaVersion version) =>
            BitLibraryGlobalName(version).IsSome;

        public static string GetHumanName(LuaVersion version)
        {
            return version switch
            {
                LuaVersion.Lua51 => "Lua 5.1",
                LuaVersion.Lua52 => "Lua 5.2",
                LuaVersion.Lua53 => "Lua 5.3",
                LuaVersion.Lua54 => "Lua 5.4",
                LuaVersion.FiveM => "FiveM",
                LuaVersion.LuaJIT20 => "LuaJIT 2.0",
                LuaVersion.LuaJIT21 => "LuaJIT 2.1",
                LuaVersion.GMod => "Garry's Mod",
                LuaVersion.Luau => "Luau (Roblox Lua)",
                _ => throw new ArgumentException("Invalid lua version.", nameof(version))
            };
        }
    }
}
