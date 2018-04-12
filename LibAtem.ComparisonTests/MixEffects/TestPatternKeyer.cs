using System;
using System.Linq;
using AtemEmulator.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using Xunit;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests.MixEffects
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
            using (var helper = new AtemComparisonHelper(Client))
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

                    Pattern? Getter() => helper.FindWithMatching(new MixEffectKeyPatternGetCommand {MixEffectIndex = key.Item1, KeyerIndex = key.Item2})?.Style;

                    EnumValueComparer<Pattern, _BMDSwitcherPatternStyle>.Run(helper, TestWipeTransition.PatternMap, Setter, key.Item3.GetPattern, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestPatternKeyerSize()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyPatternGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.Size;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetSize, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetSize, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestPatternKeyerSymmetry()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.01 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyPatternSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyPatternSetCommand.MaskFlags.Symmetry,
                        Symmetry = v,
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyPatternGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.Symmetry;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetSymmetry, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetSymmetry, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestPatternKeyerSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyPatternGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.Softness;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetSoftness, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetSoftness, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestPatternKeyerHorizontalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyPatternGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.XPosition;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetHorizontalOffset, Getter, testValues);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetHorizontalOffset, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestPatternKeyerVerticalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyPatternGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.YPosition;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetVerticalOffset, Getter, testValues);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetVerticalOffset, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestPatternKeyerInverse()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    bool? Getter() => helper.FindWithMatching(new MixEffectKeyPatternGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.Inverse;

                    BoolValueComparer.Run(helper, Setter, key.Item3.GetInverse, Getter, testValues);
                }
            }
        }
    }
}