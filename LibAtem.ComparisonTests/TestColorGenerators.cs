using System.Collections.Generic;
using System.Linq;
using LibAtem.ComparisonTests.Util;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using Xunit;
using Xunit.Abstractions;
using LibAtem.State;

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

        private abstract class ColorGeneratorTestDefinition : TestDefinitionBase<ColorGeneratorSetCommand, double>
        {
            protected readonly IBMDSwitcherInputColor _sdk;
            protected readonly ColorGeneratorId _colId;

            public ColorGeneratorTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputColor sdk, VideoSource id) : base(helper, id != VideoSource.Color1)
            {
                _sdk = sdk;
                _colId = AtemEnumMaps.GetSourceIdForGen(id);
            }

            public override double[] GoodValues => new double[] { 0, 87.4, 14.7, 99.9, 100, 0.1 };
            public override double[] BadValues => new double[] { 100.1, 110, 101, -0.01, -1, -10 };

            public override void SetupCommand(ColorGeneratorSetCommand cmd)
            {
                cmd.Index = _colId;
            }

            public abstract double MangleBadValue(double v);

            public override void UpdateExpectedState(AtemState state, bool goodValue, double v)
            {
                ColorState obj = state.ColorGenerators[(int)_colId];
                SetCommandProperty(obj, PropertyName, goodValue ? v : MangleBadValue(v));
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

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetHue(20);

            public override string PropertyName => "Hue";
            public override double MangleBadValue(double v)
            {
                ushort ui = (ushort)((ushort)(v * 10) % 3600);
                return ui / 10d;
            }

            public override double[] GoodValues => new double[] { 0, 123, 233.4, 359.9 };
            public override double[] BadValues => new double[] { 360, 360.1, 361, -1, -0.01 };
        }

        [Fact]
        public void TestColorGenHue()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (KeyValuePair<VideoSource, IBMDSwitcherInputColor> c in helper.GetSdkInputsOfType<IBMDSwitcherInputColor>())
                    new ColorGeneratorHueTestDefinition(helper, c.Value, c.Key).Run();
            }
        }

        private class ColorGeneratorSaturationTestDefinition : ColorGeneratorTestDefinition
        {
            public ColorGeneratorSaturationTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputColor sdk, VideoSource id) : base(helper, sdk, id)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetSaturation(20);

            public override string PropertyName => "Saturation";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;
        }

        [Fact]
        public void TestColorGenSaturation()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (KeyValuePair<VideoSource, IBMDSwitcherInputColor> c in helper.GetSdkInputsOfType<IBMDSwitcherInputColor>())
                    new ColorGeneratorSaturationTestDefinition(helper, c.Value, c.Key).Run();
            }
        }

        private class ColorGeneratorLumaTestDefinition : ColorGeneratorTestDefinition
        {
            public ColorGeneratorLumaTestDefinition(AtemComparisonHelper helper, IBMDSwitcherInputColor sdk, VideoSource id) : base(helper, sdk, id)
            {
            }

            // Ensure the first value will have a change
            public override void Prepare() => _sdk.SetLuma(20);

            public override string PropertyName => "Luma";
            public override double MangleBadValue(double v) => v >= 100 ? 100 : 0;
        }

        [Fact]
        public void TestColorGenLuma()
        {
            using (var helper = new AtemComparisonHelper(_client, _output))
            {
                foreach (KeyValuePair<VideoSource, IBMDSwitcherInputColor> c in helper.GetSdkInputsOfType<IBMDSwitcherInputColor>())
                    new ColorGeneratorLumaTestDefinition(helper, c.Value, c.Key).Run();
            }
        }
    }
}