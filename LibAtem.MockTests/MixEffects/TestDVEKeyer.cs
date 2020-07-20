using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    [Collection("ServerClientPool")]
    public class TestDVEKeyer : MixEffectsTestBase
    {
        public TestDVEKeyer(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        [Fact]
        public void TestBorderShadowEnabled()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("BorderShadowEnabled");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    keyerBefore.DVE.BorderShadowEnabled = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetShadow(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestLightSourceAltitude()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("LightSourceAltitude");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    uint target = 10 + Randomiser.RangeInt(90);
                    keyerBefore.DVE.LightSourceAltitude = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetLightSourceAltitude(target / 100.0); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestLightSourceDirection()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("LightSourceDirection");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    var target = Randomiser.Range(0, 359.9, 10);
                    keyerBefore.DVE.LightSourceDirection = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetLightSourceDirection(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderEnabled()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("BorderEnabled");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    keyerBefore.DVE.BorderEnabled = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBorderEnabled(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderBevel()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("BorderBevel");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    BorderBevel target = Randomiser.EnumValue<BorderBevel>();
                    _BMDSwitcherBorderBevelOption target2 = AtemEnumMaps.BorderBevelMap[target];
                    keyerBefore.DVE.BorderBevel = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBorderBevel(target2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderInnerWidth()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("BorderInnerWidth");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    double target = Randomiser.Range(0, 16, 100);
                    keyerBefore.DVE.BorderInnerWidth = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBorderWidthIn(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderOuterWidth()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("BorderOuterWidth");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    double target = Randomiser.Range(0, 16, 100);
                    keyerBefore.DVE.BorderOuterWidth = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBorderWidthOut(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderInnerSoftness()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("BorderInnerSoftness");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    uint target = Randomiser.RangeInt(100);
                    keyerBefore.DVE.BorderInnerSoftness = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBorderSoftnessIn(target / 100.0); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderOuterSoftness()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("BorderOuterSoftness");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    uint target = Randomiser.RangeInt(100);
                    keyerBefore.DVE.BorderOuterSoftness = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBorderSoftnessOut(target / 100.0); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderBevelSoftness()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("BorderBevelSoftness");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    uint target = Randomiser.RangeInt(100);
                    keyerBefore.DVE.BorderBevelSoftness = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBorderBevelSoftness(target / 100.0); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderBevelPosition()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("BorderBevelPosition");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    uint target = Randomiser.RangeInt(100);
                    keyerBefore.DVE.BorderBevelPosition = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBorderBevelPosition(target / 100.0); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderOpacity()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("BorderOpacity");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    uint target = Randomiser.RangeInt(100);
                    keyerBefore.DVE.BorderOpacity = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBorderOpacity(target / 100.0); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderHue()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("BorderHue");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    double target = Randomiser.Range(0, 359.9, 10);
                    keyerBefore.DVE.BorderHue = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBorderHue(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderSaturation()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("BorderSaturation");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    double target = Randomiser.Range(0, 100, 10);
                    keyerBefore.DVE.BorderSaturation = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBorderSaturation(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBorderLuma()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("BorderLuma");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    double target = Randomiser.Range(0, 100, 10);
                    keyerBefore.DVE.BorderLuma = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetBorderLuma(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMaskEnabled()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("MaskEnabled");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    keyerBefore.DVE.MaskEnabled = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetMasked(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMaskTop()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("MaskTop");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    double target = Randomiser.Range(0, 38, 1000);
                    keyerBefore.DVE.MaskTop = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetMaskTop(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMaskBottom()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("MaskBottom");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    double target = Randomiser.Range(0, 38, 1000);
                    keyerBefore.DVE.MaskBottom = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetMaskBottom(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMaskLeft()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("MaskLeft");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    double target = Randomiser.Range(0, 52, 1000);
                    keyerBefore.DVE.MaskLeft = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetMaskLeft(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMaskRight()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>("MaskRight");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    double target = Randomiser.Range(0, 52, 1000);
                    keyerBefore.DVE.MaskRight = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetMaskRight(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMaskReset()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyDVESetCommand, MixEffectKeyDVEGetCommand>(new[] { "MaskRight", "MaskLeft", "MaskTop", "MaskBottom" });
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKeyDVEParameters>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;
                    Assert.NotNull(keyerBefore.DVE);

                    keyerBefore.DVE.MaskRight = keyerBefore.DVE.MaskLeft = keyerBefore.DVE.MaskTop = keyerBefore.DVE.MaskBottom = 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.ResetMask(); });
                });
            });
            Assert.True(tested);
        }

    }

}