using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.MockTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    [Collection("ServerClientPool")]
    public class TestMixTransition : MixEffectsTestBase
    {
        public TestMixTransition(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        [Fact]
        public void TestRate()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionMixSetCommand, TransitionMixGetCommand>("Rate", true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionMixParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;
                    Assert.NotNull(meBefore.Transition.Mix);

                    uint target = Randomiser.RangeInt(250);
                    meBefore.Transition.Mix.Rate = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetRate(target); });
                });
            });
            Assert.True(tested);
        }
    }

}