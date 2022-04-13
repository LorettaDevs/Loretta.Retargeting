using System.Diagnostics.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.Retargeting.Core.CachedNodes
{
    internal sealed class BitLibraryGlobals
    {
        private readonly LuaVersion _version;
        private MemberAccessExpressionSyntax? _global, _arshift, _band, _bnot, _bor, _bswap, _bxor, _lshift, _rol, _ror, _rshift, _tobit, _tohex;

        public BitLibraryGlobals(LuaVersion version)
        {
            _version = version;
            HasBitLibrary = LuaVersionFacts.HasBitLibrary(_version);
        }

        [MemberNotNullWhen(
            true,
            nameof(Global),
            nameof(ArithmeticRightShift),
            nameof(BitwiseAnd),
            nameof(BitwiseNot),
            nameof(BitwiseOr),
            nameof(BitSwap),
            nameof(BitwiseExclusiveOr),
            nameof(LeftShift),
            nameof(RotateLeft),
            nameof(RotateRight),
            nameof(RightShift),
            nameof(ToBit),
            nameof(ToHex))]
        public bool HasBitLibrary { get; }

        public MemberAccessExpressionSyntax? Global
        {
            get
            {
                if (_global is null && HasBitLibrary)
                {
                    var global = SyntaxFactory.MemberAccessExpression(
                        ConstantNodes.Global,
                        LuaVersionFacts.BitLibraryGlobalName(_version).Value);
                    Interlocked.CompareExchange(ref _global, global, null);
                }
                return _global;
            }
        }

        public MemberAccessExpressionSyntax? ArithmeticRightShift => GetOrInit(ref _arshift, "arshift");

        public MemberAccessExpressionSyntax? BitwiseAnd => GetOrInit(ref _band, "band");

        public MemberAccessExpressionSyntax? BitwiseNot => GetOrInit(ref _bnot, "bnot");

        public MemberAccessExpressionSyntax? BitwiseOr => GetOrInit(ref _bor, "bor");

        public MemberAccessExpressionSyntax? BitSwap => GetOrInit(ref _bswap, "bswap");

        public MemberAccessExpressionSyntax? BitwiseExclusiveOr => GetOrInit(ref _bxor, "bxor");

        public MemberAccessExpressionSyntax? LeftShift => GetOrInit(ref _lshift, "lshift");

        public MemberAccessExpressionSyntax? RotateLeft => GetOrInit(ref _rol, "rol");

        public MemberAccessExpressionSyntax? RotateRight => GetOrInit(ref _ror, "ror");

        public MemberAccessExpressionSyntax? RightShift => GetOrInit(ref _rshift, "rshift");

        public MemberAccessExpressionSyntax? ToBit => GetOrInit(ref _tobit, "tobit");

        public MemberAccessExpressionSyntax? ToHex => GetOrInit(ref _tohex, "tohex");

        private MemberAccessExpressionSyntax? GetOrInit(ref MemberAccessExpressionSyntax? field, string name)
        {
            if (field is null && HasBitLibrary)
            {
                var value = SyntaxFactory.MemberAccessExpression(Global, name);
                Interlocked.CompareExchange(ref field, value, null);
            }

            return field;
        }
    }
}
