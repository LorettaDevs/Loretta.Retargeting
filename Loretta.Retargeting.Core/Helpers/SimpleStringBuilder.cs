namespace Loretta.Retargeting.Core
{
    internal ref struct StackStringBuilder
    {
        private readonly Span<char> _span;
        private int _length;

        public StackStringBuilder(Span<char> span)
        {
            _span = span;
            _length = 0;
        }

        private void CheckDefault()
        {
            if (_span == default)
                throw new InvalidOperationException("Operation attempted on defaulted builder.");
        }

        private void EnsureRemainingLength(int requiredRemaining)
        {
            if (_span.Length - _length < requiredRemaining)
                throw new InvalidOperationException("Not enough length remaining for this operation.");
        }

        public StackStringBuilder Append(string text)
        {
            CheckDefault();
            EnsureRemainingLength(text.Length);

            text.CopyTo(_span[_length..]);
            _length += text.Length;
            return this;
        }

        public StackStringBuilder Append(char ch)
        {
            CheckDefault();
            EnsureRemainingLength(1);

            _span[_length] = ch;
            _length++;
            return this;
        }

        public StackStringBuilder Append(char ch, int count)
        {
            CheckDefault();
            EnsureRemainingLength(count);

            var end = _length + count;
            while (_length < end)
            {
                _span[_length] = ch;
                _length++;
            }

            return this;
        }

        public override string ToString() => new(_span);
    }
}
