using System.Collections.Immutable;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;

namespace Loretta.Retargeting.Core.Converters
{
    internal abstract class ConverterBase
    {
        public abstract SyntaxNode Convert(LuaSyntaxNode node, out ImmutableArray<Diagnostic> diagnostics);
    }
}
