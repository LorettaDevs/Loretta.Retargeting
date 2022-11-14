using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.Retargeting.Core;

namespace Loretta.Retargeting.Test.Rewrites
{
    public class BitwiseIntegerRewritingTests : RewritingTestsBase
    {
        [Theory]
        [InlineData("local x = ~a")]
        [InlineData("local x = a & b")]
        [InlineData("local x = a | b")]
        [InlineData("local x = a ~ b")]
        [InlineData("local x = a >> b")]
        [InlineData("local x = a << b")]
        public void RetargetingRewriter_GeneratesAnnotationsForBitwiseOperators_WhenTargetVersionDoesNotHaveTheBitLibrary(string text)
        {
            var preOptions = LuaSyntaxOptions.Lua54;
            var postOptions = LuaSyntaxOptions.Lua51;
            var node = RewriteNode(
                preOptions,
                postOptions,
                text,
                version: LuaVersion.Lua51);

            var compilationUnit = Assert.IsType<CompilationUnitSyntax>(node);
            var singleStatement = Assert.Single(compilationUnit.Statements.Statements);
            var localDeclaration = Assert.IsType<LocalVariableDeclarationStatementSyntax>(singleStatement);
            Assert.NotNull(localDeclaration.EqualsValues);
            var expression = Assert.Single(localDeclaration.EqualsValues!.Values);

            Assert.True(expression.HasAnnotation(RetargetingAnnotations.TargetVersionHasNoBitLibrary));
        }

        [Fact]
        public void RetargetingRewriter_GeneratesAnnotationsForOperandsOfUnaryExpressions_WhenTheOperandsExceed32Bits()
        {
            var preOptions = LuaSyntaxOptions.Lua54;
            var postOptions = LuaSyntaxOptions.Lua52;
            var node = RewriteNode(
                preOptions,
                postOptions,
                """
                local x = ~0x100000000
                """,
                version: LuaVersion.Lua52);

            var compilationUnit = Assert.IsType<CompilationUnitSyntax>(node);
            var singleStatement = Assert.Single(compilationUnit.Statements.Statements);
            var localDeclaration = Assert.IsType<LocalVariableDeclarationStatementSyntax>(singleStatement);
            Assert.NotNull(localDeclaration.EqualsValues);
            var expression = Assert.Single(localDeclaration.EqualsValues!.Values);
            var functionCall = Assert.IsType<FunctionCallExpressionSyntax>(expression);
            var arguments = Assert.IsType<ExpressionListFunctionArgumentSyntax>(functionCall.Argument);
            var argument = Assert.Single(arguments.Expressions);

            Assert.True(argument.HasAnnotation(RetargetingAnnotations.OperandHasMoreThan32Bits));
        }

        [Theory]
        [InlineData("local x = 0x100000000 & 0x100000000")]
        [InlineData("local x = 0x100000000 | 0x100000000")]
        [InlineData("local x = 0x100000000 ~ 0x100000000")]
        [InlineData("local x = 0x100000000 >> 0x100000000")]
        [InlineData("local x = 0x100000000 << 0x100000000")]
        public void RetargetingRewriter_GeneratesAnnotationsForOperandsOfBinaryExpressions_WhenTheOperandsExceed32Bits(string text)
        {
            var preOptions = LuaSyntaxOptions.Lua54;
            var postOptions = LuaSyntaxOptions.Lua52;
            var node = RewriteNode(
                preOptions,
                postOptions,
                text,
                version: LuaVersion.Lua52);

            var compilationUnit = Assert.IsType<CompilationUnitSyntax>(node);
            var singleStatement = Assert.Single(compilationUnit.Statements.Statements);
            var localDeclaration = Assert.IsType<LocalVariableDeclarationStatementSyntax>(singleStatement);
            Assert.NotNull(localDeclaration.EqualsValues);
            var expression = Assert.Single(localDeclaration.EqualsValues!.Values);
            var functionCall = Assert.IsType<FunctionCallExpressionSyntax>(expression);
            var arguments = Assert.IsType<ExpressionListFunctionArgumentSyntax>(functionCall.Argument);
            Assert.Equal(2, arguments.Expressions.Count);

            Assert.True(arguments.Expressions[0].HasAnnotation(RetargetingAnnotations.OperandHasMoreThan32Bits));
            Assert.True(arguments.Expressions[1].HasAnnotation(RetargetingAnnotations.OperandHasMoreThan32Bits));
        }

        [Fact]
        public void RetargetingRewriter_RewritesBitwiseNot_ToGlobalCalls()
        {
            var preOptions = LuaSyntaxOptions.Lua54;
            var postOptions = preOptions.With(acceptBitwiseOperators: false);
            AssertRewrite(
                preOptions,
                postOptions,
                """
                local x = ~x
                """,
                """
                local x = _G.bit.bnot(x)
                """,
                LuaVersion.LuaJIT21);
            AssertRewrite(
                preOptions,
                postOptions,
                """
                local x = ~x
                """,
                """
                local x = _G.bit32.bnot(x)
                """,
                LuaVersion.Lua54);
        }

        [Theory]
        [InlineData("&", "band")]
        [InlineData("|", "bor")]
        [InlineData("~", "bxor")]
        [InlineData("<<", "lshift")]
        [InlineData(">>", "rshift")]
        public void RetargetingRewriter_RewritesBinaryOperators_ToGlobalCalls(string op, string func)
        {
            var preOptions = LuaSyntaxOptions.Lua54;
            var postOptions = preOptions.With(acceptBitwiseOperators: false);
            AssertRewrite(
                preOptions,
                postOptions,
                $"""
                local x = x {op} x
                """,
                $"""
                local x = _G.bit.{func}(x, x)
                """,
                LuaVersion.LuaJIT21);
            AssertRewrite(
                preOptions,
                postOptions,
                $"""
                local x = x {op} x
                """,
                $"""
                local x = _G.bit32.{func}(x, x)
                """,
                LuaVersion.Lua54);
        }
    }
}
