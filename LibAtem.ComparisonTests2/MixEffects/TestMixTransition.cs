using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
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
    public class TestMixTransition : MixEffectsTestBase
    {
        public TestMixTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        private class MixTransitionRateTestDefinition : TestDefinitionBase<uint>
        {
            private readonly MixEffectBlockId _id;
            private readonly IBMDSwitcherTransitionMixParameters _sdk;

            public MixTransitionRateTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionMixParameters> me) : base(helper)
            {
                _id = me.Item1;
                _sdk = me.Item2;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetRate(20);
            }

            public override uint[] GoodValues()
            {
                return new uint[] { 1, 18, 28, 95, 234, 244, 250 };
            }
            public override uint[] BadValues()
            {
                return new uint[] { 251, 255, 0 };
            }

            public override ICommand GenerateCommand(uint v)
            {
                return new TransitionMixSetCommand
                {
                    Index = _id,
                    Rate = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                if (goodValue)
                {
                    state.MixEffects[_id].Transition.Mix.Rate = v;
                }
                else
                {
                    state.MixEffects[_id].Transition.Mix.Rate = v >= 250 ? 250 : (uint)1;
                }
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, uint v)
            {
                yield return new CommandQueueKey(new TransitionMixGetCommand() { Index = _id });
            }
        }
        
        [Fact]
        public void TestRate()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionMixParameters>())
                {
                    new MixTransitionRateTestDefinition(helper, me).Run();
                }
            }
        }
    }
}