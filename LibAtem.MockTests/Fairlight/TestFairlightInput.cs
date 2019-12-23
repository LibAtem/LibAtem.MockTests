using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.Common;
using LibAtem.ComparisonTests;
using LibAtem.ComparisonTests.State.SDK;
using LibAtem.MockTests.Util;
using LibAtem.State;
using LibAtem.Util;
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
        
        private static IBMDSwitcherFairlightAudioInput GetInput(AtemMockServerWrapper helper, long targetId)
        {
            IBMDSwitcherFairlightAudioMixer mixer = TestFairlightProgramOut.GetFairlightMixer(helper);
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioInputIterator>(mixer.CreateIterator);

            iterator.GetById(targetId, out IBMDSwitcherFairlightAudioInput input);
            Assert.NotNull(input);
            return input;
        }

        [Fact]
        public void TestActiveConfiguration()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerInputSetCommand, FairlightMixerInputGetCommand>("ActiveConfiguration");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                IEnumerable<long> useIds = Randomiser.SelectionOfGroup(helper.Helper.LibState.Fairlight.Inputs.Keys.ToList());
                foreach (long id in useIds)
                {
                    IBMDSwitcherFairlightAudioInput input = GetInput(helper, id);

                    AtemState stateBefore = helper.Helper.LibState;
                    FairlightAudioState.InputState inputState = stateBefore.Fairlight.Inputs[id];

                    input.GetSupportedConfigurations(out _BMDSwitcherFairlightAudioInputConfiguration supportedConfigurations);
                    var testConfigs = supportedConfigurations.FindFlagComponents();
                    Assert.NotEmpty(testConfigs);
                    // Need more than 1 config to allow for switching around
                    if (1 == testConfigs.Count) continue;
                    tested = true;

                    for (int i = 0; i < 5; i++)
                    {
                        _BMDSwitcherFairlightAudioInputConfiguration target = testConfigs[i % testConfigs.Count];
                        inputState.ActiveConfiguration = AtemEnumMaps.FairlightInputConfigurationMap.FindByValue(target);
                        helper.SendAndWaitForChange(stateBefore, () => { input.SetConfiguration(target); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestAddRemoveSources()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.FairlightMain, helper =>
            {
                IEnumerable<long> useIds = Randomiser.SelectionOfGroup(helper.Helper.LibState.Fairlight.Inputs.Keys.ToList());
                foreach (long id in useIds)
                {
                    IBMDSwitcherFairlightAudioInput input = GetInput(helper, id);

                    AtemState stateBefore = helper.Helper.LibState;
                    FairlightAudioState.InputState inputState = stateBefore.Fairlight.Inputs[id];
                    inputState.Sources.Add(new FairlightAudioState.InputSourceState
                    {
                        SourceId = 944,
                        MixOption = FairlightAudioMixOption.Off,
                        Gain = -23.7,
                    });

                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new FairlightMixerSourceGetCommand
                        {
                            Index = (AudioSource)id,
                            SourceId = 944,
                            MixOption = FairlightAudioMixOption.Off,
                            Gain = -23.7,
                        });
                    });

                    var removeSourceId = inputState.Sources[0].SourceId;
                    inputState.Sources.RemoveAt(0);

                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new FairlightMixerSourceDeleteCommand
                        {
                            Index = (AudioSource)id,
                            SourceId = removeSourceId,
                        });
                    });
                }

            });
        }

    }
}