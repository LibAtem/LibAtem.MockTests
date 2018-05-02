using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
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

        [Fact] // TODO - this is completely broken as ExternalPortType is not a flags enum
        public void TestInputPortTypes()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                Dictionary<VideoSource, ComparisonInputState> inputs = helper.LibState.Inputs;
                foreach (var input in inputs)
                {
                    ExternalPortType[] testValues = input.Value.AvailableExternalPortTypes.FindFlagComponents().ToArray();
                    if (testValues.Length > 1)
                        testValues = testValues.Where(v => v != ExternalPortType.Internal).ToArray();
                    ExternalPortType[] badValues = Enum.GetValues(typeof(ExternalPortType)).OfType<ExternalPortType>().Where(v => !testValues.Contains(v)).ToArray();

                    ICommand Setter(ExternalPortType v)
                    {
                        return new InputPropertiesSetCommand
                        {
                            Mask = InputPropertiesSetCommand.MaskFlags.ExternalPortType,
                            Id = input.Key,
                            ExternalPortType = v,
                        };
                    }

                    void UpdateExpectedState(ComparisonState state, ExternalPortType v) => state.Inputs[input.Key].CurrentExternalPortType = v;

                    ValueTypeComparer<ExternalPortType>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<ExternalPortType>.Fail(helper, Setter, badValues);
                }
            }
        }

        [Fact]
        public void TestInputProperties()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                Dictionary<VideoSource, IBMDSwitcherInput> sdkInputs = helper.GetSdkInputsOfType<IBMDSwitcherInput>();
                List<InputPropertiesGetCommand> libAtemInputs = helper.FindAllOfType<InputPropertiesGetCommand>();

                var failures = new List<string>();

                foreach (InputPropertiesGetCommand libAtemInput in libAtemInputs.OrderBy(i => i.Id))
                {
                    if (!sdkInputs.TryGetValue(libAtemInput.Id, out IBMDSwitcherInput sdkInput))
                    {
                        failures.Add(string.Format("Missing sdk input: {0}", libAtemInput.Id));
                        continue;
                    }
                    
                    sdkInput.GetPortType(out _BMDSwitcherPortType portType);
                    InternalPortType[] expectedLibAtem = AtemEnumMaps.InternalPortTypeMap.Where(i => i.Value == portType).Select(i => i.Key).ToArray();
                    if (expectedLibAtem.Length == 0 || expectedLibAtem[0] != libAtemInput.InternalPortType)
                    {
                        var current = expectedLibAtem.Length == 0 ? "" : expectedLibAtem[0].ToString();
                        failures.Add(string.Format("{0}: Internal port type mismatch: {1}, {2}", libAtemInput.Id, current, libAtemInput.InternalPortType));
                    }

                    sdkInput.GetInputAvailability(out _BMDSwitcherInputAvailability availability);
                    int libAtemValue = ((int) libAtemInput.SourceAvailability << 2) + (int) libAtemInput.MeAvailability;
                    if (libAtemValue != (int) availability)
                    {
                        failures.Add(string.Format("{0}: Soure availability mismatch: {1}, {2}", libAtemInput.Id, (int) availability, libAtemValue));
                    }
                }

                Assert.Equal(new List<string>(), failures);
            }
        }

        [Fact]
        public void TestNames()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                Dictionary<VideoSource, ComparisonInputState> inputs = helper.LibState.Inputs;
                foreach (var input in inputs)
                {
                    Tuple<string, string> defaults = input.Key.GetDefaultName(helper.Profile);

                    // ensure names start off as default
                    helper.SendCommand(new InputPropertiesSetCommand
                    {
                        Mask = InputPropertiesSetCommand.MaskFlags.LongName | InputPropertiesSetCommand.MaskFlags.ShortName,
                        Id = input.Key,
                        LongName = defaults.Item1,
                        ShortName = defaults.Item2,
                    });
                    helper.Sleep();

                    string[] longValues = {null, "", Guid.NewGuid().ToString(), "aaaa", defaults.Item1};
                    ICommand LongSetter(string v)
                    {
                        return new InputPropertiesSetCommand
                        {
                            Mask = InputPropertiesSetCommand.MaskFlags.LongName,
                            Id = input.Key,
                            LongName = v,
                        };
                    }

                    void UpdateLongState(ComparisonState state, string v)
                    {
                        v = v ?? "";
                        if (v.Length > 20)
                            v = v.Substring(0, 20);

                        ComparisonInputState props = state.Inputs[input.Key];
                        props.LongName = v;
                        props.AreNamesDefault = v == defaults.Item1;
                    }

                    ValueTypeComparer<string>.Run(helper, LongSetter, UpdateLongState, longValues);

                    string[] shortValues = { null, "", Guid.NewGuid().ToString(), "aaaa", defaults.Item2 };
                    ICommand ShortSetter(string v)
                    {
                        return new InputPropertiesSetCommand
                        {
                            Mask = InputPropertiesSetCommand.MaskFlags.ShortName,
                            Id = input.Key,
                            ShortName = v,
                        };
                    }
                    void UpdateShortState(ComparisonState state, string v)
                    {
                        v = v ?? "";
                        if (v.Length > 4)
                            v = v.Substring(0, 4);

                        ComparisonInputState props = state.Inputs[input.Key];
                        props.ShortName = v;
                        props.AreNamesDefault = v == defaults.Item2;
                    }

                    ValueTypeComparer<string>.Run(helper, ShortSetter, UpdateShortState, shortValues);
                }
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

                meBlock.SetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, (long)VideoSource.Black);
                helper.Sleep();

                Dictionary<VideoSource, ComparisonInputState> inputs = helper.LibState.Inputs;
                foreach (var input in inputs)
                {
                    meBlock.SetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, (long) input.Key);
                    helper.Sleep();

                    List<string> before = ComparisonStateComparer.AreEqual(helper.SdkState, helper.LibState);
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