using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using LibAtem.State;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.MixEffects
{
    [Collection("Client")]
    public class TestMixTransition : MixEffectsTestBase
    {
        public TestMixTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        private abstract class MixTransitionTestDefinition<T> : TestDefinitionBase<TransitionMixSetCommand, T>
        {
            private readonly MixEffectBlockId _meId;
            protected readonly IBMDSwitcherTransitionMixParameters _sdk;

            public MixTransitionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionMixParameters> me) : base(helper)
            {
                _meId = me.Item1;
                _sdk = me.Item2;
            }

            public override void SetupCommand(TransitionMixSetCommand cmd)
            {
                cmd.Index = _meId;
            }

            public abstract T MangleBadValue(T v);

            public sealed override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                MixEffectState.TransitionMixState obj = state.MixEffects[(int)_meId].Transition.Mix;
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new TransitionMixGetCommand() { Index = _meId });
            }
        }

        private class MixTransitionRateTestDefinition : MixTransitionTestDefinition<uint>
        {
            public MixTransitionRateTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionMixParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetRate(20);

            public override string PropertyName => "Rate";
            public override uint MangleBadValue(uint v) => v >= 250 ? 250 : (uint)1;

            public override uint[] GoodValues => new uint[] { 1, 18, 28, 95, 234, 244, 250 };
            public override uint[] BadValues => new uint[] { 251, 255, 0 };
        }
        
        [Fact]
        public void TestRate()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionMixParameters>().ForEach(k => new MixTransitionRateTestDefinition(helper, k).Run());
        }
    }
}