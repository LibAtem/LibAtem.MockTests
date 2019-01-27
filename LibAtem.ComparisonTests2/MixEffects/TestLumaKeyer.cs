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
    public class TestLumaKeyer : MixEffectsTestBase
    {
        public TestLumaKeyer(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        private abstract class LumaKeyerTestDefinition<T> : TestDefinitionBase<T>
        {
            protected readonly MixEffectBlockId _meId;
            protected readonly UpstreamKeyId _keyId;
            protected readonly IBMDSwitcherKeyLumaParameters _sdk;

            public LumaKeyerTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyLumaParameters> key) : base(helper)
            {
                _meId = key.Item1;
                _keyId = key.Item2;
                _sdk = key.Item3;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new MixEffectKeyLumaGetCommand()
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                });
            }
        }

        private class LumaKeyerPreMultipliedTestDefinition : LumaKeyerTestDefinition<bool>
        {
            public LumaKeyerPreMultipliedTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyLumaParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetPreMultiplied(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new MixEffectKeyLumaSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyLumaSetCommand.MaskFlags.PreMultiplied,
                    PreMultiplied = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_meId].Keyers[_keyId].Luma.PreMultiplied = v;
            }
        }

        [Fact]
        public void TestPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyLumaParameters>())
                {
                    new LumaKeyerPreMultipliedTestDefinition(helper, key).Run();
                }
            }
        }

        private class LumaKeyerClipTestDefinition : LumaKeyerTestDefinition<double>
        {
            public LumaKeyerClipTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyLumaParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetClip(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyLumaSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyLumaSetCommand.MaskFlags.Clip,
                    Clip = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].Luma.Clip = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].Luma.Clip = v >= 100 ? 100 : 0;
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
        public void TestClip()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyLumaParameters>())
                {
                    new LumaKeyerClipTestDefinition(helper, key).Run();
                }
            }
        }

        private class LumaKeyerGainTestDefinition : LumaKeyerTestDefinition<double>
        {
            public LumaKeyerGainTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyLumaParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetGain(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyLumaSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyLumaSetCommand.MaskFlags.Gain,
                    Gain = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].Luma.Gain = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].Luma.Gain = v >= 100 ? 100 : 0;
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
                foreach (var key in GetKeyers<IBMDSwitcherKeyLumaParameters>())
                {
                    new LumaKeyerGainTestDefinition(helper, key).Run();
                }
            }
        }

        private class LumaKeyerInvertKeyTestDefinition : LumaKeyerTestDefinition<bool>
        {
            public LumaKeyerInvertKeyTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyLumaParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInverse(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new MixEffectKeyLumaSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyLumaSetCommand.MaskFlags.Invert,
                    Invert = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_meId].Keyers[_keyId].Luma.Invert = v;
            }
        }

        [Fact]
        public void TestInvertKey()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyLumaParameters>())
                {
                    new LumaKeyerInvertKeyTestDefinition(helper, key).Run();
                }
            }
        }
    }
}