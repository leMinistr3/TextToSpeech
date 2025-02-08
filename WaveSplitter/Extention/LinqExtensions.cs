using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveSplitter.Extention
{
    static public class LinqExtensions
    {
        static public IEnumerable<T> BigSkip<T>(this IEnumerable<T> items, long howMany)
            => BigSkip(items, Int32.MaxValue, howMany);

        internal static IEnumerable<T> BigSkip<T>(this IEnumerable<T> items, int segmentSize, long howMany)
        {
            long segmentCount = Math.DivRem(howMany, segmentSize, out long remainder);

            for (long i = 0; i < segmentCount; i += 1)
                items = items.Skip(segmentSize);

            if (remainder != 0)
                items = items.Skip((int)remainder);

            return items;
        }

        static public IEnumerable<T> BigTake<T>(this IEnumerable<T> items, long howMany) 
            => BigTake(items, Int32.MaxValue, howMany);

        internal static IEnumerable<T> BigTake<T>(this IEnumerable<T> items, int segmentSize, long howMany)
        {
            long segmentCount = Math.DivRem(howMany, segmentSize, out long remainder);

            for (long i = 0; i < segmentCount; i += 1)
                items = items.Take(segmentSize);

            if (remainder != 0)
                items = items.Take((int)remainder);

            return items;
        }
    }
}
