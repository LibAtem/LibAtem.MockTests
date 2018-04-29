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
    public class TestChromaKeyer : ComparisonTestBase
    {
        public TestChromaKeyer(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        [Fact]
        public void TestChromaKeyerHue()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
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
                    
                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Chroma.Hue = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)((ushort)(v * 10) % 3600);
                        state.MixEffects[key.Item1].Keyers[key.Item2].Chroma.Hue = ui / 10d;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestChromaKeyerGain()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyChromaParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.1 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyChromaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyChromaSetCommand.MaskFlags.Gain,
                        Gain = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Chroma.Gain = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Chroma.Gain = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestChromaKeyerYSuppress()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyChromaParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.1 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyChromaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyChromaSetCommand.MaskFlags.YSuppress,
                        YSuppress = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Chroma.YSuppress = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Chroma.YSuppress = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestChromaKeyerLift()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyChromaParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.1 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyChromaSetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyChromaSetCommand.MaskFlags.Lift,
                        Lift = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Chroma.Lift = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].Chroma.Lift = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestChromaKeyerNarrow()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
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

                    void UpdateExpectedState(ComparisonState state, bool v) => state.MixEffects[key.Item1].Keyers[key.Item2].Chroma.Narrow = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }
    }
}