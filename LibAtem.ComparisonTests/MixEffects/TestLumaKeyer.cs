using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.ComparisonTests.State;
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
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, bool v) => state.MixEffects[key.Item1].Keyers[key.Item2].Luma.PreMultiplied = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestLumaKeyerClip()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyLumaParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.1 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyLumaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyLumaSetCommand.MaskFlags.Clip,
                        Clip = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Luma.Clip = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Luma.Clip = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestLumaKeyerGain()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyLumaParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.1 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyLumaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyLumaSetCommand.MaskFlags.Gain,
                        Gain = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Luma.Gain = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Luma.Gain = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestLumaKeyerInverse()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, bool v) => state.MixEffects[key.Item1].Keyers[key.Item2].Luma.Invert = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }
    }
}