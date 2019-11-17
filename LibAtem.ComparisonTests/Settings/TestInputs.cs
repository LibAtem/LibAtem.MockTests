using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.Settings
{
    [Collection("Client")]
    public class TestInputs
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestInputs(AtemClientWrapper client, ITestOutputHelper output)
        {
            _client = client;
            _output = output;
        }

        // TODO - test LibAtem setters

        private abstract class InputPropertiesTestDefinition<T> : TestDefinitionBase<InputPropertiesSetCommand, T>
        {
            protected readonly VideoSource _id;
            protected readonly IBMDSwitcherInput _sdk;

            public InputPropertiesTestDefinition(AtemComparisonHelper helper, KeyValuePair<VideoSource, IBMDSwitcherInput> inp) : base(helper)
            {
                _id = inp.Key;
                _sdk = inp.Value;
            }

            public override void SetupCommand(InputPropertiesSetCommand cmd)
            {
                cmd.Id = _id;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                InputState obj = state.Settings.Inputs[_id];
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new InputPropertiesGetCommand() { Id = _id });
            }
        }

        private class InputPortTypeTestDefinition : InputPropertiesTestDefinition<ExternalPortTypeFlags>
        {
            public InputPortTypeTestDefinition(AtemComparisonHelper helper, KeyValuePair<VideoSource, IBMDSwitcherInput> inp) : base(helper, inp)
            {
            }

            public override void Prepare() { }

            public override string PropertyName => "ExternalPortType";
            public override ExternalPortTypeFlags MangleBadValue(ExternalPortTypeFlags v) => v;

            public override ExternalPortTypeFlags[] GoodValues
            {
                get
                {
                    _sdk.GetAvailableExternalPortTypes(out _BMDSwitcherExternalPortType types);

                    ExternalPortTypeFlags[] testValues = ((ExternalPortTypeFlags)types).FindFlagComponents().Where(v => v != ExternalPortTypeFlags.Unknown).ToArray();
                    if (testValues.Length <= 1) return new ExternalPortTypeFlags[0];
                    return testValues.Where(v => v != ExternalPortTypeFlags.Internal).ToArray();
                }
            }
            //public override ExternalPortType[] BadValues => Enum.GetValues(typeof(ExternalPortType)).OfType<ExternalPortType>().Except(GoodValues).ToArray();

            public override void UpdateExpectedState(AtemState state, bool goodValue, ExternalPortTypeFlags v)
            {
                if (goodValue)
                {
                    state.Settings.Inputs[_id].Properties.CurrentExternalPortType = v;
                    var audioId = (AudioSource)_id;
                    if (audioId.IsAvailable(_helper.Profile) && state.Audio.Inputs.ContainsKey((long)audioId))
                    {
                        state.Audio.Inputs[(long)audioId].ExternalPortType = v;
                    }
                }
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, ExternalPortTypeFlags v)
            {
                if (goodValue) return base.ExpectedCommands(goodValue, v);
                return new CommandQueueKey[0];
            }
        }
        [Fact]
        public void TestExternalPortType()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                var resolutions = new[] { VideoMode.P625i50PAL, VideoMode.N720p5994 };
                foreach (var mode in resolutions)
                {
                    helper.EnsureVideoMode(mode);
                    helper.Sleep();

                    foreach (var input in helper.GetSdkInputsOfType<IBMDSwitcherInput>())
                        new InputPortTypeTestDefinition(helper, input).Run();
                }
            }
        }

        [Fact]
        public void TestInputProperties()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                var failures = new List<string>();

                Dictionary<VideoSource, IBMDSwitcherInput> sdkInputs = helper.GetSdkInputsOfType<IBMDSwitcherInput>();
                foreach (var input in sdkInputs.OrderBy(i => i.Key))
                {
                    var libCmd = helper.FindWithMatching(new InputPropertiesGetCommand() { Id = input.Key });
                    if (libCmd == null)
                    {
                        failures.Add(string.Format("Missing InputPropertiesGetCommand with Id: {0}", input.Key));
                        continue;
                    }

                    input.Value.GetPortType(out _BMDSwitcherPortType portType);
                    InternalPortType[] expectedLibAtem = AtemEnumMaps.InternalPortTypeMap.Where(i => i.Value == portType).Select(i => i.Key).ToArray();
                    if (expectedLibAtem.Length == 0 || expectedLibAtem[0] != libCmd.InternalPortType)
                    {
                        var current = expectedLibAtem.Length == 0 ? "" : expectedLibAtem[0].ToString();
                        failures.Add(string.Format("{0}: Internal port type mismatch: {1}, {2}", libCmd.Id, current, libCmd.InternalPortType));
                    }

                    input.Value.GetInputAvailability(out _BMDSwitcherInputAvailability availability);
                    int libAtemValue = ((int)libCmd.SourceAvailability << 2) + (int)libCmd.MeAvailability;
                    if (libAtemValue != (int) availability)
                    {
                        failures.Add(string.Format("{0}: Source availability mismatch: {1}, {2}", libCmd.Id, (int) availability, libAtemValue));
                    }
                }

                Assert.Equal(new List<string>(), failures);
            }
        }

        private class InputLongNameTestDefinition : InputPropertiesTestDefinition<string>
        {
            private readonly string _defVal;

            public InputLongNameTestDefinition(AtemComparisonHelper helper, KeyValuePair<VideoSource, IBMDSwitcherInput> inp, string defVal) : base(helper, inp)
            {
                _defVal = defVal;
            }

            public override void Prepare() => _sdk.ResetNames();

            public override string PropertyName => "LongName";
            public override string MangleBadValue(string v) => v == null ? "" : v.Substring(0, 20);
            public override void UpdateExpectedState(AtemState state, bool goodValue, string v)
            {
                base.UpdateExpectedState(state, goodValue, v);

                // InputState obj = state.Settings.Inputs[_id];
                // obj.AreNamesDefault = (obj.Properties.LongName == _defVal);
            }

            public override string[] GoodValues => new string[] { "", "aaaa", Guid.NewGuid().ToString().Substring(0, 20), _defVal };
            public override string[] BadValues => new string[] { null, Guid.NewGuid().ToString() };
        }
        private class InputShortNameTestDefinition : InputPropertiesTestDefinition<string>
        {
            private readonly string _defVal;

            public InputShortNameTestDefinition(AtemComparisonHelper helper, KeyValuePair<VideoSource, IBMDSwitcherInput> inp, string defVal) : base(helper, inp)
            {
                _defVal = defVal;
            }

            public override void Prepare() => _sdk.ResetNames();

            public override string PropertyName => "ShortName";
            public override string MangleBadValue(string v) => v == null ? "" : v.Substring(0, 4);
            public override void UpdateExpectedState(AtemState state, bool goodValue, string v)
            {
                base.UpdateExpectedState(state, goodValue, v);

                // InputState obj = state.Inputs[_id];
                // obj.AreNamesDefault = (obj.ShortName == _defVal);
            }

            public override string[] GoodValues => new string[] { "", "aaaa", _defVal };
            public override string[] BadValues => new string[] { null, Guid.NewGuid().ToString(), "aaaab" };
        }
        [Fact]
        public void TestNames()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                var failures = new List<string>();

                Dictionary<VideoSource, IBMDSwitcherInput> sdkInputs = helper.GetSdkInputsOfType<IBMDSwitcherInput>();
                foreach (var input in sdkInputs.OrderBy(i => i.Key))
                {
                    // ensure names start off as default
                    input.Value.ResetNames();
                    helper.Sleep();

                    if (!helper.LibState.Settings.Inputs.TryGetValue(input.Key, out InputState libVal))
                    {
                        failures.Add(string.Format("Missing LibAtem Input with Id: {0}", input.Key));
                        continue;
                    }

                    Tuple<string, string> defaults = input.Key.GetDefaultName(helper.Profile);
                    if (defaults.Item1 != libVal.Properties.LongName)
                        failures.Add(string.Format("Mismatch in long name default for input {0}. Expected: {1}, Got: {2}", input.Key, libVal.Properties.LongName, defaults.Item1));
                    if (defaults.Item2 != libVal.Properties.ShortName)
                        failures.Add(string.Format("Mismatch in short name default for input {0}. Expected: {1}, Got: {2}", input.Key, libVal.Properties.ShortName, defaults.Item2));

                    new InputShortNameTestDefinition(helper, input, defaults.Item2).Run();
                    new InputLongNameTestDefinition(helper, input, defaults.Item1).Run();
                }

                _output.WriteLine(string.Join("\n", failures));
                Assert.Empty(failures);
            }
        }

        [Fact]
        public void TestTally()
        {
            // Note: This doesn't test every input, but does some sampling based on those not live and routed
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                Guid itId = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
                helper.SdkSwitcher.CreateIterator(ref itId, out IntPtr itPtr);
                IBMDSwitcherMixEffectBlockIterator iterator = (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(itPtr);

                iterator.Next(out IBMDSwitcherMixEffectBlock meBlock);
                Assert.NotNull(meBlock);

                meBlock.SetProgramInput((long)VideoSource.Black);
                helper.Sleep();

                Dictionary<VideoSource, InputState> inputs = helper.LibState.Settings.Inputs;
                foreach (var input in inputs)
                {
                    meBlock.SetProgramInput((long)input.Key);
                    helper.Sleep();

                    List<string> before = AtemStateComparer.AreEqual(helper.SdkState, helper.LibState);
                    if (before.Count != 0 && _output != null)
                    {
                        _output.WriteLine("New state wrong:");
                        before.ForEach(_output.WriteLine);
                    }
                    Assert.Empty(before);
                }
            }
        }
    }
}