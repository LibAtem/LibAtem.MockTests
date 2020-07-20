using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    [Collection("ServerClientPool")]
    public class TestWipeTransition : MixEffectsTestBase
    {
        public TestWipeTransition(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        [Fact]
        public void TestRate()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionWipeSetCommand, TransitionWipeGetCommand>("Rate");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionWipeParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Wipe);

                    uint target = Randomiser.RangeInt(250);
                    meBefore.Transition.Wipe.Rate = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetRate(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestPattern()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionWipeSetCommand, TransitionWipeGetCommand>("Pattern");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionWipeParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Wipe);

                    Pattern target = Randomiser.EnumValue<Pattern>();
                    _BMDSwitcherPatternStyle target2 = AtemEnumMaps.PatternMap[target];
                    meBefore.Transition.Wipe.Pattern = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetPattern(target2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderWidth()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionWipeSetCommand, TransitionWipeGetCommand>("BorderWidth");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionWipeParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Wipe);

                    double target = Randomiser.Range(0, 100);
                    meBefore.Transition.Wipe.BorderWidth = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderSize(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderInput()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionWipeSetCommand, TransitionWipeGetCommand>("BorderInput");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                var sampleSources = GetTransitionSourcesSelection(helper);
                EachMixEffect<IBMDSwitcherTransitionWipeParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Wipe);

                    VideoSource target = sampleSources[i];
                    meBefore.Transition.Wipe.BorderInput = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetInputBorder((long)target); });
                }, sampleSources.Length);
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestSymmetry()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionWipeSetCommand, TransitionWipeGetCommand>("Symmetry");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionWipeParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Wipe);

                    double target = Randomiser.Range(0, 100);
                    meBefore.Transition.Wipe.Symmetry = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetSymmetry(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderSoftness()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionWipeSetCommand, TransitionWipeGetCommand>("BorderSoftness");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionWipeParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Wipe);

                    double target = Randomiser.Range(0, 100);
                    meBefore.Transition.Wipe.BorderSoftness = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetSoftness(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestXPosition()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionWipeSetCommand, TransitionWipeGetCommand>("XPosition");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionWipeParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Wipe);

                    double target = Randomiser.Range(0, 1, 10000);
                    meBefore.Transition.Wipe.XPosition = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetHorizontalOffset(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestYPosition()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionWipeSetCommand, TransitionWipeGetCommand>("YPosition");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionWipeParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Wipe);

                    double target = Randomiser.Range(0, 1, 10000);
                    meBefore.Transition.Wipe.YPosition = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetVerticalOffset(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestReverseDirection()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionWipeSetCommand, TransitionWipeGetCommand>("ReverseDirection");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionWipeParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Wipe);

                    meBefore.Transition.Wipe.ReverseDirection = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetReverse(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestFlipFlop()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionWipeSetCommand, TransitionWipeGetCommand>("FlipFlop");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionWipeParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Wipe);

                    meBefore.Transition.Wipe.FlipFlop = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetFlipFlop(i % 2); });
                });
            });
            Assert.True(tested);
        }

    }

}