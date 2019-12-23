using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.ComparisonTests;
using LibAtem.ComparisonTests.State.SDK;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Fairlight
{
    [Collection("ServerClientPool")]
    public class TestFairlightInputSource
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestFairlightInputSource(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private static IBMDSwitcherFairlightAudioSource GetSource(IBMDSwitcherFairlightAudioInput input, long? targetId = null)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioSourceIterator>(input.CreateIterator);
            if (targetId.HasValue)
            {
                iterator.GetById(targetId.Value, out IBMDSwitcherFairlightAudioSource src);
                return src;
            }
            else
            {
                iterator.Next(out IBMDSwitcherFairlightAudioSource src);
                return src;
            }
        }

        private static IBMDSwitcherFairlightAudioSource GetSource(AtemMockServerWrapper helper, long inputId,
            long? sourceId = null)
        {
            return GetSource(TestFairlightInput.GetInput(helper, inputId), sourceId);
        }

        public static IBMDSwitcherFairlightAudioDynamicsProcessor GetDynamics(IBMDSwitcherFairlightAudioSource src)
        {
            var dynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(src.GetEffect);
            Assert.NotNull(dynamics);
            return dynamics;
        }

        public static IBMDSwitcherFairlightAudioEqualizer GetEqualizer(IBMDSwitcherFairlightAudioSource src)
        {
            var eq = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioEqualizer>(src.GetEffect);
            Assert.NotNull(eq);
            return eq;
        }

        public static void EachRandomSource(AtemMockServerWrapper helper, Action<AtemState, FairlightAudioState.InputSourceState, IBMDSwitcherFairlightAudioSource, int> fcn)
        {
            List<long> rawInputIds = helper.Helper.LibState.Fairlight.Inputs.Keys.ToList();
            IEnumerable<long> useIds = Randomiser.SelectionOfGroup(rawInputIds, 2);
            foreach (long id in useIds)
            {
                IBMDSwitcherFairlightAudioSource src = GetSource(helper, id);
                src.GetId(out long sourceId);

                AtemState stateBefore = helper.Helper.LibState;
                FairlightAudioState.InputSourceState srcState = stateBefore.Fairlight.Inputs[id].Sources.Single(s => s.SourceId == sourceId);

                for (int i = 0; i < 5; i++)
                {
                    fcn(stateBefore, srcState, src, i);
                }
            }
        }

        // TODO - test modifying multiple sources

        [Fact]
        public void TestFaderGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("FaderGain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, src, i) =>
                {
                    tested = true;
                    var target = Randomiser.Range();
                    srcState.FaderGain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { src.SetFaderGain(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("Gain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, src, i) =>
                {
                    tested = true;
                    var target = Randomiser.Range();
                    srcState.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { src.SetInputGain(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBalance()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("Balance");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, src, i) =>
                {
                    tested = true;
                    var target = Randomiser.Range(-100, 100);
                    srcState.Balance = target;
                    helper.SendAndWaitForChange(stateBefore, () => { src.SetPan(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMixOption()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("MixOption");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, src, i) =>
                {
                    var testConfigs = AtemSDKConverter.GetFlagsValues(src.GetSupportedMixOptions,
                        AtemEnumMaps.FairlightAudioMixOptionMap);
                    // Need more than 1 config to allow for switching around
                    if (1 == testConfigs.Count) return;
                    tested = true;
                    var target = testConfigs[i % testConfigs.Count];

                    srcState.MixOption = target.Item2;
                    helper.SendAndWaitForChange(stateBefore, () => { src.SetMixOption(target.Item1); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMakeUpGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("MakeUpGain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, src, i) =>
                {
                    IBMDSwitcherFairlightAudioDynamicsProcessor dynamics = GetDynamics(src);
                    tested = true;

                    var target = Randomiser.Range(0, 20);
                    srcState.Dynamics.MakeUpGain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { dynamics.SetMakeupGain(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestEqualizerEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("EqualizerEnabled");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, src, i) =>
                {
                    IBMDSwitcherFairlightAudioEqualizer eq = GetEqualizer(src);
                    tested = true;

                    srcState.Equalizer.Enabled = i % 2 == 1;
                    helper.SendAndWaitForChange(stateBefore, () => { eq.SetEnabled(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestEqualizerGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceSetCommand, FairlightMixerSourceGetCommand>("EqualizerGain");
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                EachRandomSource(helper, (stateBefore, srcState, src, i) =>
                {
                    IBMDSwitcherFairlightAudioEqualizer eq = GetEqualizer(src);
                    tested = true;

                    var target = Randomiser.Range(-20, 20);
                    srcState.Equalizer.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { eq.SetGain(target); });
                });
            });
            Assert.True(tested);
        }

    }
}