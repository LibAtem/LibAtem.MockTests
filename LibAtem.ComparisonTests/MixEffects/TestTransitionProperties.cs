using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestTransitionProperties : MixEffectsTestBase
    {
        public TestTransitionProperties(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        private abstract class TransitionPropertiesTestDefinition<T> : TestDefinitionBase<TransitionPropertiesSetCommand, T>
        {
            protected readonly MixEffectBlockId _meId;
            protected readonly IBMDSwitcherTransitionParameters _sdk;

            public TransitionPropertiesTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionParameters> key) : base(helper)
            {
                _meId = key.Item1;
                _sdk = key.Item2;
            }

            public override void SetupCommand(TransitionPropertiesSetCommand cmd)
            {
                cmd.Index = _meId;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new TransitionPropertiesGetCommand() { Index = _meId});
            }
        }

        private class TransitionPropertiesStyleTestDefinition : TransitionPropertiesTestDefinition<TStyle>
        {
            private readonly bool _inTransition;

            public TransitionPropertiesStyleTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionParameters> me, bool inTransition) : base(helper, me)
            {
                _inTransition = inTransition;
            }

            public override void Prepare()
            {
                _sdk.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleWipe);
            }

            public override string PropertyName => "Style";

            public override void UpdateExpectedState(AtemState state, bool goodValue, TStyle v)
            {
                if (goodValue)
                {
                    MixEffectState.TransitionState obj = state.MixEffects[(int)_meId].Transition;

                    if (!_inTransition) SetCommandProperty(obj, "Style", v);
                    SetCommandProperty(obj, "NextStyle", v);
                }
            }

            public override TStyle[] GoodValues => Enum.GetValues(typeof(TStyle)).OfType<TStyle>().Where(s => s.IsAvailable(_helper.Profile)).ToArray();
            public override TStyle[] BadValues => Enum.GetValues(typeof(TStyle)).OfType<TStyle>().Except(GoodValues).ToArray();
        }

        [Fact]
        public void TestTransitionStyle()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key in GetKeyers<IBMDSwitcherKey>())
                {
                    key.Item3.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeLuma);
                    key.Item3.SetOnAir(0);
                }
                helper.Sleep();

                foreach (var me in GetMixEffects<IBMDSwitcherTransitionParameters>())
                {
                    new TransitionPropertiesStyleTestDefinition(helper, me, false).Run();

                    // Now run a mix transition, and ensure the props line up correctly
                    var sdkMix = me.Item2 as IBMDSwitcherTransitionMixParameters;
                    Assert.NotNull(sdkMix);
                    var sdkMe = me.Item2 as IBMDSwitcherMixEffectBlock;
                    Assert.NotNull(sdkMe);

                    me.Item2.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix);
                    sdkMix.SetRate(20);

                    sdkMe.PerformAutoTransition();
                    helper.Sleep();

                    try
                    {
                        new TransitionPropertiesStyleTestDefinition(helper, me, true).RunSingle(TStyle.Wipe);
                    }
                    finally
                    {
                        helper.Sleep(1000);
                    }

                    // Check it updated properly after the timeout
                    Assert.True(AtemStateComparer.AreEqual(Output, helper.LibState, helper.SdkState));
                }
            }
        }

        private class TransitionPropertiesSelectionTestDefinition : TransitionPropertiesTestDefinition<TransitionLayer>
        {
            private readonly bool _inTransition;

            public TransitionPropertiesSelectionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionParameters> me, bool inTransition) : base(helper, me)
            {
                _inTransition = inTransition;
            }

            public override void Prepare()
            {
                _sdk.SetNextTransitionSelection(_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1);
            }

            public override string PropertyName => "Selection";

            public override void UpdateExpectedState(AtemState state, bool goodValue, TransitionLayer v)
            {
                MixEffectState.TransitionState obj = state.MixEffects[(int)_meId].Transition;

                if (goodValue)
                {
                    if (!_inTransition) SetCommandProperty(obj, "Selection", v);
                    SetCommandProperty(obj, "NextSelection", v);
                }
                else
                {
                    foreach (TransitionLayer i in BadValues)
                        v &= ~i;

                    if (v != 0)
                    {
                        if (!_inTransition) SetCommandProperty(obj, "Selection", v);
                        SetCommandProperty(obj, "NextSelection", v);
                    }
                }
            }

            public override TransitionLayer[] GoodValues => EnumUtil.GetAllCombinations<TransitionLayer>().Where(s => s.IsAvailable(_helper.Profile)).ToArray();
            public override TransitionLayer[] BadValues => EnumUtil.GetAllCombinations<TransitionLayer>().Except(GoodValues).ToArray();
        }

        [Fact]
        public void TestTransitionSelection()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionParameters>())
                {
                    // Ensure all keyers are not dve
                    List<IBMDSwitcherKey> keyers = GetKeyers<IBMDSwitcherKey>().Select(k => k.Item3).ToList();
                    foreach (var key in keyers)
                        key.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeLuma);
                    helper.Sleep();

                    new TransitionPropertiesSelectionTestDefinition(helper, me, false).Run();

                    // Clear the value, to ensure the below will change it
                    me.Item2.SetNextTransitionSelection(_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1);

                    // Now run a mix transition, and ensure the props line up correctly
                    var sdkMix = me.Item2 as IBMDSwitcherTransitionMixParameters;
                    Assert.NotNull(sdkMix);
                    var sdkMe = me.Item2 as IBMDSwitcherMixEffectBlock;
                    Assert.NotNull(sdkMe);

                     me.Item2.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix);
                    sdkMix.SetRate(20);

                    sdkMe.PerformAutoTransition();
                    helper.Sleep();

                    try
                    {
                        new TransitionPropertiesSelectionTestDefinition(helper, me, true).RunSingle(TransitionLayer.Background);
                    }
                    finally
                    {
                        helper.Sleep(1000);
                    }

                    // Check it updated properly after the timeout
                    Assert.True(AtemStateComparer.AreEqual(Output, helper.LibState, helper.SdkState));
                }
            }
        }

        // TODO - ensure trans cant be set to dve when keyer is
    }
}
