using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.DeviceProfile;
using LibAtem.State;
using LibAtem.Util;

namespace LibAtem.ComparisonTests.Util
{
    internal abstract class TestDefinitionBase<Tc, T> where Tc : ICommand, new()
    {
        protected readonly AtemComparisonHelper _helper;

        public TestDefinitionBase(AtemComparisonHelper helper)
        {
            _helper = helper;
        }

        public static void Run(TestDefinitionBase<Tc, T> def)
        {
            def.Run();
        }

        public TestDefinitionBase<Tc, T> Run()
        {
            Prepare();
            _helper.Sleep();

            ValueTypeComparer<Tc, T>.Run(_helper, this);
            ValueTypeComparer<Tc, T>.Fail(_helper, this);
            
            return this;
        }

        public TestDefinitionBase<Tc, T> RunSingle(T val)
        {
            ValueTypeComparer<Tc, T>.Run(_helper, this, val);

            return this;
        }

        public abstract void Prepare();

        public virtual T[] GoodValues
        {
            get {
                if (typeof(T) == typeof(bool))
                {
                    dynamic r = new bool[] { true, false };
                    return r;
                }

                throw new NotImplementedException("GoodValues");
            }
        }

        public virtual T[] BadValues
        {
            get
            {
                if (typeof(T) == typeof(VideoSource))
                {
                    dynamic goodValues = GoodValues.ToList();
                    dynamic r = VideoSourceLists.All.Where(s => !goodValues.Contains(s)).ToArray();
                    return r;
                }

                return new T[0];
            }
        }

        public abstract string PropertyName { get; }

        public virtual void SetupCommand(Tc cmd) { }

        public virtual ICommand GenerateCommand(T v)
        {
            Tc cmd = new Tc();

            var typeInfo = cmd.GetType();

            PropertyInfo prop = typeInfo.GetProperty(PropertyName);
            if (prop == null) throw new MissingMemberException(PropertyName);
            prop.SetValue(cmd, v);

            PropertyInfo maskProp = typeInfo.GetProperty("Mask");
            if (maskProp != null && maskProp.PropertyType != typeof(uint))
            {
                bool matchedMask = false;
                foreach (var m in Enum.GetValues(maskProp.PropertyType))
                {
                    if (m.ToString() == PropertyName)
                    {
                        maskProp.SetValue(cmd, m);
                        matchedMask = true;
                        break;
                    }
                }

                if (!matchedMask) throw new MissingFieldException("Missing mask value: " + PropertyName);
            }

            SetupCommand(cmd);

            return cmd;
        }

        protected static void SetCommandProperty(object obj, string name, object val)
        {
            PropertyInfo prop = obj.GetType().GetProperty(name);
            if (prop == null) throw new MissingMemberException(name);

            prop.SetValue(obj, val);
        }

        public abstract IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v);
        public abstract void UpdateExpectedState(AtemState state, bool goodValue, T v);
    }

    internal static class ValueTypeComparer<Tc, T> where Tc : ICommand, new()
    {
        private static void LogErrors(AtemComparisonHelper helper, string identifier, T val, AtemState origSdk, AtemState origLib)
        {
            List<string> before = AtemStateComparer.AreEqual(origSdk, origLib);
            List<string> sdk = AtemStateComparer.AreEqual(origSdk, helper.SdkState);
            List<string> lib = AtemStateComparer.AreEqual(origLib, helper.LibState);

            if (before.Count > 0 || sdk.Count > 0 || lib.Count > 0)
            {
                helper.Output.WriteLine("Setting " + identifier + " value: " + val);

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

        public static void Run(AtemComparisonHelper helper, TestDefinitionBase<Tc, T> definition)
        {
            var newVals = definition.GoodValues;
            newVals.ForEach(v => Run(helper, definition, v));
        }

        public static void Run(AtemComparisonHelper helper, TestDefinitionBase<Tc, T> definition, T newVal)
        {
            AtemState origSdk = helper.SdkState;
            AtemState origLib = helper.LibState;
            definition.UpdateExpectedState(origSdk, true, newVal);
            definition.UpdateExpectedState(origLib, true, newVal);

            ICommand cmd = definition.GenerateCommand(newVal);
            helper.SendAndWaitForMatching(definition.ExpectedCommands(true, newVal).ToList(), cmd);

            LogErrors(helper, "good", newVal, origSdk, origLib);

            IReadOnlyList<string> cmdIssues = CommandValidator.Validate(helper.Profile, cmd);
            if (cmdIssues.Count > 0)
            {
                cmdIssues.ForEach(helper.Output.WriteLine);
                helper.TestResult = false;
            }
        }

        public static void Fail(AtemComparisonHelper helper, TestDefinitionBase<Tc, T> definition)
        {
            var newVals = definition.BadValues;
            newVals.ForEach(v => Fail(helper, definition, v));
        }

        public static void Fail(AtemComparisonHelper helper, TestDefinitionBase<Tc, T> definition, T newVal)
        {
            AtemState origSdk = helper.SdkState;
            AtemState origLib = helper.LibState;

            definition.UpdateExpectedState(origSdk, false, newVal);
            definition.UpdateExpectedState(origLib, false, newVal);

            helper.SendAndWaitForMatching(definition.ExpectedCommands(false, newVal).ToList(), definition.GenerateCommand(newVal));

            LogErrors(helper, "bad", newVal, origSdk, origLib);
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
    internal sealed class IgnoreStateNodeEnabler : IDisposable
    {
        private readonly string _name;
        public IgnoreStateNodeEnabler(string name)
        {
            _name = name;
            AtemStateComparer.IgnoreNodes.Add(name);
        }

        public void Dispose()
        {
            AtemStateComparer.IgnoreNodes.Remove(_name);
        }
    }

}