using LibAtem.Commands.SuperSource;
using LibAtem.Common;
using LibAtem.DeviceProfile;
using LibAtem.MockTests.Util;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.SuperSource
{
    [Collection("ServerClientPool")]
    public class TestSuperSourceBox : SuperSourceTestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestSuperSourceBox(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        [Fact]
        public void TestEnabled()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBoxSetV8Command, SuperSourceBoxGetV8Command>("Enabled");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBox(helper, (stateBefore, boxBefore, sdk, ssrcId, boxId, i) =>
                {
                    tested = true;

                    boxBefore.Enabled = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetEnabled(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestInputSource()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBoxSetV8Command, SuperSourceBoxGetV8Command>("Source");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                VideoSource[] validSources = helper.Helper.BuildLibState().Settings.Inputs.Where(
                    i => i.Value.Properties.SourceAvailability.HasFlag(SourceAvailability.SuperSourceBox)
                ).Select(i => i.Key).ToArray();
                var sampleSources = VideoSourceUtil.TakeSelection(validSources);

                EachSuperSourceBox(helper, (stateBefore, boxBefore, sdk, ssrcId, boxId, i) =>
                {
                    tested = true;

                    // TODO GetInputAvailabilityMask

                    VideoSource target = sampleSources[i];
                    boxBefore.Source = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetInputSource((long)target); });
                }, sampleSources.Length);
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestPositionX()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBoxSetV8Command, SuperSourceBoxGetV8Command>("PositionX");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBox(helper, (stateBefore, boxBefore, sdk, ssrcId, boxId, i) =>
                {
                    tested = true;

                    var target = Randomiser.Range(-48, 48, 100);
                    boxBefore.PositionX = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetPositionX(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestPositionY()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBoxSetV8Command, SuperSourceBoxGetV8Command>("PositionY");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBox(helper, (stateBefore, boxBefore, sdk, ssrcId, boxId, i) =>
                {
                    tested = true;

                    var target = Randomiser.Range(-34, 34, 100);
                    boxBefore.PositionY = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetPositionY(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestSize()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBoxSetV8Command, SuperSourceBoxGetV8Command>("Size");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBox(helper, (stateBefore, boxBefore, sdk, ssrcId, boxId, i) =>
                {
                    tested = true;

                    var target = Randomiser.Range(0.07, 1, 1000);
                    boxBefore.Size = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetSize(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestCropped()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBoxSetV8Command, SuperSourceBoxGetV8Command>("Cropped");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBox(helper, (stateBefore, boxBefore, sdk, ssrcId, boxId, i) =>
                {
                    tested = true;

                    boxBefore.Cropped = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetCropped(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestCropTop()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBoxSetV8Command, SuperSourceBoxGetV8Command>("CropTop");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBox(helper, (stateBefore, boxBefore, sdk, ssrcId, boxId, i) =>
                {
                    tested = true;

                    var target = Randomiser.Range(0, 18, 1000);
                    boxBefore.CropTop = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetCropTop(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestCropBottom()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBoxSetV8Command, SuperSourceBoxGetV8Command>("CropBottom");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBox(helper, (stateBefore, boxBefore, sdk, ssrcId, boxId, i) =>
                {
                    tested = true;

                    var target = Randomiser.Range(0, 18, 1000);
                    boxBefore.CropBottom = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetCropBottom(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestCropLeft()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBoxSetV8Command, SuperSourceBoxGetV8Command>("CropLeft");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBox(helper, (stateBefore, boxBefore, sdk, ssrcId, boxId, i) =>
                {
                    tested = true;

                    var target = Randomiser.Range(0, 32, 1000);
                    boxBefore.CropLeft = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetCropLeft(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestCropRight()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBoxSetV8Command, SuperSourceBoxGetV8Command>("CropRight");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBox(helper, (stateBefore, boxBefore, sdk, ssrcId, boxId, i) =>
                {
                    tested = true;

                    var target = Randomiser.Range(0, 32, 1000);
                    boxBefore.CropRight = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetCropRight(target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestCropReset()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<SuperSourceBoxSetV8Command, SuperSourceBoxGetV8Command>(new[] { "CropLeft", "CropRight", "CropTop", "CropBottom" });
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.SuperSource, helper =>
            {
                EachSuperSourceBox(helper, (stateBefore, boxBefore, sdk, ssrcId, boxId, i) =>
                {
                    tested = true;

                    boxBefore.CropBottom = boxBefore.CropTop = boxBefore.CropRight = boxBefore.CropLeft = 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.ResetCrop(); });
                });
            });
            Assert.True(tested);
        }

    }
}