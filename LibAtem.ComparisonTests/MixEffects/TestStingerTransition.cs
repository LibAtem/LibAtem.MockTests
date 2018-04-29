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
    public class TestStingerTransition : ComparisonTestBase
    {
        public TestStingerTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        private void ResetProps(IBMDSwitcherTransitionStingerParameters props)
        {
            props.SetTriggerPoint(1);
            props.SetPreroll(1);
            props.SetMixRate(1);
            props.SetClipDuration(40);
            props.SetTriggerPoint(10);
            props.SetPreroll(5);
            props.SetMixRate(15);
        }

        [Fact]
        public void TestStingerSource()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    StingerSource[] testValues = Enum.GetValues(typeof(StingerSource)).OfType<StingerSource>().Where(s => s.IsAvailable(helper.Profile)).ToArray();
                    StingerSource[] badValues = Enum.GetValues(typeof(StingerSource)).OfType<StingerSource>().Where(s => !testValues.Contains(s)).ToArray();

                    ICommand Setter(StingerSource v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.Source,
                        Source = v,
                    };

                    void UpdateExpectedState(ComparisonState state, StingerSource v) => state.MixEffects[me.Item1].Transition.Stinger.Source = v;

                    ValueTypeComparer<StingerSource>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<StingerSource>.Fail(helper, Setter, badValues);
                }
            }
        }

        [Fact]
        public void TestStingerPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.PreMultipliedKey,
                        PreMultipliedKey = v
                    };

                    void UpdateExpectedState(ComparisonState state, bool v)
                    {
                        state.MixEffects[me.Item1].Transition.Stinger.PreMultipliedKey = v;
                        state.MixEffects[me.Item1].Transition.DVE.PreMultiplied = v;
                    }

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestStingerClip()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.1 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.Clip,
                        Clip = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v)
                    {
                        state.MixEffects[me.Item1].Transition.Stinger.Clip = v;
                        state.MixEffects[me.Item1].Transition.DVE.Clip = v;
                    }

                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        state.MixEffects[me.Item1].Transition.Stinger.Clip = v >= 100 ? 100 : 0;
                        state.MixEffects[me.Item1].Transition.DVE.Clip = v >= 100 ? 100 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestStingerGain()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    double[] testValues = {0, 87.4, 14.7, 99.9, 100, 0.1};
                    double[] badValues = {100.1, 110, 101, -0.01, -1, -10};

                    ICommand Setter(double v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.Gain,
                        Gain = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v)
                    {
                        state.MixEffects[me.Item1].Transition.Stinger.Gain = v;
                        state.MixEffects[me.Item1].Transition.DVE.Gain = v;
                    }

                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        state.MixEffects[me.Item1].Transition.Stinger.Gain = v >= 100 ? 100 : 0;
                        state.MixEffects[me.Item1].Transition.DVE.Gain = v >= 100 ? 100 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestStingerInverse()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    bool[] testValues = {true, false};

                    ICommand Setter(bool v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.Invert,
                        Invert = v
                    };

                    void UpdateExpectedState(ComparisonState state, bool v)
                    {
                        state.MixEffects[me.Item1].Transition.Stinger.Invert = v;
                        state.MixEffects[me.Item1].Transition.DVE.InvertKey = v;
                    }

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestStingerPreRoll()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    uint[] testValues = {0, 1, 18, 28, 90};
                    uint[] badValues = {999, 251};

                    ICommand Setter(uint v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.Preroll,
                        Preroll = v,
                    };

                    uint clipMaxFrames = 90; // TODO - dynamic based on me selected

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Stinger.Preroll = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Stinger.Preroll = v >= clipMaxFrames ? clipMaxFrames : 0;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestStingerClipDuration()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    // These props all have various relations, that need better testing
                    ResetProps(me.Item2);
                    me.Item2.SetTriggerPoint(17);
                    me.Item2.SetMixRate(13);
                    me.Item2.SetPreroll(5);
                    helper.Sleep();

                    uint[] testValues = {35, 48, 95, 199, 30};
                    uint[] badValues = {1, 29, 5};

                    ICommand Setter(uint v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.ClipDuration,
                        ClipDuration = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Stinger.ClipDuration = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Stinger.ClipDuration = v <= 30 ? 30 : (uint) 0;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);

                    me.Item2.SetTriggerPoint(4);
                    me.Item2.SetMixRate(6);
                    helper.Sleep();

                    uint[] testValues2 = {11, 30, 10};
                    uint[] badValues2 = {9, 1};

                    void UpdateExpectedState2(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Stinger.ClipDuration = v;
                    void UpdateFailedState2(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Stinger.ClipDuration = v <= 10 ? 10 : (uint) 0;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState2, testValues2);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState2, badValues2);
                }
            }
        }

        [Fact]
        public void TestStingerTriggerPoint()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    // These props all have various relations, that need better testing
                    ResetProps(me.Item2);
                    me.Item2.SetMixRate(15);
                    helper.Sleep();

                    uint[] testValues = {1, 18, 28, 39};
                    uint[] badValues = {40, 41, 50};

                    ICommand Setter(uint v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.TriggerPoint,
                        TriggerPoint = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v)
                    {
                        var props = state.MixEffects[me.Item1].Transition.Stinger;
                        props.TriggerPoint = v;
                        if (props.ClipDuration - props.TriggerPoint < props.MixRate)
                            props.MixRate = props.ClipDuration - props.TriggerPoint;
                    }

                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Stinger.TriggerPoint = v >= 39 ? 39 : (uint) 0;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);

                    me.Item2.SetTriggerPoint(1);
                    me.Item2.SetClipDuration(25);
                    helper.Sleep();

                    uint[] testValues2 = {11, 24, 10};
                    uint[] badValues2 = {25, 26, 30};

                    void UpdateFailedState2(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Stinger.TriggerPoint = v >= 24 ? 24 : (uint) 0;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues2);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState2, badValues2);
                }
            }
        }

        [Fact]
        public void TestStingerMixRate()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionStingerParameters>())
                {
                    // These props all have various relations, that need better testing
                    ResetProps(me.Item2);
                    helper.Sleep();

                    uint[] testValues = {1, 18, 28, 30};
                    uint[] badValues = {31, 32, 40};

                    ICommand Setter(uint v) => new TransitionStingerSetCommand
                    {
                        Index = me.Item1,
                        Mask = TransitionStingerSetCommand.MaskFlags.MixRate,
                        MixRate = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Stinger.MixRate = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Stinger.MixRate = v >= 30 ? 30 : (uint)0;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);

                    me.Item2.SetMixRate(5);
                    me.Item2.SetClipDuration(20);
                    helper.Sleep();

                    uint[] testValues2 = {9, 1, 10};
                    uint[] badValues2 = {11, 12, 20};

                    void UpdateFailedState2(ComparisonState state, uint v) => state.MixEffects[me.Item1].Transition.Stinger.MixRate = v >= 10 ? 10 : (uint)0;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues2);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState2, badValues2);
                }
            }
        }
    }
}
