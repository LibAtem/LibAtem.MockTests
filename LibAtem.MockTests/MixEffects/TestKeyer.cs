using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    [Collection("ServerClientPool")]
    public class TestKeyer : MixEffectsTestBase
    {
        public TestKeyer(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        // TODO DoesSupportAdvancedChroma
        // TODO CanBeDVEKey
        // TODO GetTransitionSelectionMask

        [Fact]
        public void TestKeyType()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyTypeSetCommand, MixEffectKeyPropertiesGetCommand>("KeyType");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKey>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    MixEffectKeyType target = Randomiser.EnumValue<MixEffectKeyType>();
                    _BMDSwitcherKeyType target2 = AtemEnumMaps.MixEffectKeyTypeMap[target];
                    keyerBefore.Properties.KeyType = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetType(target2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestOnAir()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyOnAirSetCommand, MixEffectKeyOnAirGetCommand>("OnAir", true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKey>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    keyerBefore.OnAir = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetOnAir(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMaskEnabled()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyMaskSetCommand, MixEffectKeyPropertiesGetCommand>("MaskEnabled");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKey>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    keyerBefore.Properties.MaskEnabled = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetMasked(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMaskTop()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyMaskSetCommand, MixEffectKeyPropertiesGetCommand>("MaskTop");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKey>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(-9, 9, 1000);
                    keyerBefore.Properties.MaskTop = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetMaskTop(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMaskBottom()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyMaskSetCommand, MixEffectKeyPropertiesGetCommand>("MaskBottom");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKey>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(-9, 9, 1000);
                    keyerBefore.Properties.MaskBottom = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetMaskBottom(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMaskLeft()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyMaskSetCommand, MixEffectKeyPropertiesGetCommand>("MaskLeft");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKey>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(-16, 16, 1000);
                    keyerBefore.Properties.MaskLeft = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetMaskLeft(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMaskRight()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyMaskSetCommand, MixEffectKeyPropertiesGetCommand>("MaskRight");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKey>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(-16, 16, 1000);
                    keyerBefore.Properties.MaskRight = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetMaskRight(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestResetMask()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyMaskSetCommand, MixEffectKeyPropertiesGetCommand>(new[] { "MaskRight", "MaskLeft", "MaskTop", "MaskBottom" });
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                SelectionOfKeyers<IBMDSwitcherKey>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    tested = true;

                    keyerBefore.Properties.MaskRight = 16;
                    keyerBefore.Properties.MaskLeft = -16;
                    keyerBefore.Properties.MaskTop = 9;
                    keyerBefore.Properties.MaskBottom = -9;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.ResetMask(); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestFillSource()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyFillSourceSetCommand, MixEffectKeyPropertiesGetCommand>("FillSource", true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                List<VideoSource> deviceSources = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                VideoSource[] validSources = deviceSources.Where(s =>
                    s.IsAvailable(helper.Helper.Profile, InternalPortType.Mask | InternalPortType.Auxiliary | InternalPortType.MEOutput | InternalPortType.SuperSource) &&
                    s.IsAvailable(SourceAvailability.KeySource)).ToArray();
                var sampleSources = VideoSourceUtil.TakeSelection(validSources);

                SelectionOfKeyers<IBMDSwitcherKey>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    // TODO GetFillInputAvailabilityMask

                    VideoSource target = sampleSources[i];
                    keyerBefore.Properties.FillSource = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetInputFill((long)target); });
                }, sampleSources.Length);
            });
        }

        [Fact]
        public void TestCutSource()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<MixEffectKeyCutSourceSetCommand, MixEffectKeyPropertiesGetCommand>("CutSource", true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                List<VideoSource> deviceSources = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                VideoSource[] validSources = deviceSources.Where(s =>
                    s.IsAvailable(helper.Helper.Profile, InternalPortType.Mask | InternalPortType.Auxiliary | InternalPortType.MEOutput | InternalPortType.SuperSource) &&
                    s.IsAvailable(SourceAvailability.KeySource)).ToArray();
                var sampleSources = VideoSourceUtil.TakeSelection(validSources);

                SelectionOfKeyers<IBMDSwitcherKey>(helper, (stateBefore, keyerBefore, sdkKeyer, meId, keyId, i) =>
                {
                    // TODO GetCutInputAvailabilityMask

                    VideoSource target = sampleSources[i];
                    keyerBefore.Properties.CutSource = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdkKeyer.SetInputCut((long)target); });
                }, sampleSources.Length);
            });
        }

    }

}