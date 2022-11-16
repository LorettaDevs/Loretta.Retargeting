using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.Retargeting.Core;
using Loretta.Retargeting.Core.CachedNodes;

namespace Loretta.Retargeting.Test.Rewrites
{
    public abstract class RewritingTestsBase
    {
        protected static string NormalizeLineBreaks(string input) =>
            Regex.Replace(input, @"\r\n|[\r\n]", "\n");

        protected static SyntaxNode RewriteNode(
            LuaSyntaxOptions preOptions,
            LuaSyntaxOptions postOptions,
            string inputString,
            LuaVersion version = LuaVersion.GMod)
        {
            var inputTree = LuaSyntaxTree.ParseText(
                NormalizeLineBreaks(inputString),
                new LuaParseOptions(preOptions));
            var script = new Script(ImmutableArray.Create(inputTree));
            var rewriter = new RetargetingRewriter(
                postOptions,
                script,
                new BitLibraryGlobals(version));

            return rewriter.Visit(inputTree.GetRoot()).NormalizeWhitespace(eol: "\n");
        }

        protected static SyntaxNode AssertRewrite(
            LuaSyntaxOptions preOptions,
            LuaSyntaxOptions postOptions,
            string inputString,
            string outputString,
            LuaVersion version = LuaVersion.GMod)
        {
            var output = RewriteNode(preOptions, postOptions, inputString, version);

            Assert.Equal(
                NormalizeLineBreaks(outputString.Trim()),
                NormalizeLineBreaks(output.ToFullString().Trim()));
            return output;
        }
    }
}
