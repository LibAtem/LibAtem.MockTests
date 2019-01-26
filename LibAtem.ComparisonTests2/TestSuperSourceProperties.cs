using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.SuperSource;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using LibAtem.DeviceProfile;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2
{
    [Collection("Client")]
    public class TestSuperSourceProperties
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestSuperSourceProperties(ITestOutputHelper output, AtemClientWrapper client)
        {
            _output = output;
            _client = client;
        }

        private IBMDSwitcherInputSuperSource GetSuperSource(AtemComparisonHelper helper)
        {
            return helper.GetSdkInputsOfType<IBMDSwitcherInputSuperSource>().Select(s => s.Value).FirstOrDefault();
        }

        [Fact]
        public void TestSuperSourceCount()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                var srcs = helper.GetSdkInputsOfType<IBMDSwitcherInputSuperSource>();
                Assert.Equal(srcs.Count, (int) helper.Profile.SuperSource);
                Assert.True(srcs.Count == 0 || srcs.Count == 1); // Tests expect 0 or 1
            }
        }

        private abstract class SuperSourceTestDefinition<T> : TestDefinitionBase<T>
        {
            protected readonly IBMDSwitcherInputSuperSource _sdk;

            public SuperSourceTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper)
            {
                _sdk = ssrc;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new SuperSourcePropertiesGetCommand());
            }
        }

        private class SuperSourceArtCutTestDefinition : SuperSourceTestDefinition<VideoSource>
        {
            public SuperSourceArtCutTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInputCut((long)VideoSource.ColorBars);
            }

            public override VideoSource[] GoodValues()
            {
                return VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.SuperSourceArt | SourceAvailability.KeySource)).ToArray();
            }

            public override ICommand GenerateCommand(VideoSource v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtCutSource,
                    ArtCutSource = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                    state.SuperSource.ArtKeyInput = v;
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);

                return new CommandQueueKey[0];
            }
        }

        [SkippableFact]
        public void TestArtCut()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                // We track this another way
                _BMDSwitcherInputAvailability availabilityMask = 0;
                ssrc.GetCutInputAvailabilityMask(ref availabilityMask);
                Assert.Equal(availabilityMask, _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityInputCut | _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilitySuperSourceArt);

                new SuperSourceArtCutTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceArtFillTestDefinition : SuperSourceTestDefinition<VideoSource>
        {
            public SuperSourceArtFillTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInputFill((long)VideoSource.ColorBars);
            }

            public override VideoSource[] GoodValues()
            {
                return VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.SuperSourceArt)).ToArray();
            }

            public override ICommand GenerateCommand(VideoSource v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtFillSource,
                    ArtFillSource = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                {
                    state.SuperSource.ArtFillInput = v;
                    if (VideoSourceLists.MediaPlayers.Contains(v))
                        state.SuperSource.ArtKeyInput = v + 1;
                }
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);

                return new CommandQueueKey[0];
            }
        }

        [SkippableFact]
        public void TestArtFill()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                // We track this another way
                _BMDSwitcherInputAvailability availabilityMask = 0;
                ssrc.GetFillInputAvailabilityMask(ref availabilityMask);
                Assert.Equal(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilitySuperSourceArt, availabilityMask);

                new SuperSourceArtFillTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceArtOptionTestDefinition : SuperSourceTestDefinition<SuperSourceArtOption>
        {
            public SuperSourceArtOptionTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetArtOption(_BMDSwitcherSuperSourceArtOption.bmdSwitcherSuperSourceArtOptionForeground);
            }

            public override SuperSourceArtOption[] GoodValues()
            {
                return new[]
                {
                    SuperSourceArtOption.Background,
                    SuperSourceArtOption.Foreground
                };
            }

            public override ICommand GenerateCommand(SuperSourceArtOption v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtOption,
                    ArtOption = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, SuperSourceArtOption v)
            {
                state.SuperSource.ArtOption = v;
            }
        }

        [SkippableFact]
        public void TestArtOption()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceArtOptionTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceArtPreMultipliedTestDefinition : SuperSourceTestDefinition<bool>
        {
            public SuperSourceArtPreMultipliedTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetPreMultiplied(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtPreMultiplied,
                    ArtPreMultiplied = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.SuperSource.ArtPreMultiplied = v;
            }
        }

        [SkippableFact]
        public void TestArtPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceArtPreMultipliedTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceArtClipTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceArtClipTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetClip(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtClip,
                    ArtClip= v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.SuperSource.ArtClip = v;
                else
                    state.SuperSource.ArtClip = v >= 100 ? 100 : 0;
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

        [SkippableFact]
        public void TestClip()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceArtClipTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceArtGainTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceArtGainTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetGain(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtGain,
                    ArtGain = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.SuperSource.ArtGain = v;
                else
                    state.SuperSource.ArtGain = v >= 100 ? 100 : 0;
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

        [SkippableFact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceArtGainTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceArtInvertKeyTestDefinition : SuperSourceTestDefinition<bool>
        {
            public SuperSourceArtInvertKeyTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetInverse(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.ArtInvertKey,
                    ArtInvertKey = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.SuperSource.ArtInvertKey = v;
            }
        }

        [SkippableFact]
        public void TestArtInvertKey()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceArtInvertKeyTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceBorderEnabledTestDefinition : SuperSourceTestDefinition<bool>
        {
            public SuperSourceBorderEnabledTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderEnabled(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderEnabled,
                    BorderEnabled = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.SuperSource.BorderEnabled = v;
            }
        }

        [SkippableFact]
        public void TestBorderEnabled()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceBorderEnabledTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceBorderBevelTestDefinition : SuperSourceTestDefinition<BorderBevel>
        {
            public SuperSourceBorderBevelTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderBevel(_BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionInOut);
            }

            public override BorderBevel[] GoodValues()
            {
                return new[]
                {
                    BorderBevel.In,
                    BorderBevel.InOut,
                    BorderBevel.None,
                    BorderBevel.Out
                };
            }

            public override ICommand GenerateCommand(BorderBevel v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderBevel,
                    BorderBevel = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, BorderBevel v)
            {
                state.SuperSource.BorderBevel = v;
            }
        }

        [SkippableFact]
        public void TestBorderBevel()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceBorderBevelTestDefinition(helper, ssrc).Run();
            }
        }

        private abstract class SuperSourceBorderWidthTestDefinition : SuperSourceTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public SuperSourceBorderWidthTestDefinition(AtemComparisonHelper helper, VideoMode mode, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
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
                _sdk.SetBorderWidthIn(2);
                _sdk.SetBorderWidthOut(2);
                _helper.Sleep();
            }

            protected double ClampValueToRange(bool goodValue, double v)
            {
                if (goodValue)
                    return v;

                ushort v2 = (ushort)(v * 100);
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return v2 > 1600 ? 16 : 0;
                    case VideoMode.P625i50PAL:
                        return v2 > 400 ? 4 : 0;
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
                        return new double[] { 0, 0.01, 1, 15.99, 15.9, 15, 9.4, 12.7, 16 };
                    case VideoMode.P625i50PAL:
                        return new double[] { 0, 0.01, 1, 3.99, 3.9, 3, 2.7, 4 };
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
                        return new double[] { -0.01, -1, 16.1, 16.01, 17 };
                    case VideoMode.P625i50PAL:
                        return new double[] { -0.01, -1, 4.1, 4.01, 6 };
                    default:
                        throw new NotSupportedException();
                }
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, double v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);

                return new CommandQueueKey[0];
            }
        }

        private class SuperSourceBorderWidthInTestDefinition : SuperSourceBorderWidthTestDefinition
        {
            public SuperSourceBorderWidthInTestDefinition(AtemComparisonHelper helper, VideoMode mode, IBMDSwitcherInputSuperSource ssrc) : base(helper, mode, ssrc)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderInnerWidth,
                    BorderInnerWidth = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.SuperSource.BorderWidthIn = ClampValueToRange(goodValue, v);
            }
        }

        [SkippableFact]
        public void TestBorderWidthIn()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                foreach (var mode in SuperSourceBorderWidthTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    new SuperSourceBorderWidthInTestDefinition(helper, mode, ssrc).Run();
                }
            }
        }

        private class SuperSourceBorderWidthOutTestDefinition : SuperSourceBorderWidthTestDefinition
        {
            public SuperSourceBorderWidthOutTestDefinition(AtemComparisonHelper helper, VideoMode mode, IBMDSwitcherInputSuperSource ssrc) : base(helper, mode, ssrc)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderOuterWidth,
                    BorderOuterWidth = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.SuperSource.BorderWidthOut = ClampValueToRange(goodValue, v);
            }
        }

        [SkippableFact]
        public void TestBorderWidthOut()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                foreach (var mode in SuperSourceBorderWidthTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    new SuperSourceBorderWidthOutTestDefinition(helper, mode, ssrc).Run();
                }
            }
        }

        private abstract class SuperSourceUint100TestDefinition : SuperSourceTestDefinition<uint>
        {
            public SuperSourceUint100TestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderSoftnessIn(0.5);
                _sdk.SetBorderSoftnessOut(0.5);
                _sdk.SetBorderBevelSoftness(0.5);
                _sdk.SetBorderBevelPosition(0.5);
                _helper.Sleep();
            }

            protected uint ClampValueToRange(bool goodValue, uint v)
            {
                if (goodValue)
                    return v;

                return v > 100 ? 100 : (uint)0;
            }

            public override uint[] GoodValues()
            {
                return new uint[]{ 0, 87, 14, 99, 100, 1 };
            }

            public override uint[] BadValues()
            {
                return new uint[] { 101, 110 };
            }
        }

        private class SuperSourceBorderSoftnessOutTestDefinition : SuperSourceUint100TestDefinition
        {
            public SuperSourceBorderSoftnessOutTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override ICommand GenerateCommand(uint v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderOuterSoftness,
                    BorderOuterSoftness = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                state.SuperSource.BorderSoftnessOut = ClampValueToRange(goodValue, v);
            }
        }

        [SkippableFact]
        public void TestBorderSoftnessOut()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceBorderSoftnessOutTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceBorderSoftnessInTestDefinition : SuperSourceUint100TestDefinition
        {
            public SuperSourceBorderSoftnessInTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override ICommand GenerateCommand(uint v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderInnerSoftness,
                    BorderInnerSoftness = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                state.SuperSource.BorderSoftnessIn = ClampValueToRange(goodValue, v);
            }
        }

        [SkippableFact]
        public void TestBorderSoftnessIn()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceBorderSoftnessInTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceBorderBevelSoftnessTestDefinition : SuperSourceUint100TestDefinition
        {
            public SuperSourceBorderBevelSoftnessTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override ICommand GenerateCommand(uint v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderBevelSoftness,
                    BorderBevelSoftness = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                state.SuperSource.BorderBevelSoftness = ClampValueToRange(goodValue, v);
            }
        }

        [SkippableFact]
        public void TestBorderBevelSoftness()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceBorderBevelSoftnessTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceBorderBevelPositionTestDefinition : SuperSourceUint100TestDefinition
        {
            public SuperSourceBorderBevelPositionTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override ICommand GenerateCommand(uint v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderBevelPosition,
                    BorderBevelPosition = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                state.SuperSource.BorderBevelPosition = ClampValueToRange(goodValue, v);
            }
        }

        [SkippableFact]
        public void TestBorderBevelPosition()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceBorderBevelPositionTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceBorderHueTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceBorderHueTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderHue(20);
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 123, 233.4, 359.9 };
            }
            public override double[] BadValues()
            {
                return new double[] { 360, 360.1, 361, -1, -0.01 };
            }

            public override ICommand GenerateCommand(double v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderHue,
                    BorderHue = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.SuperSource.BorderHue = v;
                }
                else
                {
                    ushort ui = (ushort)((ushort)(v * 10) % 3600);
                    state.SuperSource.BorderHue = ui / 10d;
                }
            }
        }

        [SkippableFact]
        public void TestBorderHue()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceBorderHueTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceBorderSaturationTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceBorderSaturationTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderSaturation(20);
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
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderSaturation,
                    BorderSaturation = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.SuperSource.BorderSaturation = v;
                }
                else
                {
                    state.SuperSource.BorderSaturation = v >= 100 ? 100 : 0;
                }
            }
        }

        [SkippableFact]
        public void TestBorderSaturation()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceBorderSaturationTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceBorderLumaTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceBorderLumaTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderLuma(20);
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
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderLuma,
                    BorderLuma = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.SuperSource.BorderLuma = v;
                }
                else
                {
                    state.SuperSource.BorderLuma = v >= 100 ? 100 : 0;
                }
            }
        }

        [SkippableFact]
        public void TestBorderLuma()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceBorderLumaTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceBorderLightSourceDirectionTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceBorderLightSourceDirectionTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderLightSourceDirection(20);
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 123, 233.4, 359.9 };
            }
            public override double[] BadValues()
            {
                return new double[] { 360, 360.1, 361, -1, -0.01 };
            }

            public override ICommand GenerateCommand(double v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderLightSourceDirection,
                    BorderLightSourceDirection = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.SuperSource.BorderLightSourceDirection = v;
                }
                else
                {
                    state.SuperSource.BorderLightSourceDirection = 0;
                }
            }
        }

        [SkippableFact]
        public void TestBorderLightSourceDirection()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceBorderLightSourceDirectionTestDefinition(helper, ssrc).Run();
            }
        }

        private class SuperSourceBorderLightSourceAltitudeTestDefinition : SuperSourceTestDefinition<uint>
        {
            public SuperSourceBorderLightSourceAltitudeTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderLightSourceAltitude(20);
            }

            public override uint[] GoodValues()
            {
                return new uint[] { 10, 100, 34, 99, 11, 78 };
            }
            public override uint[] BadValues()
            {
                return new uint[] { 101, 110, 0, 9 };
            }

            public override ICommand GenerateCommand(uint v)
            {
                return new SuperSourcePropertiesSetCommand
                {
                    Mask = SuperSourcePropertiesSetCommand.MaskFlags.BorderLightSourceAltitude,
                    BorderLightSourceAltitude = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                if (goodValue)
                {
                    state.SuperSource.BorderLightSourceAltitude = v;
                }
                else
                {
                    state.SuperSource.BorderLightSourceAltitude = v < 10 ? 10 : 100;
                }
            }
        }

        [SkippableFact]
        public void TestBorderLightSourceAltitude()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);
                Skip.If(ssrc == null, "Model does not support SuperSource");

                new SuperSourceBorderLightSourceAltitudeTestDefinition(helper, ssrc).Run();
            }
        }
    }
}