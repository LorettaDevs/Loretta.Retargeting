using Loretta.CodeAnalysis;

namespace Loretta.Retargeting.Core
{
    internal static class RetargetingAnnotations
    {
        public static readonly SyntaxAnnotation CannotConvertToDouble = new();
        public static readonly SyntaxAnnotation MightHaveFloatingPointPrecisionLoss = new();
    }
}
