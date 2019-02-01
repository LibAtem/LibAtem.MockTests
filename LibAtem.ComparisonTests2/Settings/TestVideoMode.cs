using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DownstreamKey;
using LibAtem.Commands.MixEffects;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using LibAtem.DeviceProfile;
using LibAtem.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.Settings
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
                var failures = new List<VideoMode>();

                foreach(var vals in AtemEnumMaps.VideoModesMap)
                {
                    helper.SdkSwitcher.DoesSupportVideoMode(vals.Value, out int supported);

                    bool libAtemEnabled = vals.Key.IsAvailable(helper.Profile);
                    if (libAtemEnabled != (supported != 0))
                        failures.Add(vals.Key);
                }

                _output.WriteLine("Mismatch in videomode support for: " + string.Join(", ", failures));
                Assert.Empty(failures);
            }
        }

        private class VideoModeTestDefinition : TestDefinitionBase<VideoModeSetCommand, VideoMode>
        {
            public VideoModeTestDefinition(AtemComparisonHelper helper) : base(helper)
            {
            }

            public override void Prepare() => _helper.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode1080i50);


            public override string PropertyName => "VideoMode";

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, VideoMode v)
            {
                if (goodValue)
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
                    state.DownstreamKeyers.ForEach(k =>
                    {
                        k.Value.Rate = rate;
                        k.Value.RemainingFrames = rate;
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

                                k.Value.DVE.BorderOuterWidth = 0.12;
                                k.Value.DVE.BorderInnerWidth = 0.12;
                            });
                            state.DownstreamKeyers.ForEach(k =>
                            {
                                k.Value.MaskBottom = -3;
                                k.Value.MaskTop = 3;
                                k.Value.MaskLeft = -4;
                                k.Value.MaskRight = 4;
                            });
                            break;
                        default:
                            state.MixEffects.SelectMany(me => me.Value.Keyers).ForEach(k =>
                            {
                                k.Value.MaskBottom = -9;
                                k.Value.MaskTop = 9;
                                k.Value.MaskLeft = -16;
                                k.Value.MaskRight = 16;

                                k.Value.DVE.BorderOuterWidth = 0.5;
                                k.Value.DVE.BorderInnerWidth = 0.5;
                            });
                            state.DownstreamKeyers.ForEach(k =>
                            {
                                k.Value.MaskBottom = -9;
                                k.Value.MaskTop = 9;
                                k.Value.MaskLeft = -16;
                                k.Value.MaskRight = 16;
                            });
                            break;
                    }
                }
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, VideoMode v)
            {
                if (goodValue)
                {
                    yield return new CommandQueueKey(new VideoModeGetCommand());

                    foreach (var dsk in _helper.LibState.DownstreamKeyers)
                        yield return new CommandQueueKey(new DownstreamKeyStateGetCommand() { Index = dsk.Key});
                    foreach (var me in _helper.LibState.MixEffects)
                    {
                        yield return new CommandQueueKey(new TransitionMixGetCommand() { Index = me.Key });
                        yield return new CommandQueueKey(new TransitionDipGetCommand() { Index = me.Key });
                        yield return new CommandQueueKey(new TransitionWipeGetCommand() { Index = me.Key });

                        if (me.Value.Transition.DVE != null)
                            yield return new CommandQueueKey(new TransitionDVEGetCommand() { Index = me.Key });
                    }
                }
            }

            public override VideoMode[] GoodValues => AtemEnumMaps.VideoModesMap.Keys.Where(m => m.IsAvailable(_helper.Profile)).ToArray();
            public override VideoMode[] BadValues => Enum.GetValues(typeof(VideoMode)).OfType<VideoMode>().Except(GoodValues).ToArray();
        }
        [Fact]
        public void TestVideoModeProp()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            using (new IgnoreStateNodeEnabler("Inputs"))
            {
                new VideoModeTestDefinition(helper).Run();
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