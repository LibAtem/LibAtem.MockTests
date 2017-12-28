using System;
using LibAtem.Commands;
using LibAtem.Util;
using Xunit;

namespace AtemEmulator.ComparisonTests.Util
{
    internal static class DoubleValueComparer
    {
        public delegate void SdkGetter(out double val);

        public static void Run(AtemComparisonHelper helper, Func<double, ICommand> setter, SdkGetter getter, Func<double?> libget, double[] newVals, double scale=1)
        {
            Run(helper, setter, getter, libget, (double?)null, scale);
            newVals.ForEach(v => Run(helper, setter, getter, libget, (double?) v, scale));
        }

        public static void Run(AtemComparisonHelper helper, Func<double, ICommand> setter, SdkGetter getter, Func<double?> libget, double? newVal, double scale)
        {
            if (newVal.HasValue)
            {
                helper.SendCommand(setter(newVal.Value));
                helper.Sleep();
            }

            getter(out double val);
            double? libVal = libget();

            Assert.NotNull(libVal);
            Assert.True(Math.Abs(libVal.Value / scale - val) < 0.01);

            if (newVal.HasValue)
                Assert.True(Math.Abs(val - newVal.Value / scale) < 0.01);
        }
    }
}