using System;
using LibAtem.Commands;
using LibAtem.ComparisonTests.State;
using LibAtem.Util;
using Xunit;

namespace LibAtem.ComparisonTests.Util
{
    internal static class ValueTypeComparer<T> where T : struct
    {
        public static void Run(AtemComparisonHelper helper, Func<T, ICommand> setter, Action<ComparisonState, T> updater, T[] newVals)
        {
            newVals.ForEach(v => Run(helper, setter, updater, v));
        }
        
        public static void Run(AtemComparisonHelper helper, Func<T, ICommand> setter, Action<ComparisonState, T> updater, T newVal)
        {
            ComparisonState origSdk = helper.SdkState;
            ComparisonState origLib = helper.LibState;
            updater(origSdk, newVal);
            updater(origLib, newVal);

            bool res = ComparisonStateComparer.AreEqual(helper.Output, origSdk, origLib);

            helper.SendCommand(setter(newVal));
            helper.Sleep();

            res = res && ComparisonStateComparer.AreEqual(helper.Output, origSdk, helper.SdkState) && ComparisonStateComparer.AreEqual(helper.Output, origLib, helper.LibState);
            if (!res)
            {
                helper.Output.WriteLine("Setting value: " + newVal);
                helper.TestResult = false;
            }
        }

        public static void Fail(AtemComparisonHelper helper, Func<T, ICommand> setter, Action<ComparisonState, T> updater, T[] newVals)
        {
            newVals.ForEach(v => Fail(helper, setter, updater, v));
        }

        public static void Fail(AtemComparisonHelper helper, Func<T, ICommand> setter, params T[] newVals)
        {
            newVals.ForEach(v => Fail(helper, setter, null, v));
        }

        public static void Fail(AtemComparisonHelper helper, Func<T, ICommand> setter, Action<ComparisonState, T> updater, T newVal)
        {
            ComparisonState origSdk = helper.SdkState;
            ComparisonState origLib = helper.LibState;

            if (updater != null)
            {
                updater(origSdk, newVal);
                updater(origLib, newVal);
            }

            bool res = ComparisonStateComparer.AreEqual(helper.Output, origSdk, origLib);

            helper.SendCommand(setter(newVal));
            helper.Sleep();

            res = res && ComparisonStateComparer.AreEqual(helper.Output, origSdk, helper.SdkState) && ComparisonStateComparer.AreEqual(helper.Output, origLib, helper.LibState);
            if (!res)
            {
                helper.Output.WriteLine("Setting bad value: " + newVal);
                helper.TestResult = false;
            }
        }
    }
}