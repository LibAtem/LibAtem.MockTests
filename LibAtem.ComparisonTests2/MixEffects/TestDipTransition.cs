using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using LibAtem.DeviceProfile;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.MixEffects
{
    [Collection("Client")]
    public class TestDipTransition : MixEffectsTestBase
    {
        public TestDipTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        private abstract class DipTransitionTestDefinition<T> : TestDefinitionBase<T>
        {
            protected readonly MixEffectBlockId _id;
            protected readonly IBMDSwitcherTransitionDipParameters _sdk;

            public DipTransitionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDipParameters> me) : base(helper)
            {
                _id = me.Item1;
                _sdk = me.Item2;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new TransitionDipGetCommand()
                {
                    Index = _id,
                });
            }
        }

        private class DipTransitionRateTestDefinition : DipTransitionTestDefinition<uint>
        {
            public DipTransitionRateTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDipParameters> me) : base(helper, me)
            {
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
                return new TransitionDipSetCommand
                {
                    Index = _id,
                    Mask = TransitionDipSetCommand.MaskFlags.Rate,
                    Rate = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                if (goodValue)
                {
                    state.MixEffects[_id].Transition.Dip.Rate = v;
                }
                else
                {
                    state.MixEffects[_id].Transition.Dip.Rate = v >= 250 ? 250 : (uint)1;
                }
            }
        }

        [Fact]
        public void TestRate()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDipParameters>())
                {
                    new DipTransitionRateTestDefinition(helper, me).Run();
                }
            }
        }

        private class DipTransitionInputTestDefinition : DipTransitionTestDefinition<VideoSource>
        {
            public DipTransitionInputTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDipParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetRate(20);
            }

            public override VideoSource[] GoodValues()
            {
                return VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile) && s.IsAvailable(_id)).ToArray();
            }

            public override ICommand GenerateCommand(VideoSource v)
            {
                return new TransitionDipSetCommand
                {
                    Index = _id,
                    Mask = TransitionDipSetCommand.MaskFlags.Input,
                    Input = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                    state.MixEffects[_id].Transition.Dip.Input = v;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);

                return new CommandQueueKey[0];
            }
        }

        [Fact]
        public void TestInput()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDipParameters>())
                {
                    new DipTransitionInputTestDefinition(helper, me).Run();
                }
            }
        }
    }
}