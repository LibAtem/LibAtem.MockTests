using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.SuperSource;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests
{
    [Collection("Client")]
    public class TestSuperSource
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestSuperSource(ITestOutputHelper output, AtemClientWrapper client)
        {
            _output = output;
            _client = client;
        }

        private IBMDSwitcherInputSuperSource GetSuperSource(AtemComparisonHelper helper)
        {
            return helper.GetSdkInputsOfType<IBMDSwitcherInputSuperSource>().Select(s => s.Value).FirstOrDefault();
        }


        private IEnumerable<Tuple<SuperSourceBoxId, IBMDSwitcherSuperSourceBox>> GetSuperSourceBoxes(AtemComparisonHelper helper)
        {
            IBMDSwitcherInputSuperSource src = GetSuperSource(helper);
            if (src == null)
                yield break;

            Guid itId = typeof(IBMDSwitcherSuperSourceBoxIterator).GUID;
            src.CreateIterator(ref itId, out IntPtr itPtr);
            IBMDSwitcherSuperSourceBoxIterator iterator = (IBMDSwitcherSuperSourceBoxIterator)Marshal.GetObjectForIUnknown(itPtr);

            SuperSourceBoxId o = 0;
            for (iterator.Next(out IBMDSwitcherSuperSourceBox r); r != null; iterator.Next(out r))
            {
                yield return Tuple.Create(o, r);

                o++;
            }
        }

        #region Properties

        [Fact]
        public void TestSuperSourceCount()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                var srcs = helper.GetSdkInputsOfType<IBMDSwitcherInputSuperSource>();
                Assert.Single(srcs); // Tests expect just one
                Assert.Equal(srcs.Count, (int) helper.Profile.SuperSource);
            }
        }

        [Fact]
        public void TestInputCut()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                // We track this another way
                _BMDSwitcherInputAvailability availabilityMask = 0;
                ssrc.GetCutInputAvailabilityMask(ref availabilityMask);
                Assert.Equal(availabilityMask, _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityInputCut | _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilitySuperSourceArt);

                VideoSource[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(_client.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.SuperSourceArt | SourceAvailability.KeySource)).ToArray();
                VideoSource[] badValues = VideoSourceLists.All.Where(s => !testValues.Contains(s)).ToArray();

                ICommand Setter(VideoSource v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtCutSource,
                    ArtCutSource = v,
                };

                void UpdateExpectedState(ComparisonState state, VideoSource v) => state.SuperSource.ArtKeyInput = v;

                ValueTypeComparer<VideoSource>.Run(helper, Setter, UpdateExpectedState, testValues);
                ValueTypeComparer<VideoSource>.Fail(helper, Setter, badValues);

            }
        }

        [Fact]
        public void TestInputFill()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                // We track this another way
                _BMDSwitcherInputAvailability availabilityMask = 0;
                ssrc.GetFillInputAvailabilityMask(ref availabilityMask);
                Assert.Equal(availabilityMask, _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilitySuperSourceArt);

                VideoSource[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(_client.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.SuperSourceArt)).ToArray();
                VideoSource[] badValues = VideoSourceLists.All.Where(s => !testValues.Contains(s)).ToArray();

                ICommand Setter(VideoSource v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtFillSource,
                    ArtFillSource = v,
                };

                void UpdateExpectedState(ComparisonState state, VideoSource v)
                {
                    state.SuperSource.ArtFillInput = v;
                    if (VideoSourceLists.MediaPlayers.Contains(v))
                        state.SuperSource.ArtKeyInput = v + 1;
                }

                ValueTypeComparer<VideoSource>.Run(helper, Setter, UpdateExpectedState, testValues);
                ValueTypeComparer<VideoSource>.Fail(helper, Setter, badValues);

            }
        }

        [Fact]
        public void TestArtOption()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                ICommand Setter(SuperSourceArtOption v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtOption,
                    ArtOption = v,
                };

                void UpdateExpectedState(ComparisonState state, SuperSourceArtOption v) => state.SuperSource.ArtOption = v;

                var testValues = Enum.GetValues(typeof(SuperSourceArtOption)).OfType<SuperSourceArtOption>().ToArray();
                ValueTypeComparer<SuperSourceArtOption>.Run(helper, Setter, UpdateExpectedState, testValues);
            }
        }

        [Fact]
        public void TestArtPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                ICommand Setter(bool v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtPreMultiplied,
                    ArtPreMultiplied = v,
                };

                void UpdateExpectedState(ComparisonState state, bool v) => state.SuperSource.ArtPreMultiplied = v;

                var testValues = new[] {true, false};
                ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
            }
        }

        [Fact]
        public void TestClip()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.1 };
                double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                ICommand Setter(double v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtClip,
                    ArtClip = v,
                };

                void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.ArtClip = v;
                void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.ArtClip = v >= 100 ? 100 : 0;

                ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
            }
        }

        [Fact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.1 };
                double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                ICommand Setter(double v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtGain,
                    ArtGain = v,
                };

                void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.ArtGain = v;
                void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.ArtGain = v >= 100 ? 100 : 0;

                ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
            }
        }

        [Fact]
        public void TestArtInvertKey()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                ICommand Setter(bool v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtInvertKey,
                    ArtInvertKey = v,
                };

                void UpdateExpectedState(ComparisonState state, bool v) => state.SuperSource.ArtInvertKey = v;

                var testValues = new[] { true, false };
                ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
            }
        }

        [Fact]
        public void TestBorderEnabled()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                ICommand Setter(bool v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderEnabled,
                    BorderEnabled = v,
                };

                void UpdateExpectedState(ComparisonState state, bool v) => state.SuperSource.BorderEnabled = v;

                var testValues = new[] { true, false };
                ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
            }
        }

        [Fact]
        public void TestBorderBevel()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                BorderBevel[] testValues = Enum.GetValues(typeof(BorderBevel)).OfType<BorderBevel>().ToArray();

                ICommand Setter(BorderBevel v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderBevel,
                    BorderBevel = v,
                };

                void UpdateExpectedState(ComparisonState state, BorderBevel v) => state.SuperSource.BorderBevel = v;

                ValueTypeComparer<BorderBevel>.Run(helper, Setter, UpdateExpectedState, testValues);
            }
        }

        [Fact]
        public void TestBorderWidthIn()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(300);

                {
                    double[] testValues = {0, 0.01, 1, 15.99, 15.9, 15, 9.4, 12.7, 16};
                    double[] badValues = {-0.01, -1, 16.1, 16.01, 17};

                    ICommand Setter(double v) => new SuperSourcePropertiesSetCommand
                    {
                        Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderInnerWidth,
                        BorderInnerWidth = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.BorderWidthIn = v;

                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort) (v * 100);
                        state.SuperSource.BorderWidthIn = ui > 1600 ? 16 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(300);

                {
                    double[] testValues = { 0, 0.01, 1, 3.99, 3.9, 3, 2.7, 4 };
                    double[] badValues = { -0.01, -1, 4.1, 4.01, 6 };

                    ICommand Setter(double v) => new SuperSourcePropertiesSetCommand
                    {
                        Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderInnerWidth,
                        BorderInnerWidth = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.BorderWidthIn = v;

                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.SuperSource.BorderWidthIn = ui > 400 ? 4 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBorderWidthOut()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(300);

                {
                    double[] testValues = { 0, 0.01, 1, 15.99, 15.9, 15, 9.4, 12.7, 16 };
                    double[] badValues = { -0.01, -1, 16.1, 16.01, 17 };

                    ICommand Setter(double v) => new SuperSourcePropertiesSetCommand
                    {
                        Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderOuterWidth,
                        BorderOuterWidth = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.BorderWidthOut = v;

                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.SuperSource.BorderWidthOut = ui > 1600 ? 16 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(300);

                {
                    double[] testValues = { 0, 0.01, 1, 3.99, 3.9, 3, 2.7, 4 };
                    double[] badValues = { -0.01, -1, 4.1, 4.01, 6 };

                    ICommand Setter(double v) => new SuperSourcePropertiesSetCommand
                    {
                        Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderOuterWidth,
                        BorderOuterWidth = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.BorderWidthOut = v;

                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.SuperSource.BorderWidthOut = ui > 400 ? 4 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBorderSoftnessOut()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                uint[] testValues = { 0, 87, 14, 99, 100, 1 };
                uint[] badValues = { 101, 110 };

                ICommand Setter(uint v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderOuterSoftness,
                    BorderOuterSoftness = v,
                };

                void UpdateExpectedState(ComparisonState state, uint v) => state.SuperSource.BorderSoftnessOut = v;
                void UpdateFailedState(ComparisonState state, uint v) => state.SuperSource.BorderSoftnessOut = v > 100 ? 100 : (uint)0;

                ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
            }
        }

        [Fact]
        public void TestBorderSoftnessIn()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                uint[] testValues = { 0, 87, 14, 99, 100, 1 };
                uint[] badValues = { 101, 110 };

                ICommand Setter(uint v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderInnerSoftness,
                    BorderInnerSoftness = v,
                };

                void UpdateExpectedState(ComparisonState state, uint v) => state.SuperSource.BorderSoftnessIn = v;
                void UpdateFailedState(ComparisonState state, uint v) => state.SuperSource.BorderSoftnessIn = v > 100 ? 100 : (uint)0;

                ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
            }
        }

        [Fact]
        public void TestBorderBevelSoftness()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                uint[] testValues = { 0, 87, 14, 99, 100, 1 };
                uint[] badValues = { 101, 110 };

                ICommand Setter(uint v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderBevelSoftness,
                    BorderBevelSoftness = v,
                };

                void UpdateExpectedState(ComparisonState state, uint v) => state.SuperSource.BorderBevelSoftness = v;
                void UpdateFailedState(ComparisonState state, uint v) => state.SuperSource.BorderBevelSoftness = v > 100 ? 100 : (uint)0;

                ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
            }
        }

        [Fact]
        public void TestBorderBevelPosition()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                uint[] testValues = { 0, 87, 14, 99, 100, 1 };
                uint[] badValues = { 101, 110 };

                ICommand Setter(uint v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderBevelPosition,
                    BorderBevelPosition = v,
                };

                void UpdateExpectedState(ComparisonState state, uint v) => state.SuperSource.BorderBevelPosition = v;
                void UpdateFailedState(ComparisonState state, uint v) => state.SuperSource.BorderBevelPosition = v > 100 ? 100 : (uint)0;

                ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
            }
        }


        [Fact]
        public void TestDVEKeyerBorderHue()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                double[] testValues = {0, 123, 233.4, 359.9};
                double[] badValues = {360, 360.1, 361, -1, -0.01};

                ICommand Setter(double v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderHue,
                    BorderHue = v,
                };

                void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.BorderHue = v;

                void UpdateFailedState(ComparisonState state, double v)
                {
                    ushort ui = (ushort) ((ushort) (v * 10) % 3600);
                    state.SuperSource.BorderHue = ui / 10d;
                }

                ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
            }
        }

        [Fact]
        public void TestDVEKeyerBorderSaturation()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                double[] testValues = {0, 87.4, 14.7, 99.9, 100, 0.1};
                double[] badValues = {100.1, 110, 101, -0.01, -1, -10};

                ICommand Setter(double v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderSaturation,
                    BorderSaturation = v,
                };

                void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.BorderSaturation = v;
                void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.BorderSaturation = v >= 100 ? 100 : 0;

                ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
            }
        }

        [Fact]
        public void TestDVEKeyerBorderLuma()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                double[] testValues = {0, 87.4, 14.7, 99.9, 100, 0.1};
                double[] badValues = {100.1, 110, 101, -0.01, -1, -10};

                ICommand Setter(double v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderLuma,
                    BorderLuma = v,
                };

                void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.BorderLuma = v;
                void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.BorderLuma = v >= 100 ? 100 : 0;

                ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
            }
        }

        [Fact]
        public void TestDVEKeyerLightSourceDirection()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                double[] testValues = {0, 123, 233.4, 359.9};
                double[] badValues = {360, 360.1, 361, -1, -0.01};

                ICommand Setter(double v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderLightSourceDirection,
                    BorderLightSourceDirection = v,
                };

                void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.BorderLightSourceDirection = v;
                void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.BorderLightSourceDirection = 0;

                ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
            }
        }

        [Fact]
        public void TestDVEKeyerLightSourceAltitude()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                if (ssrc == null)
                    return;

                uint[] testValues = {10, 100, 34, 99, 11, 78};
                 uint[] badValues = {101, 110, 0, 9};

                ICommand Setter(uint v) => new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderLightSourceAltitude,
                    BorderLightSourceAltitude = v,
                };

                void UpdateExpectedState(ComparisonState state, uint v) => state.SuperSource.BorderLightSourceAltitude = v;

                void UpdateFailedState(ComparisonState state, uint v) => state.SuperSource.BorderLightSourceAltitude = v < 10 ? 10 : 100;

                ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                // Note: Limits are not enforced by atem
                ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
            }
        }

        #endregion Properties

        #region Box

        [Fact]
        public void TestBoxEnabled()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    ICommand Setter(bool v) => new SuperSourceBoxSetCommand
                    {
                        Mask = SuperSourceBoxSetCommand.MaskFlags.Enabled,
                        Index = box.Item1,
                        Enabled = v,
                    };

                    void UpdateExpectedState(ComparisonState state, bool v) => state.SuperSource.Boxes[box.Item1].Enabled = v;

                    var testValues = new[] {true, false};
                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);

                }
            }
        }

        [Fact]
        public void TestBoxInputSource()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                    // We track this another way
                    _BMDSwitcherInputAvailability availabilityMask = 0;
                    box.Item2.GetInputAvailabilityMask(ref availabilityMask);
                    Assert.Equal(availabilityMask, _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilitySuperSourceBox);

                    VideoSource[] testValues = VideoSourceLists.All.Where(s => s.IsAvailable(_client.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.SuperSourceBox)).ToArray();
                    VideoSource[] badValues = VideoSourceLists.All.Where(s => !testValues.Contains(s)).ToArray();

                    ICommand Setter(VideoSource v) => new SuperSourceBoxSetCommand
                    {
                        Mask = SuperSourceBoxSetCommand.MaskFlags.Source,
                        Index = box.Item1,
                        Source = v,
                    };

                    void UpdateExpectedState(ComparisonState state, VideoSource v) => state.SuperSource.Boxes[box.Item1].InputSource = v;

                    ValueTypeComparer<VideoSource>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<VideoSource>.Fail(helper, Setter, badValues);
                }
            }
        }

        [Fact]
        public void TestBoxPositionX()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(300);

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    double[] testValues = {0, 0.87, 48, 47.99, -48, -47.99, 9.65};
                    double[] badValues = {-48.01, 48.01, 48.1, -48.1, -55, 55};

                    ICommand Setter(double v) => new SuperSourceBoxSetCommand
                    {
                        Index = box.Item1,
                        Mask = SuperSourceBoxSetCommand.MaskFlags.PositionX,
                        PositionX = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].PositionX = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].PositionX = v >= 48 ? 48 : -48;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(300);

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    double[] testValues = { 0, 0.87, 12, 11.99, -12, -11.99, 9.65 };
                    double[] badValues = { -12.01, 12.01, 12.1, -12.1, -15, 15 };

                    ICommand Setter(double v) => new SuperSourceBoxSetCommand
                    {
                        Index = box.Item1,
                        Mask = SuperSourceBoxSetCommand.MaskFlags.PositionX,
                        PositionX = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].PositionX = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].PositionX = v >= 12 ? 12 : -12;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBoxPositionY()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(300);

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    double[] testValues = { 0, 0.87, 34, 33.99, -34, -33.99, 9.65 };
                    double[] badValues = { -34.01, 34.01, 34.1, -34.1, -39, 39 };

                    ICommand Setter(double v) => new SuperSourceBoxSetCommand
                    {
                        Index = box.Item1,
                        Mask = SuperSourceBoxSetCommand.MaskFlags.PositionY,
                        PositionY = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].PositionY = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].PositionY = v >= 34 ? 34 : -34;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(300);

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    double[] testValues = { 0, 0.87, 10, 9.99, -10, -9.99, 6.65 };
                    double[] badValues = { -10.01, 10.01, 10.1, -10.1, -15, 15 };

                    ICommand Setter(double v) => new SuperSourceBoxSetCommand
                    {
                        Index = box.Item1,
                        Mask = SuperSourceBoxSetCommand.MaskFlags.PositionY,
                        PositionY = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].PositionY = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].PositionY = v >= 10 ? 10 : -10;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBoxSize()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    double[] testValues = { 0.07, 0.874, 0.147, 0.999, 1.00 };
                    double[] badValues = { 0, 0.06, 1.001, 1.1, 1.01, -0.01, -1, -0.10 };

                    ICommand Setter(double v) => new SuperSourceBoxSetCommand
                    {
                        Index = box.Item1,
                        Mask = SuperSourceBoxSetCommand.MaskFlags.Size,
                        Size = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].Size = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].Size = v >= 1 || v < 0 ? 1 : 0.07;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBoxCropped()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    ICommand Setter(bool v) => new SuperSourceBoxSetCommand
                    {
                        Mask = SuperSourceBoxSetCommand.MaskFlags.Cropped,
                        Index = box.Item1,
                        Cropped = v,
                    };

                    void UpdateExpectedState(ComparisonState state, bool v) => state.SuperSource.Boxes[box.Item1].Cropped = v;

                    var testValues = new[] { true, false };
                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);

                }
            }
        }

        [Fact]
        public void TestBoxCropTop()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(300);

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    double[] testValues = { 0, 0.87, 18, 17.99, 0.01, 9.65 };
                    double[] badValues = { -0.01, 18.01, 18.1, -0.1, -29, 29 };

                    ICommand Setter(double v) => new SuperSourceBoxSetCommand
                    {
                        Index = box.Item1,
                        Mask = SuperSourceBoxSetCommand.MaskFlags.CropTop,
                        CropTop = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropTop = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropTop = v >= 18 ? 18 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(300);

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    double[] testValues = { 0, 0.87, 6, 5.99, 0.01, 3.65 };
                    double[] badValues = { -0.01, 6.01, 6.1, -0.1, -15, 15 };

                    ICommand Setter(double v) => new SuperSourceBoxSetCommand
                    {
                        Index = box.Item1,
                        Mask = SuperSourceBoxSetCommand.MaskFlags.CropTop,
                        CropTop = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropTop = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropTop = v >= 6 ? 6 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBoxCropBottom()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(300);

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    double[] testValues = { 0, 0.87, 18, 17.99, 0.01, 9.65 };
                    double[] badValues = { -0.01, 18.01, 18.1, -0.1, -29, 29 };

                    ICommand Setter(double v) => new SuperSourceBoxSetCommand
                    {
                        Index = box.Item1,
                        Mask = SuperSourceBoxSetCommand.MaskFlags.CropBottom,
                        CropBottom = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropBottom = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropBottom = v >= 18 ? 18 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(300);

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    double[] testValues = { 0, 0.87, 6, 5.99, 0.01, 3.65 };
                    double[] badValues = { -0.01, 6.01, 6.1, -0.1, -15, 15 };

                    ICommand Setter(double v) => new SuperSourceBoxSetCommand
                    {
                        Index = box.Item1,
                        Mask = SuperSourceBoxSetCommand.MaskFlags.CropBottom,
                        CropBottom = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropBottom = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropBottom = v >= 6 ? 6 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBoxCropLeft()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(300);

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    double[] testValues = { 0, 0.87, 32, 31.99, 0.01, 9.65 };
                    double[] badValues = { -0.01, 32.01, 32.1, -0.1, -29 };

                    ICommand Setter(double v) => new SuperSourceBoxSetCommand
                    {
                        Index = box.Item1,
                        Mask = SuperSourceBoxSetCommand.MaskFlags.CropLeft,
                        CropLeft = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropLeft = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropLeft = v >= 32 ? 32 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(300);

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    double[] testValues = { 0, 0.87, 8, 7.99, 0.01, 3.65 };
                    double[] badValues = { -0.01, 8.01, 8.1, -0.1, -15, 15 };

                    ICommand Setter(double v) => new SuperSourceBoxSetCommand
                    {
                        Index = box.Item1,
                        Mask = SuperSourceBoxSetCommand.MaskFlags.CropLeft,
                        CropLeft = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropLeft = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropLeft = v >= 8 ? 8 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestBoxCropRight()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(300);

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    double[] testValues = { 0, 0.87, 32, 31.99, 0.01, 9.65 };
                    double[] badValues = { -0.01, 32.01, 32.1, -0.1, -29 };

                    ICommand Setter(double v) => new SuperSourceBoxSetCommand
                    {
                        Index = box.Item1,
                        Mask = SuperSourceBoxSetCommand.MaskFlags.CropRight,
                        CropRight = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropRight = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropRight = v >= 32 ? 32 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                _client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(300);

                foreach (var box in GetSuperSourceBoxes(helper))
                {
                    double[] testValues = { 0, 0.87, 8, 7.99, 0.01, 3.65 };
                    double[] badValues = { -0.01, 8.01, 8.1, -0.1, -15, 15 };

                    ICommand Setter(double v) => new SuperSourceBoxSetCommand
                    {
                        Index = box.Item1,
                        Mask = SuperSourceBoxSetCommand.MaskFlags.CropRight,
                        CropRight = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropRight = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.SuperSource.Boxes[box.Item1].CropRight = v >= 8 ? 8 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        #endregion Box

    }
}