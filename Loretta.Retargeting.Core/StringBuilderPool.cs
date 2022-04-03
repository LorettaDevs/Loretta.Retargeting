using System.Text;

namespace Loretta.Retargeting.Core
{
    /// <summary>
    /// The usage is:
    ///        var sb = StringBuilderPool.GetBuilder();
    ///        ... Do Stuff...
    ///        var result = StringBuilderPool.ToStringAndFree(sb);
    /// </summary>
    internal static class StringBuilderPool
    {
        private struct Element
        {
            public StringBuilder? Value;
        }

        private static StringBuilder? s_first;
        private static readonly Element[] s_items = new Element[31];

        private static StringBuilder GetBuilderSlow()
        {
            var items = s_items;

            for (var idx = 0; idx < items.Length; idx++)
            {
                // Note that the initial read is optimistically not synchronized. That is intentional. 
                // We will interlock only when we have a candidate. in a worst case we may miss some
                // recently returned objects. Not a big deal.
                var inst = items[idx].Value;
                if (inst != null
                    && inst == Interlocked.CompareExchange(ref items[idx].Value, null, inst))
                {
                    return inst;
                }
            }

            return new StringBuilder();
        }

        public static StringBuilder GetBuilder()
        {
            // PERF: Examine the first element. If that fails, AllocateSlow will look at the remaining elements.
            // Note that the initial read is optimistically not synchronized. That is intentional. 
            // We will interlock only when we have a candidate. in a worst case we may miss some
            // recently returned objects. Not a big deal.
            var inst = s_first;
            if (inst == null
                || inst != Interlocked.CompareExchange(ref s_first, null, inst))
            {
                inst = GetBuilderSlow();
            }
            return inst;
        }

        public static string ToStringAndFree(StringBuilder builder)
        {
            var result = builder.ToString();
            Free(builder);
            return result;
        }

        public static void Free(StringBuilder builder)
        {
            if (builder.Capacity <= 1024)
            {
                builder.Clear();
                if (s_first == null)
                    // Intentionally not using interlocked here. 
                    // In a worst case scenario two objects may be stored into same slot.
                    // It is very unlikely to happen and will only mean that one of the objects will get collected.
                    s_first = builder;
                else
                {
                    FreeSlow(builder);
                }
            }
        }

        private static void FreeSlow(StringBuilder builder)
        {
            var items = s_items;
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i].Value == null)
                {
                    // Intentionally not using interlocked here. 
                    // In a worst case scenario two objects may be stored into same slot.
                    // It is very unlikely to happen and will only mean that one of the objects will get collected.
                    items[i].Value = builder;
                    break;
                }
            }
        }
    }
}
