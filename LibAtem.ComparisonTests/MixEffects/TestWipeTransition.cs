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
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.MixEffects
{
    [Collection("Client")]
    public class TestWipeTransition : MixEffectsTestBase
    {
        public TestWipeTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        private abstract class WipeTransitionTestDefinition<T> : TestDefinitionBase<TransitionWipeSetCommand, T>
        {
            protected readonly MixEffectBlockId _id;
            protected readonly IBMDSwitcherTransitionWipeParameters _sdk;

            public WipeTransitionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper)
            {
                _id = me.Item1;
                _sdk = me.Item2;
            }

            public override void SetupCommand(TransitionWipeSetCommand cmd)
            {
                cmd.Index = _id;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                MixEffectState.TransitionWipeState obj = state.MixEffects[(int)_id].Transition.Wipe;
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }


            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new TransitionWipeGetCommand() { Index = _id });
            }
        }

        private class WipeTransitionRateTestDefinition : WipeTransitionTestDefinition<uint>
        {
            public WipeTransitionRateTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
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
                GetMixEffects<IBMDSwitcherTransitionWipeParameters>().ForEach(k => new WipeTransitionRateTestDefinition(helper, k).Run());
        }

        private class WipeTransitionPatternTestDefinition : WipeTransitionTestDefinition<Pattern>
        {
            public WipeTransitionPatternTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetRate(20);

            public override string PropertyName => "Pattern";
            public override Pattern MangleBadValue(Pattern v) => v;

            public override Pattern[] GoodValues => Enum.GetValues(typeof(Pattern)).OfType<Pattern>().ToArray();

            public override void UpdateExpectedState(AtemState state, bool goodValue, Pattern v)
            {
                var props = state.MixEffects[(int)_id].Transition.Wipe;
                props.Pattern = v;
                props.XPosition = 0.5;
                props.YPosition = 0.5;
                props.Symmetry = v.GetDefaultPatternSymmetry();
            }
        }

        [Fact]
        public void TestPattern()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionWipeParameters>().ForEach(k => new WipeTransitionPatternTestDefinition(helper, k).Run());
        }

        private class WipeTransitionBorderSizeTestDefinition : WipeTransitionTestDefinition<double>
        {
            public WipeTransitionBorderSizeTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderSize(20);

            public override string PropertyName => "BorderWidth";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.01 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestBorderSize()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionWipeParameters>().ForEach(k => new WipeTransitionBorderSizeTestDefinition(helper, k).Run());
        }

        private class WipeTransitionBorderInputTestDefinition : WipeTransitionTestDefinition<VideoSource>
        {
            public WipeTransitionBorderInputTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInputBorder((long)VideoSource.ColorBars);

            public override string PropertyName => "BorderInput";
            public override VideoSource MangleBadValue(VideoSource v) => v;

            public override VideoSource[] GoodValues => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile) && s.IsAvailable(_id)).ToArray();
            
            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                    state.MixEffects[(int)_id].Transition.Wipe.BorderInput = v;
            }
        }

        [Fact]
        public void TestBorderInput()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionWipeParameters>().ForEach(k => new WipeTransitionBorderInputTestDefinition(helper, k).Run());
        }

        private class WipeTransitionSymmetryTestDefinition : WipeTransitionTestDefinition<double>
        {
            public WipeTransitionSymmetryTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris);
                _sdk.SetSymmetry(20);
                _helper.Sleep();
            }

            public override string PropertyName => "Symmetry";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.01 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestSymmetry()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionWipeParameters>().ForEach(k => new WipeTransitionSymmetryTestDefinition(helper, k).Run());
        }

        private class WipeTransitionSoftnessTestDefinition : WipeTransitionTestDefinition<double>
        {
            public WipeTransitionSoftnessTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetSoftness(20);

            public override string PropertyName => "Symmetry";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.01 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionWipeParameters>().ForEach(k => new WipeTransitionSoftnessTestDefinition(helper, k).Run());
        }

        private class WipeTransitionHorizontalOffsetTestDefinition : WipeTransitionTestDefinition<double>
        {
            public WipeTransitionHorizontalOffsetTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris);
                _sdk.SetHorizontalOffset(0.5);
                _helper.Sleep();
            }

            public override string PropertyName => "XPosition";
            public override double MangleBadValue(double v) => v >= 1 ? 1 : 0;

            public override double[] GoodValues => new double[] { 0, 0.874, 0.147, 0.999, 1.00, 0.01 };
            public override double[] BadValues => new double[] { 1.001, 1.1, 1.01, -0.01, -1, -0.10 };
        }

        [Fact]
        public void TestHorizontalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionWipeParameters>().ForEach(k => new WipeTransitionHorizontalOffsetTestDefinition(helper, k).Run());
        }

        private class WipeTransitionVerticalOffsetTestDefinition : WipeTransitionTestDefinition<double>
        {
            public WipeTransitionVerticalOffsetTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleCircleIris);
                _sdk.SetVerticalOffset(0.5);
                _helper.Sleep();
            }

            public override string PropertyName => "YPosition";
            public override double MangleBadValue(double v) => v >= 1 ? 1 : 0;

            public override double[] GoodValues => new double[] { 0, 0.874, 0.147, 0.999, 1.00, 0.01 };
            public override double[] BadValues => new double[] { 1.001, 1.1, 1.01, -0.01, -1, -0.10 };
        }

        [Fact]
        public void TestVerticalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionWipeParameters>().ForEach(k => new WipeTransitionVerticalOffsetTestDefinition(helper, k).Run());
        }

        private class WipeTransitionReverseTestDefinition : WipeTransitionTestDefinition<bool>
        {
            public WipeTransitionReverseTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetReverse(0);

            public override string PropertyName => "ReverseDirection";
            public override bool MangleBadValue(bool v) => v;

            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                state.MixEffects[(int)_id].Transition.Wipe.ReverseDirection = v;
                state.MixEffects[(int)_id].Transition.DVE.Reverse = v;
            }
        }

        [Fact]
        public void TestReverse()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionWipeParameters>().ForEach(k => new WipeTransitionReverseTestDefinition(helper, k).Run());
        }

        private class WipeTransitionFlipFlopTestDefinition : WipeTransitionTestDefinition<bool>
        {
            public WipeTransitionFlipFlopTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetFlipFlop(0);

            public override string PropertyName => "FlipFlop";
            public override bool MangleBadValue(bool v) => v;

            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                state.MixEffects[(int)_id].Transition.Wipe.FlipFlop = v;
                state.MixEffects[(int)_id].Transition.DVE.FlipFlop = v;
            }
        }

        [Fact]
        public void TestFlipFlop()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionWipeParameters>().ForEach(k => new WipeTransitionFlipFlopTestDefinition(helper, k).Run());
        }
    }
}