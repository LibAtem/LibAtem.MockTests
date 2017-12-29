using System.Linq;
using AtemEmulator.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.DeviceProfile;
using LibAtem.Util;
using LibAtem.XmlState;
using Xunit;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestDVETransition : TestTransitionBase
    {
        public TestDVETransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        [Fact]
        public void TestDveRate()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionDVEParameters>(helper);
                Assert.NotNull(sdkProps);

                uint[] testValues = { 18, 28, 95 };

                ICommand Setter(uint v) => new TransitionDVESetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionDVESetCommand.MaskFlags.Rate,
                    Rate = v,
                };

                uint? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand { Index = MixEffectBlockId.One })?.Rate;

                ValueTypeComparer<uint>.Run(helper, Setter, sdkProps.GetRate, Getter, testValues);
            }
        }

        [Fact]
        public void TestDveLogoRate()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionDVEParameters>(helper);
                Assert.NotNull(sdkProps);

                uint[] testValues = { 18, 28, 95 };

                ICommand Setter(uint v) => new TransitionDVESetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionDVESetCommand.MaskFlags.LogoRate,
                    LogoRate = v,
                };

                uint? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand { Index = MixEffectBlockId.One })?.LogoRate;

                ValueTypeComparer<uint>.Run(helper, Setter, sdkProps.GetLogoRate, Getter, testValues);
            }
        }

        [Fact]
        public void TestDveReverse()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionDVEParameters>(helper);
                Assert.NotNull(sdkProps);

                bool[] testValues = { true, false };

                ICommand Setter(bool v) => new TransitionDVESetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionDVESetCommand.MaskFlags.Reverse,
                    Reverse = v
                };

                bool? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand { Index = MixEffectBlockId.One })?.Reverse;

                BoolValueComparer.Run(helper, Setter, sdkProps.GetReverse, Getter, testValues);
            }
        }

        [Fact]
        public void TestDveFlipFlop()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionDVEParameters>(helper);
                Assert.NotNull(sdkProps);

                bool[] testValues = { true, false };

                ICommand Setter(bool v) => new TransitionDVESetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionDVESetCommand.MaskFlags.FlipFlop,
                    FlipFlop = v
                };

                bool? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand { Index = MixEffectBlockId.One })?.FlipFlop;

                BoolValueComparer.Run(helper, Setter, sdkProps.GetFlipFlop, Getter, testValues);
            }
        }

        // TODO: GetStyle, DoesSupportStyle, GetNumSupportedStyles, GetSupportedStyle
        
        [Fact]
        public void TestDveInputFill()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionDVEParameters>(helper);
                Assert.NotNull(sdkProps);

                _BMDSwitcherInputAvailability availability = 0;
                sdkProps.GetFillInputAvailabilityMask(ref availability);
                Assert.Equal(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMixEffectBlock0, availability);

                long[] testValues = VideoSourceLists.All
                    .Where(s => s.IsAvailable(helper.Profile, InternalPortType.Mask))
                    .Where(s => s.GetAttribute<VideoSource, VideoSourceAvailabilityAttribute>().MeAvailability.HasFlag(MeAvailability.Me1))
                    .Select(s => (long) s).ToArray();
                long[] badValues = VideoSourceLists.All
                    .Where(s => s.IsAvailable(helper.Profile, InternalPortType.Mask))
                    .Where(s => !s.GetAttribute<VideoSource, VideoSourceAvailabilityAttribute>().MeAvailability.HasFlag(MeAvailability.Me1))
                    .Select(s => (long)s).ToArray();
                
                ICommand Setter(long v) => new TransitionDVESetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionDVESetCommand.MaskFlags.FillSource,
                    FillSource = (VideoSource)v,
                };

                long? Getter() => (long?)helper.FindWithMatching(new TransitionDVEGetCommand { Index = MixEffectBlockId.One })?.FillSource;

                ValueTypeComparer<long>.Run(helper, Setter, sdkProps.GetInputFill, Getter, testValues);
                ValueTypeComparer<long>.Fail(helper, Setter, sdkProps.GetInputFill, Getter, badValues);
            }
        }

        [Fact]
        public void TestDveInputKey()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionDVEParameters>(helper);
                Assert.NotNull(sdkProps);

                _BMDSwitcherInputAvailability availability = 0;
                sdkProps.GetFillInputAvailabilityMask(ref availability);
                Assert.Equal(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMixEffectBlock0, availability);

                long[] testValues = VideoSourceLists.All
                    .Where(s => s.IsAvailable(helper.Profile, InternalPortType.Mask))
                    .Where(s =>
                    {
                        var attr = s.GetAttribute<VideoSource, VideoSourceAvailabilityAttribute>();
                        return attr.MeAvailability.HasFlag(MeAvailability.Me1) && attr.SourceAvailability.HasFlag(SourceAvailability.KeySource);
                    })
                    .Select(s => (long)s).ToArray();
                long[] badValues = VideoSourceLists.All
                    .Where(s => s.IsAvailable(helper.Profile, InternalPortType.Mask))
                    .Where(s =>
                    {
                        var attr = s.GetAttribute<VideoSource, VideoSourceAvailabilityAttribute>();
                        return !attr.MeAvailability.HasFlag(MeAvailability.Me1) || !attr.SourceAvailability.HasFlag(SourceAvailability.KeySource);
                    })
                    .Select(s => (long)s).ToArray();

                ICommand Setter(long v) => new TransitionDVESetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionDVESetCommand.MaskFlags.KeySource,
                    KeySource = (VideoSource)v,
                };

                long? Getter() => (long?)helper.FindWithMatching(new TransitionDVEGetCommand { Index = MixEffectBlockId.One })?.KeySource;

                ValueTypeComparer<long>.Run(helper, Setter, sdkProps.GetInputCut, Getter, testValues);
                ValueTypeComparer<long>.Fail(helper, Setter, sdkProps.GetInputCut, Getter, badValues);
            }
        }

        [Fact]
        public void TestDveEnableKey()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionDVEParameters>(helper);
                Assert.NotNull(sdkProps);

                bool[] testValues = { true, false };

                ICommand Setter(bool v) => new TransitionDVESetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionDVESetCommand.MaskFlags.EnableKey,
                    EnableKey = v
                };

                bool? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand { Index = MixEffectBlockId.One })?.EnableKey;

                BoolValueComparer.Run(helper, Setter, sdkProps.GetEnableKey, Getter, testValues);
            }
        }

        [Fact]
        public void TestDvePreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionDVEParameters>(helper);
                Assert.NotNull(sdkProps);

                bool[] testValues = { true, false };

                ICommand Setter(bool v) => new TransitionDVESetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionDVESetCommand.MaskFlags.PreMultiplied,
                    PreMultiplied = v
                };

                bool? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand { Index = MixEffectBlockId.One })?.PreMultiplied;

                BoolValueComparer.Run(helper, Setter, sdkProps.GetPreMultiplied, Getter, testValues);
            }
        }

        [Fact]
        public void TestDveClip()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionDVEParameters>(helper);
                Assert.NotNull(sdkProps);
                
                double[] testValues = { 87.4, 14.7 };

                ICommand Setter(double v) => new TransitionDVESetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionDVESetCommand.MaskFlags.Clip,
                    Clip = v
                };

                double? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand { Index = MixEffectBlockId.One })?.Clip;

                DoubleValueComparer.Run(helper, Setter, sdkProps.GetClip, Getter, testValues, 100);
            }
        }

        [Fact]
        public void TestDveGain()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionDVEParameters>(helper);
                Assert.NotNull(sdkProps);

                double[] testValues = { 87.4, 14.7 };

                ICommand Setter(double v) => new TransitionDVESetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionDVESetCommand.MaskFlags.Gain,
                    Gain = v
                };

                double? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand { Index = MixEffectBlockId.One })?.Gain;

                DoubleValueComparer.Run(helper, Setter, sdkProps.GetGain, Getter, testValues, 100);
            }
        }

        [Fact]
        public void TestDveInvertKey()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionDVEParameters>(helper);
                Assert.NotNull(sdkProps);

                bool[] testValues = { true, false };

                ICommand Setter(bool v) => new TransitionDVESetCommand
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionDVESetCommand.MaskFlags.InvertKey,
                    InvertKey = v
                };

                bool? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand { Index = MixEffectBlockId.One })?.InvertKey;

                BoolValueComparer.Run(helper, Setter, sdkProps.GetInverse, Getter, testValues);
            }
        }
    }
}