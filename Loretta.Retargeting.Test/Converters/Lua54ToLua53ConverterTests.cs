using Loretta.CodeAnalysis.Lua;
using Loretta.Retargeting.Core.Converters;

namespace Loretta.Retargeting.Test.Converters
{
    public class Lua54ToLua53ConverterTests : ConverterTestsBase
    {
        internal override ConverterBase CreateConverter() => new Lua54ToLua53Converter();

        [Fact]
        public void Lua54ToLua53Converter_Converts_LocalVariableAttributesProperly()
        {
            AssertConversion("""
                local --[[pre]] x --[[ <const> ]] --[[post]], y --[[ <close> ]]
                """,
                LuaSyntaxOptions.Lua54,
                """
                local --[[pre]] x<const> --[[post]], y<close>
                """);
        }
    }
}
