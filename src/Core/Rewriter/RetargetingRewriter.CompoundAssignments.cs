using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.Retargeting.Core
{
    internal sealed partial class RetargetingRewriter : LuaSyntaxRewriter
    {
        public override partial SyntaxNode? VisitCompoundAssignmentStatement(CompoundAssignmentStatementSyntax node)
        {
            if (!_targetOptions.AcceptCompoundAssignment)
            {
                var operationKind = SyntaxFacts.GetCompoundAssignmentOperator(node.Kind()).Value;
                var operatorKind = SyntaxFacts.GetOperatorTokenKind(operationKind).Value;

                switch (node.Variable.Kind())
                {
                    case SyntaxKind.IdentifierName:
                        return SyntaxFactory.AssignmentStatement(
                            SyntaxFactory.SingletonSeparatedList(node.Variable),
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                SyntaxFactory.BinaryExpression(
                                    operationKind,
                                    node.Variable,
                                    SyntaxFactory.Token(operatorKind),
                                    node.Expression)));

                    case SyntaxKind.MemberAccessExpression:
                    {
                        var memberAccess = (MemberAccessExpressionSyntax) node.Variable;
                        var identName = GetImplDetailIdentifierName();
                        var implDetailMemberAccess = memberAccess.WithExpression(identName);

                        var localDecl = SyntaxFactory.LocalVariableDeclarationStatement(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.LocalDeclarationName(identName)),
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                memberAccess.Expression));
                        var assignment = SyntaxFactory.AssignmentStatement(
                            SyntaxFactory.SingletonSeparatedList<PrefixExpressionSyntax>(implDetailMemberAccess),
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                SyntaxFactory.BinaryExpression(
                                    operationKind,
                                    implDetailMemberAccess,
                                    SyntaxFactory.Token(operatorKind),
                                    node.Expression)));
                        return SyntaxFactory.DoStatement(SyntaxFactory.StatementList(
                            localDecl,
                            assignment))
                            .WithAdditionalAnnotations(RetargetingAnnotations.ToFlatten);
                    }

                    case SyntaxKind.ElementAccessExpression:
                    {
                        var elementAccess = (ElementAccessExpressionSyntax) node.Variable;
                        var identName = GetImplDetailIdentifierName();
                        var implDetailElementAccess = elementAccess.WithExpression(identName);

                        var localDecl = SyntaxFactory.LocalVariableDeclarationStatement(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.LocalDeclarationName(identName)),
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                elementAccess.Expression));
                        var assignment = SyntaxFactory.AssignmentStatement(
                            SyntaxFactory.SingletonSeparatedList<PrefixExpressionSyntax>(implDetailElementAccess),
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                SyntaxFactory.BinaryExpression(
                                    operationKind,
                                    implDetailElementAccess,
                                    SyntaxFactory.Token(operatorKind),
                                    node.Expression)));
                        return SyntaxFactory.DoStatement(SyntaxFactory.StatementList(
                            localDecl,
                            assignment))
                            .WithAdditionalAnnotations(RetargetingAnnotations.ToFlatten);

                    }

                    default:
                        return base.VisitCompoundAssignmentStatement(node)!
                                   .WithAdditionalAnnotations(RetargetingAnnotations.UnableToRewriteCompoundAssignment);
                }
            }

            return base.VisitCompoundAssignmentStatement(node);
        }
    }
}
