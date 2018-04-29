using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
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
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.DVE.Rate = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.DVE.Rate = v >= 250 ? 250 : (uint)1;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDveLogoRate()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.DVE.LogoRate = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.DVE.LogoRate = v >= 250 ? 250 : (uint)1;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDveReverse()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, bool v) => state.MixEffects[me.Item1].Transition.DVE.Reverse = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestDveFlipFlop()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, bool v) => state.MixEffects[me.Item1].Transition.DVE.FlipFlop = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        // TODO: GetStyle, DoesSupportStyle, GetNumSupportedStyles, GetSupportedStyle
        
        [Fact]
        public void TestDveInputFill()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, long v)
                    {
                        state.MixEffects[me.Item1].Transition.DVE.FillSource = (VideoSource) v;
                        if (VideoSourceLists.MediaPlayers.Contains((VideoSource) v))
                            state.MixEffects[me.Item1].Transition.DVE.KeySource = (VideoSource) v + 1;
                    }

                    ValueTypeComparer<long>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<long>.Fail(helper, Setter, badValues);
                }
            }
        }

        [Fact]
        public void TestDveInputKey()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, long v) => state.MixEffects[me.Item1].Transition.DVE.KeySource = (VideoSource)v;

                    ValueTypeComparer<long>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<long>.Fail(helper, Setter, badValues);
                }
            }
        }

        [Fact]
        public void TestDveEnableKey()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, bool v) => state.MixEffects[me.Item1].Transition.DVE.EnableKey = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestDvePreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, bool v)
                    {
                        state.MixEffects[me.Item1].Transition.DVE.PreMultiplied = v;
                        state.MixEffects[me.Item1].Transition.Stinger.PreMultipliedKey = v;
                    }

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestDveClip()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, double v)
                    {
                        state.MixEffects[me.Item1].Transition.DVE.Clip = v;
                        state.MixEffects[me.Item1].Transition.Stinger.Clip = v;
                    }

                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        state.MixEffects[me.Item1].Transition.DVE.Clip = v > 100 ? 100 : 0;
                        state.MixEffects[me.Item1].Transition.Stinger.Clip = v > 100 ? 100 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDveGain()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, double v)
                    {
                        state.MixEffects[me.Item1].Transition.DVE.Gain = v;
                        state.MixEffects[me.Item1].Transition.Stinger.Gain = v;
                    }

                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        state.MixEffects[me.Item1].Transition.DVE.Gain = v > 100 ? 100 : 0;
                        state.MixEffects[me.Item1].Transition.Stinger.Gain = v > 100 ? 100 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDveInvertKey()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, bool v)
                    {
                        state.MixEffects[me.Item1].Transition.DVE.InvertKey = v;
                        state.MixEffects[me.Item1].Transition.Stinger.Invert = v;
                    }

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }
    }
}