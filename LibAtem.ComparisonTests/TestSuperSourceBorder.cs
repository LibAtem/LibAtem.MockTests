using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands.SuperSource;
using LibAtem.Common;
using LibAtem.ComparisonTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests
{
    [Collection("Client")]
    public class TestSuperSourceBorder
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestSuperSourceBorder(ITestOutputHelper output, AtemClientWrapper client)
        {
            _output = output;
            _client = client;
        }

        private IBMDSwitcherSuperSourceBorder GetSuperSourceBorder(AtemComparisonHelper helper)
        {
            var ssrc = helper.GetSdkInputsOfType<IBMDSwitcherSuperSourceBorder>().Select(s => s.Value).SingleOrDefault();
            Skip.If(ssrc == null, "Model does not support SuperSource");
            return ssrc;
        }


        private abstract class SuperSourceBorderTestDefinition<T> : TestDefinitionBase<SuperSourceBorderSetCommand, T>
        {
            protected readonly SuperSourceId _ssrcId;
            protected readonly IBMDSwitcherSuperSourceBorder _sdk;

            public SuperSourceBorderTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherSuperSourceBorder ssrc) : base(helper, ssrcId != SuperSourceId.One)
            {
                _ssrcId = ssrcId;
                _sdk = ssrc;
            }

            public abstract T MangleBadValue(T v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                SetCommandProperty(state.SuperSources[(int)_ssrcId].Border, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<string> ExpectedCommands(bool goodValue, T v)
            {
                yield return $"SuperSources.{_ssrcId:D}.Border";
            }
        }


        private class SuperSourceBorderEnabledTestDefinition : SuperSourceBorderTestDefinition<bool>
        {
            public SuperSourceBorderEnabledTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherSuperSourceBorder ssrc) : base(helper, ssrcId, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderEnabled(0);

            public override string PropertyName => "Enabled";
            public override bool MangleBadValue(bool v) => v;
        }

        [SkippableFact]
        public void TestBorderEnabled()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceBorderEnabledTestDefinition(helper, SuperSourceId.One, GetSuperSourceBorder(helper)).Run();
        }

        private class SuperSourceBorderBevelTestDefinition : SuperSourceBorderTestDefinition<BorderBevel>
        {
            public SuperSourceBorderBevelTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherSuperSourceBorder ssrc) : base(helper, ssrcId, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderBevel(_BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionInOut);

            public override string PropertyName => "Bevel";
            public override BorderBevel MangleBadValue(BorderBevel v) => v;

            public override BorderBevel[] GoodValues => Enum.GetValues(typeof(BorderBevel)).OfType<BorderBevel>().ToArray();
        }

        [SkippableFact]
        public void TestBorderBevel()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceBorderBevelTestDefinition(helper, SuperSourceId.One, GetSuperSourceBorder(helper)).Run();
        }

        private class SuperSourceBorderWidthTestDefinition : SuperSourceBorderTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public override string PropertyName { get; }

            public SuperSourceBorderWidthTestDefinition(AtemComparisonHelper helper, VideoMode mode, SuperSourceId ssrcId, IBMDSwitcherSuperSourceBorder ssrc, string propName) : base(helper, ssrcId, ssrc)
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

            public override IEnumerable<string> ExpectedCommands(bool goodValue, double v)
            {
                if (goodValue)
                    return base.ExpectedCommands(goodValue, v);

                return new string[0];
            }
        }

        [SkippableFact]
        public void TestBorderInnerWidth()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherSuperSourceBorder ssrc = GetSuperSourceBorder(helper);

                foreach (var mode in SuperSourceBorderWidthTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    new SuperSourceBorderWidthTestDefinition(helper, mode, SuperSourceId.One, ssrc, "InnerWidth").Run();
                }
            }
        }

        [SkippableFact]
        public void TestBorderOuterWidth()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                IBMDSwitcherSuperSourceBorder ssrc = GetSuperSourceBorder(helper);

                foreach (var mode in SuperSourceBorderWidthTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    new SuperSourceBorderWidthTestDefinition(helper, mode, SuperSourceId.One, ssrc, "OuterWidth").Run();
                }
            }
        }

        private class SuperSourceUint100TestDefinition : SuperSourceBorderTestDefinition<uint>
        {
            public override string PropertyName { get; }

            public SuperSourceUint100TestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherSuperSourceBorder ssrc, string propName) : base(helper, ssrcId, ssrc)
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
        public void TestBorderOuterSoftness()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceUint100TestDefinition(helper, SuperSourceId.One, GetSuperSourceBorder(helper), "OuterSoftness").Run();
        }

        [SkippableFact]
        public void TestBorderInnerSoftness()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceUint100TestDefinition(helper, SuperSourceId.One, GetSuperSourceBorder(helper), "InnerSoftness").Run();
        }

        [SkippableFact]
        public void TestBorderBevelSoftness()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceUint100TestDefinition(helper, SuperSourceId.One, GetSuperSourceBorder(helper), "BevelSoftness").Run();
        }

        [SkippableFact]
        public void TestBorderBevelPosition()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceUint100TestDefinition(helper, SuperSourceId.One, GetSuperSourceBorder(helper), "BevelPosition").Run();
        }

        private class SuperSourceBorderHueTestDefinition : SuperSourceBorderTestDefinition<double>
        {
            public SuperSourceBorderHueTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherSuperSourceBorder ssrc) : base(helper, ssrcId, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderHue(20);

            public override string PropertyName => "Hue";
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
                new SuperSourceBorderHueTestDefinition(helper, SuperSourceId.One, GetSuperSourceBorder(helper)).Run();
        }

        private class SuperSourceBorderSaturationTestDefinition : SuperSourceBorderTestDefinition<double>
        {
            public SuperSourceBorderSaturationTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherSuperSourceBorder ssrc) : base(helper, ssrcId, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderSaturation(20);

            public override string PropertyName => "Saturation";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [SkippableFact]
        public void TestBorderSaturation()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceBorderSaturationTestDefinition(helper, SuperSourceId.One, GetSuperSourceBorder(helper)).Run();
        }

        private class SuperSourceBorderLumaTestDefinition : SuperSourceBorderTestDefinition<double>
        {
            public SuperSourceBorderLumaTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherSuperSourceBorder ssrc) : base(helper, ssrcId, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderLuma(20);

            public override string PropertyName => "Luma";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [SkippableFact]
        public void TestBorderLuma()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceBorderLumaTestDefinition(helper, SuperSourceId.One, GetSuperSourceBorder(helper)).Run();
        }

        private class SuperSourceBorderLightSourceDirectionTestDefinition : SuperSourceBorderTestDefinition<double>
        {
            public SuperSourceBorderLightSourceDirectionTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherSuperSourceBorder ssrc) : base(helper, ssrcId, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderLightSourceDirection(20);

            public override string PropertyName => "LightSourceDirection";
            public override double MangleBadValue(double v) => 0;

            public override double[] GoodValues => new double[] { 0, 123, 233.4, 359.9 };
            public override double[] BadValues => new double[] { 360, 360.1, 361, -1, -0.01 };
        }

        [SkippableFact]
        public void TestBorderLightSourceDirection()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceBorderLightSourceDirectionTestDefinition(helper, SuperSourceId.One, GetSuperSourceBorder(helper)).Run();
        }

        private class SuperSourceBorderLightSourceAltitudeTestDefinition : SuperSourceBorderTestDefinition<uint>
        {
            public SuperSourceBorderLightSourceAltitudeTestDefinition(AtemComparisonHelper helper, SuperSourceId ssrcId, IBMDSwitcherSuperSourceBorder ssrc) : base(helper, ssrcId, ssrc)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderLightSourceAltitude(20);

            public override string PropertyName => "LightSourceAltitude";
            public override uint MangleBadValue(uint v) => v < 10 ? (uint)10 : 100;

            public override uint[] GoodValues => new uint[] { 10, 100, 34, 99, 11, 78 };
            public override uint[] BadValues => new uint[] { 101, 110, 0, 9 };
        }

        [SkippableFact]
        public void TestBorderLightSourceAltitude()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
                new SuperSourceBorderLightSourceAltitudeTestDefinition(helper, SuperSourceId.One, GetSuperSourceBorder(helper)).Run();
        }
    }
}