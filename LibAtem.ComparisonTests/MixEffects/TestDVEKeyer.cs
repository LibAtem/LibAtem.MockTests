using System;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestDVEKeyer : ComparisonTestBase
    {
        public TestDVEKeyer(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        [Fact]
        public void EnsureBevelMapIsComplete()
        {
            EnumMap.EnsureIsComplete(AtemEnumMaps.BorderBevelMap);
        }

        [Fact]
        public void TestDVEKeyerShadowEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    bool[] testValues = { true, false };

                    ICommand Setter(bool v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.ShadowEnabled,
                        ShadowEnabled = v,
                    };

                    void UpdateExpectedState(ComparisonState state, bool v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BorderShadow = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerLightSourceDirection()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 0, 123, 233.4, 359.9 };
                    double[] badValues = { 360, 360.1, 361, -1, -0.01 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.LightSourceDirection,
                        LightSourceDirection = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.LightSourceDirection = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)((ushort)(v * 10) % 3600);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.LightSourceDirection = ui / 10d;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerLightSourceAltitude()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    uint[] testValues = {10, 100, 34, 99, 11, 78};
                    // uint[] badValues = {101, 110};

                    ICommand Setter(uint v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.LightSourceAltitude,
                        LightSourceAltitude = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.LightSourceAltitude = v;
                    
                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    // Note: Limits are not enforced by atem
                    //ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    bool[] testValues = { true, false };

                    ICommand Setter(bool v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderEnabled,
                        BorderEnabled = v,
                    };

                    void UpdateExpectedState(ComparisonState state, bool v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BorderEnabled = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderBevel()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    BorderBevel[] testValues = Enum.GetValues(typeof(BorderBevel)).OfType<BorderBevel>().ToArray();

                    ICommand Setter(BorderBevel v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderBevel,
                        BorderBevel = v,
                    };

                    void UpdateExpectedState(ComparisonState state, BorderBevel v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BorderBevel = v;

                    ValueTypeComparer<BorderBevel>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderWidthIn()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep();

                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = {0, 0.01, 1, 15.99, 15.9, 15, 9.4, 12.7, 16};
                    double[] badValues = {-0.01, -1, 16.1, 16.01, 17};

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderInnerWidth,
                        InnerWidth = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.InnerWidth = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.InnerWidth = ui > 1600 ? 16 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep();

                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 0, 0.01, 1, 3.99, 3.9, 3, 2.7, 4 };
                    double[] badValues = { -0.01, -1, 4.1, 4.01, 6 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderInnerWidth,
                        InnerWidth = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.InnerWidth = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.InnerWidth = ui > 400 ? 4 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderWidthOuter()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep();

                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = {0, 0.01, 1, 15.99, 15.9, 15, 9.4, 12.7, 16};
                    double[] badValues = {-0.01, -1, 16.1, 16.01, 17};

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderOuterWidth,
                        OuterWidth = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.OuterWidth = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.OuterWidth = ui > 1600 ? 16 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep();

                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 0, 0.01, 1, 3.99, 3.9, 3, 2.7, 4 };
                    double[] badValues = { -0.01, -1, 4.1, 4.01, 6 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderOuterWidth,
                        OuterWidth = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.OuterWidth = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.OuterWidth = ui > 400 ? 4 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderSoftnessIn()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    uint[] testValues = {0, 87, 14, 99, 100, 1};
                    uint[] badValues = {101, 110};

                    ICommand Setter(uint v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderInnerSoftness,
                        InnerSoftness = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.InnerSoftness = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.InnerSoftness = v > 100 ? 100 : (uint)0;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderSoftnessOut()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    uint[] testValues = { 0, 87, 14, 99, 100, 1 };
                    uint[] badValues = { 101, 110 };

                    ICommand Setter(uint v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderOuterSoftness,
                        OuterSoftness = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.OuterSoftness = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.OuterSoftness = v > 100 ? 100 : (uint)0;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderBevelSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    uint[] testValues = { 0, 87, 14, 99, 100, 1 };
                    uint[] badValues = { 101, 110 };

                    ICommand Setter(uint v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderBevelSoftness,
                        BevelSoftness = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BevelSoftness = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BevelSoftness = v > 100 ? 100 : (uint)0;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderBevelPosition()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    uint[] testValues = { 0, 87, 14, 99, 100, 1 };
                    uint[] badValues = { 101, 110 };

                    ICommand Setter(uint v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderBevelPosition,
                        BevelPosition = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BevelPosition = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BevelPosition = v > 100 ? 100 : (uint)0;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderOpacity()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    uint[] testValues = { 0, 87, 14, 99, 100, 1 };
                    uint[] badValues = { 101, 110 };

                    ICommand Setter(uint v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderOpacity,
                        BorderOpacity = v,
                    };

                    void UpdateExpectedState(ComparisonState state, uint v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BorderOpacity = v;
                    void UpdateFailedState(ComparisonState state, uint v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BorderOpacity = v > 100 ? 100 : (uint)0;

                    ValueTypeComparer<uint>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderHue()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 0, 123, 233.4, 359.9 };
                    double[] badValues = { 360, 360.1, 361, -1, -0.01 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderHue,
                        BorderHue = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BorderHue = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)((ushort)(v * 10) % 3600);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BorderHue = ui / 10d;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderSaturation()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.1 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderSaturation,
                        BorderSaturation = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BorderSaturation = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BorderSaturation = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderLuma()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.1 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderLuma,
                        BorderLuma = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BorderLuma = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.BorderLuma = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerMaskEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    bool[] testValues = { true, false };

                    ICommand Setter(bool v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskEnabled,
                        MaskEnabled = v,
                    };

                    void UpdateExpectedState(ComparisonState state, bool v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskEnabled = v;

                    ValueTypeComparer<bool>.Run(helper, Setter, UpdateExpectedState, testValues);
                }
            }
        }

        [Fact]
        public void TestKeyerMaskTop()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(1000); // TODO - reduce this. Currently needs to be this due to the amount of commands received when changing res

                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = {1, 0, 5, 38, 24.78, 12};
                    double[] badValues = {-0.1, 38.1, -1};

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskTop,
                        MaskTop = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskTop = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskTop = ui >= 3800 ? 38 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(1000); // TODO - reduce this.

                // Repeat in 4:3
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 1, 0, 2.5, 11, 8.78 };
                    double[] badValues = { -0.01, 11.1, -1 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskTop,
                        MaskTop = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskTop = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskTop = ui >= 1100 ? 11 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestKeyerMaskBottom()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(1000); // TODO - reduce this.

                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 1, 0, 5, 38, 24.78, 12 };
                    double[] badValues = { -0.1, 38.1, -1 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskBottom,
                        MaskBottom = v
                    };
                    
                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskBottom = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskBottom = ui >= 3800 ? 38 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(1000); // TODO - reduce this.

                // Repeat in 4:3
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 1, 0, 2.5, 11, 8.78 };
                    double[] badValues = { -0.01, 11.1, -1 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskBottom,
                        MaskBottom = v
                    };
                    
                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskBottom = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskBottom = ui >= 1100 ? 11 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestKeyerMaskLeft()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(1000); // TODO - reduce this.

                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 1, 0, 5, 52, 24.78, 12 };
                    double[] badValues = { -0.1, 52.1, -1 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskLeft,
                        MaskLeft = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskLeft = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskLeft = ui >= 5200 ? 52 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(1000); // TODO - reduce this.

                // Repeat in 4:3
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 1, 0, 2.5, 13, 8.78 };
                    double[] badValues = { -0.01, 13.1, -1 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskLeft,
                        MaskLeft = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskLeft = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskLeft = ui >= 1300 ? 13 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestKeyerMaskRight()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(1000); // TODO - reduce this.

                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 1, 0, 5, 52, 24.78, 12 };
                    double[] badValues = { -0.1, 52.1, -1 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskRight,
                        MaskRight = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskRight = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskRight = ui >= 5200 ? 52 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(1000); // TODO - reduce this.

                // Repeat in 4:3
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 1, 0, 2.5, 13, 8.78 };
                    double[] badValues = { -0.01, 13.1, -1 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand()
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.MaskRight,
                        MaskRight = v
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskRight = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort)(v * 100);
                        state.MixEffects[key.Item1].Keyers[key.Item2].DVE.MaskRight = ui >= 1300 ? 13 : 0;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
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