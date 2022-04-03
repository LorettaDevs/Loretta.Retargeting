using System.Collections.Immutable;
using Loretta.CodeAnalysis;

namespace Loretta.Retargeting.Core.Converters
{
    internal abstract class ConverterBase
    {
        public abstract SyntaxNode Convert(SyntaxNode node, out ImmutableArray<Diagnostic> diagnostics);
    }
}
