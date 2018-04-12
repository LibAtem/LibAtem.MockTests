using System;
using LibAtem.Commands;
using LibAtem.Util;
using Xunit;

namespace LibAtem.ComparisonTests.Util
{
    internal static class ValueTypeComparer<T> where T : struct
    {
        public delegate void SdkGetter(out T val);

        public static void Run(AtemComparisonHelper helper,  Func<T, ICommand> setter, SdkGetter getter, Func<T?> libget, T[] newVals)
        {
            Run(helper, setter, getter, libget, (T?)null);
            newVals.ForEach(v => Run(helper, setter, getter, libget, (T?) v));
        }

        public static void Run(AtemComparisonHelper helper, Func<T, ICommand> setter, SdkGetter getter, Func<T?> libget, T? newVal)
        {
            if (newVal.HasValue)
            {
                helper.SendCommand(setter(newVal.Value));
                helper.Sleep();
            }

            getter(out T val);
            T? libVal = libget();

            Assert.NotNull(libVal);
            Assert.Equal(val, libVal.Value);

            if (newVal.HasValue)
                Assert.Equal(newVal.Value, libVal.Value);
        }

        public static void Fail(AtemComparisonHelper helper, Func<T, ICommand> setter, SdkGetter getter, Func<T?> libget, T[] newVals)
        {
            newVals.ForEach(v => Fail(helper, setter, getter, libget, v));
        }

        public static void Fail(AtemComparisonHelper helper, Func<T, ICommand> setter, SdkGetter getter, Func<T?> libget, T newVal)
        {
            getter(out T val);
            if (val.Equals(newVal))
                return;

            helper.SendCommand(setter(newVal));
            helper.Sleep();

            getter(out val);
            T? libVal = libget();

            Assert.NotNull(libVal);
            Assert.Equal(val, libVal.Value);
            Assert.NotEqual(newVal, libVal.Value);
        }
    }
}