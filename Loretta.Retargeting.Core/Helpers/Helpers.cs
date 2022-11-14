using System.Runtime.CompilerServices;
using System.Text;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;

namespace Loretta.Retargeting.Core
{
    internal static class Helpers
    {
        private static readonly ConditionalWeakTable<string, object> s_commentCache = new();

        public static int FindLongestClosing(string text)
        {
            var start = -1;
            var maxLength = 0;
            for (var idx = 0; idx < text.Length; idx++)
            {
                var ch = text[idx];
                if (ch == ']')
                {
                    if (start != -1)
                    {
                        // idx + 1 since we want the length of the entire
                        // closing
                        var length = idx + 1 - start;
                        maxLength = Math.Max(maxLength, length);
                    }
                    else
                    {
                        start = idx;
                    }
                }
                else if (ch == '=')
                {
                    continue;
                }
                else
                {
                    // Reset the start since this is an invalid comment.
                    start = -1;
                }
            }

            return maxLength;
        }

        public static int FindLongestClosing(StringBuilder builder)
        {
            var start = -1;
            var maxLength = 0;
            for (var idx = 0; idx < builder.Length; idx++)
            {
                var ch = builder[idx];
                if (ch == ']')
                {
                    if (start != -1)
                    {
                        // idx + 1 since we want the length of the entire
                        // closing
                        var length = idx + 1 - start;
                        maxLength = Math.Max(maxLength, length);
                    }
                    else
                    {
                        start = idx;
                    }
                }
                else if (ch == '=')
                {
                    continue;
                }
                else
                {
                    // Reset the start since this is an invalid comment.
                    start = -1;
                }
            }

            return maxLength;
        }

        public static SyntaxTrivia MultiLineComment(string text, int maxStack = Constants.MaxStackAlloc)
        {
            if (!s_commentCache.TryGetValue(text, out var objComment))
            {
                var longestClosing = FindLongestClosing(text);
                var equalsCount = Math.Max(longestClosing - 2 + 1, 0);
                string commentText;
                if (text.Length + longestClosing * 2 + 6 < maxStack)
                {
                    var builder = new StackStringBuilder(stackalloc char[maxStack]);

                    builder.Append("--[")
                           .Append('=', equalsCount)
                           .Append('[')
                           .Append(text)
                           .Append(']')
                           .Append('=', equalsCount)
                           .Append(']');

                    commentText = builder.ToString();
                }
                else
                {
                    var builder = StringBuilderPool.GetBuilder();

                    builder.Append("--[")
                           .Append('=', equalsCount)
                           .Append('[')
                           .Append(text)
                           .Append(']')
                           .Append('=', equalsCount)
                           .Append(']');

                    commentText = StringBuilderPool.ToStringAndFree(builder);
                }

                objComment = SyntaxFactory.Comment(commentText);
                s_commentCache.AddOrUpdate(text, objComment);
            }

            return (SyntaxTrivia) objComment;
        }

        public static void TurnIntoMultiLineComment(StringBuilder builder)
        {
            var longestClosing = FindLongestClosing(builder);
            var equalsCount = Math.Max(longestClosing - 2 + 1, 0);

            builder.Insert(0, "--[")
                .Insert(3, "=", equalsCount)
                .Insert(3 + equalsCount, '[')
                .Append(']')
                .Append('=', equalsCount)
                .Append(']');
        }

        public static bool CanConvertToDouble(long value)
        {
            var converted = (long) (double) value;
            return value == converted;
        }

        public static bool CanConvertToDouble(ulong value)
        {
            var converted = (ulong) (double) value;
            return value == converted;
        }

        public static bool CanGeneratePrecisionLossAsDouble(long value)
        {
            return value - Math.BitDecrement(value) > 1
                || Math.BitIncrement(value) - value > 1;
        }

        public static bool CanGeneratePrecisionLossAsDouble(ulong value)
        {
            return value - Math.BitDecrement(value) > 1
                || Math.BitIncrement(value) - value > 1;
        }

        public static string EncodeToUtf8(long codepoint)
        {
            if (codepoint < 0x7F)
            {
                return $"\\x{codepoint:X2}";
            }
            else if (codepoint < 0x7FF)
            {
                // 00000000 00000yyy yyxxxxxx -> [ 110yyyyy 10xxxxxx ]
                var byte01 = (byte) (0b11000000 | ((codepoint >> 06) & 0b11111));
                var byte02 = (byte) (0b10000000 | (codepoint & 0b111111));
                return $"\\x{byte01:X2}\\x{byte02:X2}";
            }
            else if (codepoint < 0xFFFF)
            {
                // 00000000 zzzzyyyy yyxxxxxx -> [ 1110zzzz 10yyyyyy 10xxxxxx ]
                var byte01 = (byte) (0b11100000 | ((codepoint >> 12) & 0b001111));
                var byte02 = (byte) (0b10000000 | ((codepoint >> 06) & 0b111111));
                var byte03 = (byte) (0b10000000 | (codepoint & 0b111111));
                return $"\\x{byte01:X2}\\x{byte02:X2}\\x{byte03:X2}";
            }
            else
            {
                // 000wwwzz zzzzyyyy yyxxxxxx -> [ 11110www 10zzzzzz 10yyyyyy 10xxxxxx ]
                var byte01 = (byte) (0b11110000 | ((codepoint >> 18) & 0b000111));
                var byte02 = (byte) (0b10000000 | ((codepoint >> 12) & 0b111111));
                var byte03 = (byte) (0b10000000 | ((codepoint >> 06) & 0b111111));
                var byte04 = (byte) (0b10000000 | (codepoint & 0b111111));
                return $"\\x{byte01:X2}\\x{byte02:X2}\\x{byte03:X2}\\x{byte04:X2}";
            }
        }
    }
}
