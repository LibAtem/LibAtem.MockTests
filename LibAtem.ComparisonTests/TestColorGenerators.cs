using System.Collections.Generic;
using System.Linq;
using LibAtem.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests
{
    [Collection("Client")]
    public class TestColorGenerators
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestColorGenerators(ITestOutputHelper output, AtemClientWrapper client)
        {
            _output = output;
            _client = client;
        }

        [Fact]
        public void TestColorGenCount()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                Dictionary<VideoSource, IBMDSwitcherInputColor> sdkCols = helper.GetSdkInputsOfType<IBMDSwitcherInputColor>();
                Assert.Equal((int) helper.Profile.ColorGenerators, sdkCols.Count);

                Assert.True(sdkCols.Keys.All(k => k.GetPortType() == InternalPortType.ColorGenerator));
            }
        }

        [Fact]
        public void TestColorGenHue()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (KeyValuePair<VideoSource, IBMDSwitcherInputColor> c in helper.GetSdkInputsOfType<IBMDSwitcherInputColor>())
                {
                    ColorGeneratorId colId = AtemEnumMaps.GetSourceIdForGen(c.Key);
                    
                    double[] testValues = {0, 123, 233.4, 359.9};
                    double[] badValues = {360, 360.1, 361, -1, -0.01};

                    ICommand Setter(double v) => new ColorGeneratorSetCommand
                    {
                        Index = colId,
                        Mask = ColorGeneratorSetCommand.MaskFlags.Hue,
                        Hue = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.Colors[colId].Hue = v;
                    void UpdateFailedState(ComparisonState state, double v)
                    {
                        ushort ui = (ushort) ((ushort) (v * 10) % 3600);
                        state.Colors[colId].Hue = ui / 10d;
                    }

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestColorGenSaturation()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (KeyValuePair<VideoSource, IBMDSwitcherInputColor> c in helper.GetSdkInputsOfType<IBMDSwitcherInputColor>())
                {
                    ColorGeneratorId colId = AtemEnumMaps.GetSourceIdForGen(c.Key);

                    double[] testValues = {0, 100, 23, 87};
                    double[] badValues = {100.1, 101, -0.1, -1};

                    ICommand Setter(double v) => new ColorGeneratorSetCommand
                    {
                        Index = colId,
                        Mask = ColorGeneratorSetCommand.MaskFlags.Saturation,
                        Saturation = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.Colors[colId].Saturation = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.Colors[colId].Saturation = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }

        [Fact]
        public void TestColorGenLuma()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (KeyValuePair<VideoSource, IBMDSwitcherInputColor> c in helper.GetSdkInputsOfType<IBMDSwitcherInputColor>())
                {
                    ColorGeneratorId colId = AtemEnumMaps.GetSourceIdForGen(c.Key);

                    double[] testValues = { 0, 100, 23, 87 };
                    double[] badValues = { 100.1, 101, -0.1, -1 };

                    ICommand Setter(double v) => new ColorGeneratorSetCommand
                    {
                        Index = colId,
                        Mask = ColorGeneratorSetCommand.MaskFlags.Luma,
                        Luma = v,
                    };

                    void UpdateExpectedState(ComparisonState state, double v) => state.Colors[colId].Luma = v;
                    void UpdateFailedState(ComparisonState state, double v) => state.Colors[colId].Luma = v >= 100 ? 100 : 0;

                    ValueTypeComparer<double>.Run(helper, Setter, UpdateExpectedState, testValues);
                    ValueTypeComparer<double>.Fail(helper, Setter, UpdateFailedState, badValues);
                }
            }
        }
    }
}