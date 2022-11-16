using Loretta.CodeAnalysis.Lua;

namespace Loretta.Retargeting.Test.Rewrites;

public class IfExpressionRewritingTests : RewritingTestsBase
{
    [Fact]
    public void RetargetingRewriter_DoesNotRewriteIfExpressionsIfOptionsAllowThem()
    {
        var preOptions = LuaSyntaxOptions.AllWithIntegers;
        var postOptions = preOptions;

        AssertRewrite(
            preOptions,
            postOptions,
            """
            print(if a then b else c)
            """, """
            print(if a then b else c)
            """);
    }

    [Fact]
    public void RetargetingRewriter_RewritesIfExpressionsIfOptionsDoNotAllowThem()
    {
        var preOptions = LuaSyntaxOptions.AllWithIntegers;
        var postOptions = preOptions.With(acceptIfExpression: false);

        AssertRewrite(
            preOptions,
            postOptions,
            """
            print(if a then b else c)
            """,
            """
            local __impldetail__1
            if a then
                __impldetail__1 = b
            else
                __impldetail__1 = c
            end
            print(__impldetail__1)
            """);
    }

    [Fact]
    public void RetargetingRewriter_RewritesIfExpressionsWithElseIfClausesIfOptionsDoNotAllowThem()
    {
        var preOptions = LuaSyntaxOptions.AllWithIntegers;
        var postOptions = preOptions.With(acceptIfExpression: false);

        AssertRewrite(
            preOptions,
            postOptions,
            """
            print(if a then b elseif c then d elseif e then f else g)
            """,
            """
            local __impldetail__1
            if a then
                __impldetail__1 = b
            elseif c then
                __impldetail__1 = d
            elseif e then
                __impldetail__1 = f
            else
                __impldetail__1 = g
            end
            print(__impldetail__1)
            """);
    }
}
