using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DownstreamKey;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using LibAtem.DeviceProfile;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2
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
            Guid itId = typeof(IBMDSwitcherDownstreamKeyIterator).GUID;
            _client.SdkSwitcher.CreateIterator(ref itId, out IntPtr itPtr);
            IBMDSwitcherDownstreamKeyIterator iterator = (IBMDSwitcherDownstreamKeyIterator)Marshal.GetObjectForIUnknown(itPtr);

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

            public DownstreamKeyerCutSourceTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper)
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

            public override VideoSource[] GoodValues => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile) && s.IsAvailable(MixEffectBlockId.One) && s.IsAvailable(SourceAvailability.KeySource)).ToArray();

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                    state.DownstreamKeyers[_keyId].CutSource = v;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    yield return new CommandQueueKey(new DownstreamKeySourceGetCommand() { Index = _keyId });
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

            public DownstreamKeyerFillSourceTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper)
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

            public override VideoSource[] GoodValues => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile) && s.IsAvailable(MixEffectBlockId.One)).ToArray();

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[_keyId].FillSource = v;
                    if (v.GetPortType() == InternalPortType.MediaPlayerFill)
                        state.DownstreamKeyers[_keyId].CutSource = v + 1;
                }
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    yield return new CommandQueueKey(new DownstreamKeySourceGetCommand() { Index = _keyId });
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

            public DownstreamKeyerTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper)
            {
                _sdk = keyer.Item2;
                _keyId = keyer.Item1;
            }

            public override void SetupCommand(DownstreamKeyGeneralSetCommand cmd)
            {
                cmd.Index = _keyId;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, T v)
            {
                SetCommandProperty(state.DownstreamKeyers[_keyId], PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _keyId });
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

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[_keyId].Tie = v;
                    state.Inputs[VideoSource.MediaPlayer1].PreviewTally = v;
                    state.Inputs[VideoSource.MediaPlayer1Key].PreviewTally = v;
                }
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, bool v)
            {
                yield return new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _keyId });
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
            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                state.DownstreamKeyers[_keyId].Rate = goodValue ? v : MangleBadValue(v);
                state.DownstreamKeyers[_keyId].RemainingFrames = goodValue ? v : MangleBadValue(v);
            }

            public override uint[] GoodValues => new uint[] { 1, 18, 28, 95, 234, 244, 250 };
            public override uint[] BadValues => new uint[] { 251, 255, 0 };

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, uint v)
            {
                yield return new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _keyId });
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
            
            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[_keyId].OnAir = v;
                    state.Inputs[VideoSource.MediaPlayer1].ProgramTally = v;
                    state.Inputs[VideoSource.MediaPlayer1Key].ProgramTally = v;
                }
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, bool v)
            {
                yield return new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _keyId });
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

                        ComparisonState beforeState = helper.LibState;
                        Assert.True(ComparisonStateComparer.AreEqual(helper.Output, helper.SdkState, beforeState));

                        var props = beforeState.DownstreamKeyers[key.Item1];
                        bool origOnAir = props.OnAir;
                        Assert.False(props.InTransition);
                        Assert.False(props.IsAuto);
                        Assert.Equal((uint)30, props.RemainingFrames);

                        helper.SendCommand(new DownstreamKeyAutoCommand { Index = key.Item1 });
                        helper.Sleep(500);

                        // Get states, they will change still during this test
                        ComparisonState libState = helper.LibState;
                        ComparisonState sdkState = helper.SdkState;
                        props = libState.DownstreamKeyers[key.Item1];
                        Assert.True(props.RemainingFrames > 0 && props.RemainingFrames < 30);

                        // Update expected
                        props = beforeState.DownstreamKeyers[key.Item1];
                        props.RemainingFrames = libState.DownstreamKeyers[key.Item1].RemainingFrames;
                        props.IsAuto = true;
                        props.InTransition = true;
                        props.OnAir = true;
                        beforeState.Inputs[VideoSource.MediaPlayer1].ProgramTally = true;
                        beforeState.Inputs[VideoSource.MediaPlayer1Key].ProgramTally = true;

                        // Ensure remaining is correct within a frame
                        Assert.True(Math.Abs(beforeState.DownstreamKeyers[key.Item1].RemainingFrames - libState.DownstreamKeyers[key.Item1].RemainingFrames) <= 1);
                        Assert.True(Math.Abs(beforeState.DownstreamKeyers[key.Item1].RemainingFrames - sdkState.DownstreamKeyers[key.Item1].RemainingFrames) <= 1);
                        libState.DownstreamKeyers[key.Item1].RemainingFrames = sdkState.DownstreamKeyers[key.Item1].RemainingFrames = beforeState.DownstreamKeyers[key.Item1].RemainingFrames;

                        Assert.True(ComparisonStateComparer.AreEqual(helper.Output, beforeState, sdkState));
                        Assert.True(ComparisonStateComparer.AreEqual(helper.Output, beforeState, libState));

                        helper.Sleep(1000);
                        // back to normal
                        props = beforeState.DownstreamKeyers[key.Item1];
                        props.RemainingFrames = 30;
                        props.IsAuto = false;
                        props.InTransition = false;
                        props.OnAir = !origOnAir;
                        Assert.True(ComparisonStateComparer.AreEqual(helper.Output, beforeState, helper.SdkState));
                        Assert.True(ComparisonStateComparer.AreEqual(helper.Output, beforeState, helper.LibState));

                    }
                    finally
                    {
                        // Sleep until transition has definitely ended
                        var props = helper.LibState.DownstreamKeyers[key.Item1];

                        int sleepDuration = 90;
                        if (props.InTransition)
                            sleepDuration += (int)(40 * props.RemainingFrames);

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

            public override string PropertyName => "PreMultiply";
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

            public DownstreamKeyerMaskTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper)
            {
                _sdk = keyer.Item2;
                _keyId = keyer.Item1;
            }

            public override void SetupCommand(DownstreamKeyMaskSetCommand cmd)
            {
                cmd.Index = _keyId;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, T v)
            {
                SetCommandProperty(state.DownstreamKeyers[_keyId], PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _keyId });
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