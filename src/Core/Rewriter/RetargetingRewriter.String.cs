using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;

namespace Loretta.Retargeting.Core
{
    internal sealed partial class RetargetingRewriter : LuaSyntaxRewriter
    {
        private partial SyntaxToken VisitString(SyntaxToken token)
        {
            var text = token.Text;
            var canUseHex = _targetOptions.AcceptHexEscapesInStrings;
            var canUseUnicode = _targetOptions.AcceptUnicodeEscape;
            var canUseWhitespace = _targetOptions.AcceptWhitespaceEscape;

            var requiresRewrite = false;
            if (token.ContainsDiagnostics)
                // Don't touch tokens with diagnostics as they might have broken escapes.
                requiresRewrite = false;
            else if (!canUseHex && text.Contains("\\x"))
                requiresRewrite = true;
            else if (!canUseUnicode && text.Contains("\\u"))
                requiresRewrite = true;
            else if (!canUseWhitespace && text.Contains("\\z"))
                requiresRewrite = true;
            if (!requiresRewrite)
                return token;

            var newText = Regex.Replace(
                text,
                @"\\(?:(?<type>u)\{(?<value>\w+)\}|(?<type>x)(?<value>[A-Fa-f0-9]{1,2})|(?<type>z)(?<value>[ \t\n\v\f\r]+))",
                match =>
                {
                    var canUseHex = _targetOptions.AcceptHexEscapesInStrings;
                    var canUseUnicode = _targetOptions.AcceptUnicodeEscape;
                    var canUseWhitespace = _targetOptions.AcceptWhitespaceEscape;

                    var type = match.Groups["type"].ValueSpan[0];
                    var value = match.Groups["value"].ValueSpan;
                    return type switch
                    {
                        'u' when !canUseUnicode => EncodeCharToUtf8((char) ushort.Parse(value, System.Globalization.NumberStyles.AllowHexSpecifier), canUseHex),
                        'x' when !canUseHex => $"\\{ushort.Parse(value, System.Globalization.NumberStyles.AllowHexSpecifier):000}",
                        'z' when !canUseWhitespace => string.Empty,
                        _ => match.Value
                    };
                });

            return token.CopyAnnotationsTo(SyntaxFactory.Literal(
                token.LeadingTrivia,
                newText,
                (string) token.Value!,
                token.TrailingTrivia));
        }

        private bool StringRequiresRewrite(string text)
        {
            var canUseHex = _targetOptions.AcceptHexEscapesInStrings;
            var canUseUnicode = _targetOptions.AcceptUnicodeEscape;
            var canUseWhitespace = _targetOptions.AcceptWhitespaceEscape;

            var slashIdx = -1;
            // Keep going until we don't find any more slashes.
            while ((slashIdx = text.IndexOf('\\', slashIdx + 1)) >= 0
                   && slashIdx < text.Length - 1)
            {
                var escapeId = text[slashIdx + 1];
                switch (escapeId)
                {
                    case 'x' when !canUseHex:
                    case 'u' when !canUseUnicode:
                    case 'z' when !canUseWhitespace: return true;
                }
            }

            return false;
        }

        // Vendored version of CharUtils.IsWhitespace:
        // https://github.com/LorettaDevs/Loretta/blob/d6c81002aadcc0f5427ddd36242f65f49fe9e2b2/src/Compilers/Lua/Portable/Utilities/CharUtils.cs#L155
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsWhitespace(char ch) =>
            ch == ' ' || (uint) (ch - '\t') <= ('\r' - '\t');

        // Vendored version of CharUtils.EncodeCharToUtf8:
        // https://github.com/LorettaDevs/Loretta/blob/d6c81002aadcc0f5427ddd36242f65f49fe9e2b2/src/Compilers/Lua/Portable/Utilities/CharUtils.cs#L195
        private static string EncodeCharToUtf8(char ch, bool useHex)
        {
            var n = (ushort) ch;
            if (n < 0x7F)
            {
                return useHex
                    ? $"\\x{n:X2}"
                    : $"\\{n:000}";
            }
            else if (n < 0x7FF)
            {
                // 00000yyy yyxxxxxx -> [ 110yyyyy 10xxxxxx ]
                var byte01 = (byte) (0b11000000 | ((n >> 6) & 0b11111));
                var byte02 = (byte) (0b10000000 | (n & 0b111111));
                return useHex
                    ? $"\\x{byte01:X2}\\x{byte02:X2}"
                    : $"\\{byte01:000}\\{byte02:000}";
            }
            else
            {
                // zzzzyyyy yyxxxxxx -> [ 1110zzzz 10yyyyyy 10xxxxxx ]
                var byte01 = (byte) (0b11100000 | ((n >> 12) & 0b1111));
                var byte02 = (byte) (0b10000000 | ((n >> 6) & 0b111111));
                var byte03 = (byte) (0b10000000 | (n & 0b111111));
                return useHex
                    ? $"\\x{byte01:X2}\\x{byte02:X2}\\x{byte03:X2}"
                    : $"\\{byte01:000}\\{byte02:000}\\{byte03:000}";
            }
        }
    }
}
