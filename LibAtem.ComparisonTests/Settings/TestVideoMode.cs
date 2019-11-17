using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DownstreamKey;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using LibAtem.DeviceProfile;
using LibAtem.State;
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

            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoMode v)
            {
                if (goodValue)
                {
                    state.Settings.VideoMode = v;

                    uint rate = (uint)Math.Round(v.GetRate());
                    if (rate > 30) rate /= 2;
                    if (rate == 24) rate = 25;

                    state.MixEffects.SelectMany(me => me.Keyers).ForEach(k => { k.DVE.Rate = rate; });
                    state.MixEffects.ForEach(k =>
                    {
                        k.Transition.Mix.Rate = rate;
                        k.Transition.Dip.Rate = rate;
                        k.Transition.Wipe.Rate = rate;
                        k.Transition.DVE.Rate = rate;
                        k.Transition.DVE.LogoRate = rate;
                    });
                    state.DownstreamKeyers.ForEach(k =>
                    {
                        k.Properties.Rate = rate;
                        k.State.RemainingFrames = rate;
                    });

                    switch (v)
                    {
                        case VideoMode.N525i5994NTSC:
                        case VideoMode.P625i50PAL:
                            state.MixEffects.SelectMany(me => me.Keyers).ForEach(k =>
                            {
                                k.Properties.MaskBottom = -3;
                                k.Properties.MaskTop = 3;
                                k.Properties.MaskLeft = -4;
                                k.Properties.MaskRight = 4;

                                k.DVE.BorderOuterWidth = 0.12;
                                k.DVE.BorderInnerWidth = 0.12;
                            });
                            state.DownstreamKeyers.ForEach(k =>
                            {
                                k.Properties.MaskBottom = -3;
                                k.Properties.MaskTop = 3;
                                k.Properties.MaskLeft = -4;
                                k.Properties.MaskRight = 4;
                            });
                            break;
                        default:
                            state.MixEffects.SelectMany(me => me.Keyers).ForEach(k =>
                            {
                                k.Properties.MaskBottom = -9;
                                k.Properties.MaskTop = 9;
                                k.Properties.MaskLeft = -16;
                                k.Properties.MaskRight = 16;

                                k.DVE.BorderOuterWidth = 0.5;
                                k.DVE.BorderInnerWidth = 0.5;
                            });
                            state.DownstreamKeyers.ForEach(k =>
                            {
                                k.Properties.MaskBottom = -9;
                                k.Properties.MaskTop = 9;
                                k.Properties.MaskLeft = -16;
                                k.Properties.MaskRight = 16;
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

                    for (var i = 0; i < _helper.LibState.DownstreamKeyers.Count; i++)
                        yield return new CommandQueueKey(new DownstreamKeyStateGetCommand() { Index = (DownstreamKeyId)i });
                    for (var i = 0; i < _helper.LibState.MixEffects.Count; i++)
                    {
                        var id = (MixEffectBlockId)i;
                        yield return new CommandQueueKey(new TransitionMixGetCommand() { Index = id });
                        yield return new CommandQueueKey(new TransitionDipGetCommand() { Index = id });
                        yield return new CommandQueueKey(new TransitionWipeGetCommand() { Index = id });

                        var me = _helper.LibState.MixEffects[i];
                        if (me != null && me.Transition.DVE != null)
                            yield return new CommandQueueKey(new TransitionDVEGetCommand() { Index = id });
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