using System.Collections.Immutable;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;

namespace Loretta.Retargeting.Core.Converters
{
    [Converter(LuaVersion.Lua53, LuaVersion.FiveM)]
    internal sealed class Lua53ToFivemConverter : ConverterBase
    {
        public override SyntaxNode Convert(LuaSyntaxNode node, out ImmutableArray<Diagnostic> diagnostics)
        {
            diagnostics = ImmutableArray<Diagnostic>.Empty;
            return node;
        }
    }
}
