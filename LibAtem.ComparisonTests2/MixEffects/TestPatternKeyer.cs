using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.MixEffects
{
    [Collection("Client")]
    public class TestPatternKeyer : MixEffectsTestBase
    {
        public TestPatternKeyer(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        private abstract class PatternKeyerTestDefinition<T> : TestDefinitionBase<T>
        {
            protected readonly MixEffectBlockId _meId;
            protected readonly UpstreamKeyId _keyId;
            protected readonly IBMDSwitcherKeyPatternParameters _sdk;

            public PatternKeyerTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper)
            {
                _meId = key.Item1;
                _keyId = key.Item2;
                _sdk = key.Item3;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new MixEffectKeyPatternGetCommand()
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                });
            }
        }

        private class PatternKeyerPatternTestDefinition : PatternKeyerTestDefinition<Pattern>
        {
            public PatternKeyerPatternTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleDiamondIris);
            }

            public override ICommand GenerateCommand(Pattern v)
            {
                return new MixEffectKeyPatternSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyPatternSetCommand.MaskFlags.Pattern,
                    Pattern = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, Pattern v)
            {
                var props = state.MixEffects[_meId].Keyers[_keyId].Pattern;
                props.Style = v;
                props.XPosition = 0.5;
                props.YPosition = 0.5;
                props.Symmetry = v.GetDefaultPatternSymmetry();
            }

            public override Pattern[] GoodValues()
            {
                return Enum.GetValues(typeof(Pattern)).OfType<Pattern>().ToArray();
            }
        }

        [Fact]
        public void TestPattern()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    new PatternKeyerPatternTestDefinition(helper, key).Run();
                }
            }
        }

        private class PatternKeyerSizeTestDefinition : PatternKeyerTestDefinition<double>
        {
            public PatternKeyerSizeTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetSize(40);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyPatternSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyPatternSetCommand.MaskFlags.Size,
                    Size = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].Pattern.Size = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].Pattern.Size = v >= 100 ? 100 : 0;
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 87.4, 14.7, 99.9, 100, 0.01 };
            }
            public override double[] BadValues()
            {
                return new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            }
        }

        [Fact]
        public void TestSize()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    new PatternKeyerSizeTestDefinition(helper, key).Run();
                }
            }
        }

        private class PatternKeyerSymmetryTestDefinition : PatternKeyerTestDefinition<double>
        {
            public PatternKeyerSymmetryTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetSymmetry(40);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyPatternSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyPatternSetCommand.MaskFlags.Symmetry,
                    Symmetry = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].Pattern.Symmetry = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].Pattern.Symmetry = v >= 100 ? 100 : 0;
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 87.4, 14.7, 99.9, 100, 0.01 };
            }
            public override double[] BadValues()
            {
                return new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            }
        }

        [Fact]
        public void TestSymmetry()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    new PatternKeyerSymmetryTestDefinition(helper, key).Run();
                }
            }
        }

        private class PatternKeyerSoftnessTestDefinition : PatternKeyerTestDefinition<double>
        {
            public PatternKeyerSoftnessTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetSoftness(40);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyPatternSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyPatternSetCommand.MaskFlags.Softness,
                    Softness = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].Pattern.Softness = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].Pattern.Softness = v >= 100 ? 100 : 0;
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 87.4, 14.7, 99.9, 100, 0.01 };
            }
            public override double[] BadValues()
            {
                return new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            }
        }

        [Fact]
        public void TestSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    new PatternKeyerSoftnessTestDefinition(helper, key).Run();
                }
            }
        }

        private class PatternKeyerHorizontalOffsetTestDefinition : PatternKeyerTestDefinition<double>
        {
            public PatternKeyerHorizontalOffsetTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetHorizontalOffset(0.5);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyPatternSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyPatternSetCommand.MaskFlags.XPosition,
                    XPosition = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].Pattern.XPosition = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].Pattern.XPosition = v >= 1 ? 1 : 0;
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 0.874, 0.147, 0.999, 1.00, 0.01 };
            }
            public override double[] BadValues()
            {
                return new double[] { 1.001, 1.1, 1.01, -0.01, -1, -0.10 };
            }
        }

        [Fact]
        public void TestHorizontalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    new PatternKeyerHorizontalOffsetTestDefinition(helper, key).Run();
                }
            }
        }

        private class PatternKeyerVerticalOffsetTestDefinition : PatternKeyerTestDefinition<double>
        {
            public PatternKeyerVerticalOffsetTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetVerticalOffset(0.5);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyPatternSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyPatternSetCommand.MaskFlags.YPosition,
                    YPosition = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].Pattern.YPosition = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].Pattern.YPosition = v >= 1 ? 1 : 0;
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 0.874, 0.147, 0.999, 1.00, 0.01 };
            }
            public override double[] BadValues()
            {
                return new double[] { 1.001, 1.1, 1.01, -0.01, -1, -0.10 };
            }
        }

        [Fact]
        public void TestVertictalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    new PatternKeyerVerticalOffsetTestDefinition(helper, key).Run();
                }
            }
        }

        private class PatternKeyerInvertKeyTestDefinition : PatternKeyerTestDefinition<bool>
        {
            public PatternKeyerInvertKeyTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInverse(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new MixEffectKeyPatternSetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyPatternSetCommand.MaskFlags.Inverse,
                    Inverse = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_meId].Keyers[_keyId].Pattern.Inverse = v;
            }
        }

        [Fact]
        public void TestInvertKey()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyPatternParameters>())
                {
                    new PatternKeyerInvertKeyTestDefinition(helper, key).Run();
                }
            }
        }
    }
}