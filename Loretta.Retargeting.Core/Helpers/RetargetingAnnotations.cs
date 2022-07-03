using Loretta.CodeAnalysis;

namespace Loretta.Retargeting.Core
{
    internal static class RetargetingAnnotations
    {
        public static readonly SyntaxAnnotation CannotConvertToDouble = new();
        public static readonly SyntaxAnnotation MightHaveFloatingPointPrecisionLoss = new();
        public static readonly SyntaxAnnotation UnableToRewriteCompoundAssignment = new();
        public static readonly SyntaxAnnotation ToFlatten = new();
        public static readonly SyntaxAnnotation TargetVersionHasNoBitLibrary = new();
        public static readonly SyntaxAnnotation OperandHasMoreThan32Bits = new();
        public static readonly SyntaxAnnotation IdentifierHasLuajitOnlyChars = new();
    }
}
