using BMDSwitcherAPI;
using LibAtem.Commands.SuperSource;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.SuperSource
{
    [Collection("ServerClientPool")]
    public class TestSuperSourceBorder : SuperSourceTestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestSuperSourceBorder(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        [Fact]
        public void TestEnabled()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBorderSetCommand, SuperSourceBorderGetCommand>("Enabled");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBorder(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    ssrcBefore.Enabled = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderEnabled(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBevel()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBorderSetCommand, SuperSourceBorderGetCommand>("Bevel");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBorder(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    BorderBevel target = Randomiser.EnumValue<BorderBevel>();
                    _BMDSwitcherBorderBevelOption target2 = AtemEnumMaps.BorderBevelMap[target];
                    ssrcBefore.Bevel = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderBevel(target2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestOuterWidth()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBorderSetCommand, SuperSourceBorderGetCommand>("OuterWidth");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBorder(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(0, 16, 100);
                    ssrcBefore.OuterWidth = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderWidthOut(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestInnerWidth()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBorderSetCommand, SuperSourceBorderGetCommand>("InnerWidth");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBorder(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(0, 16, 100);
                    ssrcBefore.InnerWidth = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderWidthIn(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestOuterSoftness()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBorderSetCommand, SuperSourceBorderGetCommand>("OuterSoftness");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBorder(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    uint target = Randomiser.RangeInt(100);
                    ssrcBefore.OuterSoftness = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderSoftnessOut(target / 100.0); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestInnerSoftness()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBorderSetCommand, SuperSourceBorderGetCommand>("InnerSoftness");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBorder(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    uint target = Randomiser.RangeInt(100);
                    ssrcBefore.InnerSoftness = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderSoftnessIn(target / 100.0); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBevelSoftness()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBorderSetCommand, SuperSourceBorderGetCommand>("BevelSoftness");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBorder(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    uint target = Randomiser.RangeInt(100);
                    ssrcBefore.BevelSoftness = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderBevelSoftness(target / 100.0); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestBevelPosition()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBorderSetCommand, SuperSourceBorderGetCommand>("BevelPosition");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBorder(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    uint target = Randomiser.RangeInt(100);
                    ssrcBefore.BevelPosition = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderBevelPosition(target / 100.0); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestHue()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBorderSetCommand, SuperSourceBorderGetCommand>("Hue");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBorder(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(0, 359.9, 10);
                    ssrcBefore.Hue = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderHue(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestSaturation()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBorderSetCommand, SuperSourceBorderGetCommand>("Saturation");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBorder(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(0, 100, 10);
                    ssrcBefore.Saturation = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderSaturation(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestLuma()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBorderSetCommand, SuperSourceBorderGetCommand>("Saturation");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBorder(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(0, 100, 10);
                    ssrcBefore.Saturation = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderSaturation(target / 100); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestLightSourceDirection()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBorderSetCommand, SuperSourceBorderGetCommand>("LightSourceDirection");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBorder(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    double target = Randomiser.Range(0, 359.9, 10);
                    ssrcBefore.LightSourceDirection = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderLightSourceDirection(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestLightSourceAltitude()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBorderSetCommand, SuperSourceBorderGetCommand>("LightSourceAltitude");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBorder(helper, (stateBefore, ssrcBefore, sdk, ssrcId, i) =>
                {
                    tested = true;

                    uint target = Randomiser.RangeInt(100);
                    ssrcBefore.LightSourceAltitude = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetBorderLightSourceAltitude(target / 100.0); });
                });
            });
            Assert.True(tested);
        }

    }
}