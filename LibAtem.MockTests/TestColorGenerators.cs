using System;
using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.SdkStateBuilder;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests
{
    [Collection("ServerClientPool")]
    public class TestColorGenerators
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemServerClientPool _pool;

        public TestColorGenerators(ITestOutputHelper output, AtemServerClientPool pool)
        {
            _output = output;
            _pool = pool;
        }

        private void EachColor(AtemMockServerWrapper helper, Action<AtemState, ColorState, IBMDSwitcherInputColor, ColorGeneratorId, int> fcn, int iterations = 5)
        {
            foreach (KeyValuePair<VideoSource, IBMDSwitcherInputColor> c in helper.GetSdkInputsOfType<IBMDSwitcherInputColor>())
            {
                ColorGeneratorId id = AtemEnumMaps.GetSourceIdForGen(c.Key);
                AtemState stateBefore = helper.Helper.BuildLibState();
                ColorState colBefore = stateBefore.ColorGenerators[(int)id];

                for (int i = 0; i < iterations; i++)
                {
                    fcn(stateBefore, colBefore, c.Value, id, i);
                }
            }
        }

        [Fact]
        public void TestHue()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<ColorGeneratorSetCommand, ColorGeneratorGetCommand>("Hue");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachColor(helper, (stateBefore, state, props, id, i) =>
                {
                    Assert.NotNull(state);

                    var target = Randomiser.Range(0, 359.9, 10);
                    state.Hue = target;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetHue(target); });
                });
            });
        }

        [Fact]
        public void TestSaturation()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<ColorGeneratorSetCommand, ColorGeneratorGetCommand>("Saturation");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachColor(helper, (stateBefore, state, props, id, i) =>
                {
                    Assert.NotNull(state);

                    var target = Randomiser.Range(0, 100, 10);
                    state.Saturation = target;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetSaturation(target / 100); });
                });
            });
        }

        [Fact]
        public void TestLuma()
        {
            var handler = CommandGenerator.CreateAutoCommandHandler<ColorGeneratorSetCommand, ColorGeneratorGetCommand>("Luma");
            AtemMockServerWrapper.Each(_output, _pool, handler, DeviceTestCases.All, helper =>
            {
                EachColor(helper, (stateBefore, state, props, id, i) =>
                {
                    Assert.NotNull(state);

                    var target = Randomiser.Range(0, 100, 10);
                    state.Luma = target;
                    helper.SendAndWaitForChange(stateBefore, () => { props.SetLuma(target / 100); });
                });
            });
        }
    }
}