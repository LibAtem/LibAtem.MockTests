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
    public class TestDVETransition : MixEffectsTestBase
    {
        public TestDVETransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        private abstract class DVETransitionTestDefinition<T> : TestDefinitionBase<TransitionDVESetCommand, T>
        {
            protected readonly MixEffectBlockId _id;
            protected readonly IBMDSwitcherTransitionDVEParameters _sdk;

            public DVETransitionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper)
            {
                _id = me.Item1;
                _sdk = me.Item2;
            }

            public override void SetupCommand(TransitionDVESetCommand cmd)
            {
                cmd.Index = _id;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                MixEffectState.TransitionDVEState obj = state.MixEffects[(int)_id].Transition.DVE;
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new TransitionDVEGetCommand() { Index = _id });
            }
        }

        private class DVETransitionRateTestDefinition : DVETransitionTestDefinition<uint>
        {
            public DVETransitionRateTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetRate(20);

            public override string PropertyName => "Rate";
            public override uint MangleBadValue(uint v) => v >= 250 ? 250 : (uint)1;

            public override uint[] GoodValues => new uint[] { 1, 18, 28, 95, 234, 244, 250 };
            public override uint[] BadValues => new uint[] { 251, 255, 0 };

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, uint v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);
                return new CommandQueueKey[0];
            }
        }

        [Fact]
        public void TestRate()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionDVEParameters>().ForEach(k => new DVETransitionRateTestDefinition(helper, k).Run());
        }

        private class DVETransitionLogoRateTestDefinition : DVETransitionTestDefinition<uint>
        {
            public DVETransitionLogoRateTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetLogoRate(20);

            public override string PropertyName => "LogoRate";
            public override uint MangleBadValue(uint v) => v >= 250 ? 250 : (uint)1;

            public override uint[] GoodValues => new uint[] { 1, 18, 28, 95, 234, 244, 250 };
            public override uint[] BadValues => new uint[] { 251, 255, 0 };

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, uint v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);
                return new CommandQueueKey[0];
            }
        }

        [Fact]
        public void TestLogoRate()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionDVEParameters>().ForEach(k => new DVETransitionLogoRateTestDefinition(helper, k).Run());
        }

        private class DVETransitionReverseTestDefinition : DVETransitionTestDefinition<bool>
        {
            public DVETransitionReverseTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetReverse(0);

            public override string PropertyName => "Reverse";
            public override bool MangleBadValue(bool v) => v;

            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                state.MixEffects[(int)_id].Transition.DVE.Reverse = v;
                state.MixEffects[(int)_id].Transition.Wipe.ReverseDirection = v;
            }
        }

        [Fact]
        public void TestReverse()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionDVEParameters>().ForEach(k => new DVETransitionReverseTestDefinition(helper, k).Run());
        }

        private class DVETransitionFlipFlopTestDefinition : DVETransitionTestDefinition<bool>
        {
            public DVETransitionFlipFlopTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetFlipFlop(0);

            public override string PropertyName => "FlipFlop";
            public override bool MangleBadValue(bool v) => v;

            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                state.MixEffects[(int)_id].Transition.DVE.FlipFlop = v;
                state.MixEffects[(int)_id].Transition.Wipe.FlipFlop = v;
            }
        }

        [Fact]
        public void TestFlipFlop()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionDVEParameters>().ForEach(k => new DVETransitionFlipFlopTestDefinition(helper, k).Run());
        }

        // TODO: GetStyle, DoesSupportStyle, GetNumSupportedStyles, GetSupportedStyle

        private class DVETransitionFillSourceTestDefinition : DVETransitionTestDefinition<VideoSource>
        {
            public DVETransitionFillSourceTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInputFill((long)VideoSource.ColorBars);

            public override string PropertyName => "FillSource";

            public override VideoSource[] GoodValues => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, InternalPortType.Mask) && s.IsAvailable(_id)).ToArray();
            
            public override VideoSource MangleBadValue(VideoSource v) => v;
            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                {
                    state.MixEffects[(int)_id].Transition.DVE.FillSource = v;
                    if (VideoSourceLists.MediaPlayers.Contains(v))
                        state.MixEffects[(int)_id].Transition.DVE.KeySource = v + 1;
                }
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);
                return new CommandQueueKey[0];
            }
        }

        [Fact]
        public void TestFillSource()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    me.Item2.GetFillInputAvailabilityMask(out _BMDSwitcherInputAvailability availability);
                    Assert.Equal((_BMDSwitcherInputAvailability)me.Item1 + 1, availability);

                    new DVETransitionFillSourceTestDefinition(helper, me).Run();
                }
            }
        }

        private class DVETransitionCutSourceTestDefinition : DVETransitionTestDefinition<VideoSource>
        {
            public DVETransitionCutSourceTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInputCut((long)VideoSource.ColorBars);

            public override string PropertyName => "KeySource";

            public override VideoSource[] GoodValues => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, InternalPortType.Mask) && s.IsAvailable(_id) && s.IsAvailable(SourceAvailability.KeySource)).ToArray();

            public override VideoSource MangleBadValue(VideoSource v) => v;
            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                    state.MixEffects[(int)_id].Transition.DVE.KeySource = v;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);
                return new CommandQueueKey[0];
            }
        }

        [Fact]
        public void TestCutSource()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    me.Item2.GetFillInputAvailabilityMask(out _BMDSwitcherInputAvailability availability);
                    Assert.Equal((_BMDSwitcherInputAvailability)me.Item1 + 1, availability);

                    new DVETransitionCutSourceTestDefinition(helper, me).Run();
                }
            }
        }

        private class DVETransitionEnableKeyTestDefinition : DVETransitionTestDefinition<bool>
        {
            public DVETransitionEnableKeyTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetEnableKey(0);

            public override string PropertyName => "EnableKey";
            public override bool MangleBadValue(bool v) => v;
        }

        [Fact]
        public void TestEnableKey()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionDVEParameters>().ForEach(k => new DVETransitionEnableKeyTestDefinition(helper, k).Run());
        }

        private class DVETransitionPreMultipliedTestDefinition : DVETransitionTestDefinition<bool>
        {
            public DVETransitionPreMultipliedTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetPreMultiplied(0);

            public override string PropertyName => "PreMultiplied";
            public override bool MangleBadValue(bool v) => v;

            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                state.MixEffects[(int)_id].Transition.DVE.PreMultiplied = v;
                state.MixEffects[(int)_id].Transition.Stinger.PreMultipliedKey = v;
            }
        }

        [Fact]
        public void TestPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionDVEParameters>().ForEach(k => new DVETransitionPreMultipliedTestDefinition(helper, k).Run());
        }

        private class DVETransitionClipTestDefinition : DVETransitionTestDefinition<double>
        {
            public DVETransitionClipTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetClip(20);

            public override string PropertyName => "Clip";
            public override double MangleBadValue(double v) => v;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };

            public override void UpdateExpectedState(AtemState state, bool goodValue, double v)
            {
                state.MixEffects[(int)_id].Transition.DVE.Clip = goodValue ? v : v > 100 ? 100 : 0;
                state.MixEffects[(int)_id].Transition.Stinger.Clip = goodValue ? v : v > 100 ? 100 : 0;
            }
        }

        [Fact]
        public void TestClip()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionDVEParameters>().ForEach(k => new DVETransitionClipTestDefinition(helper, k).Run());
        }

        private class DVETransitionGainTestDefinition : DVETransitionTestDefinition<double>
        {
            public DVETransitionGainTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetGain(20);

            public override string PropertyName => "Gain";
            public override double MangleBadValue(double v) => v;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            
            public override void UpdateExpectedState(AtemState state, bool goodValue, double v)
            {
                state.MixEffects[(int)_id].Transition.DVE.Gain = goodValue ? v : v > 100 ? 100 : 0;
                state.MixEffects[(int)_id].Transition.Stinger.Gain = goodValue ? v : v > 100 ? 100 : 0;
            }
        }

        [Fact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionDVEParameters>().ForEach(k => new DVETransitionGainTestDefinition(helper, k).Run());
        }

        private class DVETransitionInvertKeyTestDefinition : DVETransitionTestDefinition<bool>
        {
            public DVETransitionInvertKeyTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInverse(0);

            public override string PropertyName => "InvertKey";
            public override bool MangleBadValue(bool v) => v;

            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                state.MixEffects[(int)_id].Transition.DVE.InvertKey = v;
                state.MixEffects[(int)_id].Transition.Stinger.Invert = v;
            }
        }

        [Fact]
        public void TestInvertKey()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionDVEParameters>().ForEach(k => new DVETransitionInvertKeyTestDefinition(helper, k).Run());
        }
    }
}