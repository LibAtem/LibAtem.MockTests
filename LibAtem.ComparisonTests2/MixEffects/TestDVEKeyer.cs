using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using LibAtem.ComparisonTests2.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2.MixEffects
{
    [Collection("Client")]
    public class TestDVEKeyer : MixEffectsTestBase
    {
        public TestDVEKeyer(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        private abstract class DVEKeyerTestDefinition<T> : TestDefinitionBase<T>
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

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetShadow(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.ShadowEnabled,
                    ShadowEnabled = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_meId].Keyers[_keyId].DVE.BorderShadow = v;
            }
        }

        [Fact]
        public void TestShadowEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerShadowEnabledTestDefinition(helper, key).Run();
                }
            }
        }

        private class DVEKeyerrLightSourceDirectionTestDefinition : DVEKeyerTestDefinition<double>
        {
            public DVEKeyerrLightSourceDirectionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetLightSourceDirection(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.LightSourceDirection,
                    LightSourceDirection = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.MixEffects[_meId].Keyers[_keyId].DVE.LightSourceDirection = v;
                }
                else
                {
                    ushort ui = (ushort)((ushort)(v * 10) % 3600);
                    state.MixEffects[_meId].Keyers[_keyId].DVE.LightSourceDirection = ui / 10d;
                }
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 123, 233.4, 359.9 };
            }

            public override double[] BadValues()
            {
                return new double[] { 360, 360.1, 361, -1, -0.01 };
            }
        }

        [Fact]
        public void TestLightSourceDirection()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerrLightSourceDirectionTestDefinition(helper, key).Run();
                }
            }
        }

        private class DVEKeyerrLightSourceAltitudeTestDefinition : DVEKeyerTestDefinition<uint>
        {
            public DVEKeyerrLightSourceAltitudeTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetLightSourceAltitude(20);
            }

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

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                if (goodValue)
                {
                    state.MixEffects[_meId].Keyers[_keyId].DVE.LightSourceAltitude = v;
                }
                else
                {
                    //ushort ui = (ushort)((ushort)(v * 10) % 3600);
                    state.MixEffects[_meId].Keyers[_keyId].DVE.LightSourceAltitude = v > 100 ? (uint)100 : 0;
                }
            }

            public override uint[] GoodValues()
            {
                return new uint[] { 10, 100, 34, 99, 11, 78 };
            }

            /*
             * Note: Atem does not enforce version
            public override uint[] BadValues()
            {
                return new uint[] { 101, 110, 0, 9 };
            }
            */
        }

        [Fact]
        public void TestLightSourceAltitude()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerrLightSourceAltitudeTestDefinition(helper, key).Run();
                }
            }
        }

        private class DVEKeyerBorderEnabledTestDefinition : DVEKeyerTestDefinition<bool>
        {
            public DVEKeyerBorderEnabledTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderEnabled(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderEnabled,
                    BorderEnabled = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_meId].Keyers[_keyId].DVE.BorderEnabled = v;
            }
        }

        [Fact]
        public void TestBorderEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerBorderEnabledTestDefinition(helper, key).Run();
                }
            }
        }

        private class DVEKeyerBorderBevelTestDefinition : DVEKeyerTestDefinition<BorderBevel>
        {
            public DVEKeyerBorderBevelTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderBevel(_BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionInOut);
            }

            public override ICommand GenerateCommand(BorderBevel v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderBevel,
                    BorderBevel = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, BorderBevel v)
            {
                state.MixEffects[_meId].Keyers[_keyId].DVE.BorderBevel = v;
            }

            public override BorderBevel[] GoodValues()
            {
                return Enum.GetValues(typeof(BorderBevel)).OfType<BorderBevel>().ToArray();
            }
        }

        [Fact]
        public void TestBorderBevel()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerBorderBevelTestDefinition(helper, key).Run();
                }
            }
        }


        private abstract class DVEKeyerBorderWidthTestDefinition : DVEKeyerTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public DVEKeyerBorderWidthTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
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

        private class DVEKeyerBorderWidthInTestDefinition : DVEKeyerBorderWidthTestDefinition
        {
            public DVEKeyerBorderWidthInTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, mode, key)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderInnerWidth,
                    BorderInnerWidth = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.MixEffects[_meId].Keyers[_keyId].DVE.InnerWidth = ClampValueToRange(goodValue, v);
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

                    foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                    { 
                        new DVEKeyerBorderWidthInTestDefinition(helper, mode, key).Run();
                    }
                }
            }
        }

        private class DVEKeyerBorderWidthOutTestDefinition : DVEKeyerBorderWidthTestDefinition
        {
            public DVEKeyerBorderWidthOutTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, mode, key)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderOuterWidth,
                    BorderOuterWidth = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.MixEffects[_meId].Keyers[_keyId].DVE.OuterWidth = ClampValueToRange(goodValue, v);
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

                    foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                    {
                        new DVEKeyerBorderWidthOutTestDefinition(helper, mode, key).Run();
                    }
                }
            }
        }

        private class DVEKeyerBorderSoftnessInTestDefinition : DVEKeyerTestDefinition<uint>
        {
            public DVEKeyerBorderSoftnessInTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderSoftnessIn(20);
            }

            public override ICommand GenerateCommand(uint v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderInnerSoftness,
                    BorderInnerSoftness = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].DVE.InnerSoftness = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].DVE.InnerSoftness = v > 100 ? 100 : (uint)0;
            }

            public override uint[] GoodValues()
            {
                return new uint[] { 0, 87, 14, 99, 100, 1 };
            }

            public override uint[] BadValues()
            {
                return new uint[] { 101, 110 };
            }
        }

        [Fact]
        public void TestBorderSoftnessIn()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerBorderSoftnessInTestDefinition(helper, key).Run();
                }
            }
        }

        private class DVEKeyerBorderSoftnessOutTestDefinition : DVEKeyerTestDefinition<uint>
        {
            public DVEKeyerBorderSoftnessOutTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderSoftnessOut(20);
            }

            public override ICommand GenerateCommand(uint v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderOuterSoftness,
                    BorderOuterSoftness = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].DVE.OuterSoftness = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].DVE.OuterSoftness = v > 100 ? 100 : (uint)0;
            }

            public override uint[] GoodValues()
            {
                return new uint[] { 0, 87, 14, 99, 100, 1 };
            }

            public override uint[] BadValues()
            {
                return new uint[] { 101, 110 };
            }
        }

        [Fact]
        public void TestBorderSoftnessOut()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerBorderSoftnessOutTestDefinition(helper, key).Run();
                }
            }
        }

        private class DVEKeyerBorderBevelSoftnessTestDefinition : DVEKeyerTestDefinition<uint>
        {
            public DVEKeyerBorderBevelSoftnessTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderBevelSoftness(20);
            }

            public override ICommand GenerateCommand(uint v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderBevelSoftness,
                    BorderBevelSoftness = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].DVE.BevelSoftness = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].DVE.BevelSoftness = v > 100 ? 100 : (uint)0;
            }

            public override uint[] GoodValues()
            {
                return new uint[] { 0, 87, 14, 99, 100, 1 };
            }

            public override uint[] BadValues()
            {
                return new uint[] { 101, 110 };
            }
        }

        [Fact]
        public void TestBorderBevelSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerBorderBevelSoftnessTestDefinition(helper, key).Run();
                }
            }
        }

        private class DVEKeyerBorderBevelPositionTestDefinition : DVEKeyerTestDefinition<uint>
        {
            public DVEKeyerBorderBevelPositionTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderBevelPosition(20);
            }

            public override ICommand GenerateCommand(uint v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderBevelPosition,
                    BorderBevelPosition = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].DVE.BevelPosition = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].DVE.BevelPosition = v > 100 ? 100 : (uint)0;
            }

            public override uint[] GoodValues()
            {
                return new uint[] { 0, 87, 14, 99, 100, 1 };
            }

            public override uint[] BadValues()
            {
                return new uint[] { 101, 110 };
            }
        }

        [Fact]
        public void TestBorderBevelPosition()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerBorderBevelPositionTestDefinition(helper, key).Run();
                }
            }
        }

        private class DVEKeyerBorderOpacityTestDefinition : DVEKeyerTestDefinition<uint>
        {
            public DVEKeyerBorderOpacityTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderOpacity(20);
            }

            public override ICommand GenerateCommand(uint v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderOpacity,
                    BorderOpacity = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, uint v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].DVE.BorderOpacity = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].DVE.BorderOpacity = v > 100 ? 100 : (uint)0;
            }

            public override uint[] GoodValues()
            {
                return new uint[] { 0, 87, 14, 99, 100, 1 };
            }

            public override uint[] BadValues()
            {
                return new uint[] { 101, 110 };
            }
        }

        [Fact]
        public void TestBorderOpacity()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerBorderOpacityTestDefinition(helper, key).Run();
                }
            }
        }

        private class DVEKeyerBorderHueTestDefinition : DVEKeyerTestDefinition<double>
        {
            public DVEKeyerBorderHueTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderHue(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderHue,
                    BorderHue = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.MixEffects[_meId].Keyers[_keyId].DVE.BorderHue = v;
                }
                else
                {
                    ushort ui = (ushort)((ushort)(v * 10) % 3600);
                    state.MixEffects[_meId].Keyers[_keyId].DVE.BorderHue = ui / 10d;
                }
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 123, 233.4, 359.9 };
            }

            public override double[] BadValues()
            {
                return new double[] { 360, 360.1, 361, -1, -0.01 };
            }
        }

        [Fact]
        public void TestBorderHue()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerBorderHueTestDefinition(helper, key).Run();
                }
            }
        }

        private class DVEKeyerBorderSaturationTestDefinition : DVEKeyerTestDefinition<double>
        {
            public DVEKeyerBorderSaturationTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderHue(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderSaturation,
                    BorderSaturation = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].DVE.BorderSaturation = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].DVE.BorderSaturation = v >= 100 ? 100 : 0;
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
        public void TestBorderSaturation()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerBorderSaturationTestDefinition(helper, key).Run();
                }
            }
        }

        private class DVEKeyerBorderLumaTestDefinition : DVEKeyerTestDefinition<double>
        {
            public DVEKeyerBorderLumaTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetBorderHue(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderLuma,
                    BorderLuma = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                    state.MixEffects[_meId].Keyers[_keyId].DVE.BorderLuma = v;
                else
                    state.MixEffects[_meId].Keyers[_keyId].DVE.BorderLuma = v >= 100 ? 100 : 0;
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
        public void TestBorderLuma()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerBorderLumaTestDefinition(helper, key).Run();
                }
            }
        }

        private class DVEKeyerMaskEnabledTestDefinition : DVEKeyerTestDefinition<bool>
        {
            public DVEKeyerMaskEnabledTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> key) : base(helper, key)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetMasked(0);
            }

            public override ICommand GenerateCommand(bool v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskEnabled,
                    MaskEnabled = v
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, bool v)
            {
                state.MixEffects[_meId].Keyers[_keyId].DVE.MaskEnabled = v;
            }
        }

        [Fact]
        public void TestMaskEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    new DVEKeyerMaskEnabledTestDefinition(helper, key).Run();
                }
            }
        }

        private abstract class DVEKeyerMaskYTestDefinition : DVEKeyerTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public DVEKeyerMaskYTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> keyer) : base(helper, keyer)
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
                        return 38;
                    case VideoMode.P625i50PAL:
                        return 11;
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
                        return new double[] { 1, 0, 5, 38, 24.78, 12 };
                    case VideoMode.P625i50PAL:
                        return new double[] { 1, 0, 2.5, 11, 8.78 };
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
                        return new double[] { -0.1, 38.1, -1 };
                    case VideoMode.P625i50PAL:
                        return new double[] { -0.01, 11.1, -1 };
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private class DVEKeyerMaskTopTestDefinition : DVEKeyerMaskYTestDefinition
        {
            public DVEKeyerMaskTopTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> keyer) : base(helper, mode, keyer)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskTop,
                    MaskTop = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.MixEffects[_meId].Keyers[_keyId].DVE.MaskTop = ClampValueToRange(goodValue, v);
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

                    foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                    {
                        new DVEKeyerMaskTopTestDefinition(helper, mode, key).Run();
                    }
                }
            }
        }

        private class DVEKeyerMaskBottomTestDefinition : DVEKeyerMaskYTestDefinition
        {
            public DVEKeyerMaskBottomTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> keyer) : base(helper, mode, keyer)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskBottom,
                    MaskBottom = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.MixEffects[_meId].Keyers[_keyId].DVE.MaskBottom = ClampValueToRange(goodValue, v);
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

                    foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                    {
                        new DVEKeyerMaskBottomTestDefinition(helper, mode, key).Run();
                    }
                }
            }
        }

        private abstract class DVEKeyerMaskXTestDefinition : DVEKeyerTestDefinition<double>
        {
            private readonly VideoMode _mode;

            public DVEKeyerMaskXTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> keyer) : base(helper, keyer)
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
                _sdk.SetMaskLeft(5);
                _sdk.SetMaskRight(5);
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
                        return 52;
                    case VideoMode.P625i50PAL:
                        return 13;
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
                        return new double[] { 1, 0, 5, 52, 24.78, 12 };
                    case VideoMode.P625i50PAL:
                        return new double[] { 1, 0, 2.5, 13, 8.78 };
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
                        return new double[] { -0.1, 52.1, -1 };
                    case VideoMode.P625i50PAL:
                        return new double[] { -0.01, 13.1, -1 };
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private class DVEKeyerMaskLeftTestDefinition : DVEKeyerMaskXTestDefinition
        {
            public DVEKeyerMaskLeftTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> keyer) : base(helper, mode, keyer)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskLeft,
                    MaskLeft = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.MixEffects[_meId].Keyers[_keyId].DVE.MaskLeft = ClampValueToRange(goodValue, v);
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

                    foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                    {
                        new DVEKeyerMaskLeftTestDefinition(helper, mode, key).Run();
                    }
                }
            }
        }

        private class DVEKeyerMaskRightTestDefinition : DVEKeyerMaskXTestDefinition
        {
            public DVEKeyerMaskRightTestDefinition(AtemComparisonHelper helper, VideoMode mode, Tuple<MixEffectBlockId, UpstreamKeyId, IBMDSwitcherKeyDVEParameters> keyer) : base(helper, mode, keyer)
            {
            }

            public override ICommand GenerateCommand(double v)
            {
                return new MixEffectKeyDVESetCommand
                {
                    MixEffectIndex = _meId,
                    KeyerIndex = _keyId,
                    Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskRight,
                    MaskRight = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                state.MixEffects[_meId].Keyers[_keyId].DVE.MaskRight = ClampValueToRange(goodValue, v);
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

                    foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                    {
                        new DVEKeyerMaskRightTestDefinition(helper, mode, key).Run();
                    }
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