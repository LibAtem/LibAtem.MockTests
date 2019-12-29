using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{

    [Collection("ServerClientPool")]
    public class TestDipTransition : MixEffectsTestBase
    {
        public TestDipTransition(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        [Fact]
        public void TestRate()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionDipSetCommand, TransitionDipGetCommand>("Rate");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionDipParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Dip);

                    uint target = Randomiser.RangeInt(250);
                    meBefore.Transition.Dip.Rate = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetRate(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestInput()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionDipSetCommand, TransitionDipGetCommand>("Input");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                var sampleSources = GetTransitionSourcesSelection(helper);
                EachMixEffect<IBMDSwitcherTransitionDipParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Dip);

                    VideoSource target = sampleSources[i];
                    meBefore.Transition.Dip.Input = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetInputDip((long)target); });
                }, sampleSources.Length);
            });
            Assert.True(tested);
        }

    }

}