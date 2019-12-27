using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.SdkStateBuilder;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    [Collection("ServerClientPool")]
    public class TestPatternKeyer : MixEffectsTestBase
    {
        public TestPatternKeyer(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        [Fact]
        public void TestPattern()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyPatternSetCommand, MixEffectKeyPatternGetCommand>("Pattern");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyPatternParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Pattern);

                    var target = Randomiser.EnumValue<Pattern>();
                    var target2 = AtemEnumMaps.PatternMap[target];
                    keyerBefore.Pattern.Pattern = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetPattern(target2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestSize()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyPatternSetCommand, MixEffectKeyPatternGetCommand>("Size");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyPatternParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Pattern);

                    var target = Randomiser.Range(0, 100, 100);
                    keyerBefore.Pattern.Size = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetSize(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestSymmetry()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyPatternSetCommand, MixEffectKeyPatternGetCommand>("Symmetry");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyPatternParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Pattern);

                    var target = Randomiser.Range(0, 100, 100);
                    keyerBefore.Pattern.Symmetry = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetSymmetry(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestSoftness()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyPatternSetCommand, MixEffectKeyPatternGetCommand>("Softness");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyPatternParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Pattern);

                    var target = Randomiser.Range(0, 100, 100);
                    keyerBefore.Pattern.Softness = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetSoftness(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestXPosition()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyPatternSetCommand, MixEffectKeyPatternGetCommand>("XPosition");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyPatternParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Pattern);

                    var target = Randomiser.Range(0, 1, 10000);
                    keyerBefore.Pattern.XPosition = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetHorizontalOffset(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestYPosition()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyPatternSetCommand, MixEffectKeyPatternGetCommand>("YPosition");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyPatternParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Pattern);

                    var target = Randomiser.Range(0, 1, 10000);
                    keyerBefore.Pattern.YPosition = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetVerticalOffset(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestInverse()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyPatternSetCommand, MixEffectKeyPatternGetCommand>("Inverse");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyPatternParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.Pattern);

                    var target = Randomiser.Range(0, 1, 10000);
                    keyerBefore.Pattern.Inverse = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetInverse(i % 2); });
                });
            });
            Assert.True(tested);
        }

    }

}