using System;
using LibAtem.Commands;
using LibAtem.Util;
using Xunit;

namespace AtemEmulator.ComparisonTests.Util
{
    internal static class DoubleValueComparer
    {
        public delegate void SdkGetter(out double val);

        public static void Run(AtemComparisonHelper helper, Func<double, ICommand> setter, SdkGetter getter, Func<double?> libget, double[] newVals, double scale=1, double tol = 0.0001)
        {
            Run(helper, setter, getter, libget, (double?)null, scale, tol);
            newVals.ForEach(v => Run(helper, setter, getter, libget, v, scale, tol));
        }

        public static void Run(AtemComparisonHelper helper, Func<double, ICommand> setter, SdkGetter getter, Func<double?> libget, double? newVal, double scale, double tol=0.0001)
        {
            if (newVal.HasValue)
            {
                helper.SendCommand(setter(newVal.Value));
                helper.Sleep();
            }

            getter(out double val);
            double? libVal = libget();

            Assert.NotNull(libVal);
            Assert.True(Math.Abs(libVal.Value / scale - val) < tol);

            if (newVal.HasValue)
                Assert.True(Math.Abs(val - newVal.Value / scale) < tol);
        }

        public static void Fail(AtemComparisonHelper helper, Func<double, ICommand> setter, SdkGetter getter, Func<double?> libget, double[] newVals, double scale = 1, double tol = 0.0001)
        {
            newVals.ForEach(v => Fail(helper, setter, getter, libget, v, scale, tol));
        }

        public static void Fail(AtemComparisonHelper helper, Func<double, ICommand> setter, SdkGetter getter, Func<double?> libget, double newVal, double scale, double tol = 0.0001)
        {
            helper.SendCommand(setter(newVal));
            helper.Sleep();

            getter(out double val);
            double? libVal = libget();

            Assert.NotNull(libVal);
            Assert.True(Math.Abs(libVal.Value / scale - val) < tol);
            Assert.False(Math.Abs(val - newVal / scale) < tol);
        }
    }
}