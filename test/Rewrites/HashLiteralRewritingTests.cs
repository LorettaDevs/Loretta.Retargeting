using Loretta.CodeAnalysis.Lua;

namespace Loretta.Retargeting.Test.Rewrites
{
    public class HashLiteralRewritingTests : RewritingTestsBase
    {
        [Fact]
        public void RetargetingRewriter_DoesNotRewritesHashLiteralExpressions_WhenTargetVersionSupportsIt()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions;

            AssertRewrite(
                preOptions,
                postOptions,
                """
                local x = `hello there`
                """,
                """
                local x = `hello there`
                """);
        }

        [Fact]
        public void RetargetingRewriter_RewritesHashLiteralExpressions()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions.With(acceptHashStrings: false);

            AssertRewrite(
                preOptions,
                postOptions,
                """
                local x = `hello there`
                """,
                """
                local x = 0x5A96E386
                """);
        }
    }
}
