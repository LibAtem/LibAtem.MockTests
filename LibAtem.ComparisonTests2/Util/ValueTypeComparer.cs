using System;
using System.Collections.Generic;
using System.Linq;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.DeviceProfile;
using LibAtem.Util;

namespace LibAtem.ComparisonTests2.Util
{
    internal interface ITestDefinition<T>
    {
        void Prepare();

        T[] GoodValues();
        T[] BadValues();

        ICommand GenerateCommand(T v);

        void UpdateExpectedState(ComparisonState state, bool goodValue, T v);

        IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v);
    }

    internal abstract class TestDefinitionBase<T> : ITestDefinition<T>
    {
        protected readonly AtemComparisonHelper _helper;

        public TestDefinitionBase(AtemComparisonHelper helper)
        {
            _helper = helper;
        }

        public void Run()
        {
            Prepare();
            _helper.Sleep();

            ValueTypeComparer<T>.Run(_helper, this);
            ValueTypeComparer<T>.Fail(_helper, this);
        }

        public virtual T[] GoodValues()
        {
            if (typeof(T) == typeof(bool))
            {
                dynamic r = new bool[] { true, false };
                return r;
            }

           throw new NotImplementedException("GoodValues");
        }
        public virtual T[] BadValues() {
            if (typeof(T) == typeof(VideoSource))
            {
                dynamic goodValues = GoodValues().ToList();
                dynamic r = VideoSourceLists.All.Where(s => !goodValues.Contains(s)).ToArray();
                return r;
            }

            return new T[0];
        }

        public abstract IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v);
        public abstract ICommand GenerateCommand(T v);
        public abstract void Prepare();
        public abstract void UpdateExpectedState(ComparisonState state, bool goodValue, T v);
    }

    internal static class ValueTypeComparer<T>
    {
        public static void Run(AtemComparisonHelper helper, ITestDefinition<T> definition)
        {
            var newVals = definition.GoodValues();
            newVals.ForEach(v => Run(helper, definition, v));
        }

        public static void Run(AtemComparisonHelper helper, ITestDefinition<T> definition, T newVal)
        {
            ComparisonState origSdk = helper.SdkState;
            ComparisonState origLib = helper.LibState;
            definition.UpdateExpectedState(origSdk, true, newVal);
            definition.UpdateExpectedState(origLib, true, newVal);

            List<string> before = ComparisonStateComparer.AreEqual(origSdk, origLib);

            ICommand cmd = definition.GenerateCommand(newVal);
            helper.SendAndWaitForMatching(definition.ExpectedCommands(true, newVal).ToList(), cmd);

            List<string> sdk = ComparisonStateComparer.AreEqual(origSdk, helper.SdkState);
            List<string> lib = ComparisonStateComparer.AreEqual(origLib, helper.LibState);
            if (before.Count > 0 || sdk.Count > 0 || lib.Count > 0)
            {
                helper.Output.WriteLine("Setting good value: " + newVal);
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

            IReadOnlyList<string> cmdIssues = CommandValidator.Validate(helper.Profile, cmd);
            if (cmdIssues.Count > 0)
            {
                cmdIssues.ForEach(helper.Output.WriteLine);
                helper.TestResult = false;
            }
        }

        public static void Fail(AtemComparisonHelper helper, ITestDefinition<T> definition)
        {
            var newVals = definition.BadValues();
            newVals.ForEach(v => Fail(helper, definition, v));
        }

        public static void Fail(AtemComparisonHelper helper, ITestDefinition<T> definition, T newVal)
        {
            ComparisonState origSdk = helper.SdkState;
            ComparisonState origLib = helper.LibState;

            definition.UpdateExpectedState(origSdk, false, newVal);
            definition.UpdateExpectedState(origLib, false, newVal);

            List<string> before = ComparisonStateComparer.AreEqual(origSdk, origLib);

            helper.SendAndWaitForMatching(definition.ExpectedCommands(false, newVal).ToList(), definition.GenerateCommand(newVal));

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

    internal sealed class ValueDefaults : IDisposable
    {
        private readonly AtemComparisonHelper _helper;
        private readonly ICommand _cmd;

        public ValueDefaults(AtemComparisonHelper helper, ICommand cmd)
        {
            _helper = helper;
            _cmd = cmd;

            _helper.SendCommand(_cmd);
        }

        public void Dispose()
        {
            _helper.SendCommand(_cmd);
        }
    }

    internal sealed class SettingEnabler : IDisposable
    {
        private readonly Action<bool> _act;
        public SettingEnabler(Action<bool> act)
        {
            _act = act;
            _act(true);
        }

        public void Dispose()
        {
            _act(false);
        }
    }

}