using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.ComparisonTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestLumaKeyer : ComparisonTestBase
    {
        public TestLumaKeyer(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        [Fact]
        public void TestLumaKeyerPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyLumaParameters>())
                {
                    bool[] testValues = { true, false };

                    ICommand Setter(bool v) => new MixEffectKeyLumaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyLumaSetCommand.MaskFlags.PreMultiplied,
                        PreMultiplied = v,
                    };

                    bool? Getter() => helper.FindWithMatching(new MixEffectKeyLumaGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.PreMultiplied;

                    BoolValueComparer.Run(helper, Setter, key.Item3.GetPreMultiplied, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestLumaKeyerClip()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyLumaParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.01 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyLumaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyLumaSetCommand.MaskFlags.Clip,
                        Clip = v,
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyLumaGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.Clip;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetClip, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetClip, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestLumaKeyerGain()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyLumaParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.01 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyLumaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyLumaSetCommand.MaskFlags.Gain,
                        Gain = v,
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyLumaGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.Gain;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetGain, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetGain, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestLumaKeyerInverse()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyLumaParameters>())
                {
                    bool[] testValues = { true, false };

                    ICommand Setter(bool v) => new MixEffectKeyLumaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyLumaSetCommand.MaskFlags.Invert,
                        Invert = v,
                    };

                    bool? Getter() => helper.FindWithMatching(new MixEffectKeyLumaGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.Invert;

                    BoolValueComparer.Run(helper, Setter, key.Item3.GetInverse, Getter, testValues);
                }
            }
        }
    }
}