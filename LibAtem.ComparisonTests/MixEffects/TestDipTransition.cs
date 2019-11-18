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
    public class TestDipTransition : MixEffectsTestBase
    {
        public TestDipTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        private abstract class DipTransitionTestDefinition<T> : TestDefinitionBase<TransitionDipSetCommand, T>
        {
            protected readonly MixEffectBlockId _id;
            protected readonly IBMDSwitcherTransitionDipParameters _sdk;

            public DipTransitionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDipParameters> me) : base(helper)
            {
                _id = me.Item1;
                _sdk = me.Item2;
            }

            public override void SetupCommand(TransitionDipSetCommand cmd)
            {
                cmd.Index = _id;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                MixEffectState.TransitionDipState obj = state.MixEffects[(int)_id].Transition.Dip;
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, T v)
            {
                yield return $"MixEffects.{_id:D}.Transition.Dip";
            }
        }

        private class DipTransitionRateTestDefinition : DipTransitionTestDefinition<uint>
        {
            public DipTransitionRateTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDipParameters> me) : base(helper, me)
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
                GetMixEffects<IBMDSwitcherTransitionDipParameters>().ForEach(k => new DipTransitionRateTestDefinition(helper, k).Run());

        }

        private class DipTransitionInputTestDefinition : DipTransitionTestDefinition<VideoSource>
        {
            public DipTransitionInputTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDipParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInputDip((long)VideoSource.ColorBars);

            public override string PropertyName => "Input";
            public override VideoSource MangleBadValue(VideoSource v) => v;

            public override VideoSource[] GoodValues => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile) && s.IsAvailable(_id)).ToArray();

            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                    base.UpdateExpectedState(state, goodValue, v);
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);

                return new string[0];
            }
        }

        [Fact]
        public void TestInput()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionDipParameters>().ForEach(k => new DipTransitionInputTestDefinition(helper, k).Run());
        }
    }
}