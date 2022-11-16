using Loretta.CodeAnalysis.Lua;

namespace Loretta.Retargeting.Test.Rewrites
{
    public class NumberBaseRewritingTests : RewritingTestsBase
    {
        [Fact]
        public void RetargetingRewriter_DoesNotRewriteBinaryNumbers_WhenTargetVersionAllowsThem()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions;

            AssertRewrite(
                preOptions,
                postOptions,
                """
                local x = 0b1010
                """,
                """
                local x = 0b1010
                """);
        }

        [Fact]
        public void RetargetingRewriter_RewritesBinaryNumbers()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions.With(acceptBinaryNumbers: false);

            AssertRewrite(
                preOptions,
                postOptions,
                """
                local x = 0b1010
                """,
                """
                local x = 10
                """);
        }

        [Fact]
        public void RetargetingRewriter_DoesNotRewritesOctalNumbers_WhenTargetVersionAllowsThem()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions;

            AssertRewrite(
                preOptions,
                postOptions,
                """
                local x = 0o77
                """, """
                local x = 0o77
                """);
        }

        [Fact]
        public void RetargetingRewriter_RewritesOctalNumbers()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions.With(acceptOctalNumbers: false);

            AssertRewrite(
                preOptions,
                postOptions,
                """
                local x = 0o77
                """, """
                local x = 63
                """);
        }

        [Fact]
        public void RetargetingRewriter_DoesNotRewritesHexFloats_WhenTargetVersionAllowsThem()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions;

            AssertRewrite(
                preOptions,
                postOptions,
                """
                local x = 0xFF.FF
                """, """
                local x = 0xFF.FF
                """);
        }

        [Fact]
        public void RetargetingRewriter_RewritesHexFloats()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions.With(acceptHexFloatLiterals: false);

            AssertRewrite(
                preOptions,
                postOptions,
                """
                local x = 0xFF.FF
                """, """
                local x = 255.99609375
                """);
        }
    }
}
