using System.Numerics;
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

            if (_targetOptions.AcceptBitwiseOperators)
                return expression.WithOperand(operand);

            if (!_bitLibraryGlobals.HasBitLibrary)
            {
                return expression.WithOperand(operand)
                                 .WithAdditionalAnnotations(RetargetingAnnotations.TargetVersionHasNoBitLibrary);
            }

            if (SyntaxHelpers.GetConstantValue(expression.Operand) is { IsSome: true, Value: long value }
                && BitOperations.LeadingZeroCount((ulong) value) < 32)
            {
                operand = operand.WithAdditionalAnnotations(RetargetingAnnotations.OperandHasMoreThan32Bits);
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

            if (_targetOptions.AcceptBitwiseOperators)
                return expression.Update(left, expression.OperatorToken, right);

            if (!_bitLibraryGlobals.HasBitLibrary)
            {
                return expression.Update(left, expression.OperatorToken, right)
                                 .WithAdditionalAnnotations(RetargetingAnnotations.TargetVersionHasNoBitLibrary);
            }

            if (SyntaxHelpers.GetConstantValue(expression.Left) is { IsSome: true, Value: long leftValue }
                && BitOperations.LeadingZeroCount((ulong) leftValue) < 32)
            {
                left = left.WithAdditionalAnnotations(RetargetingAnnotations.OperandHasMoreThan32Bits);
            }

            if (SyntaxHelpers.GetConstantValue(expression.Right) is { IsSome: true, Value: long rightValue }
                && BitOperations.LeadingZeroCount((ulong) rightValue) < 32)
            {
                right = right.WithAdditionalAnnotations(RetargetingAnnotations.OperandHasMoreThan32Bits);
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
