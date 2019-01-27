using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.MixEffects
{
    [Collection("Client")]
    public class TestChromaKeyer : MixEffectsTestBase
    {
        public TestChromaKeyer(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        private abstract class ChromaKeyerTestDefinition<T> : TestDefinitionBase<T>
        {
            protected readonly MixEffectBlockId _meId;
            protected readonly UpstreamKeyId _keyId;
            protected readonly IBMDSwitcherKeyChromaParameters _sdk;

            public ChromaKeyerTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyChromaParameters> key) : base(helper)
            {
                _meId = key.Item1;
                _keyId = key.Item2;
                _sdk = key.Item3;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new MixEffectKeyChromaGetCommand()
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                });
            }
        }

        private class ChromaKeyerHueTestDefinition : ChromaKeyerTestDefinition<double>
        {
            public ChromaKeyerHueTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyChromaParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetHue(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyChromaSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyChromaSetCommand.MaskFlags.Hue,
                    Hue = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.MixEffects[_meId].Keyers[_keyId].Chroma.Hue = v;
                }
                else
                {
                    ushort ui = (ushort)((ushort)(v * 10) % 3600);
                    state.MixEffects[_meId].Keyers[_keyId].Chroma.Hue = ui / 10d;
                }
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 123, 233.4, 359.9 };
            }

            public override double[] BadValues()
            {
                return new double[] { 360, 360.1, 361, -1, -0.01 };
            }
        }

        [Fact]
        public void TestHue()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyChromaParameters>())
                {
                    new ChromaKeyerHueTestDefinition(helper, key).Run();
                }
            }
        }

        private class ChromaKeyerGainTestDefinition : ChromaKeyerTestDefinition<double>
        {
            public ChromaKeyerGainTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyChromaParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetGain(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyChromaSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyChromaSetCommand.MaskFlags.Gain,
                    Gain = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.MixEffects[_meId].Keyers[_keyId].Chroma.Gain = v;
                }
                else
                {
                    state.MixEffects[_meId].Keyers[_keyId].Chroma.Gain = v >= 100 ? 100 : 0;
                }
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            }

            public override double[] BadValues()
            {
                return new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            }
        }

        [Fact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyChromaParameters>())
                {
                    new ChromaKeyerGainTestDefinition(helper, key).Run();
                }
            }
        }

        private class ChromaKeyerYSuppressTestDefinition : ChromaKeyerTestDefinition<double>
        {
            public ChromaKeyerYSuppressTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyChromaParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetYSuppress(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyChromaSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyChromaSetCommand.MaskFlags.YSuppress,
                    YSuppress = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.MixEffects[_meId].Keyers[_keyId].Chroma.YSuppress = v;
                }
                else
                {
                    state.MixEffects[_meId].Keyers[_keyId].Chroma.YSuppress = v >= 100 ? 100 : 0;
                }
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            }

            public override double[] BadValues()
            {
                return new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            }
        }

        [Fact]
        public void TestYSuppress()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyChromaParameters>())
                {
                    new ChromaKeyerYSuppressTestDefinition(helper, key).Run();
                }
            }
        }

        private class ChromaKeyerLiftTestDefinition : ChromaKeyerTestDefinition<double>
        {
            public ChromaKeyerLiftTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyChromaParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetLift(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyChromaSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyChromaSetCommand.MaskFlags.Lift,
                    Lift = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.MixEffects[_meId].Keyers[_keyId].Chroma.Lift = v;
                }
                else
                {
                    state.MixEffects[_meId].Keyers[_keyId].Chroma.Lift = v >= 100 ? 100 : 0;
                }
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            }

            public override double[] BadValues()
            {
                return new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            }
        }

        [Fact]
        public void TestLift()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyChromaParameters>())
                {
                    new ChromaKeyerLiftTestDefinition(helper, key).Run();
                }
            }
        }

        private class ChromaKeyerNarrowTestDefinition : ChromaKeyerTestDefinition<bool>
        {
            public ChromaKeyerNarrowTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyChromaParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetNarrow(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new MixEffectKeyChromaSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyChromaSetCommand.MaskFlags.Narrow,
                    Narrow = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_meId].Keyers[_keyId].Chroma.Narrow = v;
            }
        }

        [Fact]
        public void TestNarrow()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyChromaParameters>())
                {
                    new ChromaKeyerNarrowTestDefinition(helper, key).Run();
                }
            }
        }
    }
}