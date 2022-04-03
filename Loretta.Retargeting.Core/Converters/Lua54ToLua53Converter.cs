using System.Collections.Immutable;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.Retargeting.Core.Converters
{
    [Converter(LuaVersion.Lua54, LuaVersion.Lua53)]
    internal sealed class Lua54ToLua53Converter : ConverterBase
    {
        public override SyntaxNode Convert(SyntaxNode node, out ImmutableArray<Diagnostic> diagnostics)
        {
            diagnostics = ImmutableArray<Diagnostic>.Empty;
            return Rewriter.Instance.Visit(node);
        }

        private class Rewriter : LuaSyntaxRewriter
        {
            public static readonly Rewriter Instance = new();

            public override SyntaxNode? VisitLocalDeclarationName(LocalDeclarationNameSyntax node)
            {
                if (node.Attribute is not null)
                {
                    var attributeName = node.AttributeName;
                    var trailing = node.GetTrailingTrivia();
                    node = node.WithAttribute(null);

                    var builder = StringBuilderPool.GetBuilder();
                    builder.Append($" <{attributeName}> ");
                    Helpers.TurnIntoMultiLineComment(builder);
                    var comment = SyntaxFactory.Comment(StringBuilderPool.ToStringAndFree(builder));

                    node = node.WithTrailingTrivia(
                        trailing.Prepend(comment).Prepend(SyntaxFactory.Space));

                    return node;
                }

                return base.VisitLocalDeclarationName(node);
            }
        }
    }
}
