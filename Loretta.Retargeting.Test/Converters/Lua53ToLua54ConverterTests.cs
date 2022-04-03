using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loretta.CodeAnalysis.Lua;
using Loretta.Retargeting.Core.Converters;

namespace Loretta.Retargeting.Test.Converters
{
    public class Lua53ToLua54ConverterTests : ConverterTestsBase
    {
        internal override ConverterBase CreateConverter() => new Lua53ToLua54Converter();

        [Fact]
        public void Lua53ToLua54Converter_AnnotatesConstantVariables()
        {
            AssertConversion("""
                local x<const> --[[comment]] = 1
                print(x)
                """,
                LuaSyntaxOptions.Lua53,
                """
                local x --[[comment]] = 1
                print(x)
                """);
        }
    }
}
