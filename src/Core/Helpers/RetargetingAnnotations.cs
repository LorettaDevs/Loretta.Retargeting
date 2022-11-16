using Loretta.CodeAnalysis;

namespace Loretta.Retargeting.Core
{
    internal static class RetargetingAnnotations
    {
        #region Utility Annotations
        public static readonly SyntaxAnnotation ToFlatten = new();
        public static readonly SyntaxAnnotation StatementToRemove = new();
        #endregion Utility Annotations

        #region Error/Warning Annotations
        public static readonly SyntaxAnnotation CannotConvertToDouble = new();
        public static readonly SyntaxAnnotation MightHaveFloatingPointPrecisionLoss = new();
        public static readonly SyntaxAnnotation UnableToRewriteCompoundAssignment = new();
        public static readonly SyntaxAnnotation TargetVersionHasNoBitLibrary = new();
        public static readonly SyntaxAnnotation OperandHasMoreThan32Bits = new();
        public static readonly SyntaxAnnotation IdentifierHasLuajitOnlyChars = new();
        public static readonly SyntaxAnnotation IfExpressionHasNoParentStatement = new();
        #endregion Error/Warning Annotations
    }
}
