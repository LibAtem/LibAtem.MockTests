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

        private abstract class DownstreamKeyerTestDefinition<T> : TestDefinitionBase<T>
        {
            protected readonly IBMDSwitcherDownstreamKey _sdk;
            protected readonly DownstreamKeyId _keyId;

            public DownstreamKeyerTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper)
            {
                _sdk = keyer.Item2;
                _keyId = keyer.Item1;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                //yield return new CommandQueueKey(new DownstreamKeySourceGetCommand() { Index = _keyId });
                yield return new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _keyId });
            }
        }

        private class DownstreamKeyerCutSourceTestDefinition : DownstreamKeyerTestDefinition<VideoSource>
        {
            public DownstreamKeyerCutSourceTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInputCut((long)VideoSource.ColorBars);
                _sdk.SetTie(0);
                _sdk.SetOnAir(0);
            }

            public override VideoSource[] GoodValues()
            {
                return VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile) && s.IsAvailable(MixEffectBlockId.One) && s.IsAvailable(SourceAvailability.KeySource)).ToArray();
            }

            public override ICommand GenerateCommand(VideoSource v)
            {
                return new DownstreamKeyCutSourceSetCommand
                {
                    Index = _keyId,
                    Source = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[_keyId].CutSource = v;
                }
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
            {
                foreach (var key in GetKeyers())
                {
                    key.Item2.GetCutInputAvailabilityMask(out _BMDSwitcherInputAvailability availability);
                    Assert.Equal(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityInputCut | _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMixEffectBlock0, availability);

                    new DownstreamKeyerCutSourceTestDefinition(helper, key).Run();
                }
            }
        }

        private class DownstreamKeyerFillSourceTestDefinition : DownstreamKeyerTestDefinition<VideoSource>
        {
            public DownstreamKeyerFillSourceTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInputFill((long)VideoSource.ColorBars);
                _sdk.SetTie(0);
                _sdk.SetOnAir(0);
            }

            public override VideoSource[] GoodValues()
            {
                return VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile) && s.IsAvailable(MixEffectBlockId.One)).ToArray();
            }

            public override ICommand GenerateCommand(VideoSource v)
            {
                return new DownstreamKeyFillSourceSetCommand
                {
                    Index = _keyId,
                    Source = v,
                };
            }

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
            {
                foreach (var key in GetKeyers())
                {
                    key.Item2.GetFillInputAvailabilityMask(out _BMDSwitcherInputAvailability availability);
                    Assert.Equal(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMixEffectBlock0, availability);

                    new DownstreamKeyerFillSourceTestDefinition(helper, key).Run();
                }
            }
        }

        private class DownstreamKeyerTieTestDefinition : DownstreamKeyerTestDefinition<bool>
        {
            public DownstreamKeyerTieTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetTie(0);
                _sdk.SetOnAir(0);
                _sdk.SetInputFill((long)VideoSource.MediaPlayer1);
                _sdk.SetInputCut((long)VideoSource.MediaPlayer1Key);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new DownstreamKeyTieSetCommand
                {
                    Index = _keyId,
                    Tie = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[_keyId].Tie = v;
                    state.Inputs[VideoSource.MediaPlayer1].PreviewTally = v;
                    state.Inputs[VideoSource.MediaPlayer1Key].PreviewTally = v;
                }
            }
        }

        [Fact]
        public void TestTie()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    new DownstreamKeyerTieTestDefinition(helper, key).Run();
                }
            }
        }

        private class DownstreamKeyerRateTestDefinition : DownstreamKeyerTestDefinition<uint>
        {
            public DownstreamKeyerRateTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetRate(20);
            }

            public override ICommand GenerateCommand(uint v)
            {
                return new DownstreamKeyRateSetCommand
                {
                    Index = _keyId,
                    Rate = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[_keyId].Rate = v;
                    state.DownstreamKeyers[_keyId].RemainingFrames = v;
                }
                else
                {
                    state.DownstreamKeyers[_keyId].Rate = v >= 250 ? 250 : (uint)1;
                    state.DownstreamKeyers[_keyId].RemainingFrames = v >= 250 ? 250 : (uint)1;
                }
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, uint v)
            {
                yield return new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _keyId });
            }

            public override uint[] GoodValues()
            {
                return new uint[] { 1, 18, 28, 95, 234, 244, 250 };
            }

            public override uint[] BadValues()
            {
                return new uint[] { 251, 255, 0 };
            }
        }

        [Fact]
        public void TestRate()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    new DownstreamKeyerRateTestDefinition(helper, key).Run();
                }
            }
        }

        private class DownstreamKeyerOnAirTestDefinition : DownstreamKeyerTestDefinition<bool>
        {
            public DownstreamKeyerOnAirTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetOnAir(0);
                _sdk.SetTie(0);
                _sdk.SetInputFill((long)VideoSource.MediaPlayer1);
                _sdk.SetInputCut((long)VideoSource.MediaPlayer1Key);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new DownstreamKeyOnAirSetCommand
                {
                    Index = _keyId,
                    OnAir = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[_keyId].OnAir = v;
                    state.Inputs[VideoSource.MediaPlayer1].ProgramTally = v;
                    state.Inputs[VideoSource.MediaPlayer1Key].ProgramTally = v;
                }
            }
        }

        [Fact]
        public void TestOnAir()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    new DownstreamKeyerOnAirTestDefinition(helper, key).Run();
                }
            }
        }
        
        [Fact]
        public void TestAutoTransitioning()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
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
                    Assert.Equal((uint) 30, props.RemainingFrames);

                    helper.SendCommand(new DownstreamKeyAutoCommand {Index = key.Item1});
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
            }
        }

        private class DownstreamKeyerPreMultipliedTestDefinition : DownstreamKeyerTestDefinition<bool>
        {
            public DownstreamKeyerPreMultipliedTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetPreMultiplied(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new DownstreamKeyGeneralSetCommand
                {
                    Mask = DownstreamKeyGeneralSetCommand.MaskFlags.PreMultiply,
                    Index = _keyId,
                    PreMultiply = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[_keyId].PreMultipliedKey = v;
                }
            }
        }

        [Fact]
        public void TestPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    new DownstreamKeyerPreMultipliedTestDefinition(helper, key).Run();
                }
            }
        }

        private class DownstreamKeyerClipTestDefinition : DownstreamKeyerTestDefinition<double>
        {
            public DownstreamKeyerClipTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetClip(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new DownstreamKeyGeneralSetCommand
                {
                    Mask = DownstreamKeyGeneralSetCommand.MaskFlags.Clip,
                    Index = _keyId,
                    Clip = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[_keyId].Clip = v;
                }
                else
                {
                    state.DownstreamKeyers[_keyId].Clip = v >= 100 ? 100 : 0;
                }
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            }

            public override double[] BadValues()
            {
                return new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            }
        }

        [Fact]
        public void TestClip()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    new DownstreamKeyerClipTestDefinition(helper, key).Run();
                }
            }
        }

        private class DownstreamKeyerGainTestDefinition : DownstreamKeyerTestDefinition<double>
        {
            public DownstreamKeyerGainTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetGain(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new DownstreamKeyGeneralSetCommand
                {
                    Mask = DownstreamKeyGeneralSetCommand.MaskFlags.Gain,
                    Index = _keyId,
                    Gain = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[_keyId].Gain = v;
                }
                else
                {
                    state.DownstreamKeyers[_keyId].Gain = v >= 100 ? 100 : 0;
                }
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            }

            public override double[] BadValues()
            {
                return new double[] { 100.1, 110, 101, -0.01, -1, -10 };
            }
        }

        [Fact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    new DownstreamKeyerGainTestDefinition(helper, key).Run();
                }
            }
        }

        private class DownstreamKeyerInverseTestDefinition : DownstreamKeyerTestDefinition<bool>
        {
            public DownstreamKeyerInverseTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInverse(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new DownstreamKeyGeneralSetCommand
                {
                    Mask = DownstreamKeyGeneralSetCommand.MaskFlags.Invert,
                    Index = _keyId,
                    Invert = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[_keyId].Invert = v;
                }
            }
        }

        [Fact]
        public void TestInverse()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    new DownstreamKeyerInverseTestDefinition(helper, key).Run();
                }
            }
        }

        private class DownstreamKeyerMaskedTestDefinition : DownstreamKeyerTestDefinition<bool>
        {
            public DownstreamKeyerMaskedTestDefinition(AtemComparisonHelper helper, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetMasked(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new DownstreamKeyMaskSetCommand
                {
                    Mask = DownstreamKeyMaskSetCommand.MaskFlags.Enabled,
                    Index = _keyId,
                    Enabled = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                if (goodValue)
                {
                    state.DownstreamKeyers[_keyId].MaskEnabled = v;
                }
            }
        }

        [Fact]
        public void TestMasked()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    new DownstreamKeyerMaskedTestDefinition(helper, key).Run();
                }
            }
        }

        private abstract class DownstreamKeyerMaskYTestDefinition : DownstreamKeyerTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public DownstreamKeyerMaskYTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
                _mode = mode;
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

            protected double ClampValueToRange(bool goodValue, double v)
            {
                if (goodValue)
                    return v;

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
            
            public override double[] GoodValues()
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

            public override double[] BadValues()
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

        private class DownstreamKeyerMaskTopTestDefinition : DownstreamKeyerMaskYTestDefinition
        {
            public DownstreamKeyerMaskTopTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, mode, keyer)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new DownstreamKeyMaskSetCommand
                {
                    Mask = DownstreamKeyMaskSetCommand.MaskFlags.Top,
                    Index = _keyId,
                    Top = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.DownstreamKeyers[_keyId].MaskTop = ClampValueToRange(goodValue, v);
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

                    foreach (var key in GetKeyers())
                    {
                        new DownstreamKeyerMaskTopTestDefinition(helper, mode, key).Run();
                    }
                }
            }
        }

        private class DownstreamKeyerMaskBottomTestDefinition : DownstreamKeyerMaskYTestDefinition
        {
            public DownstreamKeyerMaskBottomTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, mode, keyer)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new DownstreamKeyMaskSetCommand
                {
                    Mask = DownstreamKeyMaskSetCommand.MaskFlags.Bottom,
                    Index = _keyId,
                    Bottom = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.DownstreamKeyers[_keyId].MaskBottom = ClampValueToRange(goodValue, v);
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

                    foreach (var key in GetKeyers())
                    {
                        new DownstreamKeyerMaskBottomTestDefinition(helper, mode, key).Run();
                    }
                }
            }
        }

        private abstract class DownstreamKeyerMaskXTestDefinition : DownstreamKeyerTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public DownstreamKeyerMaskXTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, keyer)
            {
                _mode = mode;
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

            protected double ClampValueToRange(bool goodValue, double v)
            {
                if (goodValue)
                    return v;

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

            public override double[] GoodValues()
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

            public override double[] BadValues()
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

        private class DownstreamKeyerMaskLeftTestDefinition : DownstreamKeyerMaskXTestDefinition
        {
            public DownstreamKeyerMaskLeftTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, mode, keyer)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new DownstreamKeyMaskSetCommand
                {
                    Mask = DownstreamKeyMaskSetCommand.MaskFlags.Left,
                    Index = _keyId,
                    Left = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                 state.DownstreamKeyers[_keyId].MaskLeft = ClampValueToRange(goodValue, v);
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

                    foreach (var key in GetKeyers())
                    {
                        new DownstreamKeyerMaskLeftTestDefinition(helper, mode, key).Run();
                    }
                }
            }
        }

        private class DownstreamKeyerMaskRightTestDefinition : DownstreamKeyerMaskXTestDefinition
        {
            public DownstreamKeyerMaskRightTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<DownstreamKeyId, IBMDSwitcherDownstreamKey> keyer) : base(helper, mode, keyer)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new DownstreamKeyMaskSetCommand
                {
                    Mask = DownstreamKeyMaskSetCommand.MaskFlags.Right,
                    Index = _keyId,
                    Right = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.DownstreamKeyers[_keyId].MaskRight = ClampValueToRange(goodValue, v);
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

                    foreach (var key in GetKeyers())
                    {
                        new DownstreamKeyerMaskRightTestDefinition(helper, mode, key).Run();
                    }
                }
            }
        }
    }
}