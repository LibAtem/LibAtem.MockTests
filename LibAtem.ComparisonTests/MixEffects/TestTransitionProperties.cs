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
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestTransitionProperties : ComparisonTestBase
    {
        public TestTransitionProperties(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
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
                    ICommand Setter(TStyle v) => new TransitionPropertiesSetCommand()
                    {
                        Index = me.Item1,
                        Mask = TransitionPropertiesSetCommand.MaskFlags.Style,
                        Style = v,
                    };

                    void UpdateBothExpectedState(ComparisonState state, TStyle v)
                    {
                        state.MixEffects[me.Item1].Transition.Style = v;
                        state.MixEffects[me.Item1].Transition.NextStyle = v;
                    }

                    // Try and set each mode in turn
                    foreach (TStyle val in Enum.GetValues(typeof(TStyle)).OfType<TStyle>())
                    {
                        if (val.IsAvailable(helper.Profile))
                            ValueTypeComparer<TStyle>.Run(helper, Setter, UpdateBothExpectedState, val);
                        else
                            ValueTypeComparer<TStyle>.Fail(helper, Setter, val);
                    }

                    // Now run a mix transition, and ensure the props line up correctly
                    var sdkMix = GetMixEffect<IBMDSwitcherTransitionMixParameters>();
                    Assert.NotNull(sdkMix);
                    var sdkMe = GetMixEffect<IBMDSwitcherMixEffectBlock>();
                    Assert.NotNull(sdkMe);

                    me.Item2.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix);
                    sdkMix.SetRate(20);

                    sdkMe.PerformAutoTransition();
                    helper.Sleep();

                    try
                    {
                        void UpdateNextOnlyExpectedState(ComparisonState state, TStyle v) => state.MixEffects[me.Item1].Transition.NextStyle = v;
                        ValueTypeComparer<TStyle>.Run(helper, Setter, UpdateNextOnlyExpectedState, TStyle.Wipe);
                    }
                    finally
                    {
                        helper.Sleep(1000);
                    }

                    // Check it updated properly after the timeout
                    Assert.True(ComparisonStateComparer.AreEqual(Output, helper.LibState, helper.SdkState));
                }
            }
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

                    ICommand Setter(TransitionLayer v) => new TransitionPropertiesSetCommand()
                    {
                        Index = me.Item1,
                        Mask = TransitionPropertiesSetCommand.MaskFlags.Selection,
                        Selection = v,
                    };

                    void UpdateBothExpectedState(ComparisonState state, TransitionLayer v)
                    {
                        state.MixEffects[me.Item1].Transition.Selection = v;
                        state.MixEffects[me.Item1].Transition.NextSelection = v;
                    }

                    // Try and set each mode in turn
                    foreach (TransitionLayer val in EnumUtil.GetAllCombinations<TransitionLayer>())
                    {
                        if (val.IsAvailable(helper.Profile))
                            ValueTypeComparer<TransitionLayer>.Run(helper, Setter, UpdateBothExpectedState, val);
                        else
                            ValueTypeComparer<TransitionLayer>.Fail(helper, Setter, val);
                    }

                    // Clear the value, to ensure the below will change it
                    helper.SendCommand(Setter(TransitionLayer.Key1));

                    // Now run a mix transition, and ensure the props line up correctly
                    var sdkMix = GetMixEffect<IBMDSwitcherTransitionMixParameters>();
                    Assert.NotNull(sdkMix);
                    var sdkMe = GetMixEffect<IBMDSwitcherMixEffectBlock>();
                    Assert.NotNull(sdkMe);

                     me.Item2.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix);
                    sdkMix.SetRate(20);

                    sdkMe.PerformAutoTransition();
                    helper.Sleep();

                    try
                    {
                        void UpdateNextOnlyExpectedState(ComparisonState state, TransitionLayer v) => state.MixEffects[me.Item1].Transition.NextSelection = v;
                        ValueTypeComparer<TransitionLayer>.Run(helper, Setter, UpdateNextOnlyExpectedState, TransitionLayer.Background);
                    }
                    finally
                    {
                        helper.Sleep(1000);
                    }

                    // Check it updated properly after the timeout
                    Assert.True(ComparisonStateComparer.AreEqual(Output, helper.LibState, helper.SdkState));
                }
            }
        }

        // TODO - ensure trans cant be set to dve when keyer is
    }
}
