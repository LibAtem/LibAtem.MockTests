using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.MockTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    [Collection("ServerClientPool")]
    public class TestLumaKeyer : MixEffectsTestBase
    {
        public TestLumaKeyer(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        [Fact]
        public void TestPreMultiplied()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyLumaSetCommand, MixEffectKeyLumaGetCommand>("PreMultiplied");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyLumaParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Luma);

                    keyerBefore.Luma.PreMultiplied = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetPreMultiplied(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestClip()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyLumaSetCommand, MixEffectKeyLumaGetCommand>("Clip");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyLumaParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Luma);

                    var target = Randomiser.Range(0, 100, 10);
                    keyerBefore.Luma.Clip = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetClip(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestGain()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyLumaSetCommand, MixEffectKeyLumaGetCommand>("Gain");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyLumaParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Luma);

                    var target = Randomiser.Range(0, 100, 10);
                    keyerBefore.Luma.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetGain(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestInvert()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyLumaSetCommand, MixEffectKeyLumaGetCommand>("Invert");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyLumaParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Luma);

                    keyerBefore.Luma.Invert = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetInverse(i % 2); });
                });
            });
            Assert.True(tested);
        }
    }

}