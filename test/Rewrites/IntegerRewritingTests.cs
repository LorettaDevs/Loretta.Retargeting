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
                2 or 102 or 202 => preOptions.With(binaryIntegerFormat: IntegerFormats.NotSupported, acceptLuaJITNumberSuffixes: false),
                8 => preOptions.With(octalIntegerFormat: IntegerFormats.NotSupported, acceptLuaJITNumberSuffixes: false),
                10 or 110 or 210 => preOptions.With(decimalIntegerFormat: IntegerFormats.NotSupported, acceptLuaJITNumberSuffixes: false),
                16 or 116 or 216 => preOptions.With(hexIntegerFormat: IntegerFormats.NotSupported, acceptLuaJITNumberSuffixes: false),
                _ => throw new ArgumentOutOfRangeException(nameof(@base))
            };
            var prefix = @base switch
            {
                2 or 102 or 202 => "0b",
                8 => "0o",
                10 or 110 or 210 => "",
                16 or 116 or 216 => "0x",
                _ => throw new ArgumentOutOfRangeException(nameof(@base))
            };
            var suffix = @base switch
            {
                102 or 110 or 116 => "LL",
                202 or 210 or 216 => "ULL",
                _ => ""
            };
            @base = @base switch
            {
                8 => @base,
                2 or 102 or 202 => 2,
                10 or 110 or 210 => 10,
                16 or 116 or 216 => 16,
                _ => throw new ArgumentOutOfRangeException(nameof(@base))
            };

            var node = RewriteNode(
                preOptions,
                postOptions,
                $"""
                local x = {prefix}{Convert.ToString(value, @base)}{suffix}
                """);

            var compilationUnit = Assert.IsType<CompilationUnitSyntax>(node);
            var localDecl = Assert.IsType<LocalVariableDeclarationStatementSyntax>(Assert.Single(compilationUnit.Statements.Statements));
            Assert.NotNull(localDecl.EqualsValues);
            var literalExpression = Assert.IsType<LiteralExpressionSyntax>(Assert.Single(localDecl.EqualsValues!.Values));
            var literalToken = literalExpression.Token;

            Assert.True(literalToken.HasAnnotation(annotation), "Node does not have the expected annotation.");
        }

        [Theory]
        [MemberData(nameof(NoRewriteData))]
        public void RetargetingRewriter_DoesNotRewriteIntegers_WhenTargetOptionsAllowThem(long number, string text)
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions;

            var node = AssertRewrite(
                preOptions,
                postOptions,
                $"""
                local x = {text}
                """,
                $"""
                local x = {text}
                """);

            var compilationUnit = Assert.IsType<CompilationUnitSyntax>(node);
            var localDecl = Assert.IsType<LocalVariableDeclarationStatementSyntax>(Assert.Single(compilationUnit.Statements.Statements));
            Assert.NotNull(localDecl.EqualsValues);
            var literalExpression = Assert.IsType<LiteralExpressionSyntax>(Assert.Single(localDecl.EqualsValues!.Values));
            var literalToken = literalExpression.Token;

            if (text.EndsWith("ULL", StringComparison.OrdinalIgnoreCase))
                Assert.Equal((ulong) number, literalToken.Value);
            else
                Assert.Equal(number, literalToken.Value);
        }

        public static IEnumerable<object[]> NoRewriteData
        {
            get
            {
                var bases = new[] { 2, 8, 10, 16 };
                var suffixes = new[] { "", "LL", "ULL" };
                const long number = int.MaxValue + 1L;

                return from @base in bases
                       from suffix in suffixes
                       where @base != 8 || suffix == ""
                       select new object[] { number, $"{getPrefix(@base)}{Convert.ToString(number, @base)}{suffix}" };

                static string getPrefix(int @base) => @base switch
                {
                    2 => "0b",
                    8 => "0o",
                    10 => "",
                    16 => "0x",
                    _ => throw new InvalidOperationException($"Invalid base '{@base}'.")
                };
            }
        }

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableBinaryIntegers() =>
            AssertCore(2, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableLLBinaryIntegers() =>
            AssertCore(102, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableULLBinaryIntegers() =>
            AssertCore(202, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableOctalIntegers() =>
            AssertCore(8, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableDecimalIntegers() =>
            AssertCore(10, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableLLDecimalIntegers() =>
            AssertCore(110, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableULLDecimalIntegers() =>
            AssertCore(210, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableHexIntegers() =>
            AssertCore(16, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableLLHexIntegers() =>
            AssertCore(116, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksUnconvertableULLHexIntegers() =>
            AssertCore(216, UNABLE_TO_CONVERT, RetargetingAnnotations.CannotConvertToDouble);

        [Fact]
        public void RetargetingRewriter_MarksBinaryIntegersWithOverflowPotential() =>
            AssertCore(2, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);

        [Fact]
        public void RetargetingRewriter_MarksLLBinaryIntegersWithOverflowPotential() =>
            AssertCore(102, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);

        [Fact]
        public void RetargetingRewriter_MarksULLBinaryIntegersWithOverflowPotential() =>
            AssertCore(202, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);

        [Fact]
        public void RetargetingRewriter_MarksOctalIntegersWithOverflowPotential() =>
            AssertCore(8, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);

        [Fact]
        public void RetargetingRewriter_MarksDecimalIntegersWithOverflowPotential() =>
            AssertCore(10, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);

        [Fact]
        public void RetargetingRewriter_MarksLLDecimalIntegersWithOverflowPotential() =>
            AssertCore(110, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);

        [Fact]
        public void RetargetingRewriter_MarksULLDecimalIntegersWithOverflowPotential() =>
            AssertCore(210, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);

        [Fact]
        public void RetargetingRewriter_MarksHexIntegersWithOverflowPotential() =>
            AssertCore(16, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);

        [Fact]
        public void RetargetingRewriter_MarksLLHexIntegersWithOverflowPotential() =>
            AssertCore(116, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);

        [Fact]
        public void RetargetingRewriter_MarksULLHexIntegersWithOverflowPotential() =>
            AssertCore(216, PRECISION_LOSS, RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);
    }
}
