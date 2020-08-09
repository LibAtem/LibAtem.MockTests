using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.Audio.Fairlight;
using LibAtem.Common;
using LibAtem.MockTests.SdkState;
using LibAtem.MockTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.Fairlight
{
    [Collection("ServerClientPool")]
    public class TestFairlightInputSourceEqualizerBand
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestFairlightInputSourceEqualizerBand(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private static IBMDSwitcherFairlightAudioEqualizer GetEqualizer(IBMDSwitcherFairlightAudioSource src)
        {
            var equalizer = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioEqualizer>(src.GetEffect);
            Assert.NotNull(equalizer);
            return equalizer;
        }

        private static List<Tuple<IBMDSwitcherFairlightAudioEqualizerBand, uint>> GetSampleOfBands(IBMDSwitcherFairlightAudioEqualizer eq)
        {
            var it = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioEqualizerBandIterator>(eq.CreateIterator);
            var allBands =
                AtemSDKConverter
                    .IterateList<IBMDSwitcherFairlightAudioEqualizerBand,
                        Tuple<IBMDSwitcherFairlightAudioEqualizerBand, uint>>(it.Next,
                        Tuple.Create);

            return Randomiser.SelectionOfGroup(allBands, 1).ToList();
        }

        private static void ForSampleOfBands(AtemMockServerWrapper helper, Action<AtemState, FairlightAudioState.EqualizerBandState, IBMDSwitcherFairlightAudioEqualizerBand, long, long, uint> func)
        {
            TestFairlightInputSource.EachRandomSource(helper, (stateBefore, srcState, inputId, src, i) =>
            {
                IBMDSwitcherFairlightAudioEqualizer equalizer = GetEqualizer(src);

                var bands = GetSampleOfBands(equalizer);
                Assert.NotEmpty(bands);

                foreach (Tuple<IBMDSwitcherFairlightAudioEqualizerBand, uint> bandT in bands)
                {
                    uint index = bandT.Item2;
                    IBMDSwitcherFairlightAudioEqualizerBand band = bandT.Item1;

                    func(stateBefore, srcState.Equalizer.Bands[(int) index], band, inputId, srcState.SourceId, index);
                }
            }, 1);
        }

        [Fact]
        public void TestEnabled()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceEqualizerBandSetCommand, FairlightMixerSourceEqualizerBandGetCommand>("BandEnabled");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                ForSampleOfBands(helper, (stateBefore, bandState, band, inputId, sourceId, index) =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        bandState.BandEnabled = !bandState.BandEnabled;
                        helper.SendAndWaitForChange(stateBefore,
                            () => { band.SetEnabled(bandState.BandEnabled ? 1 : 0); });
                    }
                });
            });
        }

        [Fact]
        public void TestSupportedShapes()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.FairlightMain, helper =>
            {
                var eqCommands = helper.Server.GetParsedDataDump()
                    .OfType<FairlightMixerSourceEqualizerBandGetCommand>().ToList();
                ForSampleOfBands(helper, (stateBefore, bandState, band, inputId, sourceId, index) =>
                {
                    var cmd = eqCommands.Single(c =>
                        c.Band == index && c.SourceId == sourceId && (long) c.Index == inputId);

                    for (int i = 0; i < 3; i++)
                    {
                        var target = Randomiser.EnumValue<FairlightEqualizerBandShape>();
                        bandState.SupportedShapes = cmd.SupportedShapes = target;
                        helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                    }
                });
            });
        }

        [Fact]
        public void TestShape()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceEqualizerBandSetCommand, FairlightMixerSourceEqualizerBandGetCommand>("Shape");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                ForSampleOfBands(helper, (stateBefore, bandState, band, inputId, sourceId, index) =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var target = Randomiser.EnumValue<FairlightEqualizerBandShape>();
                        bandState.Shape = target;
                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            band.SetShape(AtemEnumMaps.FairlightEqualizerBandShapeMap[target]);
                        });
                    }
                });
            });
        }

        [Fact]
        public void TestSupportedFrequencyRanges()
        {
            AtemMockServerWrapper.Each(_output, _pool, null, DeviceTestCases.FairlightMain, helper =>
            {
                var eqCommands = helper.Server.GetParsedDataDump()
                    .OfType<FairlightMixerSourceEqualizerBandGetCommand>().ToList();
                ForSampleOfBands(helper, (stateBefore, bandState, band, inputId, sourceId, index) =>
                {
                    var cmd = eqCommands.Single(c =>
                        c.Band == index && c.SourceId == sourceId && (long)c.Index == inputId);

                    for (int i = 0; i < 3; i++)
                    {
                        var target = Randomiser.EnumValue<FairlightEqualizerFrequencyRange>();
                        bandState.SupportedFrequencyRanges = cmd.SupportedFrequencyRanges = target;
                        helper.SendFromServerAndWaitForChange(stateBefore, cmd);
                    }
                });
            });
        }

        [Fact]
        public void TestFrequencyRange()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceEqualizerBandSetCommand, FairlightMixerSourceEqualizerBandGetCommand>("FrequencyRange");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                ForSampleOfBands(helper, (stateBefore, bandState, band, inputId, sourceId, index) =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var target = Randomiser.EnumValue<FairlightEqualizerFrequencyRange>();
                        bandState.FrequencyRange = target;
                        helper.SendAndWaitForChange(stateBefore, () =>
                        {
                            band.SetFrequencyRange(AtemEnumMaps.FairlightEqualizerFrequencyRangeMap[target]);
                        });
                    }
                });
            });
        }

        [Fact]
        public void TestFrequency()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceEqualizerBandSetCommand, FairlightMixerSourceEqualizerBandGetCommand>("Frequency");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                ForSampleOfBands(helper, (stateBefore, bandState, band, inputId, sourceId, index) =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        uint target = (uint)Randomiser.RangeInt(30, 21700);
                        bandState.Frequency = target;
                        helper.SendAndWaitForChange(stateBefore, () => { band.SetFrequency(target); });
                    }
                });
            });
        }

        [Fact]
        public void TestGain()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceEqualizerBandSetCommand, FairlightMixerSourceEqualizerBandGetCommand>("Gain");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                ForSampleOfBands(helper, (stateBefore, bandState, band, inputId, sourceId, index) =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        double target = Randomiser.Range(-20, 20);
                        bandState.Gain = target;
                        helper.SendAndWaitForChange(stateBefore, () => { band.SetGain(target); });
                    }
                });
            });
        }

        [Fact]
        public void TestQFactor()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<FairlightMixerSourceEqualizerBandSetCommand, FairlightMixerSourceEqualizerBandGetCommand>("QFactor");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                ForSampleOfBands(helper, (stateBefore, bandState, band, inputId, sourceId, index) =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        double target = Randomiser.Range(0.3, 10.3);
                        bandState.QFactor = target;
                        helper.SendAndWaitForChange(stateBefore, () => { band.SetQFactor(target); });
                    }
                });
            });
        }
        
        [Fact]
        public void TestReset()
        {
            var target = new FairlightMixerSourceEqualizerResetCommand
            { Mask = FairlightMixerSourceEqualizerResetCommand.MaskFlags.Band };
            var handler = CommandGenerator.MatchCommand(target);
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.FairlightMain, helper =>
            {
                ForSampleOfBands(helper, (stateBefore, bandState, band, inputId, sourceId, index) =>
                {
                    uint timeBefore = helper.Server.CurrentTime;

                    target.Index = (AudioSource)inputId;
                    target.SourceId = sourceId;
                    target.Band = index;

                    helper.SendAndWaitForChange(null, () => { band.Reset(); });

                    // It should have sent a response, but we dont expect any comparable data
                    Assert.NotEqual(timeBefore, helper.Server.CurrentTime);
                });
            });
        }
    }
}