using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.RetargettingCompiler.Core.Converters
{
    [Converter(LuaVersion.Lua54, LuaVersion.Lua53)]
    internal sealed class Lua54ToLua53Converter : LuaSyntaxRewriter
    {
        public override SyntaxNode? VisitLocalDeclarationName(LocalDeclarationNameSyntax node)
        {
            if (node.Attribute is not null)
            {
                var attributeName = node.AttributeName;
                node = node.WithAttribute(null);

                var builder = StringBuilderPool.GetBuilder();
                builder.Append(" <")
                       .Append(attributeName)
                       .Append("> ");
                Helpers.TurnIntoMultiLineComment(builder);
                var comment = SyntaxFactory.Comment(StringBuilderPool.ToStringAndFree(builder));

                node = node.WithTrailingTrivia(
                    node.GetTrailingTrivia().Add(SyntaxFactory.Space).Add(comment));

                return node;
            }

            return base.VisitLocalDeclarationName(node);
        }
    }
}
