using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.DeviceProfile;
using LibAtem.Commands.Settings;
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

        /*
        [Fact]
        public void TestDownConvertMode()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<DownConvertModeSetCommand, DownConvertModeGetCommand>("DownConvertMode", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DownConvertVideoMode, helper =>
            {
                AtemState stateBefore = helper.Helper.BuildLibState();
                IBMDSwitcher switcher = helper.SdkClient.SdkSwitcher;

                VideoModeInfo candidateMode = stateBefore.Info.SupportedVideoModes.FirstOrDefault(m => m.DownConvertModes.Any());
                Assert.NotNull(candidateMode);
                stateBefore.Settings.VideoMode = candidateMode.Mode;
                helper.SendFromServerAndWaitForChange(stateBefore, new VideoModeGetCommand { VideoMode = candidateMode.Mode });

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
            var handler = CommandGenerator.CreateAutoCommandHandler<DownConvertModeSetCommand, DownConvertModeGetCommand>("DownConvertMode", true);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.DownConvertVideoMode, helper =>
            {
                AtemState stateBefore = helper.Helper.BuildLibState();
                IBMDSwitcher switcher = helper.SdkClient.SdkSwitcher;

                List<VideoModeInfo> candidateModes = stateBefore.Info.SupportedVideoModes.Where(m => m.DownConvertModes.Any()).ToList();
                Assert.NotEmpty(candidateModes);

                foreach (VideoModeInfo mode in candidateModes)
                {
                    stateBefore.Settings.VideoMode = mode.Mode;
                    helper.SendFromServerAndWaitForChange(stateBefore, new VideoModeGetCommand {VideoMode = mode.Mode});

                    //
                    foreach (VideoMode dcMode in mode.DownConvertModes)
                    {
                        //stateBefore.Settings.DownConvertVideoMode = dcMode;
                    }
                }
            });
        }
        */
    }
}