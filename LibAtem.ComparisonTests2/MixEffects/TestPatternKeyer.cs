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

        private abstract class PatternKeyerTestDefinition<T> : TestDefinitionBase2<MixEffectKeyPatternSetCommand, T>
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

            public override void SetupCommand(MixEffectKeyPatternSetCommand cmd)
            {
                cmd.MixEffectIndex = _meId;
                cmd.KeyerIndex = _keyId;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, T v)
            {
                ComparisonMixEffectKeyerPatternState obj = state.MixEffects[_meId].Keyers[_keyId].Pattern;
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new MixEffectKeyPatternGetCommand() { MixEffectIndex = _meId, KeyerIndex = _keyId });
            }
        }

        private class PatternKeyerPatternTestDefinition : PatternKeyerTestDefinition<Pattern>
        {
            public PatternKeyerPatternTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleDiamondIris);

            public override string PropertyName => "Pattern";
            public override Pattern MangleBadValue(Pattern v) => v;

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, Pattern v)
            {
                var props = state.MixEffects[_meId].Keyers[_keyId].Pattern;
                props.Style = v;
                props.XPosition = 0.5;
                props.YPosition = 0.5;
                props.Symmetry = v.GetDefaultPatternSymmetry();
            }

            public override Pattern[] GoodValues => Enum.GetValues(typeof(Pattern)).OfType<Pattern>().ToArray();
        }

        [Fact]
        public void TestPattern()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyPatternParameters>().ForEach(k => new PatternKeyerPatternTestDefinition(helper, k).Run());
        }

        private class PatternKeyerSizeTestDefinition : PatternKeyerTestDefinition<double>
        {
            public PatternKeyerSizeTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetSize(40);

            public override string PropertyName => "Size";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.01 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestSize()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyPatternParameters>().ForEach(k => new PatternKeyerSizeTestDefinition(helper, k).Run());
        }

        private class PatternKeyerSymmetryTestDefinition : PatternKeyerTestDefinition<double>
        {
            public PatternKeyerSymmetryTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetSymmetry(40);

            public override string PropertyName => "Symmetry";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.01 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestSymmetry()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyPatternParameters>().ForEach(k => new PatternKeyerSymmetryTestDefinition(helper, k).Run());
        }

        private class PatternKeyerSoftnessTestDefinition : PatternKeyerTestDefinition<double>
        {
            public PatternKeyerSoftnessTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetSoftness(40);

            public override string PropertyName => "Softness";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.01 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyPatternParameters>().ForEach(k => new PatternKeyerSoftnessTestDefinition(helper, k).Run());
        }

        private class PatternKeyerHorizontalOffsetTestDefinition : PatternKeyerTestDefinition<double>
        {
            public PatternKeyerHorizontalOffsetTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetHorizontalOffset(0.5);

            public override string PropertyName => "XPosition";
            public override double MangleBadValue(double v) => v >= 1 ? 1 : 0;

            public override double[] GoodValues => new double[] { 0, 0.874, 0.147, 0.999, 1.00, 0.01 };
            public override double[] BadValues => new double[] { 1.001, 1.1, 1.01, -0.01, -1, -0.10 };
        }

        [Fact]
        public void TestHorizontalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyPatternParameters>().ForEach(k => new PatternKeyerHorizontalOffsetTestDefinition(helper, k).Run());
        }

        private class PatternKeyerVerticalOffsetTestDefinition : PatternKeyerTestDefinition<double>
        {
            public PatternKeyerVerticalOffsetTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetVerticalOffset(0.5);

            public override string PropertyName => "YPosition";
            public override double MangleBadValue(double v) => v >= 1 ? 1 : 0;

            public override double[] GoodValues => new double[] { 0, 0.874, 0.147, 0.999, 1.00, 0.01 };
            public override double[] BadValues => new double[] { 1.001, 1.1, 1.01, -0.01, -1, -0.10 };
        }

        [Fact]
        public void TestVertictalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyPatternParameters>().ForEach(k => new PatternKeyerVerticalOffsetTestDefinition(helper, k).Run());
        }

        private class PatternKeyerInvertKeyTestDefinition : PatternKeyerTestDefinition<bool>
        {
            public PatternKeyerInvertKeyTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyPatternParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInverse(0);

            public override string PropertyName => "Inverse";
            public override bool MangleBadValue(bool v) => v;
        }

        [Fact]
        public void TestInvertKey()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyPatternParameters>().ForEach(k => new PatternKeyerInvertKeyTestDefinition(helper, k).Run());
        }
    }
}