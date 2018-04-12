using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Common;
using LibAtem.ComparisonTests.Util;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestDVEKeyer : ComparisonTestBase
    {
        private static readonly IReadOnlyDictionary<BorderBevel, _BMDSwitcherBorderBevelOption> BevelMap;

        static TestDVEKeyer()
        {
            BevelMap = new Dictionary<BorderBevel, _BMDSwitcherBorderBevelOption>()
            {
                {BorderBevel.None, _BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionNone},
                {BorderBevel.InOut, _BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionInOut},
                {BorderBevel.In, _BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionIn},
                {BorderBevel.Out, _BMDSwitcherBorderBevelOption.bmdSwitcherBorderBevelOptionOut},
            };
        }

        public TestDVEKeyer(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        [Fact]
        public void EnsureBevelMapIsComplete()
        {
            EnumMap.EnsureIsComplete(BevelMap);
        }

        [Fact]
        public void TestDVEKeyerShadowEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    bool? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.BorderShadow;

                    BoolValueComparer.Run(helper, Setter, key.Item3.GetShadow, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerLightSourceDirection()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.LightSourceDirection;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetLightSourceDirection, Getter, testValues);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetLightSourceDirection, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerLightSourceAltitude()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    uint? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.LightSourceAltitude;

                    void SdkGetter(out uint val)
                    {
                        key.Item3.GetLightSourceAltitude(out double alt);
                        val = (uint) (alt * 100);
                    }

                    ValueTypeComparer<uint>.Run(helper, Setter, SdkGetter, Getter, testValues);
                    // Note: Limits are not enforced by atem
                    // ValueTypeComparer<uint>.Fail(helper, Setter, SdkGetter, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    bool? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.BorderEnabled;

                    BoolValueComparer.Run(helper, Setter, key.Item3.GetBorderEnabled, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderBevel()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    BorderBevel? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.BorderBevel;

                    EnumValueComparer<BorderBevel, _BMDSwitcherBorderBevelOption>.Run(helper, BevelMap, Setter, key.Item3.GetBorderBevel, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderWidthIn()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.InnerWidth;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetBorderWidthIn, Getter, testValues);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetBorderWidthIn, Getter, badValues);
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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.InnerWidth;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetBorderWidthIn, Getter, testValues);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetBorderWidthIn, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderWidthOuter()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.OuterWidth;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetBorderWidthOut, Getter, testValues);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetBorderWidthOut, Getter, badValues);
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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.OuterWidth;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetBorderWidthOut, Getter, testValues);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetBorderWidthOut, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderSoftnessIn()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    uint? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.InnerSoftness;

                    void SdkGetter(out uint val)
                    {
                        key.Item3.GetBorderSoftnessIn(out double alt);
                        val = (uint) (alt * 100);
                    }

                    ValueTypeComparer<uint>.Run(helper, Setter, SdkGetter, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, SdkGetter, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderSoftnessOut()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    uint? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.OuterSoftness;

                    void SdkGetter(out uint val)
                    {
                        key.Item3.GetBorderSoftnessOut(out double alt);
                        val = (uint)(alt * 100);
                    }

                    ValueTypeComparer<uint>.Run(helper, Setter, SdkGetter, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, SdkGetter, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderBevelSoftness()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    uint? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.BevelSoftness;

                    void SdkGetter(out uint val)
                    {
                        key.Item3.GetBorderBevelSoftness(out double alt);
                        val = (uint)(alt * 100);
                    }

                    ValueTypeComparer<uint>.Run(helper, Setter, SdkGetter, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, SdkGetter, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderBevelPosition()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    uint? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.BevelPosition;

                    void SdkGetter(out uint val)
                    {
                        key.Item3.GetBorderBevelPosition(out double alt);
                        val = (uint)(alt * 100);
                    }

                    ValueTypeComparer<uint>.Run(helper, Setter, SdkGetter, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, SdkGetter, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderOpacity()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    uint? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.BorderOpacity;

                    void SdkGetter(out uint val)
                    {
                        key.Item3.GetBorderOpacity(out double alt);
                        val = (uint)(alt * 100);
                    }

                    ValueTypeComparer<uint>.Run(helper, Setter, SdkGetter, Getter, testValues);
                    ValueTypeComparer<uint>.Fail(helper, Setter, SdkGetter, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderHue()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.BorderHue;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetBorderHue, Getter, testValues);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetBorderHue, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderSaturation()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.01 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderSaturation,
                        BorderSaturation = v,
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.BorderSaturation;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetBorderSaturation, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetBorderSaturation, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerBorderLuma()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                foreach (var key in GetKeyers<IBMDSwitcherKeyDVEParameters>())
                {
                    double[] testValues = { 0, 87.4, 14.7, 99.9, 100, 0.01 };
                    double[] badValues = { 100.1, 110, 101, -0.01, -1, -10 };

                    ICommand Setter(double v) => new MixEffectKeyDVESetCommand
                    {
                        MixEffectIndex = key.Item1,
                        KeyerIndex = key.Item2,
                        Mask = MixEffectKeyDVESetCommand.MaskFlags.BorderLuma,
                        BorderLuma = v,
                    };

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.BorderLuma;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetBorderLuma, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetBorderLuma, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestDVEKeyerMaskEnabled()
        {
            using (var helper = new AtemComparisonHelper(Client))
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

                    bool? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskEnabled;

                    BoolValueComparer.Run(helper, Setter, key.Item3.GetMasked, Getter, testValues);
                }
            }
        }

        [Fact]
        public void TestKeyerMaskTop()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(3000); // TODO - reduce this. Currently needs to be this due to the amount of commands received when changing res

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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskTop;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskTop, Getter, testValues, 1, 0.05);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskTop, Getter, badValues, 1, 0.05);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(3000); // TODO - reduce this.

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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskTop;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskTop, Getter, testValues, 1, 0.05);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskTop, Getter, badValues, 1, 0.05);
                }
            }
        }

        [Fact]
        public void TestKeyerMaskBottom()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(3000); // TODO - reduce this.

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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskBottom;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskBottom, Getter, testValues, 1, 0.05);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskBottom, Getter, badValues, 1, 0.05);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(3000); // TODO - reduce this.

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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskBottom;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskBottom, Getter, testValues, 1, 0.05);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskBottom, Getter, badValues, 1, 0.05);
                }
            }
        }

        [Fact]
        public void TestKeyerMaskLeft()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(3000); // TODO - reduce this.

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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskLeft;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskLeft, Getter, testValues, 1, 0.05);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskLeft, Getter, badValues, 1, 0.05);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(3000); // TODO - reduce this.

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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskLeft;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskLeft, Getter, testValues, 1, 0.05);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskLeft, Getter, badValues, 1, 0.05);
                }
            }
        }

        [Fact]
        public void TestKeyerMaskRight()
        {
            using (var helper = new AtemComparisonHelper(Client))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(3000); // TODO - reduce this.

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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskRight;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskRight, Getter, testValues, 1, 0.05);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskRight, Getter, badValues, 1, 0.05);
                }

                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode625i50PAL);
                helper.Sleep(3000); // TODO - reduce this.

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

                    double? Getter() => helper.FindWithMatching(new MixEffectKeyDVEGetCommand() { MixEffectIndex = key.Item1, KeyerIndex = key.Item2 })?.MaskRight;

                    DoubleValueComparer.Run(helper, Setter, key.Item3.GetMaskRight, Getter, testValues, 1, 0.05);
                    DoubleValueComparer.Fail(helper, Setter, key.Item3.GetMaskRight, Getter, badValues, 1, 0.05);
                }
            }
        }

        [Fact]
        public void TestKeyerMaskReset()
        {
            // This uses a client side set
            using (var helper = new AtemComparisonHelper(Client))
            {
                Client.SdkSwitcher.SetVideoMode(_BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50);
                helper.Sleep(3000); // TODO - reduce this.

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
                helper.Sleep(3000); // TODO - reduce this.

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