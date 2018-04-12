using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestDVETransition : ComparisonTestBase
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
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    uint[] testValues = { 1, 18, 28, 95, 234, 244, 250 };
                    uint[] badValues = { 251, 255, 0 };

                    ICommand Setter(uint v) => new TransitionDVESetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionDVESetCommand.MaskFlags.Rate,
                        Rate = v,
                    };

                    uint? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand {Index = me.Item1})?.Rate;

                    ValueTypeComparer<uint>.Run(helper, Setter, me.Item2.GetRate, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, me.Item2.GetRate, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDveLogoRate()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    uint[] testValues = {1, 18, 28, 95, 234, 244, 250};
                    uint[] badValues = {251, 255, 0};

                    ICommand Setter(uint v) => new TransitionDVESetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionDVESetCommand.MaskFlags.LogoRate,
                        LogoRate = v,
                    };

                    uint? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand {Index = me.Item1})?.LogoRate;

                    ValueTypeComparer<uint>.Run(helper, Setter, me.Item2.GetLogoRate, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, me.Item2.GetLogoRate, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDveReverse()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new TransitionDVESetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionDVESetCommand.MaskFlags.Reverse,
                        Reverse = v
                    };

                    bool? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand {Index = me.Item1})?.Reverse;

                    BoolValueComparer.Run(helper, Setter, me.Item2.GetReverse, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestDveFlipFlop()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new TransitionDVESetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionDVESetCommand.MaskFlags.FlipFlop,
                        FlipFlop = v
                    };

                    bool? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand {Index = me.Item1})?.FlipFlop;

                    BoolValueComparer.Run(helper, Setter, me.Item2.GetFlipFlop, Getter, testValues);
                }
            }
        }

        // TODO: GetStyle, DoesSupportStyle, GetNumSupportedStyles, GetSupportedStyle
        
        [Fact]
        public void TestDveInputFill()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    _BMDSwitcherInputAvailability availability = 0;
                    me.Item2.GetFillInputAvailabilityMask(ref availability);
                    Assert.Equal(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMixEffectBlock0, availability);

                    long[] testValues = VideoSourceLists.All
                        .Where(s => s.IsAvailable(helper.Profile, InternalPortType.Mask))
                        .Where(s => s.IsAvailable(me.Item1))
                        .Select(s => (long) s).ToArray();
                    long[] badValues = VideoSourceLists.All
                        .Select(s => (long) s)
                        .Where(s => !testValues.Contains(s))
                        .ToArray();

                    ICommand Setter(long v) => new TransitionDVESetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionDVESetCommand.MaskFlags.FillSource,
                        FillSource = (VideoSource) v,
                    };

                    long? Getter() => (long?) helper.FindWithMatching(new TransitionDVEGetCommand {Index = me.Item1})?.FillSource;

                    ValueTypeComparer<long>.Run(helper, Setter, me.Item2.GetInputFill, Getter, testValues);
                    ValueTypeComparer<long>.Fail(helper, Setter, me.Item2.GetInputFill, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDveInputKey()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    _BMDSwitcherInputAvailability availability = 0;
                    me.Item2.GetFillInputAvailabilityMask(ref availability);
                    Assert.Equal(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMixEffectBlock0, availability);

                    long[] testValues = VideoSourceLists.All
                        .Where(s => s.IsAvailable(helper.Profile, InternalPortType.Mask))
                        .Where(s => s.IsAvailable(me.Item1) && s.IsAvailable(SourceAvailability.KeySource))
                        .Select(s => (long) s).ToArray();
                    long[] badValues = VideoSourceLists.All
                        .Select(s => (long)s)
                        .Where(s => !testValues.Contains(s))
                        .ToArray();

                    ICommand Setter(long v) => new TransitionDVESetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionDVESetCommand.MaskFlags.KeySource,
                        KeySource = (VideoSource) v,
                    };

                    long? Getter() => (long?) helper.FindWithMatching(new TransitionDVEGetCommand {Index = me.Item1})?.KeySource;

                    ValueTypeComparer<long>.Run(helper, Setter, me.Item2.GetInputCut, Getter, testValues);
                    ValueTypeComparer<long>.Fail(helper, Setter, me.Item2.GetInputCut, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDveEnableKey()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new TransitionDVESetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionDVESetCommand.MaskFlags.EnableKey,
                        EnableKey = v
                    };

                    bool? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand {Index = me.Item1})?.EnableKey;

                    BoolValueComparer.Run(helper, Setter, me.Item2.GetEnableKey, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestDvePreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new TransitionDVESetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionDVESetCommand.MaskFlags.PreMultiplied,
                        PreMultiplied = v
                    };

                    bool? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand {Index = me.Item1})?.PreMultiplied;

                    BoolValueComparer.Run(helper, Setter, me.Item2.GetPreMultiplied, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestDveClip()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    double[] testValues = {0, 87.4, 14.7, 99.9, 100, 0.1};
                    double[] badValues = {100.1, 110, 101, -0.01, -1, -10};

                    ICommand Setter(double v) => new TransitionDVESetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionDVESetCommand.MaskFlags.Clip,
                        Clip = v
                    };

                    double? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand {Index = me.Item1})?.Clip;

                    DoubleValueComparer.Run(helper, Setter, me.Item2.GetClip, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, me.Item2.GetClip, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestDveGain()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    double[] testValues = {0, 87.4, 14.7, 99.9, 100, 0.1};
                    double[] badValues = {100.1, 110, 101, -0.01, -1, -10};

                    ICommand Setter(double v) => new TransitionDVESetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionDVESetCommand.MaskFlags.Gain,
                        Gain = v
                    };

                    double? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand {Index = me.Item1})?.Gain;

                    DoubleValueComparer.Run(helper, Setter, me.Item2.GetGain, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, me.Item2.GetGain, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestDveInvertKey()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new TransitionDVESetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionDVESetCommand.MaskFlags.InvertKey,
                        InvertKey = v
                    };

                    bool? Getter() => helper.FindWithMatching(new TransitionDVEGetCommand {Index = me.Item1})?.InvertKey;

                    BoolValueComparer.Run(helper, Setter, me.Item2.GetInverse, Getter, testValues);
                }
            }
        }
    }
}