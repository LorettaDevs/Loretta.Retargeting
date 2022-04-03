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

        public static SyntaxTrivia MultiLineComment(string text, int maxStack = 512)
        {
            if (!s_commentCache.TryGetValue(text, out var objComment))
            {
                var longestClosing = FindLongestClosing(text);
                var equalsCount = Math.Max(longestClosing - 2, 0);
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
            var equalsCount = Math.Max(longestClosing - 2, 0);

            builder.Insert(0, "--[")
                .Insert(3, "=", equalsCount)
                .Insert(3 + equalsCount, '[')
                .Append(']')
                .Append('=', equalsCount)
                .Append(']');
        }
    }
}
