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

        public RetargetingRewriter(LuaSyntaxOptions targetOptions, Script script, BitLibraryGlobals bitLibraryGlobals)
        {
            _targetOptions = targetOptions ?? throw new System.ArgumentNullException(nameof(targetOptions));
            _script = script ?? throw new System.ArgumentNullException(nameof(script));
            _bitLibraryGlobals = bitLibraryGlobals ?? throw new System.ArgumentNullException(nameof(bitLibraryGlobals));
        }

        #region Shared Visitors

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            if (token.IsKind(SyntaxKind.NumericLiteralToken))
            {
                return VisitNumber(token);
            }
            else if (token.IsKind(SyntaxKind.StringLiteralToken))
            {
                return VisitString(token);
            }
            else if (token.IsKind(SyntaxKind.IdentifierToken)
                && !_targetOptions.UseLuaJitIdentifierRules
                && token.Text.Any(ch => ch >= 0x7F))
            {
                return token.WithAdditionalAnnotations(RetargetingAnnotations.IdentifierHasLuajitOnlyChars);
            }

            return base.VisitToken(token);
        }

        public override SyntaxNode? VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (!_targetOptions.AcceptHashStrings
                && node.IsKind(SyntaxKind.HashStringLiteralExpression))
            {
                var value = (uint) node.Token.Value!;

                return SyntaxFactory.LiteralExpression(
                    SyntaxKind.NumericalLiteralExpression,
                    VisitNumber(SyntaxFactory.Literal(
                        ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.UseHexadecimalNumbers),
                        value)));
            }
            return base.VisitLiteralExpression(node);
        }

        public override SyntaxNode? VisitUnaryExpression(UnaryExpressionSyntax node) => RetargetingSyntaxFacts.IsBitwiseExpression(node.Kind()) ? VisitBitwiseUnaryExpression(node) : base.VisitUnaryExpression(node);

        public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            return RetargetingSyntaxFacts.IsBitwiseExpression(node.Kind())
                ? VisitBitwiseBinaryExpression(node)
                : base.VisitBinaryExpression(node);
        }

        public override SyntaxList<TNode> VisitList<TNode>(SyntaxList<TNode> list)
        {
            if (typeof(TNode) == typeof(StatementSyntax))
            {
                var typedList = new List<StatementSyntax>(list.Count);

                for (var idx = 0; idx < list.Count; idx++)
                {
                    var originalStatement = (StatementSyntax) (object) list[idx];
                    if (!_targetOptions.AcceptEmptyStatements
                        && originalStatement.IsKind(SyntaxKind.EmptyStatement))
                    {
                        // Skip over if it's an empty statement.
                        // Empty statements shouldn't have and pre nor post statements anyways.
                        continue;
                    }

                    var statement = (StatementSyntax) Visit(originalStatement);

                    if (_preStatementList.TryGetValue(originalStatement, out var preList))
                    {
                        typedList.AddRange(preList);
                        _preStatementList.Remove(originalStatement);
                    }

                    if (!_targetOptions.AcceptEmptyStatements
                        && statement.IsKind(SyntaxKind.EmptyStatement))
                    {
                        // Skip
                    }
                    else if (statement.HasAnnotation(RetargetingAnnotations.StatementToRemove))
                    {
                        // Skip
                    }
                    else if (statement.HasAnnotation(RetargetingAnnotations.ToFlatten))
                    {
                        var doStatement = (DoStatementSyntax) statement;
                        typedList.AddRange(doStatement.Body.Statements);
                    }
                    else
                    {
                        typedList.Add(statement);
                    }

                    if (_postStatementList.TryGetValue(originalStatement, out var postList))
                    {
                        typedList.AddRange(postList);
                        _postStatementList.Remove(originalStatement);
                    }
                }

                return (SyntaxList<TNode>) (object) SyntaxFactory.List(typedList);
            }

            return base.VisitList(list);
        }

        #endregion Shared Visitors

        public override partial SyntaxNode? VisitCompoundAssignmentStatement(CompoundAssignmentStatementSyntax node);

        private partial SyntaxNode? VisitBitwiseUnaryExpression(UnaryExpressionSyntax expression);

        private partial SyntaxNode? VisitBitwiseBinaryExpression(BinaryExpressionSyntax expression);

        private partial SyntaxToken VisitNumber(SyntaxToken token);

        private partial SyntaxToken VisitString(SyntaxToken token);

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
                _ = builder.Append("--");
                if (trivia.ToFullString()[2] == '[')
                    _ = builder.Append(' ');
                _ = builder.Append(trivia.ToFullString()[2..]);
                var text = StringBuilderPool.ToStringAndFree(builder);

                return SyntaxFactory.Comment(text);
            }
            else if (!_targetOptions.AcceptCCommentSyntax
                && trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                && trivia.ToFullString().StartsWith("/*", StringComparison.Ordinal))
            {
                var builder = StringBuilderPool.GetBuilder();
                _ = builder.Append(trivia.ToFullString()[2..^2]);
                Helpers.TurnIntoMultiLineComment(builder);
                var text = StringBuilderPool.ToStringAndFree(builder);

                return SyntaxFactory.Comment(text);
            }

            return base.VisitTrivia(trivia);
        }

        #endregion Trivia Rewriting
    }
}
