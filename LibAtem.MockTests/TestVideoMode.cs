using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.DeviceProfile;
using LibAtem.Commands.Settings;
using LibAtem.Commands.Settings.Multiview;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestVideoMode
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestVideoMode(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        [Fact]
        public void TestSetAutoVideoMode()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<AutoVideoModeCommand, AutoVideoModeCommand>("Enabled", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.AutoVideoMode, helper =>
            {
                IBMDSwitcher switcher = helper.SdkClient.SdkSwitcher;

                switcher.DoesSupportAutoVideoMode(out int supported);
                Assert.Equal(1, supported);
                tested = true;

                for (int i = 0; i < 5; i++)
                {
                    AtemState stateBefore = helper.Helper.BuildLibState();
                    stateBefore.Settings.AutoVideoMode = !stateBefore.Settings.AutoVideoMode;

                    helper.SendAndWaitForChange(stateBefore,
                        () => { switcher.SetAutoVideoMode(stateBefore.Settings.AutoVideoMode ? 1 : 0); });
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestSetVideoMode()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<VideoModeSetCommand, VideoModeGetCommand>("VideoMode", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                AtemState stateBefore = helper.Helper.BuildLibState();
                IBMDSwitcher switcher = helper.SdkClient.SdkSwitcher;

                List<VideoMode> possibleModes = Enum.GetValues(typeof(VideoMode)).OfType<VideoMode>().Where(v =>
                {
                    switcher.DoesSupportVideoMode(AtemEnumMaps.VideoModesMap[v], out int supported);
                    return supported != 0;
                }).ToList();

                foreach(VideoMode videoMode in Randomiser.SelectionOfGroup(possibleModes, 5))
                {
                    stateBefore.Settings.VideoMode = videoMode;
                    helper.SendAndWaitForChange(stateBefore,
                        () => { switcher.SetVideoMode(AtemEnumMaps.VideoModesMap[videoMode]); });
                }
            });
        }

        [Fact]
        public void TestDownConvertSDMode()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownConvertModeSetCommand, DownConvertModeGetCommand>("DownConvertMode", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DownConvertSDMode, helper =>
            {
                AtemState stateBefore = helper.Helper.BuildLibState();
                IBMDSwitcher switcher = helper.SdkClient.SdkSwitcher;
                
                for (int i = 0; i < 5; i++)
                {
                    DownConvertMode val = Randomiser.EnumValue<DownConvertMode>();
                    stateBefore.Settings.DownConvertMode = val;

                    helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            switcher.SetMethodForDownConvertedSD(AtemEnumMaps.SDDownconvertModesMap[val]);
                        });
                }
            });
        }

        [Fact]
        public void TestDownConvertVideoMode()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<DownConvertModeSetCommand, DownConvertModeGetCommand>(new[] { "DownConvertMode", "CoreVideoMode" }, true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DownConvertHDMode, helper =>
            {
                AtemState stateBefore = helper.Helper.BuildLibState();
                IBMDSwitcher switcher = helper.SdkClient.SdkSwitcher;

                var possibleModes = Randomiser.SelectionOfGroup(stateBefore.Info.SupportedVideoModes
                    .Where(m => m.DownConvertModes.Length > 1).ToList());

                foreach (VideoModeInfo mode in possibleModes)
                {
                    tested = true;
                    VideoMode dcMode = mode.DownConvertModes[(int)Randomiser.RangeInt((uint)mode.DownConvertModes.Length)];
                    
                    stateBefore.Settings.DownConvertVideoModes[mode.Mode] = dcMode;

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        switcher.SetDownConvertedHDVideoMode(AtemEnumMaps.VideoModesMap[mode.Mode],
                            AtemEnumMaps.VideoModesMap[dcMode]);
                    });
                }
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestMultiviewVideoMode()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<MultiviewVideoModeSetCommand, MultiviewVideoModeGetCommand>(new []{ "MultiviewMode", "CoreVideoMode"}, true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DownConvertHDMode, helper =>
            {
                AtemState stateBefore = helper.Helper.BuildLibState();
                IBMDSwitcher switcher = helper.SdkClient.SdkSwitcher;

                var possibleModes = Randomiser.SelectionOfGroup(stateBefore.Info.SupportedVideoModes
                    .Where(m => m.MultiviewModes.Length > 1).ToList());

                foreach (VideoModeInfo mode in possibleModes)
                {
                    tested = true;
                    VideoMode dcMode = mode.MultiviewModes[(int)Randomiser.RangeInt((uint)mode.MultiviewModes.Length)];

                    stateBefore.Settings.MultiviewVideoModes[mode.Mode] = dcMode;

                    helper.SendAndWaitForChange(stateBefore, () =>
                    {
                        switcher.SetMultiViewVideoMode(AtemEnumMaps.VideoModesMap[mode.Mode],
                            AtemEnumMaps.VideoModesMap[dcMode]);
                    });
                }
            });
            Assert.True(tested);
        }
    }
}