# Loretta.Retargeting
A library as well as a console tool that re-targets code from one lua version to another

## Progress

- [x] FiveM Hash Strings (`LuaSyntaxOptions.AcceptHashStrings`)
- [x] LuaJIT Identifier Special Characters (`LuaSyntaxOptions.UseLuaJitIdentifierRules`)
- [-] Bitwise Operators (`LuaSyntaxOptions.AcceptBitwiseOperators`)
    - [ ] Implement the bitwise operators using math operators in runtimes that don't have the bit library
- [ ] String Escapes
    - [ ] `\z` Escape (`LuaSyntaxOptions.AcceptWhitespaceEscape`)
    - [ ] `\u` Escape (`LuaSyntaxOptions.AcceptUnicodeEscape`)
    - [ ] `\x` Escape (`LuaSyntaxOptions.AcceptHexEscapesInStrings`)
- [ ] `continue` (`LuaSyntaxOptions.ContinueType`)
- [ ] Luau (Roblox Lua) `if` Expressions (`LuaSyntaxOptions.AcceptIfExpressions`)
- [x] Integers
    - [x] Binary (`LuaSyntaxOptions.BinaryIntegerFormat`)
    - [x] Octal (`LuaSyntaxOptions.OctalIntegerFormat`)
    - [x] Decimal (`LuaSyntaxOptions.DecimalIntegerFormat`)
    - [x] Hexadecimal (`LuaSyntaxOptions.HexIntegerFormat`)
- [ ] Local Variable Attributes (`LuaSyntaxOptions.AcceptLocalVariableAttributes`)
- [ ] Underscore in Number Literals (`LuaSyntaxOptions.AcceptUnderscoreInNumberLiterals`)
- [ ] Luau Typed Lua (`LuaSyntaxOptions.AcceptTypedLua`)
- [ ] Invalid Escapes (`LuaSyntaxOptions.AcceptInvalidEscapes`)
- [x] Shebang Handling (`LuaSyntaxOptions.AcceptShebang`)
- [x] Compound Assignments (`LuaSyntaxOptions.AcceptCompoundAssignment`)
- [ ] Hexadecimal Float Literals (`LuaSyntaxOptions.AcceptHexFloatLiterals`)
- [x] Number Bases
    - [x] Binary (`LuaSyntaxOptions.AcceptBinaryNumbers`)
    - [x] Octal (`LuaSyntaxOptions.AcceptOctalNumbers`)
- [x] C Comment Syntax (`LuaSyntaxOptions.AcceptCCommentSyntax`)
- [ ] Floor Division (`LuaSyntaxOptions.AcceptFloorDivision`)
- [-] LuaJIT Number Suffixes (`LuaSyntaxOptions.AcceptLuaJITNumberSuffixes`)
    - [x] `LL` suffix
    - [x] `ULL` suffix
    - [ ] `i` suffix
- [ ] C boolean operators (`LuaSyntaxOptions.AcceptCBooleanOperators`)
- [ ] `goto` (`LuaSyntaxOptions.AcceptGoto`)
- [x] Empty Statements (`LuaSyntaxOptions.AcceptEmptyStatements`)