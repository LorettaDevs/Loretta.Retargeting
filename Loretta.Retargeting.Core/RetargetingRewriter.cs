using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.Retargeting.Core
{
    internal sealed partial class RetargetingRewriter : LuaSyntaxRewriter
    {
        private readonly List<Diagnostic> _diagnostics = new();
        private readonly LuaSyntaxOptions _targetOptions;
        private readonly Script _script;
        private int _localId;

        public RetargetingRewriter(LuaSyntaxOptions targetOptions!!, Script script!!)
        {
            _targetOptions = targetOptions;
            _script = script;
        }

        public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

        private SyntaxToken GetImplDetailIdentifier()
        {
            var id = Interlocked.Increment(ref _localId);
            return SyntaxFactory.Identifier($"__impldetail__{id}");
        }

        private IdentifierNameSyntax GetImplDetailIdentifierName() =>
            SyntaxFactory.IdentifierName(GetImplDetailIdentifier());

        #region Shared Visitors

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            if (token.IsKind(SyntaxKind.NumericLiteralToken))
                return VisitNumber(token);
            return base.VisitToken(token);
        }

        #endregion Shared Visitors

        public override SyntaxNode? VisitCompoundAssignmentStatement(CompoundAssignmentStatementSyntax node)
        {
            if (!_targetOptions.AcceptCompoundAssignment)
            {
                var operationKind = SyntaxFacts.GetCompoundAssignmentOperator(node.Kind()).Value;
                var operatorKind = SyntaxFacts.GetOperatorTokenKind(operationKind).Value;
                var identName = GetImplDetailIdentifierName();

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
                            assignment));
                    }

                    case SyntaxKind.ElementAccessExpression:
                    {
                        var elementAccess = (ElementAccessExpressionSyntax) node.Variable;
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
                            assignment));
                    }
                }
            }

            return base.VisitCompoundAssignmentStatement(node);
        }

        private partial SyntaxToken VisitNumber(SyntaxToken token);

        #region Trivia Rewriting

        public override SyntaxTriviaList VisitList(SyntaxTriviaList list)
        {
            var newList = base.VisitList(list);
            for (var idx = newList.Count - 1; idx >= 0; idx--)
            {
                var trivia = newList[idx];
                if (!_targetOptions.AcceptShebang && trivia.IsKind(SyntaxKind.ShebangTrivia))
                    newList = newList.RemoveAt(idx);
            }
            return newList;
        }

        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            if (!_targetOptions.AcceptCCommentSyntax
                && trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                && trivia.ToFullString().StartsWith("//", StringComparison.Ordinal))
            {
                var builder = StringBuilderPool.GetBuilder();
                builder.Append("--");
                if (trivia.ToFullString()[2] == '[')
                    builder.Append(' ');
                builder.Append(trivia.ToFullString()[2..]);
                var text = StringBuilderPool.ToStringAndFree(builder);

                return SyntaxFactory.Comment(text);
            }
            else if (!_targetOptions.AcceptCCommentSyntax
                && trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                && trivia.ToFullString().StartsWith("/*", StringComparison.Ordinal))
            {
                var builder = StringBuilderPool.GetBuilder();
                builder.Append(trivia.ToFullString()[2..^2]);
                Helpers.TurnIntoMultiLineComment(builder);
                var text = StringBuilderPool.ToStringAndFree(builder);

                return SyntaxFactory.Comment(text);
            }

            return base.VisitTrivia(trivia);
        }

        #endregion Trivia Rewriting
    }
}
