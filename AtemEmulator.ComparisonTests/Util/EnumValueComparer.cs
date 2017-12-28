using System;
using System.Collections.Generic;
using LibAtem.Commands;
using LibAtem.Util;
using Xunit;

namespace AtemEmulator.ComparisonTests.Util
{
    internal static class EnumValueComparer<T1, T2> where T1 : struct
    {
        public delegate void SdkGetter(out T2 val);

        public static void Run(AtemComparisonHelper helper, IReadOnlyDictionary<T1, T2> map, Func<T1, ICommand> setter, SdkGetter getter, Func<T1?> libget, T1[] newVals)
        {
            Run(helper, map, setter, getter, libget, (T1?)null);
            newVals.ForEach(v => Run(helper, map, setter, getter, libget, (T1?) v));
        }

        public static void Run(AtemComparisonHelper helper, IReadOnlyDictionary<T1, T2> map, Func<T1, ICommand> setter, SdkGetter getter, Func<T1?> libget, T1? newVal)
        {
            if (newVal.HasValue)
            {
                helper.SendCommand(setter(newVal.Value));
                helper.Sleep();
            }

            getter(out T2 val);
            T1? libVal = libget();

            Assert.NotNull(libVal);
            Assert.Equal(val, map[libVal.Value]);

            if (newVal.HasValue)
                Assert.Equal(newVal.Value, libVal.Value);
        }
    }
}