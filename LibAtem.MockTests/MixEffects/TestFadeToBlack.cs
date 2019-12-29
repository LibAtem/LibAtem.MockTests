using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects;
using LibAtem.MockTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    [Collection("ServerClientPool")]
    public class TestFadeToBlack: MixEffectsTestBase
    {
        public TestFadeToBlack(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        [Fact]
        public void TestPerformFadeToBlack()
        {
            var expected = new FadeToBlackAutoCommand();
            bool tested = false;
            var handler = CommandGenerator.MatchCommand(expected);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherMixEffectBlock>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    expected.Index = meId;

                    helper.SendAndWaitForChange(stateBefore, () => { sdk.PerformFadeToBlack(); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestRate()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<FadeToBlackRateSetCommand, FadeToBlackPropertiesGetCommand>("Rate", true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherMixEffectBlock>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    uint target = Randomiser.RangeInt(250);
                    meBefore.FadeToBlack.Properties.Rate = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetFadeToBlackRate(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestFullyBlack()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<FadeToBlackCutCommand, FadeToBlackStateCommand>("IsFullyBlack", true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherMixEffectBlock>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    meBefore.FadeToBlack.Status.IsFullyBlack = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetFadeToBlackFullyBlack(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestFramesRemaining()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(Output, Pool, null, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherMixEffectBlock>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    uint target = Randomiser.RangeInt(250);
                    meBefore.FadeToBlack.Status.RemainingFrames = target;
                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new FadeToBlackStateCommand
                        {
                            Index = meId,
                            RemainingFrames = target,
                            InTransition = meBefore.FadeToBlack.Status.InTransition,
                            IsFullyBlack = meBefore.FadeToBlack.Status.IsFullyBlack
                        });
                    });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestInTransition()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(Output, Pool, null, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherMixEffectBlock>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    meBefore.FadeToBlack.Status.InTransition = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new FadeToBlackStateCommand
                        {
                            Index = meId,
                            RemainingFrames = meBefore.FadeToBlack.Status.RemainingFrames,
                            InTransition = i % 2 != 0,
                            IsFullyBlack = meBefore.FadeToBlack.Status.IsFullyBlack
                        });
                    });
                });
            });
            Assert.True(tested);
        }

        // TODO GetInFadeToBlack

    }
}