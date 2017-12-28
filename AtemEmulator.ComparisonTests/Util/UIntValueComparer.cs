using System;
using LibAtem.Commands;
using LibAtem.Util;
using Xunit;

namespace AtemEmulator.ComparisonTests.Util
{
    internal static class UIntValueComparer
    {
        public delegate void SdkGetter(out uint val);

        public static void Run(AtemComparisonHelper helper, Func<uint, ICommand> setter, SdkGetter getter, Func<uint?> libget, uint[] newVals)
        {
            Run(helper, setter, getter, libget, (uint?)null);
            newVals.ForEach(v => Run(helper, setter, getter, libget, (uint?)v));
        }

        public static void Run(AtemComparisonHelper helper, Func<uint, ICommand> setter, SdkGetter getter, Func<uint?> libget, uint? newVal)
        {
            if (newVal.HasValue)
            {
                helper.SendCommand(setter(newVal.Value));
                helper.Sleep();
            }

            getter(out uint val);
            uint? libVal = libget();

            Assert.NotNull(libVal);
            Assert.Equal(val, libVal.Value);

            if (newVal.HasValue)
                Assert.Equal(newVal.Value, libVal.Value);
        }
    }
}