using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DownstreamKey;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
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

        [Fact]
        public void TestCutSource()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    key.Item2.GetCutInputAvailabilityMask(out _BMDSwitcherInputAvailability availability);
                    Assert.Equal(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityInputCut | _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityMixEffectBlock0, availability);

                    VideoSource[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(_client.Profile) && s.IsAvailable(SourceAvailability.KeySource)).ToArray();
                    VideoSource[] badValues = VideoSourceLists.All.Where(s => !testValues.Contains(s)).ToArray();

                    ICommand Setter(VideoSource v) => new DownstreamKeyCutSourceSetCommand
                    {
                        Index = key.Item1,
                        Source = v,
                    };

                    void UpdateExpectedState(ComparisonState state, VideoSource v) => state.DownstreamKeyers[key.Item1].CutSource = v;

                    ValueTypeComparer<VideoSource>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<VideoSource>.Fail(helper, Setter, badValues);
                }
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

                    VideoSource[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(_client.Profile) && s.IsAvailable(SourceAvailability.KeySource)).ToArray();
                    // TODO - fix these lists
                    VideoSource[] badValues = VideoSourceLists.All.Where(s => !testValues.Contains(s)).ToArray();

                    ICommand Setter(VideoSource v) => new DownstreamKeyFillSourceSetCommand
                    {
                        Index = key.Item1,
                        Source = v,
                    };

                    void UpdateExpectedState(ComparisonState state, VideoSource v)
                    {
                        state.DownstreamKeyers[key.Item1].FillSource = v;
                        if (VideoSourceLists.MediaPlayers.Contains(v))
                            state.DownstreamKeyers[key.Item1].CutSource = v + 1;
                    }

                    ValueTypeComparer<VideoSource>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<VideoSource>.Fail(helper, Setter, badValues);
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
                    bool[] testValues = { true, false };

                    ICommand Setter(bool v) => new DownstreamKeyTieSetCommand
                    {
                        Index = key.Item1,
                        Tie = v,
                    };

                    void UpdateExpectedState(ComparisonState state, bool v) => state.DownstreamKeyers[key.Item1].Tie = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestRate()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    uint[] testValues = { 1, 18, 28, 95, 234, 244, 250 };
                    uint[] badValues = { 251, 255, 0 };

                    ICommand Setter(uint v) => new DownstreamKeyRateSetCommand
                    {
                        Index = key.Item1,
                        Rate = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.DownstreamKeyers[key.Item1].Rate = state.DownstreamKeyers[key.Item1].RemainingFrames = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.DownstreamKeyers[key.Item1].Rate  = state.DownstreamKeyers[key.Item1].RemainingFrames = v >= 250 ? 250 : (uint) 1;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
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
                    bool[] testValues = { true, false };

                    ICommand Setter(bool v) => new DownstreamKeyOnAirSetCommand
                    {
                        Index = key.Item1,
                        OnAir = v,
                    };

                    void UpdateExpectedState(ComparisonState state, bool v) => state.DownstreamKeyers[key.Item1].OnAir = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
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
                    helper.Sleep();

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

        [Fact]
        public void TestPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    bool[] testValues = { true, false };

                    ICommand Setter(bool v) => new DownstreamKeyGeneralSetCommand()
                    {
                        Mask = DownstreamKeyGeneralSetCommand.MaskFlags.PreMultiply,
                        Index = key.Item1,
                        PreMultiply = v,
                    };

                    void UpdateExpectedState(ComparisonState state, bool v) => state.DownstreamKeyers[key.Item1].PreMultipliedKey = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestClip()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.1 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new DownstreamKeyGeneralSetCommand()
                    {
                        Mask = DownstreamKeyGeneralSetCommand.MaskFlags.Clip,
                        Index = key.Item1,
                        Clip = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].Clip = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].Clip = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var key in GetKeyers())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.1 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new DownstreamKeyGeneralSetCommand()
                    {
                        Mask = DownstreamKeyGeneralSetCommand.MaskFlags.Gain,
                        Index = key.Item1,
                        Gain = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].Gain = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].Gain = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
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
                    bool[] testValues = { true, false };

                    ICommand Setter(bool v) => new DownstreamKeyGeneralSetCommand()
                    {
                        Mask = DownstreamKeyGeneralSetCommand.MaskFlags.Invert,
                        Index = key.Item1,
                        Invert = v,
                    };

                    void UpdateExpectedState(ComparisonState state, bool v) => state.DownstreamKeyers[key.Item1].Invert = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
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
                    bool[] testValues = { true, false };

                    ICommand Setter(bool v) => new DownstreamKeyMaskSetCommand
                    {
                        Mask = DownstreamKeyMaskSetCommand.MaskFlags.Enabled,
                        Index = key.Item1,
                        Enabled = v,
                    };

                    void UpdateExpectedState(ComparisonState state, bool v) => state.DownstreamKeyers[key.Item1].MaskEnabled = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestMaskTop()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(300);

                foreach (var key in GetKeyers())
                {
                    double[] testValues = { 1, 0, 5, -5, -9, 9, 4.78 };
                    double[] badValues = { -9.01, 9.01, 9.1, -9.1 };

                    ICommand Setter(double v) => new DownstreamKeyMaskSetCommand()
                    {
                        Index = key.Item1,
                        Mask = DownstreamKeyMaskSetCommand.MaskFlags.Top,
                        Top = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskTop = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskTop = v >= 9 ? 9 : -9;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(300);

                // Repeat in 4:3
                foreach (var key in GetKeyers())
                {
                    double[] testValues = { 1, 0, 2.5, -2.5, -3, 3, 1.78 };
                    double[] badValues = { -3.01, 3.01, 3.1, -3.1 };

                    ICommand Setter(double v) => new DownstreamKeyMaskSetCommand()
                    {
                        Index = key.Item1,
                        Mask = DownstreamKeyMaskSetCommand.MaskFlags.Top,
                        Top = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskTop = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskTop = v >= 3 ? 3 : -3;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestMaskBottom()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(300);

                foreach (var key in GetKeyers())
                {
                    double[] testValues = { 1, 0, 5, -5, -9, 9, 4.78 };
                    double[] badValues = { -9.01, 9.01, 9.1, -9.1 };

                    ICommand Setter(double v) => new DownstreamKeyMaskSetCommand()
                    {
                        Index = key.Item1,
                        Mask = DownstreamKeyMaskSetCommand.MaskFlags.Bottom,
                        Bottom = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskBottom = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskBottom = v >= 9 ? 9 : -9;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(300);

                // Repeat in 4:3
                foreach (var key in GetKeyers())
                {
                    double[] testValues = { 1, 0, 2.5, -2.5, -3, 3, 1.78 };
                    double[] badValues = { -3.01, 3.01, 3.1, -3.1 };

                    ICommand Setter(double v) => new DownstreamKeyMaskSetCommand()
                    {
                        Index = key.Item1,
                        Mask = DownstreamKeyMaskSetCommand.MaskFlags.Bottom,
                        Bottom = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskBottom = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskBottom = v >= 3 ? 3 : -3;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestMaskLeft()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(300);

                foreach (var key in GetKeyers())
                {
                    double[] testValues = { 1, 0, 5, -5, -16, 16, 4.78 };
                    double[] badValues = { -16.01, 16.01, 16.1, -16.1 };

                    ICommand Setter(double v) => new DownstreamKeyMaskSetCommand()
                    {
                        Index = key.Item1,
                        Mask = DownstreamKeyMaskSetCommand.MaskFlags.Left,
                        Left = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskLeft = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskLeft = v >= 16 ? 16 : -16;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(300);

                // Repeat in 4:3
                foreach (var key in GetKeyers())
                {
                    double[] testValues = { 1, 0, 2.5, -2.5, -4, 4, 1.78 };
                    double[] badValues = { -4.01, 4.01, 4.1, -4.1 };

                    ICommand Setter(double v) => new DownstreamKeyMaskSetCommand()
                    {
                        Index = key.Item1,
                        Mask = DownstreamKeyMaskSetCommand.MaskFlags.Left,
                        Left = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskLeft = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskLeft = v >= 4 ? 4 : -4;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestMaskRight()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(300);

                foreach (var key in GetKeyers())
                {
                    double[] testValues = { 1, 0, 5, -5, -16, 16, 4.78 };
                    double[] badValues = { -16.01, 16.01, 16.1, -16.1 };

                    ICommand Setter(double v) => new DownstreamKeyMaskSetCommand()
                    {
                        Index = key.Item1,
                        Mask = DownstreamKeyMaskSetCommand.MaskFlags.Right,
                        Right = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskRight = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskRight = v >= 16 ? 16 : -16;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(300);

                // Repeat in 4:3
                foreach (var key in GetKeyers())
                {
                    double[] testValues = { 1, 0, 2.5, -2.5, -4, 4, 1.78 };
                    double[] badValues = { -4.01, 4.01, 4.1, -4.1 };

                    ICommand Setter(double v) => new DownstreamKeyMaskSetCommand()
                    {
                        Index = key.Item1,
                        Mask = DownstreamKeyMaskSetCommand.MaskFlags.Right,
                        Right = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskRight = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.DownstreamKeyers[key.Item1].MaskRight = v >= 4 ? 4 : -4;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }
    }
}