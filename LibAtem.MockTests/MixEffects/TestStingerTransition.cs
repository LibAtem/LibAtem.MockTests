using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    [Collection("ServerClientPool")]
    public class TestStingerTransition : MixEffectsTestBase
    {
        public TestStingerTransition(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        [Fact]
        public void TestSource()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionStingerSetCommand, TransitionStingerGetCommand>("Source");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                int mpCount = helper.Helper.BuildLibState().MediaPlayers.Count;
                List<StingerSource> sources = Enum.GetValues(typeof(StingerSource)).OfType<StingerSource>().Where(s => ((int)s) < mpCount).ToList();
                if (sources.Count <= 1) return;

                EachMixEffect<IBMDSwitcherTransitionStingerParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Stinger);

                    StingerSource target = sources[i];
                    _BMDSwitcherStingerTransitionSource target2 = AtemEnumMaps.StingerSourceMap[target];
                    meBefore.Transition.Stinger.Source = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetSource(target2); });
                }, sources.Count);
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMixRate()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionStingerSetCommand, TransitionStingerGetCommand>("MixRate");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionStingerParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Stinger);

                    uint target = Randomiser.RangeInt(250);
                    meBefore.Transition.Stinger.MixRate = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetMixRate(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestPreMultipliedKey()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionStingerSetCommand, TransitionStingerGetCommand>("PreMultipliedKey");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionStingerParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Stinger);

                    meBefore.Transition.Stinger.PreMultipliedKey = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetPreMultiplied(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestClip()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionStingerSetCommand, TransitionStingerGetCommand>("Clip");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionStingerParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Stinger);

                    double target = Randomiser.Range(0, 100, 10);
                    meBefore.Transition.Stinger.Clip = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetClip(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestGain()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionStingerSetCommand, TransitionStingerGetCommand>("Gain");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionStingerParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Stinger);

                    double target = Randomiser.Range(0, 100, 10);
                    meBefore.Transition.Stinger.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetGain(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestInvert()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionStingerSetCommand, TransitionStingerGetCommand>("Invert");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionStingerParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Stinger);

                    meBefore.Transition.Stinger.Invert = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetInverse(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestPreroll()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionStingerSetCommand, TransitionStingerGetCommand>("Preroll");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionStingerParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Stinger);

                    uint target = Randomiser.RangeInt(250);
                    meBefore.Transition.Stinger.Preroll = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetPreroll(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestClipDuration()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionStingerSetCommand, TransitionStingerGetCommand>("ClipDuration");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionStingerParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Stinger);

                    uint target = Randomiser.RangeInt(250);
                    meBefore.Transition.Stinger.ClipDuration = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetClipDuration(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestTriggerPoint()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionStingerSetCommand, TransitionStingerGetCommand>("TriggerPoint");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionStingerParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Stinger);

                    uint target = Randomiser.RangeInt(250);
                    meBefore.Transition.Stinger.TriggerPoint = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetTriggerPoint(target); });
                });
            });
            Assert.True(tested);
        }


    }

}