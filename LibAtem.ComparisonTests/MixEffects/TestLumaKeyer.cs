using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.State;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestLumaKeyer : MixEffectsTestBase
    {
        public TestLumaKeyer(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        private abstract class LumaKeyerTestDefinition<T> : TestDefinitionBase<MixEffectKeyLumaSetCommand, T>
        {
            private readonly MixEffectBlockId _meId;
            private readonly UpstreamKeyId _keyId;
            protected readonly IBMDSwitcherKeyLumaParameters _sdk;

            public LumaKeyerTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyLumaParameters> key) : base(helper)
            {
                _meId = key.Item1;
                _keyId = key.Item2;
                _sdk = key.Item3;
            }

            public override void SetupCommand(MixEffectKeyLumaSetCommand cmd)
            {
                cmd.MixEffectIndex = _meId;
                cmd.KeyerIndex = _keyId;
            }

            public abstract T MangleBadValue(T v);

            public sealed override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                MixEffectState.KeyerLumaState obj = state.MixEffects[(int)_meId].Keyers[(int)_keyId].Luma;
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new MixEffectKeyLumaGetCommand() { MixEffectIndex = _meId, KeyerIndex = _keyId });
            }
        }

        private class LumaKeyerPreMultipliedTestDefinition : LumaKeyerTestDefinition<bool>
        {
            public LumaKeyerPreMultipliedTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyLumaParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetPreMultiplied(0);

            public override string PropertyName => "PreMultiplied";
            public override bool MangleBadValue(bool v) => v;
        }

        [Fact]
        public void TestPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyLumaParameters>().ForEach(k => new LumaKeyerPreMultipliedTestDefinition(helper, k).Run());
        }

        private class LumaKeyerClipTestDefinition : LumaKeyerTestDefinition<double>
        {
            public LumaKeyerClipTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyLumaParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetClip(20);

            public override string PropertyName => "Clip";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;
            
            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestClip()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyLumaParameters>().ForEach(k => new LumaKeyerClipTestDefinition(helper, k).Run());
        }

        private class LumaKeyerGainTestDefinition : LumaKeyerTestDefinition<double>
        {
            public LumaKeyerGainTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyLumaParameters> key) : base(helper, key)
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
                GetKeyers<IBMDSwitcherKeyLumaParameters>().ForEach(k => new LumaKeyerGainTestDefinition(helper, k).Run());
        }

        private class LumaKeyerInvertKeyTestDefinition : LumaKeyerTestDefinition<bool>
        {
            public LumaKeyerInvertKeyTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyLumaParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInverse(0);

            public override string PropertyName => "Invert";
            public override bool MangleBadValue(bool v) => v;
        }

        [Fact]
        public void TestInvertKey()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyLumaParameters>().ForEach(k => new LumaKeyerInvertKeyTestDefinition(helper, k).Run());
        }
    }
}