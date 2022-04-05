using System.Runtime.InteropServices;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.SymbolDisplay;

namespace Loretta.Retargeting.Core
{
    internal sealed class RetargetingRewriter : LuaSyntaxRewriter
    {
        private readonly List<Diagnostic> _diagnostics = new();
        private readonly LuaSyntaxOptions _targetOptions;
        private readonly Script _script;

        public RetargetingRewriter(LuaSyntaxOptions targetOptions!!, Script script!!)
        {
            _targetOptions = targetOptions;
            _script = script;
        }

        public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

        #region Shared Visitors

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            if (token.IsKind(SyntaxKind.NumericLiteralToken))
                return VisitNumber(token);
            return base.VisitToken(token);
        }

        #endregion Shared Visitors

        #region Number Rewriting

        private SyntaxToken VisitNumber(SyntaxToken token)
        {
            // These are only used for the functions at the bottom.
            const uint Prefix0b = 0x00620030;
            const uint Prefix0o = 0x006F0030;
            const uint Prefix0x = 0x00780030;
            const uint LowerCaseMask = 0b100000 << 16;

            var numberBase = getNumberBase(token);

            // Convert integers
            if ((_targetOptions.BinaryIntegerFormat != IntegerFormats.Int64 && numberBase == 2
                || _targetOptions.OctalIntegerFormat != IntegerFormats.Int64 && numberBase == 8
                || _targetOptions.HexIntegerFormat != IntegerFormats.Int64 && numberBase == 16
                || _targetOptions.DecimalIntegerFormat != IntegerFormats.Int64 && numberBase == 10
                )
                && token.Value is long value1)
            {
                token = token.CopyAnnotationsTo(SyntaxFactory.Literal(
                    token.LeadingTrivia,
                    token.Text,
                    (double) value1,
                    token.TrailingTrivia));

                if (!Helpers.CanConvertToDouble(value1))
                    token = token.WithAdditionalAnnotations(RetargetingAnnotations.CannotConvertToDouble);
                if (Helpers.CanGeneratePrecisionLossAsDouble(value1))
                    token = token.WithAdditionalAnnotations(RetargetingAnnotations.MightHaveFloatingPointPrecisionLoss);
            }

            // Convert unsupported number formats
            if (!_targetOptions.AcceptBinaryNumbers && numberBase == 2
                || !_targetOptions.AcceptOctalNumbers && numberBase == 8
                || !_targetOptions.AcceptHexFloatLiterals && numberBase == 16 && token.Value is double)
            {
                token = token.Value is long valueN
                    ? token.CopyAnnotationsTo(SyntaxFactory.Literal(
                        token.LeadingTrivia,
                        ObjectDisplay.FormatLiteral(valueN, ObjectDisplayOptions.None),
                        valueN,
                        token.TrailingTrivia))
                    : token.CopyAnnotationsTo(SyntaxFactory.Literal(
                        token.LeadingTrivia,
                        ObjectDisplay.FormatLiteral((double) token.Value!, ObjectDisplayOptions.None),
                        (double) token.Value!,
                        token.TrailingTrivia));
            }

            return base.VisitToken(token);

            static int getNumberBase(SyntaxToken token)
            {
                return lowerNumericPrefix(token.Text) switch
                {
                    Prefix0b => 2,
                    Prefix0o => 8,
                    Prefix0x => 16,
                    _ => 10,
                };
            }
            static uint lowerNumericPrefix(string text) =>
                text.Length < 2
                ? 0
                : MemoryMarshal.Read<uint>(MemoryMarshal.Cast<char, byte>(text)) | LowerCaseMask;
        }

        #endregion Number Rewriting

        #region Trivia Rewriting

        public override SyntaxTriviaList VisitList(SyntaxTriviaList list)
        {
            var newList = base.VisitList(list);
            for (var idx = newList.Count - 1; idx >= 0; idx--)
            {
                var trivia = newList[idx];
                if (!_targetOptions.AcceptShebang && trivia.IsKind(SyntaxKind.ShebangTrivia))
                    newList = newList.RemoveAt(idx);
            }
            return newList;
        }

        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            if (!_targetOptions.AcceptCCommentSyntax
                && trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                && trivia.ToFullString().StartsWith("//", StringComparison.Ordinal))
            {
                var builder = StringBuilderPool.GetBuilder();
                builder.Append("--");
                if (trivia.ToFullString()[2] == '[')
                    builder.Append(' ');
                builder.Append(trivia.ToFullString()[2..]);
                var text = StringBuilderPool.ToStringAndFree(builder);

                return SyntaxFactory.Comment(text);
            }
            else if (!_targetOptions.AcceptCCommentSyntax
                && trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                && trivia.ToFullString().StartsWith("/*", StringComparison.Ordinal))
            {
                var builder = StringBuilderPool.GetBuilder();
                builder.Append(trivia.ToFullString()[2..^2]);
                Helpers.TurnIntoMultiLineComment(builder);
                var text = StringBuilderPool.ToStringAndFree(builder);

                return SyntaxFactory.Comment(text);
            }

            return base.VisitTrivia(trivia);
        }

        #endregion Trivia Rewriting
    }
}
