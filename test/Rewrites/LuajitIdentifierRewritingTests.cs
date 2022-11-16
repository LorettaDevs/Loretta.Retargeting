using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.Retargeting.Core;

namespace Loretta.Retargeting.Test.Rewrites
{
    public class LuajitIdentifierRewritingTests : RewritingTestsBase
    {
        [Fact]
        public void RetargetingRewriter_DoesNotMarksIdentifiers_WhenTheyDoNotHaveLuajitOnlyCharacters()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions.With(useLuaJitIdentifierRules: false);

            var node = AssertRewrite(
                preOptions,
                postOptions,
                """
                local x
                """,
                """
                local x
                """);

            var compilationUnit = Assert.IsType<CompilationUnitSyntax>(node);
            var singleStatement = Assert.Single(compilationUnit.Statements.Statements);
            var localDeclaration = Assert.IsType<LocalVariableDeclarationStatementSyntax>(singleStatement);
            var localDeclarationName = Assert.Single(localDeclaration.Names);
            var identifierToken = localDeclarationName.IdentifierName.Identifier;
            Assert.False(identifierToken.HasAnnotation(RetargetingAnnotations.IdentifierHasLuajitOnlyChars));
        }

        [Fact]
        public void RetargetingRewriter_MarksIdentifiers_WhenTheyHaveLuajitOnlyCharacters()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions.With(useLuaJitIdentifierRules: false);

            var node = RewriteNode(
                preOptions,
                postOptions,
                $"""
                local x{'\x7F'}
                """);

            var compilationUnit = Assert.IsType<CompilationUnitSyntax>(node);
            var singleStatement = Assert.Single(compilationUnit.Statements.Statements);
            var localDeclaration = Assert.IsType<LocalVariableDeclarationStatementSyntax>(singleStatement);
            var localDeclarationName = Assert.Single(localDeclaration.Names);
            var identifierToken = localDeclarationName.IdentifierName.Identifier;
            Assert.True(identifierToken.HasAnnotation(RetargetingAnnotations.IdentifierHasLuajitOnlyChars));
        }
    }
}
