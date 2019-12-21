using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DownstreamKey;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.State.SDK;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests
{
    [Collection("Client")]
    public class TestDownstreamKeyer
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestDownstreamKeyer(ITestOutputHelper output, AtemClientWrapper client)
        {
            _output = output;
            _client = client;
        }

        protected List<Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey>> GetKeyers()
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherDownstreamKeyIterator>(_client.SdkSwitcher.CreateIterator);

            var result = new List<Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey>>();
            DownstreamKeyId index = 0;
            for (iterator.Next(out IBMDSwitcherDownstreamKey r); r != null; iterator.Next(out r))
            {
                result.Add(Tuple.Create(index, r));
                index++;
            }

            return result;
        }

        private class DownstreamKeyerCutSourceTestDefinition : TestDefinitionBase<DownstreamKeyCutSourceSetCommand, VideoSource>
        {
            protected readonly IBMDSwitcherDownstreamKey _sdk;
            protected readonly DownstreamKeyId _keyId;

            public DownstreamKeyerCutSourceTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer.Item1 != DownstreamKeyId.One)
            {
                _sdk = keyer.Item2;
                _keyId = keyer.Item1;

                keyer.Item2.GetCutInputAvailabilityMask(out _BMDSwitcherInputAvailability availability);
                Assert.Equal(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityInputCut | _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMixEffectBlock0, availability);
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInputFill((long)VideoSource.ColorBars);
                _sdk.SetInputCut((long)VideoSource.ColorBars);
                _sdk.SetTie(0);
                _sdk.SetOnAir(0);
            }

            public override void SetupCommand(DownstreamKeyCutSourceSetCommand cmd)
            {
                cmd.Index = _keyId;
            }

            public override string PropertyName => "Source";

            private VideoSource[] ValidSources => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile) && s.IsAvailable(MixEffectBlockId.One) && s.IsAvailable(SourceAvailability.KeySource)).ToArray();
            public override VideoSource[] GoodValues => VideoSourceUtil.TakeSelection(ValidSources);
            public override VideoSource[] BadValues => VideoSourceUtil.TakeBadSelection(ValidSources);

            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                    state.DownstreamKeyers[(int)_keyId].Sources.CutSource = v;
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    yield return $"DownstreamKeyers.{_keyId:D}.Sources";
            }
        }

        [Fact]
        public void TestCutSource()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetKeyers().ForEach(k => new DownstreamKeyerCutSourceTestDefinition(helper, k).Run());
        }

        private class DownstreamKeyerFillSourceTestDefinition : TestDefinitionBase<DownstreamKeyFillSourceSetCommand, VideoSource>
        {
            protected readonly IBMDSwitcherDownstreamKey _sdk;
            protected readonly DownstreamKeyId _keyId;

            public DownstreamKeyerFillSourceTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer.Item1 != DownstreamKeyId.One)
            {
                _sdk = keyer.Item2;
                _keyId = keyer.Item1;

                keyer.Item2.GetFillInputAvailabilityMask(out _BMDSwitcherInputAvailability availability);
                Assert.Equal(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMixEffectBlock0, availability);
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInputFill((long)VideoSource.ColorBars);
                _sdk.SetInputCut((long)VideoSource.ColorBars);
                _sdk.SetTie(0);
                _sdk.SetOnAir(0);
            }

            public override void SetupCommand(DownstreamKeyFillSourceSetCommand cmd)
            {
                cmd.Index = _keyId;
            }

            public override string PropertyName => "Source";

            private VideoSource[] ValidSources => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile) && s.IsAvailable(MixEffectBlockId.One)).ToArray();
            public override VideoSource[] GoodValues => VideoSourceUtil.TakeSelection(ValidSources);
            public override VideoSource[] BadValues => VideoSourceUtil.TakeBadSelection(ValidSources);

            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[(int)_keyId].Sources.FillSource = v;
                    if (v.GetPortType() == InternalPortType.MediaPlayerFill)
                        state.DownstreamKeyers[(int)_keyId].Sources.CutSource = v + 1;
                }
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    yield return $"DownstreamKeyers.{_keyId:D}.Sources";
            }
        }

        [Fact]
        public void TestFillSource()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetKeyers().ForEach(k => new DownstreamKeyerFillSourceTestDefinition(helper, k).Run());
        }

        private abstract class DownstreamKeyerTestDefinition<T> : TestDefinitionBase<DownstreamKeyGeneralSetCommand, T>
        {
            protected readonly IBMDSwitcherDownstreamKey _sdk;
            protected readonly DownstreamKeyId _keyId;

            public DownstreamKeyerTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer.Item1 != DownstreamKeyId.One)
            {
                _sdk = keyer.Item2;
                _keyId = keyer.Item1;
            }

            public override void SetupCommand(DownstreamKeyGeneralSetCommand cmd)
            {
                cmd.Index = _keyId;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                SetCommandProperty(state.DownstreamKeyers[(int)_keyId].Properties, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, T v)
            {
                yield return $"DownstreamKeyers.{_keyId:D}.Properties";
            }
        }

        private class DownstreamKeyerTieTestDefinition : TestDefinitionBase<DownstreamKeyTieSetCommand, bool>
        {
            protected readonly IBMDSwitcherDownstreamKey _sdk;
            protected readonly DownstreamKeyId _keyId;

            public DownstreamKeyerTieTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper)
            {
                _sdk = keyer.Item2;
                _keyId = keyer.Item1;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetTie(0);
                _sdk.SetOnAir(0);
                _sdk.SetInputFill((long)VideoSource.MediaPlayer1);
                _sdk.SetInputCut((long)VideoSource.MediaPlayer1Key);
            }

            public override void SetupCommand(DownstreamKeyTieSetCommand cmd)
            {
                cmd.Index = _keyId;
            }

            public override string PropertyName => "Tie";

            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[(int)_keyId].Properties.Tie = v;
                    state.Settings.Inputs[VideoSource.MediaPlayer1].Tally.PreviewTally = v;
                    state.Settings.Inputs[VideoSource.MediaPlayer1Key].Tally.PreviewTally = v;
                }
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, bool v)
            {
                yield return $"DownstreamKeyers.{_keyId:D}.Properties";
            }
        }

        [Fact]
        public void TestTie()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetKeyers().ForEach(k => new DownstreamKeyerTieTestDefinition(helper, k).Run());
        }

        private class DownstreamKeyerRateTestDefinition : TestDefinitionBase<DownstreamKeyRateSetCommand, uint>
        {
            protected readonly IBMDSwitcherDownstreamKey _sdk;
            protected readonly DownstreamKeyId _keyId;

            public DownstreamKeyerRateTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper)
            {
                _sdk = keyer.Item2;
                _keyId = keyer.Item1;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetRate(20);
            }

            public override void SetupCommand(DownstreamKeyRateSetCommand cmd)
            {
                cmd.Index = _keyId;
            }

            public override string PropertyName => "Rate";

            private uint MangleBadValue(uint v) => v >= 250 ? 250 : (uint)1;
            public override void UpdateExpectedState(AtemState state, bool goodValue, uint v)
            {
                state.DownstreamKeyers[(int)_keyId].Properties.Rate = goodValue ? v : MangleBadValue(v);
                state.DownstreamKeyers[(int)_keyId].State.RemainingFrames = goodValue ? v : MangleBadValue(v);
            }

            public override uint[] GoodValues => new uint[] { 1, 18, 28, 95, 234, 244, 250 };
            public override uint[] BadValues => new uint[] { 251, 255, 0 };

            public override IEnumerable<string> ExpectedCommands(bool goodValue, uint v)
            {
                yield return $"DownstreamKeyers.{_keyId:D}.Properties";
                yield return $"DownstreamKeyers.{_keyId:D}.State";
            }
        }

        [Fact]
        public void TestRate()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetKeyers().ForEach(k => new DownstreamKeyerRateTestDefinition(helper, k).Run());
        }

        private class DownstreamKeyerOnAirTestDefinition : TestDefinitionBase<DownstreamKeyOnAirSetCommand, bool>
        {
            protected readonly IBMDSwitcherDownstreamKey _sdk;
            protected readonly DownstreamKeyId _keyId;

            public DownstreamKeyerOnAirTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper)
            {
                _sdk = keyer.Item2;
                _keyId = keyer.Item1;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetOnAir(0);
                _sdk.SetTie(0);
                _sdk.SetInputFill((long)VideoSource.MediaPlayer1);
                _sdk.SetInputCut((long)VideoSource.MediaPlayer1Key);
            }

            public override void SetupCommand(DownstreamKeyOnAirSetCommand cmd)
            {
                cmd.Index = _keyId;
            }

            public override string PropertyName => "OnAir";
            
            public override void UpdateExpectedState(AtemState state, bool goodValue, bool v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[(int)_keyId].State.OnAir = v;
                    state.DownstreamKeyers[(int)_keyId].State.IsTowardsOnAir = !v;
                    state.Settings.Inputs[VideoSource.MediaPlayer1].Tally.ProgramTally = v;
                    state.Settings.Inputs[VideoSource.MediaPlayer1Key].Tally.ProgramTally = v;
                }
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, bool v)
            {
                yield return $"DownstreamKeyers.{_keyId:D}.State";
            }
        }

        [Fact]
        public void TestOnAir()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetKeyers().ForEach(k => new DownstreamKeyerOnAirTestDefinition(helper, k).Run());
        }
        
        [Fact]
        public void TestAutoTransitioning()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    try
                    {
                        key.Item2.SetRate(30);
                        key.Item2.SetOnAir(0);
                        key.Item2.SetTie(0);
                        key.Item2.SetInputFill((long)VideoSource.MediaPlayer1);

                        helper.Sleep(500);

                        AtemState beforeState = helper.LibState;
                        Assert.True(AtemStateComparer.AreEqual(helper.Output, helper.SdkState, beforeState));

                        var props = beforeState.DownstreamKeyers[(int)key.Item1];
                        bool origOnAir = props.State.OnAir;
                        Assert.False(props.State.InTransition);
                        Assert.False(props.State.IsAuto);
                        Assert.Equal((uint)30, props.State.RemainingFrames);

                        helper.SendCommand(new DownstreamKeyAutoCommand { Index = key.Item1 });
                        helper.Sleep(500);

                        // Get states, they will change still during this test
                        AtemState libState = helper.LibState;
                        AtemState sdkState = helper.SdkState;
                        props = libState.DownstreamKeyers[(int)key.Item1];
                        Assert.True(props.State.RemainingFrames > 0 && props.State.RemainingFrames < 30);

                        // Update expected
                        props = beforeState.DownstreamKeyers[(int)key.Item1];
                        props.State.RemainingFrames = libState.DownstreamKeyers[(int)key.Item1].State.RemainingFrames;
                        props.State.IsAuto = true;
                        props.State.InTransition = true;
                        props.State.OnAir = true;
                        beforeState.Settings.Inputs[VideoSource.MediaPlayer1].Tally.ProgramTally = true;
                        beforeState.Settings.Inputs[VideoSource.MediaPlayer1Key].Tally.ProgramTally = true;

                        // Ensure remaining is correct within a frame
                        Assert.True(Math.Abs(beforeState.DownstreamKeyers[(int)key.Item1].State.RemainingFrames - libState.DownstreamKeyers[(int)key.Item1].State.RemainingFrames) <= 1);
                        Assert.True(Math.Abs(beforeState.DownstreamKeyers[(int)key.Item1].State.RemainingFrames - sdkState.DownstreamKeyers[(int)key.Item1].State.RemainingFrames) <= 1);
                        libState.DownstreamKeyers[(int)key.Item1].State.RemainingFrames = sdkState.DownstreamKeyers[(int)key.Item1].State.RemainingFrames = beforeState.DownstreamKeyers[(int)key.Item1].State.RemainingFrames;

                        Assert.True(AtemStateComparer.AreEqual(helper.Output, beforeState, sdkState));
                        Assert.True(AtemStateComparer.AreEqual(helper.Output, beforeState, libState));

                        helper.Sleep(1000);
                        // back to normal
                        props = beforeState.DownstreamKeyers[(int)key.Item1];
                        props.State.RemainingFrames = 30;
                        props.State.IsAuto = false;
                        props.State.InTransition = false;
                        props.State.OnAir = !origOnAir;
                        Assert.True(AtemStateComparer.AreEqual(helper.Output, beforeState, helper.SdkState));
                        Assert.True(AtemStateComparer.AreEqual(helper.Output, beforeState, helper.LibState));

                    }
                    finally
                    {
                        // Sleep until transition has definitely ended
                        var props = helper.LibState.DownstreamKeyers[(int)key.Item1];

                        int sleepDuration = 90;
                        if (props.State.InTransition)
                            sleepDuration += (int)(40 * props.State.RemainingFrames);

                        helper.Sleep(sleepDuration);
                    }
                }
            }
        }

        private class DownstreamKeyerPreMultipliedTestDefinition : DownstreamKeyerTestDefinition<bool>
        {
            public DownstreamKeyerPreMultipliedTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetPreMultiplied(0);

            public override string PropertyName => "PreMultipliedKey";
            public override bool MangleBadValue(bool v) => v;
        }

        [Fact]
        public void TestPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetKeyers().ForEach(k => new DownstreamKeyerPreMultipliedTestDefinition(helper, k).Run());
        }

        private class DownstreamKeyerClipTestDefinition : DownstreamKeyerTestDefinition<double>
        {
            public DownstreamKeyerClipTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetClip(20);

            public override string PropertyName => "Clip";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestClip()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetKeyers().ForEach(k => new DownstreamKeyerClipTestDefinition(helper, k).Run());
        }

        private class DownstreamKeyerGainTestDefinition : DownstreamKeyerTestDefinition<double>
        {
            public DownstreamKeyerGainTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetGain(20);

            public override string PropertyName => "Gain";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetKeyers().ForEach(k => new DownstreamKeyerGainTestDefinition(helper, k).Run());
        }

        private class DownstreamKeyerInverseTestDefinition : DownstreamKeyerTestDefinition<bool>
        {
            public DownstreamKeyerInverseTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInverse(0);

            public override string PropertyName => "Invert";
            public override bool MangleBadValue(bool v) => v;
        }

        [Fact]
        public void TestInverse()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetKeyers().ForEach(k => new DownstreamKeyerInverseTestDefinition(helper, k).Run());
        }

        private abstract class DownstreamKeyerMaskTestDefinition<T> : TestDefinitionBase<DownstreamKeyMaskSetCommand, T>
        {
            protected readonly IBMDSwitcherDownstreamKey _sdk;
            protected readonly DownstreamKeyId _keyId;

            public DownstreamKeyerMaskTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer.Item1 != DownstreamKeyId.One)
            {
                _sdk = keyer.Item2;
                _keyId = keyer.Item1;
            }

            public override void SetupCommand(DownstreamKeyMaskSetCommand cmd)
            {
                cmd.Index = _keyId;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                SetCommandProperty(state.DownstreamKeyers[(int)_keyId].Properties, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, T v)
            {
                yield return $"DownstreamKeyers.{_keyId:D}.Properties";
            }
        }

        private class DownstreamKeyerMaskedTestDefinition : DownstreamKeyerMaskTestDefinition<bool>
        {
            public DownstreamKeyerMaskedTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetMasked(0);

            public override string PropertyName => "MaskEnabled";
            public override bool MangleBadValue(bool v) => v;
        }

        [Fact]
        public void TestMasked()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                GetKeyers().ForEach(k => new DownstreamKeyerMaskedTestDefinition(helper, k).Run());
        }

        private class DownstreamKeyerMaskYTestDefinition : DownstreamKeyerMaskTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public override string PropertyName { get; }

            public DownstreamKeyerMaskYTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer, string propName) : base(helper, keyer)
            {
                _mode = mode;
                PropertyName = propName;
            }

            public static IEnumerable<VideoMode> VideoModes()
            {
                yield return VideoMode.P1080i50;
                yield return VideoMode.N720p5994;
                yield return VideoMode.P625i50PAL;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetMasked(1);
                _sdk.SetMaskTop(5);
                _sdk.SetMaskBottom(5);
                _helper.Sleep();
            }

            public override double MangleBadValue(double v)
            {
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return  v >= 9 ? 9 : -9;
                    case VideoMode.P625i50PAL:
                        return v >= 3 ? 3 : -3;
                    default:
                        throw new NotSupportedException();
                }
            }
            
            public override double[] GoodValues
            {
                get
                {
                    switch (_mode)
                    {
                        case VideoMode.P1080i50:
                        case VideoMode.N720p5994:
                            return new double[] { 1, 0, 5, -5, -9, 9, 4.78 };
                        case VideoMode.P625i50PAL:
                            return new double[] { 1, 0, 2.5, -2.5, -3, 3, 1.78 };
                        default:
                            throw new NotSupportedException();
                    }
                }
            }

            public override double[] BadValues
            {
                get
                {
                    switch (_mode)
                    {
                        case VideoMode.P1080i50:
                        case VideoMode.N720p5994:
                            return new double[] { -9.01, 9.01, 9.1, -9.1 };
                        case VideoMode.P625i50PAL:
                            return new double[] { -3.01, 3.01, 3.1, -3.1 };
                        default:
                            throw new NotSupportedException();
                    }
                }
            }
        }

        [Fact]
        public void TestMaskTop()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var mode in DownstreamKeyerMaskYTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    GetKeyers().ForEach(k => new DownstreamKeyerMaskYTestDefinition(helper, mode, k, "MaskTop").Run());
                }
            }
        }

        [Fact]
        public void TestMaskBottom()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var mode in DownstreamKeyerMaskYTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    GetKeyers().ForEach(k => new DownstreamKeyerMaskYTestDefinition(helper, mode, k, "MaskBottom").Run());
                }
            }
        }

        private class DownstreamKeyerMaskXTestDefinition : DownstreamKeyerMaskTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public override string PropertyName { get; }

            public DownstreamKeyerMaskXTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer, string propName) : base(helper, keyer)
            {
                _mode = mode;
                PropertyName = propName;
            }

            public static IEnumerable<VideoMode> VideoModes()
            {
                yield return VideoMode.P1080i50;
                yield return VideoMode.N720p5994;
                yield return VideoMode.P625i50PAL;
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetMasked(1);
                _sdk.SetMaskLeft(2);
                _sdk.SetMaskRight(2);
                _helper.Sleep();
            }

            public override double MangleBadValue(double v)
            {
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return v >= 16 ? 16 : -16;
                    case VideoMode.P625i50PAL:
                        return v >= 4 ? 4 : -4;
                    default:
                        throw new NotSupportedException();
                }
            }

            public override double[] GoodValues
            {
                get
                {
                    switch (_mode)
                    {
                        case VideoMode.P1080i50:
                        case VideoMode.N720p5994:
                            return new double[] { 1, 0, 5, -5, -16, 16, 4.78 };
                        case VideoMode.P625i50PAL:
                            return new double[] { 1, 0, 2.5, -2.5, -4, 4, 1.78 };
                        default:
                            throw new NotSupportedException();
                    }
                }
            }

            public override double[] BadValues
            {
                get
                {
                    switch (_mode)
                    {
                        case VideoMode.P1080i50:
                        case VideoMode.N720p5994:
                            return new double[] { -16.01, 16.01, 16.1, -16.1 };
                        case VideoMode.P625i50PAL:
                            return new double[] { -4.01, 4.01, 4.1, -4.1 };
                        default:
                            throw new NotSupportedException();
                    }
                }
            }
        }

        [Fact]
        public void TestMaskLeft()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var mode in DownstreamKeyerMaskYTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    GetKeyers().ForEach(k => new DownstreamKeyerMaskXTestDefinition(helper, mode, k, "MaskLeft").Run());
                }
            }
        }

        [Fact]
        public void TestMaskRight()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var mode in DownstreamKeyerMaskYTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    GetKeyers().ForEach(k => new DownstreamKeyerMaskXTestDefinition(helper, mode, k, "MaskRight").Run());
                }
            }
        }
    }
}