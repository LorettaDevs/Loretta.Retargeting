using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loretta.CodeAnalysis.Lua;
using Loretta.Retargeting.Core.Converters;

namespace Loretta.Retargeting.Test.Converters
{
    public abstract class ConverterTestsBase
    {
        internal abstract ConverterBase CreateConverter();

        protected void AssertConversion(string expected, LuaSyntaxOptions options, string input)
        {
            var converter = CreateConverter();
            var inputNode = SyntaxFactory.ParseCompilationUnit(input, options: new LuaParseOptions(options));

            var result = converter.Convert(inputNode, out var diags);

            Assert.Empty(diags);
            Assert.Equal(expected, result.ToFullString());
        }
    }
}
