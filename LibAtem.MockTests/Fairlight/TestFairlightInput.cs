using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Fairlight
{
    [Collection("ServerClientPool")]
    public class TestFairlightInput
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestFairlightInput(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }
        
        public static IBMDSwitcherFairlightAudioInput GetInput(AtemMockServerWrapper helper, long targetId)
        {
            IBMDSwitcherFairlightAudioMixer mixer = TestFairlightProgramOut.GetFairlightMixer(helper);
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioInputIterator>(mixer.CreateIterator);

            iterator.GetById(targetId, out IBMDSwitcherFairlightAudioInput input);
            Assert.NotNull(input);
            return input;
        }
        
        [Fact(Skip = "Doesn't work at the moment")]
        public void TestActiveConfiguration()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerInputSetCommand, FairlightMixerInputGetCommand>("ActiveConfiguration");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IEnumerable<long> useIds = Randomiser.SelectionOfGroup(helper.Helper.BuildLibState().Fairlight.Inputs.Keys.ToList());
                foreach (long id in useIds)
                {
                    IBMDSwitcherFairlightAudioInput input = GetInput(helper, id);

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    FairlightAudioState.InputState inputState = stateBefore.Fairlight.Inputs[id];

                    var testConfigs = AtemSDKConverter.GetFlagsValues(input.GetSupportedConfigurations,
                        AtemEnumMaps.FairlightInputConfigurationMap);
                    // Need more than 1 config to allow for switching around
                    if (1 == testConfigs.Count) continue;
                    tested = true;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = testConfigs[i % testConfigs.Count];
                        inputState.ActiveConfiguration = target.Item2;
                        helper.SendAndWaitForChange(stateBefore, () => { input.SetConfiguration(target.Item1); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact(Skip = "Doesn't work at the moment")]
        public void TestAddRemoveSources()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.FairlightMain, helper =>
            {
                IEnumerable<long> useIds = Randomiser.SelectionOfGroup(helper.Helper.BuildLibState().Fairlight.Inputs.Keys.ToList());
                foreach (long id in useIds)
                {
                    helper.Helper.SyncStates();

                    IBMDSwitcherFairlightAudioInput input = GetInput(helper, id);

                    AtemState stateBefore = helper.Helper.BuildLibState();
                    FairlightAudioState.InputState inputState = stateBefore.Fairlight.Inputs[id];
                    inputState.Sources.Add(new FairlightAudioState.InputSourceState
                    {
                        SourceId = 944,
                        MixOption = FairlightAudioMixOption.Off,
                        Gain = -23.7,
                    });

                    int sourceId = 944;
                    void mangleState(AtemState sdkState, AtemState libState)
                    {
                        FairlightAudioState.InputSourceState srcState = sdkState.Fairlight.Inputs[id].Sources.Single(s => s.SourceId == sourceId);
                        srcState.Dynamics.Limiter = null;
                        srcState.Dynamics.Compressor = null;
                        srcState.Dynamics.Expander = null;
                    }

                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new FairlightMixerSourceGetCommand
                        {
                            Index = (AudioSource)id,
                            SourceId = sourceId,
                            MixOption = FairlightAudioMixOption.Off,
                            Gain = -23.7,
                        });
                    }, -1, mangleState);

                    var removeSourceId = inputState.Sources[0].SourceId;
                    inputState.Sources.RemoveAt(0);

                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new FairlightMixerSourceDeleteCommand
                        {
                            Index = (AudioSource)id,
                            SourceId = removeSourceId,
                        });
                    }, -1, mangleState);
                }

            });
        }

#if ATEM_v8_1
        [Fact]
        public void TestAnalogInputLevel()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerAnalogAudioSetCommand, FairlightMixerAnalogAudioGetCommand>("InputLevel", true);
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightAnalog, helper =>
            {
                IEnumerable<long> useIds = helper.Helper.BuildLibState().Fairlight.Inputs.Keys.ToList();
                foreach (long id in useIds)
                {
                    IBMDSwitcherFairlightAudioInput input = GetInput(helper, id);
                    if (input is IBMDSwitcherFairlightAnalogAudioInput analogInput)
                    {
                        AtemState stateBefore = helper.Helper.BuildLibState();
                        FairlightAudioState.AnalogState inputState = stateBefore.Fairlight.Inputs[id].Analog;

                        var testConfigs = AtemSDKConverter.GetFlagsValues(analogInput.GetSupportedInputLevels,
                            AtemEnumMaps.FairlightAnalogInputLevelMap);
                        // Need more than 1 config to allow for switching around
                        if (1 == testConfigs.Count) continue;
                        tested = true;

                        for (int i = 0; i < 5; i++)
                        {
                            var target = testConfigs[i % testConfigs.Count];
                            inputState.InputLevel = target.Item2;
                            helper.SendAndWaitForChange(stateBefore, () => { analogInput.SetInputLevel(target.Item1); });
                        }
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestRcaToXlrEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerInputSetCommand, FairlightMixerInputGetCommand>("RcaToXlrEnabled");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightXLR, helper =>
            {
                IEnumerable<long> useIds = helper.Helper.BuildLibState().Fairlight.Inputs.Keys.ToList();
                foreach (long id in useIds)
                {
                    IBMDSwitcherFairlightAudioInput input = GetInput(helper, id);
                    if (input is IBMDSwitcherFairlightAudioInputXLR xlrInput)
                    {
                        AtemState stateBefore = helper.Helper.BuildLibState();
                        FairlightAudioState.InputState inputState = stateBefore.Fairlight.Inputs[id];

                        xlrInput.HasRCAToXLR(out int isAvailable);
                        Assert.Equal(1, isAvailable);
                        tested = true;

                        for (int i = 0; i < 5; i++)
                        {
                            inputState.Analog.InputLevel = i % 2 != 0
                                ? FairlightAnalogInputLevel.ConsumerLine
                                : FairlightAnalogInputLevel.ProLine;
                            helper.SendAndWaitForChange(stateBefore, () => { xlrInput.SetRCAToXLREnabled(i % 2); });
                        }
                    }
                }
            });
            Assert.True(tested);
        }
#else
        [Fact]
        public void TestHasAnalogInputLevel()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.FairlightAnalog, helper =>
            {
                var rawCommands = helper.Server.GetParsedDataDump();

                IEnumerable<long> useIds = helper.Helper.BuildLibState().Fairlight.Inputs.Keys.ToList();
                foreach (long id in useIds)
                {
                    IBMDSwitcherFairlightAudioInput input = GetInput(helper, id);
                    if (input is IBMDSwitcherFairlightAnalogAudioInput analog)
                    {
                        AtemState stateBefore = helper.Helper.BuildLibState();
                        FairlightAudioState.InputState inputState = stateBefore.Fairlight.Inputs[id];

                        analog.GetSupportedInputLevels(out _BMDSwitcherFairlightAudioAnalogInputLevel supportedLevels);
                        Assert.NotEqual(0, (int)supportedLevels);
                        Assert.NotNull(inputState.Analog);
                        tested = true;

                        var srcCommand = rawCommands.OfType<FairlightMixerInputGetV811Command>().Single(c => c.Index == (AudioSource)id);
                        var useLevels = inputState.Analog.SupportedInputLevel.FindFlagComponents();

                        for (int i = 0; i < 5; i++)
                        {
                            inputState.Analog.SupportedInputLevel = srcCommand.SupportedInputLevels =
                                useLevels[i % useLevels.Count];

                            helper.SendAndWaitForChange(stateBefore, () => { helper.Server.SendCommands(srcCommand); });
                        }
                    }
                }
            });
            Assert.True(tested);
        }
        [Fact]
        public void TestAnalogInputLevel()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerInputSetV811Command, FairlightMixerInputGetV811Command>("ActiveInputLevel");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightAnalog, helper =>
            {
                IEnumerable<long> useIds = helper.Helper.BuildLibState().Fairlight.Inputs.Keys.ToList();
                foreach (long id in useIds)
                {
                    IBMDSwitcherFairlightAudioInput input = GetInput(helper, id);
                    if (input is IBMDSwitcherFairlightAnalogAudioInput analogInput)
                    {
                        AtemState stateBefore = helper.Helper.BuildLibState();
                        FairlightAudioState.AnalogState inputState = stateBefore.Fairlight.Inputs[id].Analog;

                        var testConfigs = AtemSDKConverter.GetFlagsValues(analogInput.GetSupportedInputLevels,
                            AtemEnumMaps.FairlightAnalogInputLevelMap);
                        // Need more than 1 config to allow for switching around
                        if (1 == testConfigs.Count) continue;
                        tested = true;

                        for (int i = 0; i < 5; i++)
                        {
                            var target = testConfigs[i % testConfigs.Count];
                            inputState.InputLevel = target.Item2;
                            helper.SendAndWaitForChange(stateBefore, () => { analogInput.SetInputLevel(target.Item1); });
                        }
                    }
                }
            });
            Assert.True(tested);
        }
#endif
    }
}