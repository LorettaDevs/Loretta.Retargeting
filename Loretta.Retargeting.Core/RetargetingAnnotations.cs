﻿using Loretta.CodeAnalysis;

namespace Loretta.Retargeting.Core
{
    internal static class RetargetingAnnotations
    {
        public static readonly SyntaxAnnotation CannotConvertToDouble = new();
        public static readonly SyntaxAnnotation MightHaveFloatingPointPrecisionLoss = new();
        public static readonly SyntaxAnnotation UnableToRewriteCompoundAssignment = new();
        public static readonly SyntaxAnnotation ToFlatten = new();
    }
}