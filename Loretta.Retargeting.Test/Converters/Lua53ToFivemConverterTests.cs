using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loretta.CodeAnalysis.Lua;
using Loretta.Retargeting.Core.Converters;

namespace Loretta.Retargeting.Test.Converters
{
    public class Lua53ToFivemConverterTests : ConverterTestsBase
    {
        internal override ConverterBase CreateConverter() => new Lua53ToFivemConverter();

        [Fact]
        public void Lua53ToFivemConverter_Converts_Nothing()
        {
            var converter = CreateConverter();

            var node = SyntaxFactory.ParseCompilationUnit("""
                local x = 1
                """);

            var converted = converter.Convert(node, out var diags);

            Assert.Empty(diags);
            Assert.Same(node, converted);
        }
    }
}
