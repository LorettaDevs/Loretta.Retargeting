using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.Retargeting.Core;

namespace Loretta.Retargeting.Test.Rewrites
{
    public class IntegerRewritingTests : RewritingTestsBase
    {
        private const long UNABLE_TO_CONVERT = 0b100000000000000000000000000000000000000000000000000001;
        private const long PRECISION_LOSS = 0b100000000000000000000000000000000000000000000000000010;

        private static void AssertCore(int @base, long value, SyntaxAnnotation annotation)
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = @base switch
            {
                2 => preOptions.With(binaryIntegerFormat: IntegerFormats.NotSupported),
                8 => preOptions.With(octalIntegerFormat: IntegerFormats.NotSupported),
                10 => preOptions.With(decimalIntegerFormat: IntegerFormats.NotSupported),
                16 => preOptions.With(hexIntegerFormat: IntegerFormats.NotSupported),
                _ => throw new ArgumentOutOfRangeException(nameof(@base))
            };
            var prefix = @base switch
            {
                2 => "0b",
                8 => "0o",
                10 => "",
                16 => "0x",
                _ => throw new ArgumentOutOfRangeException(nameof(@base))
            };

            var node = RewriteNode(
                preOptions,
                postOptions,
                $"""
                local x = {prefix}{Convert.ToString(value, @base)}
                """);

            var compilationUnit = Assert.IsType<CompilationUnitSyntax>(node);
            var localDecl = Assert.IsType<LocalVariableDeclarationStatementSyntax>(Assert.Single(compilationUnit.Statements.Statements));
            Assert.NotNull(localDecl.EqualsValues);
            var literalExpression = Assert.IsType<LiteralExpressionSyntax>(Assert.Single(localDecl.EqualsValues!.Values));
            var literalToken = literalExpression.Token;

            Assert.True(literalToken.HasAnnotation(annotation), "Node does not have the expected annotation.");
        }

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableBinaryIntegers() =>
            AssertCore(2, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableOctalIntegers() =>
            AssertCore(8, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableDecimalIntegers() =>
            AssertCore(10, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableHexIntegers() =>
            AssertCore(16, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksBinaryIntegersWithOverflowPotential() =>
            AssertCore(2, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);

        [Fact]
        public void RetargetingRewriter_MarksOctalIntegersWithOverflowPotential() =>
            AssertCore(8, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);

        [Fact]
        public void RetargetingRewriter_MarksDecimalIntegersWithOverflowPotential() =>
            AssertCore(10, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);

        [Fact]
        public void RetargetingRewriter_MarksHexIntegersWithOverflowPotential() =>
            AssertCore(16, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);
    }
}
