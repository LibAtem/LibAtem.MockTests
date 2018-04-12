using System;
using System.Collections.Generic;
using System.Linq;
using LibAtem.Util;

namespace LibAtem.ComparisonTests.Util
{
    internal static class EnumUtil
    {
        public static IEnumerable<T> GetAllCombinations<T>() where T : IComparable, IConvertible, IFormattable
        {
            var max = Enum.GetValues(typeof(T)).Cast<int>().Max() * 2;
            for (int i = 0; i < max; i++)
            {
                T v = (T) (object) i;
                if (v.IsValid())
                    yield return v;
            }
        }
        
    }
}
