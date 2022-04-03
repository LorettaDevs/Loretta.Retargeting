// See https://aka.ms/new-console-template for more information

namespace Loretta.RetargettingCompiler.Core
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
        RobloxLua = Luau,

        MinVersion = Lua51,
        MaxVersion = Luau
    }
}
