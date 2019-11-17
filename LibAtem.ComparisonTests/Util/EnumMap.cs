using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LibAtem.ComparisonTests2.Util
{
    internal static class EnumMap
    {
        public static void EnsureIsComplete<T1, T2>(IReadOnlyDictionary<T1, T2> map)
        {
            List<T1> vals = Enum.GetValues(typeof(T1)).OfType<T1>().ToList();

            List<T1> missing = vals.Where(v => !map.ContainsKey(v)).ToList();
            Assert.Empty(missing);

            // Expect map and values to have the same number
            Assert.Equal(vals.Count, map.Count);
            Assert.Equal(Enum.GetValues(typeof(T2)).Length, map.Count);

            // Expect all the map values to be unique
            Assert.Equal(vals.Count, map.Select(v => v.Value).Distinct().Count());
        }

        public static void EnsureIsMatching<T1, T2>()
        {
            int vals = Enum.GetValues(typeof(T1)).OfType<T1>().Select(e => Convert.ToInt32(e)).Sum(a => a);
            int vals2 = Enum.GetValues(typeof(T2)).OfType<T2>().Select(e => Convert.ToInt32(e)).Sum(a => a);

            // We assume they are valid if their sums are equal. 
            // This only works for flags. Other types need the conversion map and EnsureIsComplete
            Assert.Equal(vals, vals2);
        }
    }
}
