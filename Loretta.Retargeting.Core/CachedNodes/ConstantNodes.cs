using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using static Loretta.CodeAnalysis.Lua.SyntaxFactory;

namespace Loretta.Retargeting.Core.CachedNodes
{
    internal static class ConstantNodes
    {
        public static readonly SyntaxTriviaList SpaceList = TriviaList(Space);

        public static readonly SyntaxToken SpacedSlashToken = Token(
            SpaceList,
            SyntaxKind.SlashToken,
            SpaceList);

        public static readonly IdentifierNameSyntax Global = IdentifierName("_G");

        public static readonly MemberAccessExpressionSyntax MathGlobal = MemberAccessExpression(
            Global,
            "math");

        public static readonly MemberAccessExpressionSyntax MathFloor = MemberAccessExpression(
            MathGlobal,
            "floor");
    }
}
