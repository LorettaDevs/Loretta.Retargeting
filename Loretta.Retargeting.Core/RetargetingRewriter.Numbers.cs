using System.Runtime.InteropServices;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.SymbolDisplay;

namespace Loretta.Retargeting.Core
{
    internal sealed partial class RetargetingRewriter : LuaSyntaxRewriter
    {
        private partial SyntaxToken VisitNumber(SyntaxToken token)
        {
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
                // These are only used for the functions at the bottom.
                const uint Prefix0b = 0x00620030;
                const uint Prefix0o = 0x006F0030;
                const uint Prefix0x = 0x00780030;
                return lowerNumericPrefix(token.Text) switch
                {
                    Prefix0b => 2,
                    Prefix0o => 8,
                    Prefix0x => 16,
                    _ => 10,
                };
            }

            static uint lowerNumericPrefix(string text)
            {
                const uint LowerCaseMask = 0b100000 << 16;
                if (text.Length < 2)
                    return 0;
                return MemoryMarshal.Read<uint>(MemoryMarshal.Cast<char, byte>(text)) | LowerCaseMask;
            }
        }
    }
}
