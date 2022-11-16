using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.Retargeting.Core
{
    internal sealed partial class RetargetingRewriter : LuaSyntaxRewriter
    {
        public override SyntaxNode? VisitIfExpression(IfExpressionSyntax node)
        {
            if (_targetOptions.AcceptIfExpressions)
                return base.VisitIfExpression(node);

            var statement = node.FirstAncestorOrSelf<StatementSyntax>();
            if (statement is null)
            {
                return base.VisitIfExpression(node)!
                           .WithAdditionalAnnotations(RetargetingAnnotations.IfExpressionHasNoParentStatement);
            }

            var name = GetImplDetailIdentifierName();
            var localDecl = SyntaxFactory.LocalVariableDeclarationStatement(
                SyntaxFactory.SingletonSeparatedList(SyntaxFactory.LocalDeclarationName(name)));

            var elseIfClauses = node.ElseIfClauses.Select(elseIfClause =>
                SyntaxFactory.ElseIfClause(
                    (ExpressionSyntax) Visit(elseIfClause.Condition),
                    SyntaxFactory.StatementList(
                        SyntaxFactory.AssignmentStatement(
                            SyntaxFactory.SingletonSeparatedList<PrefixExpressionSyntax>(
                                name),
                            SyntaxFactory.SingletonSeparatedList(
                                (ExpressionSyntax) Visit(elseIfClause.Value))))));

            var elseClause = SyntaxFactory.ElseClause(
                SyntaxFactory.StatementList(
                    SyntaxFactory.AssignmentStatement(
                        SyntaxFactory.SingletonSeparatedList<PrefixExpressionSyntax>(name),
                        SyntaxFactory.SingletonSeparatedList((ExpressionSyntax) Visit(node.FalseValue)))));

            var ifStatement = SyntaxFactory.IfStatement(
                (ExpressionSyntax) Visit(node.Condition),
                SyntaxFactory.StatementList(
                    SyntaxFactory.AssignmentStatement(
                        SyntaxFactory.SingletonSeparatedList<PrefixExpressionSyntax>(name),
                        SyntaxFactory.SingletonSeparatedList((ExpressionSyntax) Visit(node.TrueValue)))),
                SyntaxFactory.List(elseIfClauses),
                elseClause);

            AddStatementBefore(statement, localDecl);
            AddStatementBefore(statement, ifStatement);

            return name;
        }

        public override SyntaxNode? VisitTypeBinding(TypeBindingSyntax node) =>
            !_targetOptions.AcceptTypedLua ? null : node;

        public override SyntaxNode? VisitTypeParameterList(TypeParameterListSyntax node) =>
            !_targetOptions.AcceptTypedLua ? null : node;

        public override SyntaxNode? VisitTypeCastExpression(TypeCastExpressionSyntax node) =>
            !_targetOptions.AcceptTypedLua ? Visit(node.Expression) : base.VisitTypeCastExpression(node);

        public override SyntaxNode? VisitTypeDeclarationStatement(TypeDeclarationStatementSyntax node) =>
            !_targetOptions.AcceptTypedLua ? s_toRemoveStatement : node;
    }
}
