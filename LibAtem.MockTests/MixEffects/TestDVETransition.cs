using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.DeviceProfile;
using LibAtem.MockTests.Util;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    [Collection("ServerClientPool")]
    public class TestDVETransition : MixEffectsTestBase
    {
        public TestDVETransition(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        [Fact]
        public void TestRate()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionDVESetCommand, TransitionDVEGetCommand>("Rate");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionDVEParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.DVE);

                    uint target = Randomiser.RangeInt(249) + 1;
                    meBefore.Transition.DVE.Rate = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetRate(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestLogoRate()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionDVESetCommand, TransitionDVEGetCommand>("LogoRate");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionDVEParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.DVE);

                    uint target = Randomiser.RangeInt(240) + 1;
                    meBefore.Transition.DVE.LogoRate = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetLogoRate(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestReverse()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionDVESetCommand, TransitionDVEGetCommand>("Reverse");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionDVEParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.DVE);

                    meBefore.Transition.DVE.Reverse = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetReverse(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestFlipFlop()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionDVESetCommand, TransitionDVEGetCommand>("FlipFlop");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionDVEParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.DVE);

                    meBefore.Transition.DVE.FlipFlop = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetFlipFlop(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestInvertKey()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionDVESetCommand, TransitionDVEGetCommand>("InvertKey");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionDVEParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.DVE);

                    meBefore.Transition.DVE.InvertKey = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetInverse(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestPreMultiplied()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionDVESetCommand, TransitionDVEGetCommand>("PreMultiplied");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionDVEParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.DVE);

                    meBefore.Transition.DVE.PreMultiplied = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetPreMultiplied(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestEnableKey()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionDVESetCommand, TransitionDVEGetCommand>("EnableKey");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionDVEParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.DVE);

                    meBefore.Transition.DVE.EnableKey = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetEnableKey(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestClip()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionDVESetCommand, TransitionDVEGetCommand>("Clip");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionDVEParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.DVE);

                    double target = Randomiser.Range(0, 100, 10);
                    meBefore.Transition.DVE.Clip = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetClip(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestGain()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionDVESetCommand, TransitionDVEGetCommand>("Gain");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionDVEParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.DVE);

                    double target = Randomiser.Range(0, 100, 10);
                    meBefore.Transition.DVE.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetGain(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestFillSource()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionDVESetCommand, TransitionDVEGetCommand>("FillSource");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                List<VideoSource> deviceSources = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                VideoSource[] validSources = deviceSources.Where(s =>
                    s.IsAvailable(helper.Helper.Profile, InternalPortType.Mask | InternalPortType.Auxiliary | InternalPortType.MEOutput) &&
                    s.IsAvailable(SourceAvailability.KeySource)).ToArray();
                var sampleSources = VideoSourceUtil.TakeSelection(validSources);

                EachMixEffect<IBMDSwitcherTransitionDVEParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.DVE);

                    // sdk.GetFillInputAvailabilityMask(out _BMDSwitcherInputAvailability mask);
                    // Assert.Equal((long)SourceAvailability., (long)mask);

                    VideoSource target = sampleSources[i];
                    meBefore.Transition.DVE.FillSource = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetInputFill((long)target); });
                }, sampleSources.Length);
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestCutSource()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionDVESetCommand, TransitionDVEGetCommand>("KeySource");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                List<VideoSource> deviceSources = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                VideoSource[] validSources = deviceSources.Where(s =>
                    s.IsAvailable(helper.Helper.Profile, InternalPortType.Mask | InternalPortType.Auxiliary | InternalPortType.MEOutput) &&
                    s.IsAvailable(SourceAvailability.KeySource)).ToArray();
                var sampleSources = VideoSourceUtil.TakeSelection(validSources);

                EachMixEffect<IBMDSwitcherTransitionDVEParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.DVE);

                    VideoSource target = sampleSources[i];
                    meBefore.Transition.DVE.KeySource = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetInputCut((long)target); });
                }, sampleSources.Length);
            });
            Assert.True(tested);
        }



    }

}