using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.Retargeting.Core
{
    internal sealed partial class RetargetingRewriter : LuaSyntaxRewriter
    {
        private readonly IDictionary<StatementSyntax, IList<StatementSyntax>> _preStatementList = new Dictionary<StatementSyntax, IList<StatementSyntax>>();
        private readonly IDictionary<StatementSyntax, IList<StatementSyntax>> _postStatementList = new Dictionary<StatementSyntax, IList<StatementSyntax>>();
        private int _localId;

        private SyntaxToken GetImplDetailIdentifier()
        {
            var id = Interlocked.Increment(ref _localId);
            return SyntaxFactory.Identifier($"__impldetail__{id}");
        }

        private IdentifierNameSyntax GetImplDetailIdentifierName() =>
            SyntaxFactory.IdentifierName(GetImplDetailIdentifier());

        /// <summary>
        /// Adds the provided <paramref name="statement"/> before the provided <paramref name="baseStatement"/>.
        /// </summary>
        /// <param name="baseStatement">
        /// The statement to add the provided <paramref name="statement"/> before.
        /// </param>
        /// <param name="statement">
        /// The statement to add before the provided <paramref name="baseStatement"/>.
        /// </param>
        private void AddStatementBefore(StatementSyntax baseStatement, StatementSyntax statement)
        {
            if (!_preStatementList.TryGetValue(baseStatement, out var list))
            {
                _preStatementList[baseStatement] = list = new List<StatementSyntax>();
            }
            list.Add(statement);
        }

        /// <summary>
        /// Adds the provided <paramref name="statement"/> after the provided <paramref name="baseStatement"/>.
        /// </summary>
        /// <param name="baseStatement">
        /// The statement to add the provided <paramref name="statement"/> after.
        /// </param>
        /// <param name="statement">
        /// The statement to add after the provided <paramref name="baseStatement"/>.
        /// </param>
        private void AddStatementAfter(StatementSyntax baseStatement, StatementSyntax statement)
        {
            if (!_postStatementList.TryGetValue(baseStatement, out var list))
            {
                _postStatementList[baseStatement] = list = new List<StatementSyntax>();
            }
            list.Add(statement);
        }
    }
}
