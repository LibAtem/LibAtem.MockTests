using System;
using LibAtem.Commands;
using LibAtem.Util;
using Xunit;

namespace LibAtem.ComparisonTests.Util
{
    internal static class BoolValueComparer
    {
        public delegate void SdkGetter(out int val);

        public static void Run(AtemComparisonHelper helper, Func<bool, ICommand> setter, SdkGetter getter,
            Func<bool?> libget, bool[] newVals)
        {
            Run(helper, setter, getter, libget, (bool?) null);
            newVals.ForEach(v => Run(helper, setter, getter, libget, v));
        }

        public static void Run(AtemComparisonHelper helper, Func<bool, ICommand> setter, SdkGetter getter, Func<bool?> libget, bool? newVal)
        {
            if (newVal.HasValue)
            {
                helper.SendCommand(setter(newVal.Value));
                helper.Sleep();
            }

            getter(out int val);
            bool boolVal = val != 0;
            bool? libVal = libget();

            Assert.NotNull(libVal);
            Assert.Equal(boolVal, libVal.Value);

            if (newVal.HasValue)
                Assert.Equal(newVal.Value, boolVal);
        }
    }
}