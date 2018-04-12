using System;
using System.Collections.Generic;
using System.Linq;
using AtemEmulator.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using Xunit;

namespace AtemEmulator.ComparisonTests
{
    [Collection("Client")]
    public class TestColorGenerators
    {
        private readonly AtemClientWrapper _client;

        public TestColorGenerators(AtemClientWrapper client)
        {
            _client = client;
        }

        [Fact]
        public void TestColorGenCount()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                Dictionary<VideoSource, IBMDSwitcherInputColor> sdkCols = helper.GetSdkInputsOfType<IBMDSwitcherInputColor>();
                Assert.Equal((int) helper.Profile.ColorGenerators, sdkCols.Count);

                Assert.True(sdkCols.Keys.All(k => k.GetPortType() == InternalPortType.ColorGenerator));
            }
        }

        [Fact]
        public void TestColorGenHue()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                foreach (KeyValuePair<VideoSource, IBMDSwitcherInputColor> c in helper.GetSdkInputsOfType<IBMDSwitcherInputColor>())
                {
                    ColorGeneratorId colId = GetSourceIdForGen(c.Key);
                    IBMDSwitcherInputColor sdkCol = c.Value;
                    
                    double[] testValues = {0, 123, 233.4, 359.9};
                    double[] badValues = {360, 360.1, 361, -1, -0.01};

                    ICommand Setter(double v) => new ColorGeneratorSetCommand
                    {
                        Index = colId,
                        Mask = ColorGeneratorSetCommand.MaskFlags.Hue,
                        Hue = v,
                    };

                    double? Getter() => helper.FindWithMatching(new ColorGeneratorGetCommand {Index = colId})?.Hue;

                    DoubleValueComparer.Run(helper, Setter, sdkCol.GetHue, Getter, testValues);
                    DoubleValueComparer.Fail(helper, Setter, sdkCol.GetHue, Getter, badValues);
                }
            }
        }

        [Fact]
        public void TestColorGenSaturation()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                foreach (KeyValuePair<VideoSource, IBMDSwitcherInputColor> c in helper.GetSdkInputsOfType<IBMDSwitcherInputColor>())
                {
                    ColorGeneratorId colId = GetSourceIdForGen(c.Key);
                    IBMDSwitcherInputColor sdkCol = c.Value;

                    double[] testValues = {0, 100, 23, 87};
                    double[] badValues = {100.1, 101, -0.1, -1};

                    ICommand Setter(double v) => new ColorGeneratorSetCommand
                    {
                        Index = colId,
                        Mask = ColorGeneratorSetCommand.MaskFlags.Saturation,
                        Saturation = v,
                    };

                    double? Getter() => helper.FindWithMatching(new ColorGeneratorGetCommand { Index = colId })?.Saturation;

                    DoubleValueComparer.Run(helper, Setter, sdkCol.GetSaturation, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, sdkCol.GetSaturation, Getter, badValues, 100);
                }
            }
        }

        [Fact]
        public void TestColorGenLuma()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                foreach (KeyValuePair<VideoSource, IBMDSwitcherInputColor> c in helper.GetSdkInputsOfType<IBMDSwitcherInputColor>())
                {
                    ColorGeneratorId colId = GetSourceIdForGen(c.Key);
                    IBMDSwitcherInputColor sdkCol = c.Value;

                    double[] testValues = { 0, 100, 23, 87 };
                    double[] badValues = { 100.1, 101, -0.1, -1 };

                    ICommand Setter(double v) => new ColorGeneratorSetCommand
                    {
                        Index = colId,
                        Mask = ColorGeneratorSetCommand.MaskFlags.Luma,
                        Luma = v,
                    };

                    double? Getter() => helper.FindWithMatching(new ColorGeneratorGetCommand { Index = colId })?.Luma;

                    DoubleValueComparer.Run(helper, Setter, sdkCol.GetLuma, Getter, testValues, 100);
                    DoubleValueComparer.Fail(helper, Setter, sdkCol.GetLuma, Getter, badValues, 100);
                }
            }
        }

        private static ColorGeneratorId GetSourceIdForGen(VideoSource id)
        {
            switch (id)
            {
                case VideoSource.Color1:
                    return ColorGeneratorId.One;
                    case VideoSource.Color2:
                        return ColorGeneratorId.Two;
                default:
                    throw new Exception("Not a ColorGen");
            }
        }
    }
}