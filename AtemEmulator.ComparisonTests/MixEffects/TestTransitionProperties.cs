using System;
using System.Collections.Generic;
using System.Linq;
using AtemEmulator.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.DeviceProfile;
using Xunit;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestTransitionProperties : ComparisonTestBase
    {
        private static readonly IReadOnlyDictionary<TStyle, _BMDSwitcherTransitionStyle> StyleMap;

        static TestTransitionProperties()
        {
            StyleMap = new Dictionary<TStyle, _BMDSwitcherTransitionStyle>
            {
                {TStyle.Mix, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix},
                {TStyle.Dip, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleDip},
                {TStyle.DVE, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleDVE},
                {TStyle.Stinger, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleStinger},
                {TStyle.Wipe, _BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleWipe},
            };
        }

        public TestTransitionProperties(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        [Fact]
        public void EnsureStyleMapIsComplete()
        {
            EnumMap.EnsureIsComplete(StyleMap);
        }

        [Fact]
        public void EnsureSelectionIsMapped()
        {
            EnumMap.EnsureIsMatching<TransitionLayer, _BMDSwitcherTransitionSelection>();
        }

        [Fact]
        public void TestTransitionStyle()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKey> key in GetKeyers<IBMDSwitcherKey>())
                {
                    key.Item3.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeLuma);
                    key.Item3.SetOnAir(0);
                }

                foreach (var me in GetMixEffects<IBMDSwitcherTransitionParameters>())
                {
                    ICommand Setter(TStyle v) => new TransitionPropertiesSetCommand()
                    {
                        Index = me.Item1,
                        Mask = TransitionPropertiesSetCommand.MaskFlags.Style,
                        Style = v,
                    };

                    TStyle? CurrentGetter() => helper.FindWithMatching(new TransitionPropertiesGetCommand {Index = me.Item1})?.Style;
                    TStyle? NextGetter() => helper.FindWithMatching(new TransitionPropertiesGetCommand {Index = me.Item1})?.NextStyle;

                    // Check current value
                    EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Run(helper, StyleMap, Setter, me.Item2.GetTransitionStyle, CurrentGetter);
                    EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Run(helper, StyleMap, null, me.Item2.GetNextTransitionStyle, NextGetter);
                    Assert.Equal(CurrentGetter(), NextGetter());

                    // Try and set each mode in turn
                    foreach (TStyle val in Enum.GetValues(typeof(TStyle)).OfType<TStyle>())
                    {
                        if (val.IsAvailable(helper.Profile))
                        {
                            EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Run(helper, StyleMap, Setter, me.Item2.GetTransitionStyle, CurrentGetter, val);
                            EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Run(helper, StyleMap, null, me.Item2.GetNextTransitionStyle, NextGetter, val);
                        }
                        else
                        {
                            EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Fail(helper, StyleMap, Setter, me.Item2.GetTransitionStyle, CurrentGetter, val);
                            EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Fail(helper, StyleMap, Setter, me.Item2.GetNextTransitionStyle, NextGetter, val);
                        }

                        Assert.Equal(CurrentGetter(), NextGetter());
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
                        EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Fail(helper, StyleMap, Setter, me.Item2.GetTransitionStyle, CurrentGetter, TStyle.Wipe);
                        EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Run(helper, StyleMap, null, me.Item2.GetNextTransitionStyle, NextGetter, TStyle.Wipe);
                    }
                    finally
                    {
                        helper.Sleep(1000);
                    }

                    // Check it updated properly after the timeout
                    Assert.Equal(CurrentGetter(), NextGetter());
                }
            }
        }

        [Fact]
        public void TestTransitionSelection()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionParameters>())
                {
                    // Ensure all keyers are not dve
                    List<IBMDSwitcherKey> keyers = GetKeyers<IBMDSwitcherKey>().Select(k => k.Item3).ToList();
                    foreach (var key in keyers)
                        key.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeLuma);

                    ICommand Setter(TransitionLayer v) => new TransitionPropertiesSetCommand()
                    {
                        Index = me.Item1,
                        Mask = TransitionPropertiesSetCommand.MaskFlags.Selection,
                        Selection = v,
                    };

                    TransitionLayer? CurrentGetter() => helper.FindWithMatching(new TransitionPropertiesGetCommand {Index = me.Item1})?.Selection;
                    TransitionLayer? NextGetter() => helper.FindWithMatching(new TransitionPropertiesGetCommand {Index =  me.Item1})?.NextSelection;

                    // Check current value
                    FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Run(helper, Setter,  me.Item2.GetTransitionSelection, CurrentGetter);
                    FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Run(helper, null,  me.Item2.GetNextTransitionSelection, NextGetter);
                    Assert.Equal(CurrentGetter(), NextGetter());

                    // Try and set each mode in turn
                    foreach (TransitionLayer val in EnumUtil.GetAllCombinations<TransitionLayer>())
                    {
                        if (val.IsAvailable(helper.Profile))
                        {
                            FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Run(helper, Setter,  me.Item2.GetTransitionSelection, CurrentGetter, val);
                            FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Run(helper, null,  me.Item2.GetNextTransitionSelection, NextGetter, val);
                        }
                        else
                        {
                            FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Fail(helper, Setter,  me.Item2.GetTransitionSelection, CurrentGetter, val);
                            FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Fail(helper, Setter,  me.Item2.GetNextTransitionSelection, NextGetter, val);
                        }

                        Assert.Equal(CurrentGetter(), NextGetter());
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
                        FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Fail(helper, Setter,  me.Item2.GetTransitionSelection, CurrentGetter, TransitionLayer.Background);
                        FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Run(helper, null,  me.Item2.GetNextTransitionSelection, NextGetter, TransitionLayer.Background);
                    }
                    finally
                    {
                        helper.Sleep(1000);
                    }

                    // Check it updated properly after the timeout
                    Assert.Equal(CurrentGetter(), NextGetter());
                }
            }
        }

        // TODO - ensure trans cant be set to dve when keyer is
    }
}
