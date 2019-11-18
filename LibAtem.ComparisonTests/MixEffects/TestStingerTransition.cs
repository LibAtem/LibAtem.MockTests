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
using LibAtem.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestStingerTransition : MixEffectsTestBase
    {
        public TestStingerTransition(ITestOutputHelper output, AtemClientWrapper client)
            : base(output, client)
        {
        }

        private static void ResetProps(IBMDSwitcherTransitionStingerParameters props)
        {
            props.SetTriggerPoint(1);
            props.SetPreroll(1);
            props.SetMixRate(1);
            props.SetClipDuration(40);
            props.SetTriggerPoint(10);
            props.SetPreroll(5);
            props.SetMixRate(15);
        }

        private abstract class StingerTransitionTestDefinition<T> : TestDefinitionBase<TransitionStingerSetCommand, T>
        {
            protected readonly MixEffectBlockId _meId;
            protected readonly IBMDSwitcherTransitionStingerParameters _sdk;

            public StingerTransitionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionStingerParameters> me) : base(helper, me.Item1 != MixEffectBlockId.One)
            {
                _meId = me.Item1;
                _sdk = me.Item2;
            }

            public override void SetupCommand(TransitionStingerSetCommand cmd)
            {
                cmd.Index = _meId;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                MixEffectState.TransitionStingerState obj = state.MixEffects[(int)_meId].Transition.Stinger;
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, T v)
            {
                yield return $"MixEffects.{_meId:D}.Transition.Stinger";
            }
        }

        private class StingerTransitionSourceTestDefinition : StingerTransitionTestDefinition<StingerSource>
        {
            public StingerTransitionSourceTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionStingerParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetSource(_BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer2);

            public override string PropertyName => "Source";
            public override StingerSource MangleBadValue(StingerSource v) => v;

            public override StingerSource[] GoodValues => Enum.GetValues(typeof(StingerSource)).OfType<StingerSource>().Where(s => s.IsAvailable(_helper.Profile) && s > 0).ToArray();
            public override StingerSource[] BadValues => Enum.GetValues(typeof(StingerSource)).OfType<StingerSource>().Except(GoodValues).Where(s => s != 0).ToArray();

            public override void UpdateExpectedState(AtemState state, bool goodValue, StingerSource v)
            {
                if (goodValue) base.UpdateExpectedState(state, goodValue, v);
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, StingerSource v)
            {
                yield break;
            }
        }
        [Fact]
        public void TestStingerSource()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                var stingers = GetMixEffects<IBMDSwitcherTransitionStingerParameters>();
                foreach (var me in stingers)
                {
                    stingers.ForEach(s => s.Item2.SetSource(_BMDSwitcherStingerTransitionSource.bmdSwitcherStingerTransitionSourceMediaPlayer1));
                    helper.Sleep();

                    new StingerTransitionSourceTestDefinition(helper, me).Run();

                    /*
                    void UpdateExpectedState(AtemState state, StingerSource v)
                    {
                        state.MixEffects[me.Item1].Transition.Stinger.Source = v;
                        switch (v)
                        {
                            // Behaviour going to or from none changes them all
                            case StingerSource.None:
                            case StingerSource.MediaPlayer1:
                                state.MixEffects.ForEach(m => m.Value.Transition.Stinger.Source = v);
                                break;
                        }
                    }
                    */
                }
            }
        }

        private class StingerTransitionPreMultipliedTestDefinition : StingerTransitionTestDefinition<bool>
        {
            public StingerTransitionPreMultipliedTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionStingerParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetPreMultiplied(0);

            public override string PropertyName => "PreMultipliedKey";
            public override bool MangleBadValue(bool v) => v;

            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                state.MixEffects[(int)_meId].Transition.Stinger.PreMultipliedKey = v;
                state.MixEffects[(int)_meId].Transition.DVE.PreMultiplied = v;
            }
        }
        [Fact]
        public void TestStingerPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionStingerParameters>().ForEach(m => new StingerTransitionPreMultipliedTestDefinition(helper, m).Run());
        }

        private class StingerTransitionClipTestDefinition : StingerTransitionTestDefinition<double>
        {
            public StingerTransitionClipTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionStingerParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetClip(20);

            public override string PropertyName => "Clip";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override void UpdateExpectedState(AtemState state, bool goodValue, double v)
            {
                state.MixEffects[(int)_meId].Transition.Stinger.Clip = goodValue ? v : MangleBadValue(v);
                state.MixEffects[(int)_meId].Transition.DVE.Clip = goodValue ? v : MangleBadValue(v);
            }

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }
        [Fact]
        public void TestStingerClip()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionStingerParameters>().ForEach(m => new StingerTransitionClipTestDefinition(helper, m).Run());
        }

        private class StingerTransitionGainTestDefinition : StingerTransitionTestDefinition<double>
        {
            public StingerTransitionGainTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionStingerParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetGain(20);

            public override string PropertyName => "Gain";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override void UpdateExpectedState(AtemState state, bool goodValue, double v)
            {
                state.MixEffects[(int)_meId].Transition.Stinger.Gain = goodValue ? v : MangleBadValue(v);
                state.MixEffects[(int)_meId].Transition.DVE.Gain = goodValue ? v : MangleBadValue(v);
            }

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }
        [Fact]
        public void TestStingerGain()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionStingerParameters>().ForEach(m => new StingerTransitionGainTestDefinition(helper, m).Run());
        }

        private class StingerTransitionInvertTestDefinition : StingerTransitionTestDefinition<bool>
        {
            public StingerTransitionInvertTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionStingerParameters> me) : base(helper, me)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInverse(0);

            public override string PropertyName => "Invert";
            public override bool MangleBadValue(bool v) => v;

            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                state.MixEffects[(int)_meId].Transition.Stinger.Invert = v;
                state.MixEffects[(int)_meId].Transition.DVE.InvertKey = v;
            }
        }
        [Fact]
        public void TestStingerInvert()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionStingerParameters>().ForEach(m => new StingerTransitionInvertTestDefinition(helper, m).Run());
        }

        private class StingerTransitionPreRollTestDefinition : StingerTransitionTestDefinition<uint>
        {
            private readonly uint _clipMaxFrames;

            public StingerTransitionPreRollTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionStingerParameters> me, uint clipMaxFrames) : base(helper, me)
            {
                _clipMaxFrames = clipMaxFrames;
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInverse(0);

            public override string PropertyName => "Preroll";
            public override uint MangleBadValue(uint v) => v >= _clipMaxFrames ? _clipMaxFrames : 0;

            // TODO - these should be related to _clipMaxFrames
            public override uint[] GoodValues => new uint[] { 0, 1, 18, 28, 90 };
            public override uint[] BadValues => new uint[] { 999, 251 };
        }
        [Fact]
        public void TestStingerPreRoll()
        {
            uint clipMaxFrames = 90; // TODO - dynamic based on me selected

            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionStingerParameters>().ForEach(m => new StingerTransitionPreRollTestDefinition(helper, m, clipMaxFrames).Run());
        }

        private class StingerTransitionClipDurationTestDefinition : StingerTransitionTestDefinition<uint>
        {
            private readonly bool _testOne;

            public StingerTransitionClipDurationTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionStingerParameters> me, bool testOne) : base(helper, me)
            {
                _testOne = testOne;
            }

            public override void Prepare()
            {
                if (_testOne)
                {
                    ResetProps(_sdk);
                    _sdk.SetTriggerPoint(17);
                    _sdk.SetMixRate(13);
                    _sdk.SetPreroll(5);
                }
                else
                {
                    _sdk.SetTriggerPoint(4);
                    _sdk.SetMixRate(6);
                }
            }

            public override string PropertyName => "ClipDuration";
            public override uint MangleBadValue(uint v)
            {
                if (_testOne)
                    return v <= 30 ? 30 : (uint)0;
                else
                    return v <= 10 ? 10 : (uint)0;
            }

            public override uint[] GoodValues => _testOne ? new uint[] { 35, 48, 95, 199, 30 } : new uint[] { 11, 30, 10 };
            public override uint[] BadValues => _testOne ? new uint[] { 1, 29, 5 } : new uint[] { 9, 1 };
        }
        [Fact]
        public void TestStingerClipDuration()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionStingerParameters>().ForEach(m => {
                    new StingerTransitionClipDurationTestDefinition(helper, m, true).Run();
                    new StingerTransitionClipDurationTestDefinition(helper, m, false).Run();
                });
        }

        private class StingerTransitionTriggerPointTestDefinition : StingerTransitionTestDefinition<uint>
        {
            private readonly bool _testOne;

            public StingerTransitionTriggerPointTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionStingerParameters> me, bool testOne) : base(helper, me)
            {
                _testOne = testOne;
            }

            public override void Prepare()
            {
                if (_testOne)
                {
                    ResetProps(_sdk);
                    _sdk.SetMixRate(15);
                }
                else
                {
                    _sdk.SetTriggerPoint(1);
                    _sdk.SetClipDuration(25);
                }
            }

            public override string PropertyName => "TriggerPoint";
            public override uint MangleBadValue(uint v) => v;

            public override void UpdateExpectedState(AtemState state, bool goodValue, uint v)
            {
                var props = state.MixEffects[(int)_meId].Transition.Stinger;

                if (goodValue)
                {
                    props.TriggerPoint = v;
                    if (props.ClipDuration - props.TriggerPoint < props.MixRate)
                        props.MixRate = props.ClipDuration - props.TriggerPoint;
                }
                else
                {
                    if (_testOne)
                        props.TriggerPoint = v >= 39 ? 39 : (uint)0;
                    else
                        props.TriggerPoint = v >= 24 ? 24 : (uint)0;
                }
            }

            public override uint[] GoodValues => _testOne ? new uint[] { 1, 18, 28, 39 } : new uint[] { 11, 24, 10 };
            public override uint[] BadValues => _testOne ? new uint[] { 40, 41, 50 } : new uint[] { 25, 26, 30 };
        }
        [Fact]
        public void TestStingerTriggerPoint()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionStingerParameters>().ForEach(m => {
                    new StingerTransitionTriggerPointTestDefinition(helper, m, true).Run();
                    new StingerTransitionTriggerPointTestDefinition(helper, m, false).Run();
                });
        }

        private class StingerTransitionMixRateTestDefinition : StingerTransitionTestDefinition<uint>
        {
            private readonly bool _testOne;

            public StingerTransitionMixRateTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherTransitionStingerParameters> me, bool testOne) : base(helper, me)
            {
                _testOne = testOne;
            }

            public override void Prepare()
            {
                if (_testOne)
                {
                    ResetProps(_sdk);
                }
                else
                {
                    _sdk.SetMixRate(5);
                    _sdk.SetClipDuration(20);
                }
            }

            public override string PropertyName => "MixRate";
            public override uint MangleBadValue(uint v)
            {
                if (_testOne)
                    return v > 30 ? 30 : (uint)0;
                else
                    return v > 10 ? 10 : (uint)0;
            }

            public override uint[] GoodValues => _testOne ? new uint[] { 1, 18, 28, 30 } : new uint[] { 9, 1, 10 };
            public override uint[] BadValues => _testOne ? new uint[] { 31, 32, 40 } : new uint[] { 11, 12, 20 };
        }
        [Fact]
        public void TestStingerMixRate()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetMixEffects<IBMDSwitcherTransitionStingerParameters>().ForEach(m => {
                    new StingerTransitionMixRateTestDefinition(helper, m, true).Run();
                    new StingerTransitionMixRateTestDefinition(helper, m, false).Run();
                });
        }
    }
}
