using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Benchmark
{
    [SimpleJob(RuntimeMoniker.Net60, baseline: true)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [SimpleJob(RuntimeMoniker.NativeAot70)]
    [MemoryDiagnoser]
    public class StringNeedsRewriteBenchmark
    {
        public static string Text => @"aaaa\z   \u{FFFF}\xFF\xFF";

        [Benchmark(Baseline = true)]
        public bool Simple()
        {
            var res = false;
            for (var part = 0; part <= 3; part++)
            {
                res |= SimpleImpl(
                    Text,
                    part == 1,
                    part == 2,
                    part == 3);
            }
            return res;
        }

        [Benchmark]
        public bool Smart()
        {
            var res = false;
            for (var part = 0; part <= 3; part++)
            {
                res |= SmartImpl(
                    Text,
                    part == 1,
                    part == 2,
                    part == 3);
            }
            return res;
        }

        private static bool SimpleImpl(string text, bool canUseHex, bool canUseUnicode, bool canUseWhitespace)
        {
            return (!canUseHex && text.Contains("\\x"))
                || (!canUseUnicode && text.Contains("\\u"))
                || (!canUseWhitespace && text.Contains("\\z"));
        }

        private static bool SmartImpl(string text, bool canUseHex, bool canUseUnicode, bool canUseWhitespace)
        {
            var slashIdx = -1;
            // Keep going until we don't find any more slashes.
            while ((slashIdx = text.IndexOf('\\', slashIdx + 1)) >= 0 && slashIdx < text.Length - 1)
            {
                var escapeId = text[slashIdx + 1];
                switch (escapeId)
                {
                    case 'x' when !canUseHex:
                    case 'u' when !canUseUnicode:
                    case 'z' when !canUseWhitespace: return true;
                }
            }

            return false;
        }
    }
}
