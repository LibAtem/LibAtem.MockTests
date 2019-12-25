using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.MockTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    [Collection("ServerClientPool")]
    public class TestChromaKeyer : MixEffectsTestBase
    {
        public TestChromaKeyer(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        [Fact]
        public void TestHue()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyChromaSetCommand, MixEffectKeyChromaGetCommand>("Hue");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.ChromaKeyer, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyChromaParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Chroma);

                    var target = Randomiser.Range(0, 359.9, 10);
                    keyerBefore.Chroma.Hue = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetHue(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestGain()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyChromaSetCommand, MixEffectKeyChromaGetCommand>("Gain");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.ChromaKeyer, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyChromaParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Chroma);

                    var target = Randomiser.Range(0, 100, 10);
                    keyerBefore.Chroma.Gain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetGain(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestYSuppress()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyChromaSetCommand, MixEffectKeyChromaGetCommand>("YSuppress");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.ChromaKeyer, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyChromaParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Chroma);

                    var target = Randomiser.Range(0, 100, 10);
                    keyerBefore.Chroma.YSuppress = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetYSuppress(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestLift()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyChromaSetCommand, MixEffectKeyChromaGetCommand>("Lift");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.ChromaKeyer, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyChromaParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Chroma);

                    var target = Randomiser.Range(0, 100, 10);
                    keyerBefore.Chroma.Lift = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetLift(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestNarrow()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyChromaSetCommand, MixEffectKeyChromaGetCommand>("Narrow");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.ChromaKeyer, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyChromaParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Chroma);

                    keyerBefore.Chroma.Narrow = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetNarrow(i % 2); });
                });
            });
            Assert.True(tested);
        }
    }
}