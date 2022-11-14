using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using Tsu;

namespace Loretta.Retargeting.Core
{
    internal static class SyntaxHelpers
    {
        public static Option<object?> GetConstantValue(SyntaxNode node)
        {
            if (node.IsKind(SyntaxKind.ParenthesizedExpression))
                return GetConstantValue(((ParenthesizedExpressionSyntax) node).Expression);
            else if (node is LiteralExpressionSyntax literal)
                return Option.Some(literal.Token.Value);
            else
                return Option.None<object?>();
        }
    }
}
