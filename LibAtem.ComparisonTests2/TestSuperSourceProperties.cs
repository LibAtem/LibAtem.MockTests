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
            var ssrc = helper.GetSdkInputsOfType<IBMDSwitcherInputSuperSource>().Select(s => s.Value).SingleOrDefault();
            Skip.If(ssrc == null, "Model does not support SuperSource");
            return ssrc;
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

        private abstract class SuperSourceTestDefinition<T> : TestDefinitionBase<SuperSourcePropertiesSetCommand, T>
        {
            protected readonly IBMDSwitcherInputSuperSource _sdk;

            public SuperSourceTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper)
            {
                _sdk = ssrc;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, T v)
            {
                SetCommandProperty(state.SuperSource, PropertyName, goodValue ? v : MangleBadValue(v));
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
                // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                // We track this another way
                _BMDSwitcherInputAvailability availabilityMask = 0;
                ssrc.GetCutInputAvailabilityMask(ref availabilityMask);
                Assert.Equal(availabilityMask, _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilityInputCut | _BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilitySuperSourceArt);
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInputCut((long)VideoSource.ColorBars);

            public override string PropertyName => "ArtCutSource";

            public override VideoSource[] GoodValues => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.SuperSourceArt | SourceAvailability.KeySource)).ToArray();

            public override VideoSource MangleBadValue(VideoSource v) => v;
            public override void UpdateExpectedState(ComparisonState state, bool goodValue, VideoSource v)
            {
                if (goodValue) state.SuperSource.ArtKeyInput = v;
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
                new SuperSourceArtCutTestDefinition(helper, GetSuperSource(helper)).Run();
        }

        private class SuperSourceArtFillTestDefinition : SuperSourceTestDefinition<VideoSource>
        {
            public SuperSourceArtFillTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
                // GetInputAvailabilityMask is used when checking if another input can be used for this output.
                // We track this another way
                _BMDSwitcherInputAvailability availabilityMask = 0;
                ssrc.GetFillInputAvailabilityMask(ref availabilityMask);
                Assert.Equal(_BMDSwitcherInputAvailability.bmdSwitcherInputAvailabilitySuperSourceArt, availabilityMask);
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInputFill((long)VideoSource.ColorBars);

            public override string PropertyName => "ArtFillSource";

            public override VideoSource[] GoodValues => VideoSourceLists.All.Where(s => s.IsAvailable(_helper.Profile, InternalPortType.Mask) && s.IsAvailable(SourceAvailability.SuperSourceArt)).ToArray();

            public override VideoSource MangleBadValue(VideoSource v) => v;
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
                new SuperSourceArtFillTestDefinition(helper, GetSuperSource(helper)).Run();
        }

        private class SuperSourceArtOptionTestDefinition : SuperSourceTestDefinition<SuperSourceArtOption>
        {
            public SuperSourceArtOptionTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetArtOption(_BMDSwitcherSuperSourceArtOption.bmdSwitcherSuperSourceArtOptionForeground);

            public override string PropertyName => "ArtOption";
            public override SuperSourceArtOption MangleBadValue(SuperSourceArtOption v) => v;

            public override SuperSourceArtOption[] GoodValues => Enum.GetValues(typeof(SuperSourceArtOption)).OfType<SuperSourceArtOption>().ToArray();
        }

        [SkippableFact]
        public void TestArtOption()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceArtOptionTestDefinition(helper, GetSuperSource(helper)).Run();
        }

        private class SuperSourceArtPreMultipliedTestDefinition : SuperSourceTestDefinition<bool>
        {
            public SuperSourceArtPreMultipliedTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetPreMultiplied(0);

            public override string PropertyName => "ArtPreMultiplied";
            public override bool MangleBadValue(bool v) => v;
        }

        [SkippableFact]
        public void TestArtPreMultiplied()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceArtPreMultipliedTestDefinition(helper, GetSuperSource(helper)).Run();
        }

        private class SuperSourceArtClipTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceArtClipTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetClip(20);

            public override string PropertyName => "ArtClip";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [SkippableFact]
        public void TestClip()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceArtClipTestDefinition(helper, GetSuperSource(helper)).Run();
        }

        private class SuperSourceArtGainTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceArtGainTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetGain(20);

            public override string PropertyName => "ArtGain";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [SkippableFact]
        public void TestGain()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceArtGainTestDefinition(helper, GetSuperSource(helper)).Run();
        }

        private class SuperSourceArtInvertKeyTestDefinition : SuperSourceTestDefinition<bool>
        {
            public SuperSourceArtInvertKeyTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetInverse(0);

            public override string PropertyName => "ArtInvertKey";
            public override bool MangleBadValue(bool v) => v;
        }

        [SkippableFact]
        public void TestArtInvertKey()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceArtInvertKeyTestDefinition(helper, GetSuperSource(helper)).Run();
        }

        private class SuperSourceBorderEnabledTestDefinition : SuperSourceTestDefinition<bool>
        {
            public SuperSourceBorderEnabledTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderEnabled(0);

            public override string PropertyName => "BorderEnabled";
            public override bool MangleBadValue(bool v) => v;
        }

        [SkippableFact]
        public void TestBorderEnabled()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceBorderEnabledTestDefinition(helper, GetSuperSource(helper)).Run();
        }

        private class SuperSourceBorderBevelTestDefinition : SuperSourceTestDefinition<BorderBevel>
        {
            public SuperSourceBorderBevelTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderBevel(_BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionInOut);

            public override string PropertyName => "BorderBevel";
            public override BorderBevel MangleBadValue(BorderBevel v) => v;

            public override BorderBevel[] GoodValues => Enum.GetValues(typeof(BorderBevel)).OfType<BorderBevel>().ToArray();
        }

        [SkippableFact]
        public void TestBorderBevel()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceBorderBevelTestDefinition(helper, GetSuperSource(helper)).Run();
        }

        private class SuperSourceBorderWidthTestDefinition : SuperSourceTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public override string PropertyName { get; }

            public SuperSourceBorderWidthTestDefinition(AtemComparisonHelper helper, VideoMode mode, IBMDSwitcherInputSuperSource ssrc, string propName) : base(helper, ssrc)
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
                _sdk.SetBorderWidthIn(2);
                _sdk.SetBorderWidthOut(2);
                _helper.Sleep();
            }

            public override double MangleBadValue(double v)
            {
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

            public override double[] GoodValues
            {
                get
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
            }

            public override double[] BadValues
            {
                get
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
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, double v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);

                return new CommandQueueKey[0];
            }
        }

        [SkippableFact]
        public void TestBorderWidthIn()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);

                foreach (var mode in SuperSourceBorderWidthTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    new SuperSourceBorderWidthTestDefinition(helper, mode, ssrc, "BorderInnerWidth").Run();
                }
            }
        }

        [SkippableFact]
        public void TestBorderWidthOut()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherInputSuperSource ssrc = GetSuperSource(helper);

                foreach (var mode in SuperSourceBorderWidthTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    new SuperSourceBorderWidthTestDefinition(helper, mode, ssrc, "BorderOuterWidth").Run();
                }
            }
        }

        private class SuperSourceUint100TestDefinition : SuperSourceTestDefinition<uint>
        {
            public override string PropertyName { get; }

            public SuperSourceUint100TestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc, string propName) : base(helper, ssrc)
            {
                PropertyName = propName;
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

            public override uint MangleBadValue(uint v) => v > 100 ? 100 : (uint)0;

            public override uint[] GoodValues => new uint[]{ 0, 87, 14, 99, 100, 1 };
            public override uint[] BadValues => new uint[] { 101, 110 };
        }

        [SkippableFact]
        public void TestBorderSoftnessOut()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceUint100TestDefinition(helper, GetSuperSource(helper), "BorderOuterSoftness").Run();
        }

        [SkippableFact]
        public void TestBorderSoftnessIn()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceUint100TestDefinition(helper, GetSuperSource(helper), "BorderInnerSoftness").Run();
        }

        [SkippableFact]
        public void TestBorderBevelSoftness()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceUint100TestDefinition(helper, GetSuperSource(helper), "BorderBevelSoftness").Run();
        }

        [SkippableFact]
        public void TestBorderBevelPosition()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceUint100TestDefinition(helper, GetSuperSource(helper), "BorderBevelPosition").Run();
        }

        private class SuperSourceBorderHueTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceBorderHueTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderHue(20);

            public override string PropertyName => "BorderHue";
            public override double MangleBadValue(double v)
            {
                ushort ui = (ushort)((ushort)(v * 10) % 3600);
                return ui / 10d;
            }

            public override double[] GoodValues => new double[] { 0, 123, 233.4, 359.9 };
            public override double[] BadValues => new double[] { 360, 360.1, 361, -1, -0.01 };
        }

        [SkippableFact]
        public void TestBorderHue()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceBorderHueTestDefinition(helper, GetSuperSource(helper)).Run();
        }

        private class SuperSourceBorderSaturationTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceBorderSaturationTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderSaturation(20);

            public override string PropertyName => "BorderSaturation";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [SkippableFact]
        public void TestBorderSaturation()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceBorderSaturationTestDefinition(helper, GetSuperSource(helper)).Run();
        }

        private class SuperSourceBorderLumaTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceBorderLumaTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderLuma(20);

            public override string PropertyName => "BorderLuma";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [SkippableFact]
        public void TestBorderLuma()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceBorderLumaTestDefinition(helper, GetSuperSource(helper)).Run();
        }

        private class SuperSourceBorderLightSourceDirectionTestDefinition : SuperSourceTestDefinition<double>
        {
            public SuperSourceBorderLightSourceDirectionTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderLightSourceDirection(20);

            public override string PropertyName => "BorderLightSourceDirection";
            public override double MangleBadValue(double v) => 0;

            public override double[] GoodValues => new double[] { 0, 123, 233.4, 359.9 };
            public override double[] BadValues => new double[] { 360, 360.1, 361, -1, -0.01 };
        }

        [SkippableFact]
        public void TestBorderLightSourceDirection()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceBorderLightSourceDirectionTestDefinition(helper, GetSuperSource(helper)).Run();
        }

        private class SuperSourceBorderLightSourceAltitudeTestDefinition : SuperSourceTestDefinition<uint>
        {
            public SuperSourceBorderLightSourceAltitudeTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputSuperSource ssrc) : base(helper, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderLightSourceAltitude(20);

            public override string PropertyName => "BorderLightSourceAltitude";
            public override uint MangleBadValue(uint v) => v < 10 ? (uint)10 : 100;

            public override uint[] GoodValues => new uint[] { 10, 100, 34, 99, 11, 78 };
            public override uint[] BadValues => new uint[] { 101, 110, 0, 9 };
        }

        [SkippableFact]
        public void TestBorderLightSourceAltitude()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceBorderLightSourceAltitudeTestDefinition(helper, GetSuperSource(helper)).Run();
        }
    }
}