using Loretta.CodeAnalysis.Lua;

namespace Loretta.Retargeting.Test.Rewrites
{
    public class TypedLuaRewritingTests : RewritingTestsBase
    {
        [Fact]
        public void RetargetingRewriter_DoesNotRewriteTypeBindings_WhenTargetOptionsAllowThem()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions;

            AssertRewrite(
                preOptions,
                postOptions,
                """
                export type X = Y
                type Y = X
                local x: X = 10
                local y = function<T>(a: T, b: X): Y
                    return 1
                end
                local function something<T>(a: T, b: X): Y
                    return 1
                end
                function something2<T>(a: T, b: X): Y
                    return 1
                end
                """,
                """
                export type X = Y
                type Y = X
                local x: X = 10
                local y = function<T>(a: T, b: X): Y
                    return 1
                end
                local function something<T>(a: T, b: X): Y
                    return 1
                end
                function something2<T>(a: T, b: X): Y
                    return 1
                end
                """);
        }

        [Fact]
        public void RetargetingRewriter_RewritesTypeBindings()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions.With(acceptTypedLua: false);

            AssertRewrite(
                preOptions,
                postOptions,
                """
                export type X = Y
                type Y = X

                local x: X = 10
                local y = function<T>(a: T, b: X): Y
                    return 1
                end
                local function something<T>(a: T, b: X): Y
                    return 1
                end
                function something2<T>(a: T, b: X): Y
                    return 1
                end
                """,
                """
                local x = 10
                local y = function(a, b)
                    return 1
                end
                local function something(a, b)
                    return 1
                end
                function something2(a, b)
                    return 1
                end
                """);
        }
    }
}
