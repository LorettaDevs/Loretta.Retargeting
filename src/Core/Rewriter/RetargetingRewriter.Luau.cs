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
    }
}
