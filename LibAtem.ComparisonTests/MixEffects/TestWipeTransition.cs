using System;
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
    public class TestWipeTransition : ComparisonTestBase
    {
        public TestWipeTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }
        
        [Fact]
        public void TestWipeRate()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    uint[] testValues = {1, 18, 28, 95, 234, 244, 250};
                    uint[] badValues = {251, 255, 0};

                    ICommand Setter(uint v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.Rate,
                        Rate = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Wipe.Rate = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Wipe.Rate = v >= 250 ? 250 : (uint)1;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }
        
        [Fact]
        public void TestWipePattern()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    Pattern[] testValues = Enum.GetValues(typeof(Pattern)).OfType<Pattern>().ToArray();

                    ICommand Setter(Pattern v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.Pattern,
                        Pattern = v,
                    };

                    void UpdateExpectedState(ComparisonState state, Pattern v)
                    {
                        var props = state.MixEffects[me.Item1].Transition.Wipe;
                        props.Pattern = v;
                        props.XPosition = 0.5;
                        props.YPosition = 0.5;
                        props.Symmetry = v.GetDefaultPatternSymmetry();
                    }

                    ValueTypeComparer<Pattern>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }
        
        [Fact]
        public void TestWipeBorderSize()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    double[] testValues = {0, 87.4, 14.7, 99.9, 100, 0.01};
                    double[] badValues = {100.1, 110, 101, -0.01, -1, -10};

                    ICommand Setter(double v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.BorderWidth,
                        BorderWidth = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[me.Item1].Transition.Wipe.BorderWidth = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[me.Item1].Transition.Wipe.BorderWidth = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }
        
        [Fact]
        public void TestWipeBorderInput()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    VideoSource[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(Client.Profile) && s.IsAvailable(me.Item1)).ToArray();
                    VideoSource[] badValues = VideoSourceLists.All.Where(s => !testValues.Contains(s)).ToArray();

                    ICommand Setter(VideoSource v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.BorderInput,
                        BorderInput = v,
                    };

                    void UpdateExpectedState(ComparisonState state, VideoSource v) => state.MixEffects[me.Item1].Transition.Wipe.BorderInput = v;

                    ValueTypeComparer<VideoSource>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<VideoSource>.Fail(helper, Setter, badValues);
                }
            }
        }

        [Fact]
        public void TestWipeSymmetry()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    // Not all props support symmetry
                    me.Item2.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris);
                    helper.Sleep();

                    double[] testValues = {0, 87.4, 14.7, 99.9, 100, 0.01};
                    double[] badValues = {100.1, 110, 101, -0.01, -1, -10};

                    ICommand Setter(double v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.Symmetry,
                        Symmetry = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[me.Item1].Transition.Wipe.Symmetry = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[me.Item1].Transition.Wipe.Symmetry = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }
        
        [Fact]
        public void TestWipeBorderSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    double[] testValues = {0, 87.4, 14.7, 99.9, 100, 0.01};
                    double[] badValues = {100.1, 110, 101, -0.01, -1, -10};

                    ICommand Setter(double v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.BorderSoftness,
                        BorderSoftness = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[me.Item1].Transition.Wipe.BorderSoftness = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[me.Item1].Transition.Wipe.BorderSoftness = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }
        
        [Fact]
        public void TestWipeHorizontalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    // Not all props support XPosition
                    me.Item2.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris);
                    helper.Sleep();

                    double[] testValues = { 0, 0.874, 0.147, 0.999, 1.00, 0.01 };
                    double[] badValues = { 1.001, 1.1, 1.01, -0.01, -1, -0.10 };

                    ICommand Setter(double v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.XPosition,
                        XPosition = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[me.Item1].Transition.Wipe.XPosition = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[me.Item1].Transition.Wipe.XPosition = v >= 1 ? 1 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestWipeVerticalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    // Not all props support YPosition
                    me.Item2.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris);
                    helper.Sleep();

                    double[] testValues = {0, 0.874, 0.147, 0.999, 1.00, 0.01};
                    double[] badValues = {1.001, 1.1, 1.01, -0.01, -1, -0.10};

                    ICommand Setter(double v) => new TransitionWipeSetCommand
                    {
                        Index = MixEffectBlockId.One,
                        Mask = TransitionWipeSetCommand.MaskFlags.YPosition,
                        YPosition = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[me.Item1].Transition.Wipe.YPosition = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[me.Item1].Transition.Wipe.YPosition = v >= 1 ? 1 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }
        
        [Fact]
        public void TestWipeReverse()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.ReverseDirection,
                        ReverseDirection = v
                    };

                    void UpdateExpectedState(ComparisonState state, bool v)
                    {
                        state.MixEffects[me.Item1].Transition.Wipe.ReverseDirection = v;
                        state.MixEffects[me.Item1].Transition.DVE.Reverse = v;
                    }

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestWipeFlipFlop()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new TransitionWipeSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionWipeSetCommand.MaskFlags.FlipFlop,
                        FlipFlop = v
                    };

                    void UpdateExpectedState(ComparisonState state, bool v)
                    {
                        state.MixEffects[me.Item1].Transition.Wipe.FlipFlop = v;
                        state.MixEffects[me.Item1].Transition.DVE.FlipFlop = v;
                    }

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }
    }
}