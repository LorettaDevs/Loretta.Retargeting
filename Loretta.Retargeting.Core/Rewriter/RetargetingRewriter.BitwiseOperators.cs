using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.Retargeting.Core
{
    internal sealed partial class RetargetingRewriter : LuaSyntaxRewriter
    {
        private partial SyntaxNode? VisitBitwiseUnaryExpression(UnaryExpressionSyntax expression)
        {
            var operand = (ExpressionSyntax) Visit(expression.Operand);
            if (!_bitLibraryGlobals.HasBitLibrary)
            {
                return expression.WithOperand(operand)
                                 .WithAdditionalAnnotations(RetargetingAnnotations.TargetVersionHasNoBitLibrary);
            }

            if (expression.IsKind(SyntaxKind.BitwiseNotExpression))
            {
                return SyntaxFactory.FunctionCallExpression(
                    _bitLibraryGlobals.BitwiseNot!,
                    SyntaxFactory.ExpressionListFunctionArgument(
                        SyntaxFactory.SingletonSeparatedList(operand)));
            }

            throw new InvalidOperationException();
        }

        private partial SyntaxNode? VisitBitwiseBinaryExpression(BinaryExpressionSyntax expression)
        {
            var left = (ExpressionSyntax) Visit(expression.Left);
            var right = (ExpressionSyntax) Visit(expression.Right);
            if (!_bitLibraryGlobals.HasBitLibrary)
            {
                return expression.Update(left, expression.OperatorToken, right)
                                 .WithAdditionalAnnotations(RetargetingAnnotations.TargetVersionHasNoBitLibrary);
            }

            var function = expression.Kind() switch
            {
                SyntaxKind.BitwiseAndExpression => _bitLibraryGlobals.BitwiseAnd,
                SyntaxKind.BitwiseOrExpression => _bitLibraryGlobals.BitwiseOr,
                SyntaxKind.LeftShiftExpression => _bitLibraryGlobals.LeftShift,
                SyntaxKind.RightShiftExpression => _bitLibraryGlobals.RightShift,
                SyntaxKind.ExclusiveOrExpression => _bitLibraryGlobals.BitwiseExclusiveOr,
                _ => throw new InvalidOperationException(),
            };
            return SyntaxFactory.FunctionCallExpression(
                function!,
                SyntaxFactory.ExpressionListFunctionArgument(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        left,
                        right
                    })));
        }
    }
}
