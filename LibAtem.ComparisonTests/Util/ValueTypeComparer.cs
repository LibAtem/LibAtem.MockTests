using System;
using System.Collections.Generic;
using LibAtem.Commands;
using LibAtem.ComparisonTests.State;
using LibAtem.Util;

namespace LibAtem.ComparisonTests.Util
{
    internal static class ValueTypeComparer<T>
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

            List<string> before = ComparisonStateComparer.AreEqual(origSdk, origLib);

            helper.SendCommand(setter(newVal));
            helper.Sleep();

            List<string> sdk = ComparisonStateComparer.AreEqual(origSdk, helper.SdkState);
            List<string> lib = ComparisonStateComparer.AreEqual(origLib, helper.LibState);
            if (before.Count > 0 || sdk.Count > 0 || lib.Count > 0)
            {
                helper.Output.WriteLine("Setting value: " + newVal);
                if (before.Count > 0)
                {
                    helper.Output.WriteLine("Before wrong");
                    before.ForEach(helper.Output.WriteLine);
                }

                if (sdk.Count > 0)
                {
                    helper.Output.WriteLine("SDK wrong");
                    sdk.ForEach(helper.Output.WriteLine);
                }

                if (lib.Count > 0)
                {
                    helper.Output.WriteLine("Lib wrong");
                    lib.ForEach(helper.Output.WriteLine);
                }

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

            List<string> before = ComparisonStateComparer.AreEqual(origSdk, origLib);

            helper.SendCommand(setter(newVal));
            helper.Sleep();

            List<string> sdk = ComparisonStateComparer.AreEqual(origSdk, helper.SdkState);
            List<string> lib = ComparisonStateComparer.AreEqual(origLib, helper.LibState);
            if (before.Count > 0 || sdk.Count > 0 || lib.Count > 0)
            {
                helper.Output.WriteLine("Setting bad value: " + newVal);
                if (before.Count > 0)
                {
                    helper.Output.WriteLine("Before wrong");
                    before.ForEach(helper.Output.WriteLine);
                }

                if (sdk.Count > 0)
                {
                    helper.Output.WriteLine("SDK wrong");
                    sdk.ForEach(helper.Output.WriteLine);
                }

                if (lib.Count > 0)
                {
                    helper.Output.WriteLine("Lib wrong");
                    lib.ForEach(helper.Output.WriteLine);
                }

                helper.TestResult = false;
            }
        }
    }
}