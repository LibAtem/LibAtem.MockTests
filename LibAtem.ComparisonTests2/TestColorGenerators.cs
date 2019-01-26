using System.Collections.Generic;
using System.Linq;
using LibAtem.ComparisonTests2.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.ComparisonTests2.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests2
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

        private abstract class ColorGeneratorTestDefinition : TestDefinitionBase<double>
        {
            protected readonly IBMDSwitcherInputColor _sdk;
            protected readonly ColorGeneratorId _colId;

            public ColorGeneratorTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputColor sdk, VideoSource id) : base(helper)
            {
                _sdk = sdk;
                _colId = AtemEnumMaps.GetSourceIdForGen(id);
            }

            public override double[] GoodValues()
            {
                return new double[] { 0, 100, 23, 87 };
            }
            public override double[] BadValues()
            {
                return new double[] { 100.1, 101, -0.1, -1 };
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, double v)
            {
                yield return new CommandQueueKey(new ColorGeneratorGetCommand() { Index = _colId });
            }
        }

        private class ColorGeneratorHueTestDefinition : ColorGeneratorTestDefinition
        {
            public ColorGeneratorHueTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputColor sdk, VideoSource id) : base(helper, sdk, id)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetHue(20);
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
                return new ColorGeneratorSetCommand
                {
                    Index = _colId,
                    Mask = ColorGeneratorSetCommand.MaskFlags.Hue,
                    Hue = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.Colors[_colId].Hue = v;
                }
                else
                {
                    ushort ui = (ushort)((ushort)(v * 10) % 3600);
                    state.Colors[_colId].Hue = ui / 10d;
                }
            }
        }

        [Fact]
        public void TestColorGenHue()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (KeyValuePair<VideoSource, IBMDSwitcherInputColor> c in helper.GetSdkInputsOfType<IBMDSwitcherInputColor>())
                {
                    new ColorGeneratorHueTestDefinition(helper, c.Value, c.Key).Run();
                }
            }
        }

        private class ColorGeneratorSaturationTestDefinition : ColorGeneratorTestDefinition
        {
            public ColorGeneratorSaturationTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputColor sdk, VideoSource id) : base(helper, sdk, id)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetSaturation(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new ColorGeneratorSetCommand
                {
                    Index = _colId,
                    Mask = ColorGeneratorSetCommand.MaskFlags.Saturation,
                    Saturation = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.Colors[_colId].Saturation = v;
                }
                else
                {
                    state.Colors[_colId].Saturation = v >= 100 ? 100 : 0;
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
                    new ColorGeneratorSaturationTestDefinition(helper, c.Value, c.Key).Run();
                }
            }
        }

        private class ColorGeneratorLumaTestDefinition : ColorGeneratorTestDefinition
        {
            public ColorGeneratorLumaTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputColor sdk, VideoSource id) : base(helper, sdk, id)
            {
            }

            public override void Prepare()
            {
                // Ensure the first value will have a change
                _sdk.SetLuma(20);
            }

            public override ICommand GenerateCommand(double v)
            {
                return new ColorGeneratorSetCommand
                {
                    Index = _colId,
                    Mask = ColorGeneratorSetCommand.MaskFlags.Luma,
                    Luma = v,
                };
            }

            public override void UpdateExpectedState(ComparisonState state, bool goodValue, double v)
            {
                if (goodValue)
                {
                    state.Colors[_colId].Luma = v;
                }
                else
                {
                    state.Colors[_colId].Luma = v >= 100 ? 100 : 0;
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
                    new ColorGeneratorLumaTestDefinition(helper, c.Value, c.Key).Run();
                }
            }
        }
    }
}