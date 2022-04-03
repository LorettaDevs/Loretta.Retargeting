using System.Collections.Immutable;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.SymbolDisplay;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.Retargeting.Core.Converters
{
    [Converter(LuaVersion.FiveM, LuaVersion.Lua53)]
    internal sealed class FivemToLua53Converter : ConverterBase
    {
        public override SyntaxNode Convert(LuaSyntaxNode node, out ImmutableArray<Diagnostic> diagnostics)
        {
            diagnostics = ImmutableArray<Diagnostic>.Empty;
            return Rewriter.Instance.Visit(node);
        }

        private sealed class Rewriter : LuaSyntaxRewriter
        {
            public static readonly Rewriter Instance = new();

            public override SyntaxNode? VisitLiteralExpression(LiteralExpressionSyntax node)
            {
                if (node.IsKind(SyntaxKind.HashStringLiteralExpression))
                {
                    var text = node.Token.Text;
                    long value = (uint) node.Token.Value!;

                    var comment = CreateHashPrefixComment(text);
                    var newToken = SyntaxFactory.Literal(
                        node.GetLeadingTrivia().Add(comment).Add(SyntaxFactory.Space),
                        ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None),
                        value,
                        node.GetTrailingTrivia());
                    var newNode = SyntaxFactory.LiteralExpression(
                        SyntaxKind.NumericalLiteralExpression,
                        newToken);

                    return newNode;
                }
                return base.VisitLiteralExpression(node);
            }

            private static SyntaxTrivia CreateHashPrefixComment(string text)
            {
                var builder = StringBuilderPool.GetBuilder();
                builder.Append($" {text} = ");
                Helpers.TurnIntoMultiLineComment(builder);
                var comment = SyntaxFactory.Comment(StringBuilderPool.ToStringAndFree(builder));
                return comment;
            }
        }
    }
}
