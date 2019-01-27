using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
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
    public class TestDVETransition : MixEffectsTestBase
    {
        public TestDVETransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        private abstract class DVETransitionTestDefinition<T> : TestDefinitionBase<T>
        {
            protected readonly MixEffectBlockId _id;
            protected readonly IBMDSwitcherTransitionDVEParameters _sdk;

            public DVETransitionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper)
            {
                _id = me.Item1;
                _sdk = me.Item2;
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
                return new TransitionDVESetCommand
                {
                    Index = _id,
                    Mask = TransitionDVESetCommand.MaskFlags.Rate,
                    Rate = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                if (goodValue)
                    state.MixEffects[_id].Transition.DVE.Rate = v;
                else
                    state.MixEffects[_id].Transition.DVE.Rate = v >= 250 ? 250 : (uint)1;
            }

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
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    new DVETransitionRateTestDefinition(helper, me).Run();
                }
            }
        }

        private class DVETransitionLogoRateTestDefinition : DVETransitionTestDefinition<uint>
        {
            public DVETransitionLogoRateTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetLogoRate(20);
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
                return new TransitionDVESetCommand
                {
                    Index = _id,
                    Mask = TransitionDVESetCommand.MaskFlags.LogoRate,
                    LogoRate = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                if (goodValue)
                    state.MixEffects[_id].Transition.DVE.LogoRate = v;
                else
                    state.MixEffects[_id].Transition.DVE.LogoRate = v >= 250 ? 250 : (uint)1;
            }

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
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    new DVETransitionLogoRateTestDefinition(helper, me).Run();
                }
            }
        }

        private class DVETransitionReverseTestDefinition : DVETransitionTestDefinition<bool>
        {
            public DVETransitionReverseTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetReverse(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new TransitionDVESetCommand
                {
                    Index = _id,
                    Mask = TransitionDVESetCommand.MaskFlags.Reverse,
                    Reverse = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_id].Transition.DVE.Reverse = v;
                state.MixEffects[_id].Transition.Wipe.ReverseDirection = v;
            }
        }

        [Fact]
        public void TestReverse()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    new DVETransitionReverseTestDefinition(helper, me).Run();
                }
            }
        }

        private class DVETransitionFlipFlopTestDefinition : DVETransitionTestDefinition<bool>
        {
            public DVETransitionFlipFlopTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetFlipFlop(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new TransitionDVESetCommand
                {
                    Index = _id,
                    Mask = TransitionDVESetCommand.MaskFlags.FlipFlop,
                    FlipFlop = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_id].Transition.DVE.FlipFlop = v;
                state.MixEffects[_id].Transition.Wipe.FlipFlop = v;
            }
        }

        [Fact]
        public void TestFlipFlop()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    new DVETransitionFlipFlopTestDefinition(helper, me).Run();
                }
            }
        }

        // TODO: GetStyle, DoesSupportStyle, GetNumSupportedStyles, GetSupportedStyle

        private class DVETransitionFillSourceTestDefinition : DVETransitionTestDefinition<VideoSource>
        {
            public DVETransitionFillSourceTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInputFill((long)VideoSource.ColorBars);
            }

            public override VideoSource[] GoodValues()
            {
                return VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, InternalPortType.Mask) && s.IsAvailable(_id)).ToArray();
            }

            public override ICommand GenerateCommand(VideoSource v)
            {
                return new TransitionDVESetCommand
                {
                    Index = _id,
                    Mask = TransitionDVESetCommand.MaskFlags.FillSource,
                    FillSource = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                {
                    state.MixEffects[_id].Transition.DVE.FillSource = v;
                    if (VideoSourceLists.MediaPlayers.Contains(v))
                        state.MixEffects[_id].Transition.DVE.KeySource = v + 1;
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
                    _BMDSwitcherInputAvailability availability = 0;
                    me.Item2.GetFillInputAvailabilityMask(ref availability);
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

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInputCut((long)VideoSource.ColorBars);
            }

            public override VideoSource[] GoodValues()
            {
                return VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, InternalPortType.Mask) && s.IsAvailable(_id) && s.IsAvailable(SourceAvailability.KeySource)).ToArray();
            }

            public override ICommand GenerateCommand(VideoSource v)
            {
                return new TransitionDVESetCommand
                {
                    Index = _id,
                    Mask = TransitionDVESetCommand.MaskFlags.KeySource,
                    KeySource = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                    state.MixEffects[_id].Transition.DVE.KeySource = v;
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
                    _BMDSwitcherInputAvailability availability = 0;
                    me.Item2.GetFillInputAvailabilityMask(ref availability);
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

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetEnableKey(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new TransitionDVESetCommand
                {
                    Index = _id,
                    Mask = TransitionDVESetCommand.MaskFlags.EnableKey,
                    EnableKey = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_id].Transition.DVE.EnableKey = v;
            }
        }

        [Fact]
        public void TestEnableKey()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    new DVETransitionEnableKeyTestDefinition(helper, me).Run();
                }
            }
        }

        private class DVETransitionPreMultipliedTestDefinition : DVETransitionTestDefinition<bool>
        {
            public DVETransitionPreMultipliedTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetPreMultiplied(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new TransitionDVESetCommand
                {
                    Index = _id,
                    Mask = TransitionDVESetCommand.MaskFlags.PreMultiplied,
                    PreMultiplied = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_id].Transition.DVE.PreMultiplied = v;
                state.MixEffects[_id].Transition.Stinger.PreMultipliedKey = v;
            }
        }

        [Fact]
        public void TestPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    new DVETransitionPreMultipliedTestDefinition(helper, me).Run();
                }
            }
        }

        private class DVETransitionClipTestDefinition : DVETransitionTestDefinition<double>
        {
            public DVETransitionClipTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetClip(20);
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            }
            public override double[] BadValues()
            {
                return new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            }

            public override ICommand GenerateCommand(double v)
            {
                return new TransitionDVESetCommand
                {
                    Index = _id,
                    Mask = TransitionDVESetCommand.MaskFlags.Clip,
                    Clip = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.MixEffects[_id].Transition.DVE.Clip = v;
                    state.MixEffects[_id].Transition.Stinger.Clip = v;
                }
                else
                {
                    state.MixEffects[_id].Transition.DVE.Clip = v > 100 ? 100 : 0;
                    state.MixEffects[_id].Transition.Stinger.Clip = v > 100 ? 100 : 0;
                }
            }
        }

        [Fact]
        public void TestClip()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    new DVETransitionClipTestDefinition(helper, me).Run();
                }
            }
        }

        private class DVETransitionGainTestDefinition : DVETransitionTestDefinition<double>
        {
            public DVETransitionGainTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetGain(20);
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            }
            public override double[] BadValues()
            {
                return new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            }

            public override ICommand GenerateCommand(double v)
            {
                return new TransitionDVESetCommand
                {
                    Index = _id,
                    Mask = TransitionDVESetCommand.MaskFlags.Gain,
                    Gain = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.MixEffects[_id].Transition.DVE.Gain = v;
                    state.MixEffects[_id].Transition.Stinger.Gain = v;
                }
                else
                {
                    state.MixEffects[_id].Transition.DVE.Gain = v > 100 ? 100 : 0;
                    state.MixEffects[_id].Transition.Stinger.Gain = v > 100 ? 100 : 0;
                }
            }
        }

        [Fact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    new DVETransitionGainTestDefinition(helper, me).Run();
                }
            }
        }

        private class DVETransitionInvertKeyTestDefinition : DVETransitionTestDefinition<bool>
        {
            public DVETransitionInvertKeyTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionDVEParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInverse(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new TransitionDVESetCommand
                {
                    Index = _id,
                    Mask = TransitionDVESetCommand.MaskFlags.InvertKey,
                    InvertKey = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_id].Transition.DVE.InvertKey = v;
                state.MixEffects[_id].Transition.Stinger.Invert = v;
            }
        }

        [Fact]
        public void TestInvertKey()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionDVEParameters>())
                {
                    new DVETransitionInvertKeyTestDefinition(helper, me).Run();
                }
            }
        }
    }
}