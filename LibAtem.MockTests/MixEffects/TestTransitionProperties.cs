using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.MockTests.Util;
using LibAtem.MockTests.SdkState;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    [Collection("ServerClientPool")]
    public class TestTransitionProperties : MixEffectsTestBase
    {
        public TestTransitionProperties(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        [Fact]
        public void TestNextStyle()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionPropertiesSetCommand, TransitionPropertiesGetCommand>("NextStyle");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    var omitStyles = new List<TransitionStyle>();
                    if (stateBefore.Info.DVE == null)
                        omitStyles.Add(TransitionStyle.DVE);
                    if (stateBefore.MediaPool.Clips.Count == 0)
                        omitStyles.Add(TransitionStyle.Stinger);

                    TransitionStyle target = Randomiser.EnumValue(omitStyles.ToArray());
                    _BMDSwitcherTransitionStyle target2 = AtemEnumMaps.TransitionStyleMap[target];
                    meBefore.Transition.Properties.NextStyle = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetNextTransitionStyle(target2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestGetStyle()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(Output, Pool, null, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    TransitionStyle target = Randomiser.EnumValue<TransitionStyle>();
                    meBefore.Transition.Properties.Style = target;
                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new TransitionPropertiesGetCommand
                        {
                            Index = meId,
                            NextStyle = meBefore.Transition.Properties.NextStyle,
                            Style = target,
                            NextSelection = meBefore.Transition.Properties.NextSelection,
                            Selection = meBefore.Transition.Properties.Selection,
                        });
                    });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestNextSelection()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionPropertiesSetCommand, TransitionPropertiesGetCommand>("NextSelection");
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    uint maxSelectionValue = (uint)1 << meBefore.Keyers.Count;
                    uint maxSelection = (maxSelectionValue << 1) - 1;
                    uint target = 1 + Randomiser.RangeInt(maxSelection - 1);
                    meBefore.Transition.Properties.NextSelection = (TransitionLayer)target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetNextTransitionSelection((_BMDSwitcherTransitionSelection)target); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestGetSelection()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(Output, Pool, null, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherTransitionParameters>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    uint maxSelectionValue = (uint)1 << meBefore.Keyers.Count;
                    uint maxSelection = (maxSelectionValue << 1) - 1;
                    uint target = 1 + Randomiser.RangeInt(maxSelection - 1);
                    meBefore.Transition.Properties.Selection = (TransitionLayer)target;
                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new TransitionPropertiesGetCommand
                        {
                            Index = meId,
                            NextStyle = meBefore.Transition.Properties.NextStyle,
                            Style = meBefore.Transition.Properties.Style,
                            NextSelection = meBefore.Transition.Properties.NextSelection,
                            Selection = (TransitionLayer)target,
                        });
                    });
                });
            });
            Assert.True(tested);
        }

    }

}