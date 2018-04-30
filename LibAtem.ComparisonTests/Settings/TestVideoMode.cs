using System;
using System.Linq;
using LibAtem.Commands;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using LibAtem.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.Settings
{
    [Collection("Client")]
    public class TestVideoMode
    {
        // TODO IBMDSwitcher::GetDownConvertedHDVideoMode
        // IBMDSwitcher::SetDownConvertedHDVideoMode
        // IBMDSwitcher::DoesSupportDownConvertedHDVideoMode
        // IBMDSwitcher::GetMultiViewVideoMode
        // IBMDSwitcher::Get3GSDIOutputLevel
        // IBMDSwitcher::DoesSupportMultiViewVideoMode

        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestVideoMode(ITestOutputHelper output, AtemClientWrapper client)
        {
            _output = output;
            _client = client;
        }

        #region VideoMode
        
        [Fact]
        public void TestSupportedVideoModesMatchesSdk()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach(var vals in AtemEnumMaps.VideoModesMap)
                {
                    helper.SdkSwitcher.DoesSupportVideoMode(vals.Value, out int supported);

                    bool libAtemEnabled = vals.Key.IsAvailable(helper.Profile);
                    Assert.Equal(libAtemEnabled, supported != 0);
                }
            }
        }
        
        [Fact]
        public void TestVideoModeProp()
        {
            using (var conn = new AtemComparisonHelper(_client, _output))
            {
                ICommand Setter(VideoMode v) => new VideoModeSetCommand { VideoMode = v };

                void UpdateExpectedState(ComparisonState state, VideoMode v)
                {
                    state.Settings.VideoMode = v;
                    
                    uint rate = (uint)Math.Round(v.GetRate());
                    if (rate > 30) rate /= 2;
                    if (rate == 24) rate = 25;

                    state.MixEffects.SelectMany(me => me.Value.Keyers).ForEach(k => { k.Value.Fly.Rate = rate; });
                    state.MixEffects.ForEach(k =>
                    {
                        k.Value.Transition.Mix.Rate = rate;
                        k.Value.Transition.Dip.Rate = rate;
                        k.Value.Transition.Wipe.Rate = rate;
                        k.Value.Transition.DVE.Rate = rate;
                        k.Value.Transition.DVE.LogoRate = rate;
                    });

                    switch (v)
                    {
                        case VideoMode.N525i5994NTSC:
                        case VideoMode.P625i50PAL:
                            state.MixEffects.SelectMany(me => me.Value.Keyers).ForEach(k =>
                            {
                                k.Value.MaskBottom = -3;
                                k.Value.MaskTop = 3;
                                k.Value.MaskLeft = -4;
                                k.Value.MaskRight = 4;

                                k.Value.DVE.OuterWidth = 0.12;
                                k.Value.DVE.InnerWidth = 0.12;
                            });
                            break;
                        default:
                            state.MixEffects.SelectMany(me => me.Value.Keyers).ForEach(k =>
                            {
                                k.Value.MaskBottom = -9;
                                k.Value.MaskTop = 9;
                                k.Value.MaskLeft = -16;
                                k.Value.MaskRight = 16;

                                k.Value.DVE.OuterWidth = 0.5;
                                k.Value.DVE.InnerWidth = 0.5;
                            });
                            break;
                    }
                }

                VideoMode[] newVals = AtemEnumMaps.VideoModesMap.Keys.Where(m => m.IsAvailable(conn.Profile)).ToArray();
                VideoMode[] badVals = AtemEnumMaps.VideoModesMap.Keys.Where(m => !newVals.Contains(m)).ToArray();

                ValueTypeComparer<VideoMode>.Run(conn, Setter, UpdateExpectedState, newVals);
                ValueTypeComparer<VideoMode>.Fail(conn, Setter, badVals);
            }
        }

        #endregion VideoMode

        #region DownConvertMode
        
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