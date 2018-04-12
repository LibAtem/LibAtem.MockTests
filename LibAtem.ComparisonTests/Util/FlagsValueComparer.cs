using System;
using LibAtem.Commands;
using LibAtem.Util;
using Xunit;

namespace AtemEmulator.ComparisonTests.Util
{
    internal static class FlagsValueComparer<T1, T2> where T1 : struct where T2 : struct
    {
        public delegate void SdkGetter(out T2 val);

        public static void Run(AtemComparisonHelper helper, Func<T1, ICommand> setter, SdkGetter getter, Func<T1?> libget, T1[] newVals)
        {
            Run(helper, setter, getter, libget);
            newVals.ForEach(v => Run(helper, setter, getter, libget, (T1?)v));
        }

        public static void Run(AtemComparisonHelper helper, Func<T1, ICommand> setter, SdkGetter getter, Func<T1?> libget, T1? newVal = null)
        {
            if (newVal.HasValue && setter != null)
            {
                helper.SendCommand(setter(newVal.Value));
                helper.Sleep();
            }

            getter(out T2 val);
            T1? libVal = libget();

            Assert.NotNull(libVal);
            Assert.Equal(Convert.ToInt32(val), Convert.ToInt32(libVal.Value));

            if (newVal.HasValue)
                Assert.Equal(newVal.Value, libVal.Value);
        }

        public static void Fail(AtemComparisonHelper helper, Func<T1, ICommand> setter, SdkGetter getter, Func<T1?> libget, T1[] newVals)
        {
            newVals.ForEach(v => Fail(helper, setter, getter, libget, v));
        }

        public static void Fail(AtemComparisonHelper helper, Func<T1, ICommand> setter, SdkGetter getter, Func<T1?> libget, T1 newVal)
        {
            if (setter != null)
            {
                helper.SendCommand(setter(newVal));
                helper.Sleep();
            }

            getter(out T2 val);
            T1? libVal = libget();

            Assert.NotNull(libVal);
            Assert.Equal(Convert.ToInt32(val), Convert.ToInt32(libVal.Value));
            Assert.NotEqual(newVal, libVal.Value);
        }
    }
}
