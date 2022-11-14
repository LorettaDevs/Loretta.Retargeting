using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loretta.CodeAnalysis.Lua;

namespace Loretta.Retargeting.Test.Rewrites
{
    public class StringLiteralRewritingTests : RewritingTestsBase
    {
        private const string RawString = @"aaaa\z   \u{FFFF}\xFF\xFF";

        [Fact]
        public void RetargetingRewriter_LeavesStringsUnchangedIfOptionsAllowForEverything()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = LuaSyntaxOptions.AllWithIntegers;

            AssertRewrite(
                preOptions,
                postOptions,
                $$"""
                local x = "aaaa\z   \u{FFFF}\xFF\xFF"
                """,
                $$"""
                local x = "aaaa\z   \u{FFFF}\xFF\xFF"
                """);
        }

        [Fact]
        public void RetargetingRewriter_RewritesWhitespaceEscapes()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions.With(acceptWhitespaceEscape: false);

            AssertRewrite(
                preOptions,
                postOptions,
                $$"""
                local x = "aaaa\z   \u{FFFF}\xFF\xFF"
                """,
                $$"""
                local x = "aaaa\u{FFFF}\xFF\xFF"
                """);
        }

        [Fact]
        public void RetargetingRewriter_RewritesUnicodeEscapes()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions.With(acceptUnicodeEscape: false);

            AssertRewrite(
                preOptions,
                postOptions,
                $$"""
                local x = "aaaa\z   \u{FFFF}\xFF\xFF"
                """,
                $$"""
                local x = "aaaa\z   \xEF\xBF\xBF\xFF\xFF"
                """);
        }

        [Fact]
        public void RetargetingRewriter_RewritesHexEscapes()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions.With(acceptHexEscapesInStrings: false);

            AssertRewrite(
                preOptions,
                postOptions,
                $$"""
                local x = "aaaa\z   \u{FFFF}\x0F\xFF"
                """,
                $$"""
                local x = "aaaa\z   \u{FFFF}\015\255"
                """);
        }

        [Fact]
        public void RetargetingRewriter_RewritesUnicodeIntoDecimal()
        {
            var preOptions = LuaSyntaxOptions.AllWithIntegers;
            var postOptions = preOptions.With(
                acceptHexEscapesInStrings: false,
                acceptUnicodeEscape: false);

            AssertRewrite(
                preOptions,
                postOptions,
                $$"""
                local x = "aaaa\z   \u{FFFF}\x0F\xFF"
                """,
                $$"""
                local x = "aaaa\z   \239\191\191\015\255"
                """);
        }
    }
}
