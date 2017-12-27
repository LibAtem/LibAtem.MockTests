using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.Util;
using Xunit;
using LibAtem.DeviceProfile;

namespace AtemEmulator.ComparisonTests.Settings
{
    [Collection("Client")]
    public class TestVideoMode
    {
        // TODO IBMDSwitcher::DoesSupportVideoMode
        // TODO IBMDSwitcher::GetDownConvertedHDVideoMode
        // IBMDSwitcher::SetDownConvertedHDVideoMode
        // IBMDSwitcher::DoesSupportDownConvertedHDVideoMode
        // IBMDSwitcher::GetMultiViewVideoMode
        // IBMDSwitcher::SetMultiViewVideoMode
        // IBMDSwitcher::Get3GSDIOutputLevel
        // IBMDSwitcher::Set3GSDIOutputLevel
        // IBMDSwitcher::DoesSupportMultiViewVideoMode

        private static readonly IReadOnlyDictionary<VideoMode, _BMDSwitcherVideoMode> videoModes;
        private static readonly IReadOnlyDictionary<DownConvertMode, _BMDSwitcherDownConversionMethod> sdDownconvertModes;
//        private static readonly IReadOnlyList<VideoMode> unsupportedVideoModes;

        static TestVideoMode()
        {
            videoModes = new Dictionary<VideoMode, _BMDSwitcherVideoMode>
            {
                {VideoMode.N525i5994NTSC, _BMDSwitcherVideoMode.bmdSwitcherVideoMode525i5994NTSC},
                {VideoMode.P625i50PAL, _BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL},
                {VideoMode.N525i5994169, _BMDSwitcherVideoMode.bmdSwitcherVideoMode525i5994Anamorphic},
                {VideoMode.P625i50169, _BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50Anamorphic},
                {VideoMode.P720p50, _BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50},
                {VideoMode.N720p5994, _BMDSwitcherVideoMode.bmdSwitcherVideoMode720p5994},
                {VideoMode.P1080i50, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080i50},
                {VideoMode.N1080i5994, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080i5994},
                {VideoMode.N1080p2398, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p2398},
                {VideoMode.N1080p24, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p24},
                {VideoMode.P1080p25, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p25},
                {VideoMode.N1080p2997, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p2997},
                {VideoMode.P1080p50, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p50},
                {VideoMode.N1080p5994, _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p5994},
                {VideoMode.N4KHDp2398, _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp2398},
                {VideoMode.N4KHDp24, _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp24},
                {VideoMode.P4KHDp25, _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp25},
                {VideoMode.N4KHDp2997, _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp2997},
                {VideoMode.P4KHDp5000, _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp50},
                {VideoMode.N4KHDp5994, _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp5994},
            };
            
            sdDownconvertModes = new Dictionary<DownConvertMode, _BMDSwitcherDownConversionMethod>()
            {
                {DownConvertMode.CentreCut, _BMDSwitcherDownConversionMethod.bmdSwitcherDownConversionMethodCentreCut},
                {DownConvertMode.Letterbox, _BMDSwitcherDownConversionMethod.bmdSwitcherDownConversionMethodLetterbox},
                {DownConvertMode.Anamorphic, _BMDSwitcherDownConversionMethod.bmdSwitcherDownConversionMethodAnamorphic},
            };
        }

        private AtemClientWrapper _client;

        public TestVideoMode(AtemClientWrapper client)
        {
            _client = client;
        }

        #region VideoMode

        [Fact]
        public void EnsureAllLibAtemVideoModesAreMapped()
        {
            List<VideoMode> vals = Enum.GetValues(typeof(VideoMode)).OfType<VideoMode>().ToList();

            var missing = vals.Where(v => !videoModes.ContainsKey(v)).ToList();
            Assert.Equal(new List<VideoMode>(), missing);

            // Expect map and values to have the same number
            Assert.Equal(vals.Count, videoModes.Count);
            Assert.Equal(Enum.GetValues(typeof(_BMDSwitcherVideoMode)).Length, videoModes.Count);

            // Expect all the map values to be unique
            Assert.Equal(vals.Count, videoModes.Select(v => v.Value).Distinct().Count());
        }

        [Fact]
        public void TestSupportedVideoModesMatchesSdk()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                foreach(var vals in videoModes)
                {
                    helper.SdkSwitcher.DoesSupportVideoMode(vals.Value, out int supported);

                    bool libAtemEnabled = vals.Key.IsAvailable(helper.Profile);
                    Assert.Equal(libAtemEnabled, supported != 0);
                }
            }
         }

        [Fact]
        public void TestGetCurrentMode()
        {
            using (var conn = new AtemComparisonHelper(_client))
            {
                conn.SdkSwitcher.GetVideoMode(out _BMDSwitcherVideoMode sdkMode);

                var cmd = conn.FindWithMatching(new VideoModeGetCommand());
                VideoMode myMode = cmd.VideoMode;

                _BMDSwitcherVideoMode expectedSdkMode = videoModes[myMode];
                Assert.Equal(expectedSdkMode, sdkMode);
            }
        }
        
        [Fact]
        public void TestSetModeViaSDK()
        {
            using (var conn = new AtemComparisonHelper(_client))
            {
                foreach (KeyValuePair<VideoMode, _BMDSwitcherVideoMode> mode in videoModes.Shuffle())
                {
                    conn.ClearReceivedCommands();

                    conn.SdkSwitcher.DoesSupportVideoMode(mode.Value, out int supported);
                    if (supported == 0)
                        continue;

                    conn.SdkSwitcher.SetVideoMode(mode.Value);

                    conn.Sleep();

                    conn.SdkSwitcher.GetVideoMode(out _BMDSwitcherVideoMode sdkMode);
                    Assert.Equal(mode.Value, sdkMode);

                    var cmd = conn.GetSingleReceivedCommands<VideoModeGetCommand>();
                    Assert.Equal(mode.Key, cmd.VideoMode);
                }
            }
        }

        [Fact]
        public void TestSetModeViaLibAtem()
        {
            using (var conn = new AtemComparisonHelper(_client))
            {
                foreach (KeyValuePair<VideoMode, _BMDSwitcherVideoMode> mode in videoModes.Shuffle())
                {
                    conn.ClearReceivedCommands();

                    conn.SendCommand(new VideoModeSetCommand()
                    {
                        VideoMode = mode.Key,
                    });

                    conn.Sleep();

                    var cmds = conn.GetReceivedCommands<VideoModeGetCommand>();
                    if (!mode.Key.IsAvailable(conn.Profile))
                    {
                        Assert.Empty(cmds);
                        continue;
                    }

                    Assert.Single(cmds);
                    Assert.Equal(mode.Key, cmds[0].VideoMode);

                    conn.SdkSwitcher.GetVideoMode(out _BMDSwitcherVideoMode sdkMode);
                    Assert.Equal(mode.Value, sdkMode);
                }
            }
        }

        #endregion VideoMode

        #region DownConvertMode

        [Fact]
        public void EnsureAllLibAtemDownConvertModesAreMapped()
        {
            List<DownConvertMode> vals = Enum.GetValues(typeof(DownConvertMode)).OfType<DownConvertMode>().ToList();

            var missing = vals.Where(v => !sdDownconvertModes.ContainsKey(v)).ToList();
            Assert.Equal(new List<DownConvertMode>(), missing);

            // Expect map and values to have the same number
            Assert.Equal(vals.Count, sdDownconvertModes.Count);
            Assert.Equal(Enum.GetValues(typeof(_BMDSwitcherDownConversionMethod)).Length, sdDownconvertModes.Count);

            // Expect all the map values to be unique
            Assert.Equal(vals.Count, sdDownconvertModes.Select(v => v.Value).Distinct().Count());
        }

        // TODO - current device profile doesnt state sd output support
        // IBMDSwitcher::GetMethodForDownConvertedSD
        // IBMDSwitcher::SetMethodForDownConvertedSD

        //        [Fact]
        //        public void TestGetCurrentSDDownConvertMode()
        //        {
        //            // ensure current mode supports sd output
        //            _sdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode1080i50);
        //
        //            _BMDSwitcherDownConversionMethod sdkMode;
        //            _sdkSwitcher.GetMethodForDownConvertedSD(out sdkMode);
        //
        //            var cmds = GetReceivedCommands<DownConvertModeGetCommand>();
        //            Assert.Equal(1, cmds.Count);
        //
        //            _BMDSwitcherDownConversionMethod expectedSdkMode = sdDownconvertModes[cmds[0].DownConvertMode];
        //            Assert.Equal(expectedSdkMode, sdkMode);
        //        }

        #endregion DownConvertMode

    }
}