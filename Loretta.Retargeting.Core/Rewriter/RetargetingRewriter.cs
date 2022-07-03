using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.SymbolDisplay;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.Retargeting.Core.CachedNodes;

namespace Loretta.Retargeting.Core
{
    internal sealed partial class RetargetingRewriter : LuaSyntaxRewriter
    {
        private readonly LuaSyntaxOptions _targetOptions;
        private readonly Script _script;
        private readonly BitLibraryGlobals _bitLibraryGlobals;
        private int _localId;

        public RetargetingRewriter(LuaSyntaxOptions targetOptions, Script script, BitLibraryGlobals bitLibraryGlobals)
        {
            _targetOptions = targetOptions ?? throw new System.ArgumentNullException(nameof(targetOptions));
            _script = script ?? throw new System.ArgumentNullException(nameof(script));
            _bitLibraryGlobals = bitLibraryGlobals ?? throw new System.ArgumentNullException(nameof(bitLibraryGlobals));
        }

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

        public override SyntaxNode? VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.HashStringLiteralExpression))
            {
                ulong value = (ulong) node.Token.Value!;

                return SyntaxFactory.LiteralExpression(
                    SyntaxKind.NumericalLiteralExpression,
                    VisitNumber(SyntaxFactory.Literal(
                        ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.UseHexadecimalNumbers),
                        value)));
            }
            return base.VisitLiteralExpression(node);
        }

        public override SyntaxNode? VisitUnaryExpression(UnaryExpressionSyntax node)
        {
            if (RetargetingSyntaxFacts.IsBitwiseExpression(node.Kind()))
                return VisitBitwiseUnaryExpression(node);
            return base.VisitUnaryExpression(node);
        }

        public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (RetargetingSyntaxFacts.IsBitwiseExpression(node.Kind()))
                return VisitBitwiseBinaryExpression(node);
            return base.VisitBinaryExpression(node);
        }

        public override SyntaxList<TNode> VisitList<TNode>(SyntaxList<TNode> list)
        {
            list = base.VisitList(list);

            if (typeof(TNode) == typeof(StatementSyntax))
            {
                var typedList = new List<StatementSyntax>((SyntaxList<StatementSyntax>) (object) list);

                for (var idx = typedList.Count - 1; idx >= 0; idx--)
                {
                    var statement = typedList[idx];
                    if (!_targetOptions.AcceptEmptyStatements
                        && statement.IsKind(SyntaxKind.EmptyStatement))
                    {
                        typedList.RemoveAt(idx);
                    }
                    else if (statement.HasAnnotation(RetargetingAnnotations.ToFlatten))
                    {
                        var doStatement = (DoStatementSyntax) statement;
                        typedList.RemoveAt(idx);
                        typedList.InsertRange(idx, doStatement.Body.Statements);
                    }
                }

                list = (SyntaxList<TNode>) (object) SyntaxFactory.List(typedList);
            }

            return list;
        }

        #endregion Shared Visitors

        public override partial SyntaxNode? VisitCompoundAssignmentStatement(CompoundAssignmentStatementSyntax node);

        private partial SyntaxNode? VisitBitwiseUnaryExpression(UnaryExpressionSyntax expression);

        private partial SyntaxNode? VisitBitwiseBinaryExpression(BinaryExpressionSyntax expression);

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
