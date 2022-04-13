using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.Retargeting.Core;

namespace Loretta.Retargeting.Test.Rewrites
{
    public abstract class RewritingTestsBase
    {
        protected static string NormalizeLineBreaks(string input) =>
            Regex.Replace(input, @"\r\n|[\r\n]", "\n");

        protected static SyntaxNode RewriteNode(
            LuaSyntaxOptions preOptions,
            LuaSyntaxOptions postOptions,
            string inputString)
        {
            var inputTree = LuaSyntaxTree.ParseText(
                NormalizeLineBreaks(inputString),
                new LuaParseOptions(preOptions));
            var script = new Script(ImmutableArray.Create(inputTree));
            var rewriter = new RetargetingRewriter(postOptions, script);

            return rewriter.Visit(inputTree.GetRoot()).NormalizeWhitespace(eol: "\n");
        }

        protected static void AssertRewrite(
            LuaSyntaxOptions preOptions,
            LuaSyntaxOptions postOptions,
            string inputString,
            string outputString)
        {
            var output = RewriteNode(preOptions, postOptions, inputString);

            Assert.Equal(
                NormalizeLineBreaks(outputString.Trim()),
                NormalizeLineBreaks(output.ToFullString().Trim()));
        }
    }
}
