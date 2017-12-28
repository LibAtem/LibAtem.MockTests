using System;
using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.XmlState;
using Xunit;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests
{
    [Collection("Client")]
    public class TestColorGenerators
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestColorGenerators(ITestOutputHelper output, AtemClientWrapper client)
        {
            _client = client;
            _output = output;
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
        public void TestColorGenProperties()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                Dictionary<VideoSource, IBMDSwitcherInputColor> sdkCols = helper.GetSdkInputsOfType<IBMDSwitcherInputColor>();

                var failures = new List<string>();

                foreach(KeyValuePair<VideoSource, IBMDSwitcherInputColor> c in sdkCols)
                {
                    ColorGeneratorId colId = GetSourceIdForGen(c.Key);
                    IBMDSwitcherInputColor sdkCol = c.Value;
                    
                    failures.AddRange(CheckColGenProps(helper, sdkCol, colId));
                    helper.ClearReceivedCommands();
                    
                    // Now try changing values in differenc combinations and ensure an update is received

                    helper.SendCommand(new ColorGeneratorSetCommand
                    {
                        Index = colId,
                        Mask = ColorGeneratorSetCommand.MaskFlags.Luma | ColorGeneratorSetCommand.MaskFlags.Hue |
                               ColorGeneratorSetCommand.MaskFlags.Saturation,
                        Hue = 62 * (int) colId,
                        Luma = 16 * (int) colId,
                        Saturation = 22.8 * (int) colId,
                    });
                    helper.Sleep();
                    failures.AddRange(CheckColGenProps(helper, sdkCol, colId));
                    if (helper.CountAndClearReceivedCommands<ColorGeneratorGetCommand>() == 0)
                        failures.Add("No response when setting all color values");

                    helper.SendCommand(new ColorGeneratorSetCommand
                    {
                        Index = colId,
                        Mask = ColorGeneratorSetCommand.MaskFlags.Luma,
                        Luma = 32 * (int)colId,
                    });
                    helper.Sleep();
                    failures.AddRange(CheckColGenProps(helper, sdkCol, colId));
                    if (helper.CountAndClearReceivedCommands<ColorGeneratorGetCommand>() == 0)
                        failures.Add("No response when setting luma color value");

                    helper.SendCommand(new ColorGeneratorSetCommand
                    {
                        Index = colId,
                        Mask = ColorGeneratorSetCommand.MaskFlags.Hue,
                        Hue = 98 * (int)colId,
                    });
                    helper.Sleep();
                    failures.AddRange(CheckColGenProps(helper, sdkCol, colId));
                    if (helper.CountAndClearReceivedCommands<ColorGeneratorGetCommand>() == 0)
                        failures.Add("No response when setting hue color value");

                    helper.SendCommand(new ColorGeneratorSetCommand
                    {
                        Index = colId,
                        Mask = ColorGeneratorSetCommand.MaskFlags.Luma,
                        Luma = 17.4 * (int)colId,
                    });
                    helper.Sleep();
                    failures.AddRange(CheckColGenProps(helper, sdkCol, colId));
                    if (helper.CountAndClearReceivedCommands<ColorGeneratorGetCommand>() == 0)
                        failures.Add("No response when setting luma color value");
                }

                failures.ForEach(f => _output.WriteLine(f));
                Assert.Equal(new List<string>(), failures);
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

        private static IEnumerable<string> CheckColGenProps(AtemComparisonHelper helper, IBMDSwitcherInputColor sdkProps, ColorGeneratorId id)
        {
            var colCmd = helper.FindWithMatching(new ColorGeneratorGetCommand { Index = id });
            if (colCmd == null)
            {
                yield return string.Format("{0}: ColGen missing state props", id);
                yield break;
            }

            sdkProps.GetHue(out double hue);
            if (Math.Abs(colCmd.Hue - hue) > 0.01)
                yield return string.Format("{0}: ColGen hue mismatch: {1}, {2}", id, hue, colCmd.Hue);

            sdkProps.GetSaturation(out double saturation);
            saturation *= 100;
            if (Math.Abs(colCmd.Saturation - saturation) > 0.01)
                yield return string.Format("{0}: ColGen saturation mismatch: {1}, {2}", id, saturation, colCmd.Saturation);

            sdkProps.GetLuma(out double luma);
            luma *= 100;
            if (Math.Abs(colCmd.Luma - luma) > 0.01)
                yield return string.Format("{0}: ColGen luma mismatch: {1}, {2}", id, luma, colCmd.Luma);
        }
    }
}