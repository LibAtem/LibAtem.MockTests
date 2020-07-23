using BMDSwitcherAPI;
using LibAtem.Commands.SuperSource;
using LibAtem.Common;
using LibAtem.DeviceProfile;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.SuperSource
{
    [Collection("ServerClientPool")]
    public class TestSuperSourceProperties : SuperSourceTestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestSuperSourceProperties(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        [Fact]
        public void TestArtOption()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourcePropertiesSetV8Command, SuperSourcePropertiesGetV8Command>("ArtOption");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSource(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    SuperSourceArtOption target = Randomiser.EnumValue<SuperSourceArtOption>();
                    _BMDSwitcherSuperSourceArtOption target2 = AtemEnumMaps.SuperSourceArtOptionMap[target];
                    ssrcBefore.Properties.ArtOption = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetArtOption(target2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestArtPreMultiplied()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourcePropertiesSetV8Command, SuperSourcePropertiesGetV8Command>("ArtPreMultiplied");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSource(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    ssrcBefore.Properties.ArtPreMultiplied = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetPreMultiplied(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestArtClip()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourcePropertiesSetV8Command, SuperSourcePropertiesGetV8Command>("ArtClip");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSource(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    var target = Randomiser.Range(0, 100, 10);
                    ssrcBefore.Properties.ArtClip = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetClip(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestArtGain()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourcePropertiesSetV8Command, SuperSourcePropertiesGetV8Command>("ArtGain");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSource(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    var target = Randomiser.Range(0, 100, 10);
                    ssrcBefore.Properties.ArtGain = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetGain(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestArtInvertKey()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourcePropertiesSetV8Command, SuperSourcePropertiesGetV8Command>("ArtInvertKey");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSource(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    ssrcBefore.Properties.ArtInvertKey = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetInverse(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestSupportsBorder()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSource(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    sdk.SupportsBorder(out int supports);
                    Assert.Equal(1, supports);
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestArtFillInput()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourcePropertiesSetV8Command, SuperSourcePropertiesGetV8Command>("ArtFillSource");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                VideoSource[] validSources = helper.Helper.BuildLibState().Settings.Inputs.Where(
                    i => i.Value.Properties.SourceAvailability.HasFlag(SourceAvailability.SuperSourceArt)
                ).Select(i => i.Key).ToArray();
                var sampleSources = VideoSourceUtil.TakeSelection(validSources);

                EachSuperSource(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    // TODO GetFillInputAvailabilityMask

                    VideoSource target = sampleSources[i];
                    ssrcBefore.Properties.ArtFillSource = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetInputFill((long)target); });
                }, sampleSources.Length);
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestArtKeyInput()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourcePropertiesSetV8Command, SuperSourcePropertiesGetV8Command>("ArtCutSource");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                VideoSource[] validSources = helper.Helper.BuildLibState().Settings.Inputs.Where(
                    i => i.Value.Properties.SourceAvailability.HasFlag(SourceAvailability.SuperSourceArt)
                ).Select(i => i.Key).ToArray();
                var sampleSources = VideoSourceUtil.TakeSelection(validSources);

                EachSuperSource(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    // TODO GetCutInputAvailabilityMask

                    VideoSource target = sampleSources[i];
                    ssrcBefore.Properties.ArtCutSource = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetInputCut((long)target); });
                }, sampleSources.Length);
            });
            Assert.True(tested);
        }

    }
}