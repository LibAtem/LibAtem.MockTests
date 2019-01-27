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
    public class TestWipeTransition : MixEffectsTestBase
    {
        public TestWipeTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        private abstract class WipeTransitionTestDefinition<T> : TestDefinitionBase<T>
        {
            protected readonly MixEffectBlockId _id;
            protected readonly IBMDSwitcherTransitionWipeParameters _sdk;

            public WipeTransitionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper)
            {
                _id = me.Item1;
                _sdk = me.Item2;
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
                return new TransitionWipeSetCommand
                {
                    Index = _id,
                    Mask = TransitionWipeSetCommand.MaskFlags.Rate,
                    Rate = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                if (goodValue)
                    state.MixEffects[_id].Transition.Wipe.Rate = v;
                else
                    state.MixEffects[_id].Transition.Wipe.Rate = v >= 250 ? 250 : (uint)1;
            }
        }

        [Fact]
        public void TestRate()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    new WipeTransitionRateTestDefinition(helper, me).Run();
                }
            }
        }

        private class WipeTransitionPatternTestDefinition : WipeTransitionTestDefinition<Pattern>
        {
            public WipeTransitionPatternTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetRate(20);
            }

            public override Pattern[] GoodValues()
            {
                return Enum.GetValues(typeof(Pattern)).OfType<Pattern>().ToArray();
            }

            public override ICommand GenerateCommand(Pattern v)
            {
                return new TransitionWipeSetCommand
                {
                    Index = _id,
                    Mask = TransitionWipeSetCommand.MaskFlags.Pattern,
                    Pattern = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, Pattern v)
            {
                var props = state.MixEffects[_id].Transition.Wipe;
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
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    new WipeTransitionPatternTestDefinition(helper, me).Run();
                }
            }
        }

        private class WipeTransitionBorderSizeTestDefinition : WipeTransitionTestDefinition<double>
        {
            public WipeTransitionBorderSizeTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderSize(20);
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 87.4, 14.7, 99.9, 100, 0.01 };
            }
            public override double[] BadValues()
            {
                return new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            }

            public override ICommand GenerateCommand(double v)
            {
                return new TransitionWipeSetCommand
                {
                    Index = _id,
                    Mask = TransitionWipeSetCommand.MaskFlags.BorderWidth,
                    BorderWidth = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_id].Transition.Wipe.BorderWidth = v;
                else
                    state.MixEffects[_id].Transition.Wipe.BorderWidth = v >= 100 ? 100 : 0;
            }
        }

        [Fact]
        public void TestBorderSize()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    new WipeTransitionBorderSizeTestDefinition(helper, me).Run();
                }
            }
        }

        private class WipeTransitionBorderInputTestDefinition : WipeTransitionTestDefinition<VideoSource>
        {
            public WipeTransitionBorderInputTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInputBorder((long)VideoSource.ColorBars);
            }

            public override VideoSource[] GoodValues()
            {
                return VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile) && s.IsAvailable(_id)).ToArray();
            }

            public override ICommand GenerateCommand(VideoSource v)
            {
                return new TransitionWipeSetCommand
                {
                    Index = _id,
                    Mask = TransitionWipeSetCommand.MaskFlags.BorderInput,
                    BorderInput = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                    state.MixEffects[_id].Transition.Wipe.BorderInput = v;
            }
        }

        [Fact]
        public void TestBorderInput()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    new WipeTransitionBorderInputTestDefinition(helper, me).Run();
                }
            }
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

            public override double[] GoodValues()
            {
                return new double[] { 0, 87.4, 14.7, 99.9, 100, 0.01 };
            }
            public override double[] BadValues()
            {
                return new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            }

            public override ICommand GenerateCommand(double v)
            {
                return new TransitionWipeSetCommand
                {
                    Index = _id,
                    Mask = TransitionWipeSetCommand.MaskFlags.Symmetry,
                    Symmetry = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_id].Transition.Wipe.Symmetry = v;
                else
                    state.MixEffects[_id].Transition.Wipe.Symmetry = v >= 100 ? 100 : 0;
            }
        }

        [Fact]
        public void TestSymmetry()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    new WipeTransitionSymmetryTestDefinition(helper, me).Run();
                }
            }
        }

        private class WipeTransitionSoftnessTestDefinition : WipeTransitionTestDefinition<double>
        {
            public WipeTransitionSoftnessTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetSoftness(20);
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 87.4, 14.7, 99.9, 100, 0.01 };
            }
            public override double[] BadValues()
            {
                return new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            }

            public override ICommand GenerateCommand(double v)
            {
                return new TransitionWipeSetCommand
                {
                    Index = _id,
                    Mask = TransitionWipeSetCommand.MaskFlags.BorderSoftness,
                    BorderSoftness = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_id].Transition.Wipe.BorderSoftness = v;
                else
                    state.MixEffects[_id].Transition.Wipe.BorderSoftness = v >= 100 ? 100 : 0;
            }
        }

        [Fact]
        public void TestSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    new WipeTransitionSoftnessTestDefinition(helper, me).Run();
                }
            }
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

            public override double[] GoodValues()
            {
                return new double[] { 0, 0.874, 0.147, 0.999, 1.00, 0.01 };
            }
            public override double[] BadValues()
            {
                return new double[] { 1.001, 1.1, 1.01, -0.01, -1, -0.10 };
            }

            public override ICommand GenerateCommand(double v)
            {
                return new TransitionWipeSetCommand
                {
                    Index = _id,
                    Mask = TransitionWipeSetCommand.MaskFlags.XPosition,
                    XPosition = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_id].Transition.Wipe.XPosition = v;
                else
                    state.MixEffects[_id].Transition.Wipe.XPosition = v >= 1 ? 1 : 0;
            }
        }

        [Fact]
        public void TestHorizontalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    new WipeTransitionHorizontalOffsetTestDefinition(helper, me).Run();
                }
            }
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

            public override double[] GoodValues()
            {
                return new double[] { 0, 0.874, 0.147, 0.999, 1.00, 0.01 };
            }
            public override double[] BadValues()
            {
                return new double[] { 1.001, 1.1, 1.01, -0.01, -1, -0.10 };
            }

            public override ICommand GenerateCommand(double v)
            {
                return new TransitionWipeSetCommand
                {
                    Index = _id,
                    Mask = TransitionWipeSetCommand.MaskFlags.YPosition,
                    YPosition = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_id].Transition.Wipe.YPosition = v;
                else
                    state.MixEffects[_id].Transition.Wipe.YPosition = v >= 1 ? 1 : 0;
            }
        }

        [Fact]
        public void TestVerticalOffset()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    new WipeTransitionVerticalOffsetTestDefinition(helper, me).Run();
                }
            }
        }

        private class WipeTransitionReverseTestDefinition : WipeTransitionTestDefinition<bool>
        {
            public WipeTransitionReverseTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetReverse(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new TransitionWipeSetCommand
                {
                    Index = _id,
                    Mask = TransitionWipeSetCommand.MaskFlags.ReverseDirection,
                    ReverseDirection = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_id].Transition.Wipe.ReverseDirection = v;
                state.MixEffects[_id].Transition.DVE.Reverse = v;
            }
        }

        [Fact]
        public void TestReverse()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    new WipeTransitionReverseTestDefinition(helper, me).Run();
                }
            }
        }

        private class WipeTransitionFlipFlopTestDefinition : WipeTransitionTestDefinition<bool>
        {
            public WipeTransitionFlipFlopTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionWipeParameters> me) : base(helper, me)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetFlipFlop(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new TransitionWipeSetCommand
                {
                    Index = _id,
                    Mask = TransitionWipeSetCommand.MaskFlags.FlipFlop,
                    FlipFlop = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_id].Transition.Wipe.FlipFlop = v;
                state.MixEffects[_id].Transition.DVE.FlipFlop = v;
            }
        }

        [Fact]
        public void TestFlipFlop()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var me in GetMixEffects<IBMDSwitcherTransitionWipeParameters>())
                {
                    new WipeTransitionFlipFlopTestDefinition(helper, me).Run();
                }
            }
        }
    }
}