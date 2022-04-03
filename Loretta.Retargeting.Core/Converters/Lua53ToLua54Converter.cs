using System.Collections.Immutable;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.Retargeting.Core.Converters
{
    [Converter(LuaVersion.Lua53, LuaVersion.Lua54)]
    internal sealed class Lua53ToLua54Converter : ConverterBase
    {
        public override SyntaxNode Convert(LuaSyntaxNode node, out ImmutableArray<Diagnostic> diagnostics)
        {
            var script = new Script(ImmutableArray.Create(node.SyntaxTree));
            var rewriter = new Rewriter(script);
            diagnostics = ImmutableArray<Diagnostic>.Empty;
            return rewriter.Visit(node);
        }

        private sealed class Rewriter : LuaSyntaxRewriter
        {
            private readonly Script _script;

            public Rewriter(Script script)
            {
                _script = script;
            }

            public override SyntaxNode? VisitLocalDeclarationName(LocalDeclarationNameSyntax node)
            {
                var variable = _script.GetVariable(node);
                if (variable is not null && IsConst(variable, node.Parent!))
                {
                    var trailing = node.GetTrailingTrivia();
                    node = node.WithoutTrailingTrivia();
                    node = node.WithAttribute(SyntaxFactory.VariableAttribute("const"));
                    node = node.WithTrailingTrivia(trailing);
                    return node;
                }
                return base.VisitLocalDeclarationName(node);
            }

            private static bool IsConst(IVariable variable, SyntaxNode declaration)
            {
                if (!variable.WriteLocations.Any())
                    return true;
                if (variable.WriteLocations.Count() == 1 && variable.WriteLocations.Single() == declaration)
                    return true;
                return false;
            }
        }
    }
}
