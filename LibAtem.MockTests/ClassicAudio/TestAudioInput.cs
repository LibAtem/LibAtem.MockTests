using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.SdkStateBuilder;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.ClassicAudio
{
    [Collection("ServerClientPool")]
    public class TestAudioInput
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestAudioInput(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        public static IBMDSwitcherAudioInput GetInput(AtemMockServerWrapper helper, long targetId)
        {
            IBMDSwitcherAudioMixer mixer = TestAudioProgramOut.GetAudioMixer(helper);
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherAudioInputIterator>(mixer.CreateIterator);

            iterator.GetById(targetId, out IBMDSwitcherAudioInput input);
            Assert.NotNull(input);
            return input;
        }

        private static Func<ImmutableList<ICommand>, ICommand, IEnumerable<ICommand>>  CreateAutoCommandHandler(string name)
        {
#if ATEM_v8_1 || ATEM_v8_1_1
            return CommandGenerator.CreateAutoCommandHandler<AudioMixerInputSetCommand, AudioMixerInputGetV8Command>(name);
#endif
        }

        [Fact]
        public void TestGain()
        {
            var handler = CreateAutoCommandHandler("Gain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMain, helper =>
            {
                IEnumerable<long> useIds = helper.Helper.BuildLibState().Audio.Inputs.Keys.ToList();
                foreach (long id in useIds)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    var inputState = stateBefore.Audio.Inputs[id].Properties;

                    IBMDSwitcherAudioInput input = GetInput(helper, id);
                    tested = true;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range();
                        inputState.Gain = target;
                        helper.SendAndWaitForChange(stateBefore, () => { input.SetGain(target); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBalance()
        {
            var handler = CreateAutoCommandHandler("Balance");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMain, helper =>
            {
                IEnumerable<long> useIds = helper.Helper.BuildLibState().Audio.Inputs.Keys.ToList();
                foreach (long id in useIds)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    var inputState = stateBefore.Audio.Inputs[id].Properties;

                    IBMDSwitcherAudioInput input = GetInput(helper, id);
                    tested = true;

                    for (int i = 0; i < 5; i++)
                    {
                        var target = Randomiser.Range(-50, 50);
                        inputState.Balance = target;
                        helper.SendAndWaitForChange(stateBefore, () => { input.SetBalance(target / 50); });
                    }
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMixOption()
        {
            var handler = CreateAutoCommandHandler("MixOption");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.ClassicAudioMain, helper =>
            {
                IEnumerable<long> useIds = helper.Helper.BuildLibState().Audio.Inputs.Keys.ToList();
                foreach (long id in useIds)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    var inputState = stateBefore.Audio.Inputs[id].Properties;

                    IBMDSwitcherAudioInput input = GetInput(helper, id);
                    tested = true;
                    
                    for (int i = 0; i < 5; i++)
                    {
                        AudioMixOption target = Randomiser.EnumValue<AudioMixOption>();
                        if (id >= 2000 && target == AudioMixOption.On)
                        {
                            // Not supported
                            continue;
                        }

                        _BMDSwitcherAudioMixOption target2 = AtemEnumMaps.AudioMixOptionMap[target];
                        inputState.MixOption = target;
                        helper.SendAndWaitForChange(stateBefore, () => { input.SetMixOption(target2); });
                    }
                }
            });
            Assert.True(tested);
        }
    }
}