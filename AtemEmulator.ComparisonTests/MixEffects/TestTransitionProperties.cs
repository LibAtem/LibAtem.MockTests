using System;
using System.Collections.Generic;
using System.Linq;
using AtemEmulator.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.DeviceProfile;
using LibAtem.Util;
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
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionParameters>();
                Assert.NotNull(sdkProps);
                
                ICommand Setter(TStyle v) => new TransitionPropertiesSetCommand()
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionPropertiesSetCommand.MaskFlags.Style,
                    Style = v,
                };

                TStyle? CurrentGetter() => helper.FindWithMatching(new TransitionPropertiesGetCommand { Index = MixEffectBlockId.One })?.Style;
                TStyle? NextGetter() => helper.FindWithMatching(new TransitionPropertiesGetCommand { Index = MixEffectBlockId.One })?.NextStyle;

                // Check current value
                EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Run(helper, StyleMap, Setter, sdkProps.GetTransitionStyle, CurrentGetter);
                EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Run(helper, StyleMap, null, sdkProps.GetNextTransitionStyle, NextGetter);
                Assert.Equal(CurrentGetter(), NextGetter());

                // Try and set each mode in turn
                foreach (TStyle val in Enum.GetValues(typeof(TStyle)).OfType<TStyle>())
                {
                    if (val.IsAvailable(helper.Profile))
                    {
                        EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Run(helper, StyleMap, Setter, sdkProps.GetTransitionStyle, CurrentGetter, val);
                        EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Run(helper, StyleMap, null, sdkProps.GetNextTransitionStyle, NextGetter, val);
                    }
                    else
                    {
                        EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Fail(helper, StyleMap, Setter, sdkProps.GetTransitionStyle, CurrentGetter, val);
                        EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Fail(helper, StyleMap, Setter, sdkProps.GetNextTransitionStyle, NextGetter, val);
                    }

                    Assert.Equal(CurrentGetter(), NextGetter());
                }

                // Now run a mix transition, and ensure the props line up correctly
                var sdkMix = GetMixEffect<IBMDSwitcherTransitionMixParameters>();
                Assert.NotNull(sdkMix);
                var sdkMe = GetMixEffect<IBMDSwitcherMixEffectBlock>();
                Assert.NotNull(sdkMe);

                sdkProps.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix);
                sdkMix.SetRate(20);

                sdkMe.PerformAutoTransition();
                helper.Sleep();

                try
                {
                    EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Fail(helper, StyleMap, Setter, sdkProps.GetTransitionStyle, CurrentGetter, TStyle.Wipe);
                    EnumValueComparer<TStyle, _BMDSwitcherTransitionStyle>.Run(helper, StyleMap, null, sdkProps.GetNextTransitionStyle, NextGetter, TStyle.Wipe);
                }
                finally
                {
                    helper.Sleep(1000);
                }

                // Check it updated properly after the timeout
                Assert.Equal(CurrentGetter(), NextGetter());
            }
        }

        [Fact]
        public void TestTransitionSelection()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                var sdkProps = GetMixEffect<IBMDSwitcherTransitionParameters>();
                Assert.NotNull(sdkProps);

                // Ensure all keyers are not dve
                List<IBMDSwitcherKey> keyers = GetKeyers<IBMDSwitcherKey>();
                foreach (var key in keyers)
                    key.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeLuma);

                ICommand Setter(TransitionLayer v) => new TransitionPropertiesSetCommand()
                {
                    Index = MixEffectBlockId.One,
                    Mask = TransitionPropertiesSetCommand.MaskFlags.Selection,
                    Selection = v,
                };

                TransitionLayer? CurrentGetter() => helper.FindWithMatching(new TransitionPropertiesGetCommand { Index = MixEffectBlockId.One })?.Selection;
                TransitionLayer? NextGetter() => helper.FindWithMatching(new TransitionPropertiesGetCommand { Index = MixEffectBlockId.One })?.NextSelection;

                // Check current value
                FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Run(helper, Setter, sdkProps.GetTransitionSelection, CurrentGetter);
                FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Run(helper, null, sdkProps.GetNextTransitionSelection, NextGetter);
                Assert.Equal(CurrentGetter(), NextGetter());

                // Try and set each mode in turn
                foreach (TransitionLayer val in EnumUtil.GetAllCombinations<TransitionLayer>())
                {
                    if (val.IsAvailable(helper.Profile))
                    {
                        FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Run(helper, Setter, sdkProps.GetTransitionSelection, CurrentGetter, val);
                        FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Run(helper, null, sdkProps.GetNextTransitionSelection, NextGetter, val);
                    }
                    else
                    {
                        FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Fail(helper, Setter, sdkProps.GetTransitionSelection, CurrentGetter, val);
                        FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Fail(helper, Setter, sdkProps.GetNextTransitionSelection, NextGetter, val);
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

                sdkProps.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix);
                sdkMix.SetRate(20);

                sdkMe.PerformAutoTransition();
                helper.Sleep();

                try
                {
                    FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Fail(helper, Setter, sdkProps.GetTransitionSelection, CurrentGetter, TransitionLayer.Background);
                    FlagsValueComparer<TransitionLayer, _BMDSwitcherTransitionSelection>.Run(helper, null, sdkProps.GetNextTransitionSelection, NextGetter, TransitionLayer.Background);
                }
                finally
                {
                    helper.Sleep(1000);
                }

                // Check it updated properly after the timeout
                Assert.Equal(CurrentGetter(), NextGetter());
            }
        }

        // TODO - ensure trans cant be set to dve when keyer is
    }
}
