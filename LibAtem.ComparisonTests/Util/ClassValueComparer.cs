using System;
using LibAtem.Commands;
using LibAtem.Util;
using Xunit;

namespace LibAtem.ComparisonTests.Util
{
    // TODO - remove this
    internal static class ClassValueComparer<T> where T : class
    {
        public static void Run(AtemComparisonHelper helper, Func<T, ICommand> setter, Func<T> getter, Func<T> libget, T[] newVals)
        {
            Run(helper, setter, getter, libget);
            newVals.ForEach(v => Run(helper, setter, getter, libget, v));
        }

        public static void Run(AtemComparisonHelper helper, Func<T, ICommand> setter, Func<T> getter, Func<T> libget, T newVal = null)
        {
            if (newVal != null)
            {
                helper.SendCommand(setter(newVal));
                helper.Sleep();
            }

            T val = getter();
            T libVal = libget();

            Assert.NotNull(libVal);
            Assert.Equal(val, libVal);

            if (newVal != null)
                Assert.Equal(newVal, libVal);
        }

        public static void Fail(AtemComparisonHelper helper, Func<T, ICommand> setter, Func<T> getter, Func<T> libget, T[] newVals)
        {
            newVals.ForEach(v => Fail(helper, setter, getter, libget, v));
        }

        public static void Fail(AtemComparisonHelper helper, Func<T, ICommand> setter, Func<T> getter, Func<T> libget, T newVal)
        {
            helper.SendCommand(setter(newVal));
            helper.Sleep();

            T val = getter();
            T libVal = libget();

            Assert.NotNull(libVal);
            Assert.Equal(val, libVal);
            Assert.NotEqual(newVal, libVal);
        }
    }
}