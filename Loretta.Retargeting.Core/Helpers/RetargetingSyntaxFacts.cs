using Loretta.CodeAnalysis.Lua;

namespace Loretta.Retargeting.Core
{
    internal static class RetargetingSyntaxFacts
    {
        public static bool IsBitwiseExpression(SyntaxKind expressionKind) =>
            expressionKind is SyntaxKind.BitwiseAndExpression
                           or SyntaxKind.BitwiseOrExpression
                           or SyntaxKind.BitwiseNotExpression
                           or SyntaxKind.ExclusiveOrExpression
                           or SyntaxKind.RightShiftExpression
                           or SyntaxKind.LeftShiftExpression;
    }
}
