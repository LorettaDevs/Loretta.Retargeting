using Loretta.CodeAnalysis.Lua;
using Loretta.Retargeting.Core.Converters;

namespace Loretta.Retargeting.Test.Converters
{
    public class FivemToLua53ConverterTests : ConverterTestsBase
    {
        internal override ConverterBase CreateConverter() => new FivemToLua53Converter();

        [Fact]
        public void FivemToLua53Converter_Converts_HashStringLiteralsProperly()
        {
            AssertConversion("""
                local x = --[[ `hi` = ]] 1867409728
                if x == --[[ `hi` = ]] 1867409728 then
                    print "hi"
                end
                """,
                LuaSyntaxOptions.FiveM,
                """
                local x = `hi`
                if x == `hi` then
                    print "hi"
                end
                """);
        }
    }
}
