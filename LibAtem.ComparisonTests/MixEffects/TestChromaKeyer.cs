using AtemEmulator.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using Xunit;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestChromaKeyer : ComparisonTestBase
    {
        public TestChromaKeyer(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        [Fact]
        public void TestChromaKeyerHue()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyChromaParameters>())
                {
                    double[] testValues = { 0, 123, 233.4, 359.9 };
                    double[] badValues = { 360, 360.1, 361, -1, -0.01 };

                    ICommand Setter(double v) => new MixEffectKeyChromaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyChromaSetCommand.MaskFlags.Hue,
                        Hue = v,
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyChromaGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.Hue;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetHue, Getter, testValues);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetHue, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestChromaKeyerGain()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyChromaParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.01 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyChromaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyChromaSetCommand.MaskFlags.Gain,
                        Gain = v,
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyChromaGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.Gain;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetGain, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetGain, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestChromaKeyerYSuppress()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyChromaParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.01 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyChromaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyChromaSetCommand.MaskFlags.YSuppress,
                        YSuppress = v,
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyChromaGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.YSuppress;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetYSuppress, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetYSuppress, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestChromaKeyerLift()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyChromaParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.01 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyChromaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyChromaSetCommand.MaskFlags.Lift,
                        Lift = v,
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyChromaGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.Lift;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetLift, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetLift, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestChromaKeyerNarrow()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyChromaParameters>())
                {
                    bool[] testValues = { true, false };

                    ICommand Setter(bool v) => new MixEffectKeyChromaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyChromaSetCommand.MaskFlags.Narrow,
                        Narrow = v,
                    };

                    bool? Getter() => helper.FindWithMatching(new MixEffectKeyChromaGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.Narrow;

                    BoolValueComparer.Run(helper, Setter, key.Item3.GetNarrow, Getter, testValues);
                }
            }
        }
    }
}