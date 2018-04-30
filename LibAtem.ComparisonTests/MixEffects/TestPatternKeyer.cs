using System;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestPatternKeyer : ComparisonTestBase
    {
        public TestPatternKeyer(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        [Fact]
        public void TestPatternKeyerPattern()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    Pattern[] testValues = Enum.GetValues(typeof(Pattern)).OfType<Pattern>().ToArray();

                    ICommand Setter(Pattern v) => new MixEffectKeyPatternSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyPatternSetCommand.MaskFlags.Pattern,
                        Pattern = v,
                    };

                    void UpdateExpectedState(ComparisonState state, Pattern v)
                    {
                        var props = state.MixEffects[key.Item1].Keyers[key.Item2].Pattern;
                        props.Style = v;
                        props.XPosition = 0.5;
                        props.YPosition = 0.5;
                        props.Symmetry = v.GetDefaultPatternSymmetry();
                    }

                    ValueTypeComparer<Pattern>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestPatternKeyerSize()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.01 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyPatternSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyPatternSetCommand.MaskFlags.Size,
                        Size = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Pattern.Size = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Pattern.Size = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestPatternKeyerSymmetry()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    key.Item3.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris);
                    helper.Sleep();

                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.01 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyPatternSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyPatternSetCommand.MaskFlags.Symmetry,
                        Symmetry = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Pattern.Symmetry = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Pattern.Symmetry = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestPatternKeyerSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.01 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyPatternSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyPatternSetCommand.MaskFlags.Softness,
                        Softness = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Pattern.Softness = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Pattern.Softness = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestPatternKeyerHorizontalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    double[] testValues = { 0, 0.874, 0.147, 0.999, 1.00, 0.01 };
                    double[] badValues = { 1.001, 1.1, 1.01, -0.01, -1, -0.10 };

                    ICommand Setter(double v) => new MixEffectKeyPatternSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyPatternSetCommand.MaskFlags.XPosition,
                        XPosition = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Pattern.XPosition = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Pattern.XPosition = v >= 1 ? 1 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestPatternKeyerVerticalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    double[] testValues = { 0, 0.874, 0.147, 0.999, 1.00, 0.01 };
                    double[] badValues = { 1.001, 1.1, 1.01, -0.01, -1, -0.10 };

                    ICommand Setter(double v) => new MixEffectKeyPatternSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyPatternSetCommand.MaskFlags.YPosition,
                        YPosition = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Pattern.YPosition = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Pattern.YPosition = v >= 1 ? 1 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestPatternKeyerInverse()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    bool[] testValues = { true, false };

                    ICommand Setter(bool v) => new MixEffectKeyPatternSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyPatternSetCommand.MaskFlags.Inverse,
                        Inverse = v,
                    };

                    void UpdateExpectedState(ComparisonState state, bool v) => state.MixEffects[key.Item1].Keyers[key.Item2].Pattern.Inverse = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }
    }
}