using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestDVEKeyer : MixEffectsTestBase
    {
        public TestDVEKeyer(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        private abstract class DVEKeyerTestDefinition<T> : TestDefinitionBase<MixEffectKeyDVESetCommand, T>
        {
            protected readonly MixEffectBlockId _meId;
            protected readonly UpstreamKeyId _keyId;
            protected readonly IBMDSwitcherKeyDVEParameters _sdk;

            public DVEKeyerTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper)
            {
                _meId = key.Item1;
                _keyId = key.Item2;
                _sdk = key.Item3;
            }

            public override void SetupCommand(MixEffectKeyDVESetCommand cmd)
            {
                cmd.MixEffectIndex = _meId;
                cmd.KeyerIndex = _keyId;
            }

            public abstract T MangleBadValue(T v);

            public sealed override void UpdateExpectedState(AtemState state, bool goodValue, T v)
            {
                MixEffectState.KeyerDVEState obj = state.MixEffects[(int)_meId].Keyers[(int)_keyId].DVE;
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, T v)
            {
                yield return new CommandQueueKey(new MixEffectKeyDVEGetCommand() { MixEffectIndex = _meId, KeyerIndex = _keyId });
            }
        }

        private class DVEKeyerShadowEnabledTestDefinition : DVEKeyerTestDefinition<bool>
        {
            public DVEKeyerShadowEnabledTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetShadow(0);

            public override string PropertyName => "BorderShadowEnabled";
            public override bool MangleBadValue(bool v) => v;
        }

        [Fact]
        public void TestShadowEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerShadowEnabledTestDefinition(helper, k).Run());
        }

        private class DVEKeyerLightSourceDirectionTestDefinition : DVEKeyerTestDefinition<double>
        {
            public DVEKeyerLightSourceDirectionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetLightSourceDirection(20);

            public override string PropertyName => "LightSourceDirection";
            public override double MangleBadValue(double v)
            {
                ushort ui = (ushort)((ushort)(v * 10) % 3600);
                return ui / 10d;
            }
            
            public override double[] GoodValues => new double[] { 0, 123, 233.4, 359.9 };
            public override double[] BadValues => new double[] { 360, 360.1, 361, -1, -0.01 };
        }

        [Fact]
        public void TestLightSourceDirection()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerLightSourceDirectionTestDefinition(helper, k).Run());
        }

        private class DVEKeyerLightSourceAltitudeTestDefinition : DVEKeyerTestDefinition<uint>
        {
            public DVEKeyerLightSourceAltitudeTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetLightSourceAltitude(20);

            public override string PropertyName => "LightSourceAltitude";
            public override uint MangleBadValue(uint v) => v > 100 ? (uint)100 : 0;
            public override ICommand GenerateCommand(uint v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.LightSourceAltitude,
                    LightSourceAltitude = v
                };
            }

            public override uint[] GoodValues => new uint[] { 10, 100, 34, 99, 11, 78 };
            /*
             * Note: Atem does not enforce
            public override uint[] BadValues => new uint[] { 101, 110, 0, 9 };
            */
        }

        [Fact]
        public void TestLightSourceAltitude()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerLightSourceAltitudeTestDefinition(helper, k).Run());
        }

        private class DVEKeyerBorderEnabledTestDefinition : DVEKeyerTestDefinition<bool>
        {
            public DVEKeyerBorderEnabledTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderEnabled(0);

            public override string PropertyName => "BorderEnabled";
            public override bool MangleBadValue(bool v) => v;
        }

        [Fact]
        public void TestBorderEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerBorderEnabledTestDefinition(helper, k).Run());
        }

        private class DVEKeyerBorderBevelTestDefinition : DVEKeyerTestDefinition<BorderBevel>
        {
            public DVEKeyerBorderBevelTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderBevel(_BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionInOut);

            public override string PropertyName => "BorderBevel";
            public override BorderBevel MangleBadValue(BorderBevel v) => v;

            public override BorderBevel[] GoodValues => Enum.GetValues(typeof(BorderBevel)).OfType<BorderBevel>().ToArray();
        }

        [Fact]
        public void TestBorderBevel()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerBorderBevelTestDefinition(helper, k).Run());
        }


        private class DVEKeyerBorderWidthTestDefinition : DVEKeyerTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public override string PropertyName { get; }

            public DVEKeyerBorderWidthTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key, string propName) : base(helper, key)
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

        [Fact]
        public void TestBorderWidthIn()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var mode in DVEKeyerBorderWidthTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerBorderWidthTestDefinition(helper, mode, k, "BorderInnerWidth").Run());
                }
            }
        }

        [Fact]
        public void TestBorderWidthOut()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var mode in DVEKeyerBorderWidthTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerBorderWidthTestDefinition(helper, mode, k, "BorderOuterWidth").Run());
                }
            }
        }

        private class DVEKeyerBorderSoftnessInTestDefinition : DVEKeyerTestDefinition<uint>
        {
            public DVEKeyerBorderSoftnessInTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderSoftnessIn(20);

            public override string PropertyName => "BorderInnerSoftness";
            public override uint MangleBadValue(uint v) => v > 100 ? 100 : (uint)0;
            
            public override uint[] GoodValues => new uint[] { 0, 87, 14, 99, 100, 1 };
            public override uint[] BadValues => new uint[] { 101, 110 };
        }

        [Fact]
        public void TestBorderSoftnessIn()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerBorderSoftnessInTestDefinition(helper, k).Run());
        }

        private class DVEKeyerBorderSoftnessOutTestDefinition : DVEKeyerTestDefinition<uint>
        {
            public DVEKeyerBorderSoftnessOutTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderSoftnessOut(20);

            public override string PropertyName => "BorderOuterSoftness";
            public override uint MangleBadValue(uint v) => v > 100 ? 100 : (uint)0;

            public override uint[] GoodValues => new uint[] { 0, 87, 14, 99, 100, 1 };
            public override uint[] BadValues => new uint[] { 101, 110 };
            }

        [Fact]
        public void TestBorderSoftnessOut()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerBorderSoftnessOutTestDefinition(helper, k).Run());
        }

        private class DVEKeyerBorderBevelSoftnessTestDefinition : DVEKeyerTestDefinition<uint>
        {
            public DVEKeyerBorderBevelSoftnessTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderBevelSoftness(20);

            public override string PropertyName => "BorderBevelSoftness";
            public override uint MangleBadValue(uint v) => v > 100 ? 100 : (uint)0;

            public override uint[] GoodValues => new uint[] { 0, 87, 14, 99, 100, 1 };
            public override uint[] BadValues => new uint[] { 101, 110 };
        }

        [Fact]
        public void TestBorderBevelSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerBorderBevelSoftnessTestDefinition(helper, k).Run());
        }

        private class DVEKeyerBorderBevelPositionTestDefinition : DVEKeyerTestDefinition<uint>
        {
            public DVEKeyerBorderBevelPositionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderBevelPosition(20);

            public override string PropertyName => "BorderBevelPosition";
            public override uint MangleBadValue(uint v) => v > 100 ? 100 : (uint)0;

            public override uint[] GoodValues => new uint[] { 0, 87, 14, 99, 100, 1 };
            public override uint[] BadValues => new uint[] { 101, 110 };
        }

        [Fact]
        public void TestBorderBevelPosition()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerBorderBevelPositionTestDefinition(helper, k).Run());
        }

        private class DVEKeyerBorderOpacityTestDefinition : DVEKeyerTestDefinition<uint>
        {
            public DVEKeyerBorderOpacityTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderOpacity(20);

            public override string PropertyName => "BorderOpacity";
            public override uint MangleBadValue(uint v) => v > 100 ? 100 : (uint)0;

            public override uint[] GoodValues => new uint[] { 0, 87, 14, 99, 100, 1 };
            public override uint[] BadValues => new uint[] { 101, 110 };
        }

        [Fact]
        public void TestBorderOpacity()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerBorderOpacityTestDefinition(helper, k).Run());
        }

        private class DVEKeyerBorderHueTestDefinition : DVEKeyerTestDefinition<double>
        {
            public DVEKeyerBorderHueTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
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

        [Fact]
        public void TestBorderHue()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerBorderHueTestDefinition(helper, k).Run());
        }

        private class DVEKeyerBorderSaturationTestDefinition : DVEKeyerTestDefinition<double>
        {
            public DVEKeyerBorderSaturationTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderSaturation(20);

            public override string PropertyName => "BorderSaturation";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestBorderSaturation()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerBorderSaturationTestDefinition(helper, k).Run());
        }

        private class DVEKeyerBorderLumaTestDefinition : DVEKeyerTestDefinition<double>
        {
            public DVEKeyerBorderLumaTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetBorderLuma(20);

            public override string PropertyName => "BorderLuma";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };
        }

        [Fact]
        public void TestBorderLuma()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerBorderLumaTestDefinition(helper, k).Run());
        }

        private class DVEKeyerMaskEnabledTestDefinition : DVEKeyerTestDefinition<bool>
        {
            public DVEKeyerMaskEnabledTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetMasked(0);

            public override string PropertyName => "MaskEnabled";
            public override bool MangleBadValue(bool v) => v;
        }

        [Fact]
        public void TestMaskEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
                GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerMaskEnabledTestDefinition(helper, k).Run());
        }

        private class DVEKeyerMaskYTestDefinition : DVEKeyerTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public override string PropertyName { get; }

            public DVEKeyerMaskYTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> keyer, string propName) : base(helper, keyer)
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
                        return 38;
                    case VideoMode.P625i50PAL:
                        return 11;
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
                            return new double[] { 1, 0, 5, 38, 24.78, 12 };
                        case VideoMode.P625i50PAL:
                            return new double[] { 1, 0, 2.5, 11, 8.78 };
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
                            return new double[] { -0.1, 38.1, -1 };
                        case VideoMode.P625i50PAL:
                            return new double[] { -0.01, 11.1, -1 };
                        default:
                            throw new NotSupportedException();
                    }
                }
            }
        }

        [Fact]
        public void TestMaskTop()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var mode in DVEKeyerMaskYTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerMaskYTestDefinition(helper, mode, k, "MaskTop").Run());
                }
            }
        }

        [Fact]
        public void TestMaskBottom()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var mode in DVEKeyerMaskYTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerMaskYTestDefinition(helper, mode, k, "MaskBottom").Run());
                }
            }
        }

        private class DVEKeyerMaskXTestDefinition : DVEKeyerTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public override string PropertyName { get; }

            public DVEKeyerMaskXTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> keyer, string propName) : base(helper, keyer)
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
                _sdk.SetMaskLeft(5);
                _sdk.SetMaskRight(5);
                _helper.Sleep();
            }

            public override double MangleBadValue(double v)
            {
                switch (_mode)
                {
                    case VideoMode.P1080i50:
                    case VideoMode.N720p5994:
                        return 52;
                    case VideoMode.P625i50PAL:
                        return 13;
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
                            return new double[] { 1, 0, 5, 52, 24.78, 12 };
                        case VideoMode.P625i50PAL:
                            return new double[] { 1, 0, 2.5, 13, 8.78 };
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
                            return new double[] { -0.1, 52.1, -1 };
                        case VideoMode.P625i50PAL:
                            return new double[] { -0.01, 13.1, -1 };
                        default:
                            throw new NotSupportedException();
                    }
                }
            }
        }

        [Fact]
        public void TestMaskLeft()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var mode in DVEKeyerMaskYTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerMaskXTestDefinition(helper, mode, k, "MaskLeft").Run());
                }
            }
        }

        [Fact]
        public void TestMaskRight()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var mode in DVEKeyerMaskYTestDefinition.VideoModes())
                {
                    helper.EnsureVideoMode(mode);

                    GetKeyers<IBMDSwitcherKeyDVEParameters>().ForEach(k => new DVEKeyerMaskXTestDefinition(helper, mode, k, "MaskRight").Run());
                }
            }
        }

        [Fact]
        public void TestKeyerMaskReset()
        {
            // This uses a client side set
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(1000); // TODO - reduce this.

                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    key.Item3.SetMaskBottom(1);
                    key.Item3.SetMaskTop(1);
                    key.Item3.SetMaskLeft(1);
                    key.Item3.SetMaskRight(1);
                    helper.Sleep();
                    key.Item3.ResetMask();
                    helper.Sleep();

                    MixEffectKeyDVEGetCommand cmd = helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 });
                    Assert.NotNull(cmd);

                    Assert.True(Math.Abs(0 - cmd.MaskLeft) < 0.001);
                    Assert.True(Math.Abs(0 - cmd.MaskRight) < 0.001);
                    Assert.True(Math.Abs(0 - cmd.MaskTop) < 0.001);
                    Assert.True(Math.Abs(0 - cmd.MaskBottom) < 0.001);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(1000); // TODO - reduce this.

                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    key.Item3.SetMaskBottom(1);
                    key.Item3.SetMaskTop(1);
                    key.Item3.SetMaskLeft(1);
                    key.Item3.SetMaskRight(1);
                    helper.Sleep();
                    key.Item3.ResetMask();
                    helper.Sleep();

                    MixEffectKeyDVEGetCommand cmd = helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 });
                    Assert.NotNull(cmd);

                    Assert.True(Math.Abs(0 - cmd.MaskLeft) < 0.001);
                    Assert.True(Math.Abs(0 - cmd.MaskRight) < 0.001);
                    Assert.True(Math.Abs(0 - cmd.MaskTop) < 0.001);
                    Assert.True(Math.Abs(0 - cmd.MaskBottom) < 0.001);
                }
            }
        }
    }
}