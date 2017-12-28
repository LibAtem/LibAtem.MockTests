using System;
using LibAtem.Commands;
using LibAtem.Util;
using Xunit;

namespace AtemEmulator.ComparisonTests.Util
{
    internal static class IntValueComparer
    {
        public delegate void SdkGetter(out long val);

        public static void Run(AtemComparisonHelper helper,  Func<long, ICommand> setter, SdkGetter getter, Func<long?> libget, long[] newVals)
        {
            Run(helper, setter, getter, libget, (long?)null);
            newVals.ForEach(v => Run(helper, setter, getter, libget, (long?) v));
        }

        public static void Run(AtemComparisonHelper helper, Func<long, ICommand> setter, SdkGetter getter, Func<long?> libget, long? newVal)
        {
            if (newVal.HasValue)
            {
                helper.SendCommand(setter(newVal.Value));
                helper.Sleep();
            }

            getter(out long val);
            long? libVal = libget();

            Assert.NotNull(libVal);
            Assert.Equal(val, libVal.Value);

            if (newVal.HasValue)
                Assert.Equal(newVal.Value, libVal.Value);
        }
    }
}