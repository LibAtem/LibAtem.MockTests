using System;
using System.Collections.Generic;
using System.Linq;
using AtemEmulator.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.DeviceProfile;
using Xunit;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestStingerTransition : TestTransitionBase
    {
        private static readonly IReadOnlyDictionary<StingerSource, _BMDSwitcherStingerTransitionSource> SourceMap;

        static TestStingerTransition()
        {
            SourceMap = new Dictionary<StingerSource, _BMDSwitcherStingerTransitionSource>
            {
                {StingerSource.None, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceNone},
                {StingerSource.MediaPlayer1, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer1},
                {StingerSource.MediaPlayer2, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer2},
                {StingerSource.MediaPlayer3, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer3},
                {StingerSource.MediaPlayer4, _BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer4},
            };
        }

        public TestStingerTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        [Fact]
        public void EnsureSourceMapIsComplete()
        {
            EnumMap.EnsureIsComplete(SourceMap);
        }

        [Fact]
        public void TestStingerSource()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionStingerParameters>(helper);
                Assert.NotNull(sdkProps);

                StingerSource[] testValues = Enum.GetValues(typeof(StingerSource)).OfType<StingerSource>().Where(s => s.IsAvailable(helper.Profile)).ToArray();
                StingerSource[] badValues = Enum.GetValues(typeof(StingerSource)).OfType<StingerSource>().Where(s => !s.IsAvailable(helper.Profile)).ToArray();

                ICommand Setter(StingerSource v) => new TransitionStingerSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionStingerSetCommand.MaskFlags.Source,
                    Source = v,
                };

                StingerSource? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand { Index = MixEffectBlockId.One })?.Source;

                EnumValueComparer<StingerSource, _BMDSwitcherStingerTransitionSource>.Run(helper, SourceMap, Setter, sdkProps.GetSource, Getter, testValues);
                EnumValueComparer<StingerSource, _BMDSwitcherStingerTransitionSource>.Fail(helper, SourceMap, Setter, sdkProps.GetSource, Getter, badValues);
            }
        }

        [Fact]
        public void TestStingerPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionStingerParameters>(helper);
                Assert.NotNull(sdkProps);

                bool[] testValues = { true, false };

                ICommand Setter(bool v) => new TransitionStingerSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionStingerSetCommand.MaskFlags.PreMultipliedKey,
                    PreMultipliedKey = v
                };

                bool? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand { Index = MixEffectBlockId.One })?.PreMultipliedKey;

                BoolValueComparer.Run(helper, Setter, sdkProps.GetPreMultiplied, Getter, testValues);
            }
        }

        [Fact]
        public void TestStingerClip()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionStingerParameters>(helper);
                Assert.NotNull(sdkProps);

                double[] testValues = { 87.4, 14.7 };
                double[] badValues = { -1, 101 };

                ICommand Setter(double v) => new TransitionStingerSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionStingerSetCommand.MaskFlags.Clip,
                    Clip = v
                };

                double? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand { Index = MixEffectBlockId.One })?.Clip;

                DoubleValueComparer.Run(helper, Setter, sdkProps.GetClip, Getter, testValues, 100);
                DoubleValueComparer.Fail(helper, Setter, sdkProps.GetClip, Getter, badValues, 100);
            }
        }

        [Fact]
        public void TestStingerGain()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionStingerParameters>(helper);
                Assert.NotNull(sdkProps);

                double[] testValues = { 87.4, 14.7 };
                double[] badValues = { -1, 101 };

                ICommand Setter(double v) => new TransitionStingerSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionStingerSetCommand.MaskFlags.Gain,
                    Gain = v
                };

                double? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand { Index = MixEffectBlockId.One })?.Gain;

                DoubleValueComparer.Run(helper, Setter, sdkProps.GetGain, Getter, testValues, 100);
                DoubleValueComparer.Fail(helper, Setter, sdkProps.GetGain, Getter, badValues, 100);
            }
        }

        [Fact]
        public void TestStingerInverse()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionStingerParameters>(helper);
                Assert.NotNull(sdkProps);

                bool[] testValues = { true, false };

                ICommand Setter(bool v) => new TransitionStingerSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionStingerSetCommand.MaskFlags.Invert,
                    Invert = v
                };

                bool? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand { Index = MixEffectBlockId.One })?.Invert;

                BoolValueComparer.Run(helper, Setter, sdkProps.GetInverse, Getter, testValues);
            }
        }

        [Fact]
        public void TestStingerPreRoll()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionStingerParameters>(helper);
                Assert.NotNull(sdkProps);
        
                uint[] testValues = { 18, 28, 95 };
        
                ICommand Setter(uint v) => new TransitionStingerSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionStingerSetCommand.MaskFlags.Preroll,
                    Preroll = v,
                };
        
                uint? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand { Index = MixEffectBlockId.One })?.Preroll;

                ValueTypeComparer<uint>.Run(helper, Setter, sdkProps.GetPreroll, Getter, testValues);
            }
        }

        [Fact]
        public void TestStingerClipDuration()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionStingerParameters>(helper);
                Assert.NotNull(sdkProps);

                uint[] testValues = { 18, 28, 95 };

                ICommand Setter(uint v) => new TransitionStingerSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionStingerSetCommand.MaskFlags.ClipDuration,
                    ClipDuration = v,
                };

                uint? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand { Index = MixEffectBlockId.One })?.ClipDuration;

                ValueTypeComparer<uint>.Run(helper, Setter, sdkProps.GetClipDuration, Getter, testValues);
            }
        }

        [Fact]
        public void TestStingerTriggerPoint()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionStingerParameters>(helper);
                Assert.NotNull(sdkProps);

                uint[] testValues = { 18, 28, 95 };

                ICommand Setter(uint v) => new TransitionStingerSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionStingerSetCommand.MaskFlags.TriggerPoint,
                    TriggerPoint = v,
                };

                uint? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand { Index = MixEffectBlockId.One })?.TriggerPoint;

                ValueTypeComparer<uint>.Run(helper, Setter, sdkProps.GetTriggerPoint, Getter, testValues);
            }
        }

        [Fact]
        public void TestStingerMixRate()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionStingerParameters>(helper);
                Assert.NotNull(sdkProps);

                uint[] testValues = { 18, 28, 95 };

                ICommand Setter(uint v) => new TransitionStingerSetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionStingerSetCommand.MaskFlags.MixRate,
                    MixRate = v,
                };

                uint? Getter() => helper.FindWithMatching(new TransitionStingerGetCommand { Index = MixEffectBlockId.One })?.MixRate;

                ValueTypeComparer<uint>.Run(helper, Setter, sdkProps.GetMixRate, Getter, testValues);
            }
        }


    }
}
