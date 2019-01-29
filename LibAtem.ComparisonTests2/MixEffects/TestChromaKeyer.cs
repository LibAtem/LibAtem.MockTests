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

        private abstract class ChromaKeyerTestDefinition<T> : TestDefinitionBase2<MixEffectKeyChromaSetCommand, T>
        {
            private readonly MixEffectBlockId _meId;
            private readonly UpstreamKeyId _keyId;
            protected readonly IBMDSwitcherKeyChromaParameters _sdk;

            public ChromaKeyerTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyChromaParameters> key) : base(helper)
            {
                _meId = key.Item1;
                _keyId = key.Item2;
                _sdk = key.Item3;
            }

            public override void SetupCommand(MixEffectKeyChromaSetCommand cmd)
            {
                cmd.MixEffectIndex = _meId;
                cmd.KeyerIndex = _keyId;
            }

            public abstract T MangleBadValue(T v);

            public sealed override void UpdateExpectedState(ComparisonState state, bool goodValue, T v)
            {
                ComparisonMixEffectKeyerChromaState obj = state.MixEffects[_meId].Keyers[_keyId].Chroma;
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new MixEffectKeyChromaGetCommand() { MixEffectIndex = _meId, KeyerIndex = _keyId });
            }
        }

        private class ChromaKeyerHueTestDefinition : ChromaKeyerTestDefinition<double>
        {
            public ChromaKeyerHueTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyChromaParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetHue(20);

            public override string PropertyName => "Hue";
            public override double MangleBadValue(double v)
            {
                ushort ui = (ushort)((ushort)(v * 10) % 3600);
                return ui / 10d;
            }

            public override double[] GoodValues => new double[] { 0, 123, 233.4, 359.9 };
            public override double[] BadValues => new double[] { 360, 360.1, 361, -1, -0.01 };
        }

        [Fact]
        public void TestHue()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyChromaParameters>().ForEach(k => new ChromaKeyerHueTestDefinition(helper, k).Run());
        }

        private class ChromaKeyerGainTestDefinition : ChromaKeyerTestDefinition<double>
        {
            public ChromaKeyerGainTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyChromaParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetGain(20);

            public override string PropertyName => "Gain";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyChromaParameters>().ForEach(k => new ChromaKeyerGainTestDefinition(helper, k).Run());
        }

        private class ChromaKeyerYSuppressTestDefinition : ChromaKeyerTestDefinition<double>
        {
            public ChromaKeyerYSuppressTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyChromaParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetYSuppress(20);

            public override string PropertyName => "YSuppress";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestYSuppress()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyChromaParameters>().ForEach(k => new ChromaKeyerYSuppressTestDefinition(helper, k).Run());
        }

        private class ChromaKeyerLiftTestDefinition : ChromaKeyerTestDefinition<double>
        {
            public ChromaKeyerLiftTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyChromaParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetLift(20);

            public override string PropertyName => "Lift";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestLift()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyChromaParameters>().ForEach(k => new ChromaKeyerLiftTestDefinition(helper, k).Run());
        }

        private class ChromaKeyerNarrowTestDefinition : ChromaKeyerTestDefinition<bool>
        {
            public ChromaKeyerNarrowTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyChromaParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetNarrow(0);

            public override string PropertyName => "Narrow";
            public override bool MangleBadValue(bool v) => v;
        }

        [Fact]
        public void TestNarrow()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyChromaParameters>().ForEach(k => new ChromaKeyerNarrowTestDefinition(helper, k).Run());
        }
    }
}