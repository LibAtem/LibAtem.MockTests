using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.DeviceProfile;
using LibAtem.MockTests.Util;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.MockTests.MixEffects
{
    [Collection("ServerClientPool")]
    public class TestMixEffect : MixEffectsTestBase
    {
        public TestMixEffect(ITestOutputHelper output, AtemServerClientPool pool) : base(output, pool)
        {
        }

        [Fact]
        public void TestProgramInput()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<ProgramInputSetCommand, ProgramInputGetCommand>("Source", true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                List<VideoSource> deviceSources = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                List<VideoSource> validSources = deviceSources.Where(s =>
                    s.IsAvailable(helper.Helper.Profile, InternalPortType.Mask | InternalPortType.Auxiliary | InternalPortType.MEOutput)).ToList();
                VideoSource[] sampleSources = Randomiser.SelectionOfGroup(validSources).ToArray();

                EachMixEffect<IBMDSwitcherMixEffectBlock>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    // TODO GetInputAvailabilityMask

                    VideoSource target = sampleSources[i];
                    meBefore.Sources.Program = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetProgramInput((long)target); });
                }, sampleSources.Length);
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestPreviewInput()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<PreviewInputSetCommand, PreviewInputGetCommand>("Source", true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                List<VideoSource> deviceSources = helper.Helper.BuildLibState().Settings.Inputs.Keys.ToList();
                List<VideoSource> validSources = deviceSources.Where(s =>
                    s.IsAvailable(helper.Helper.Profile, InternalPortType.Mask | InternalPortType.Auxiliary | InternalPortType.MEOutput)).ToList();
                VideoSource[] sampleSources = Randomiser.SelectionOfGroup(validSources).ToArray();

                EachMixEffect<IBMDSwitcherMixEffectBlock>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    VideoSource target = sampleSources[i];
                    meBefore.Sources.Preview = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetPreviewInput((long)target); });
                }, sampleSources.Length);
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestPreviewTransition()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionPreviewSetCommand, TransitionPreviewGetCommand>("PreviewTransition", true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherMixEffectBlock>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    meBefore.Transition.Properties.Preview = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetPreviewTransition(i % 2); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestPerformCut()
        {
            var expected = new MixEffectCutCommand();
            bool tested = false;
            var handler = CommandGenerator.MatchCommand(expected);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherMixEffectBlock>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    expected.Index = meId;

                    helper.SendAndWaitForChange(stateBefore, () => { sdk.PerformCut(); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestPerformAuto()
        {
            var expected = new MixEffectAutoCommand();
            bool tested = false;
            var handler = CommandGenerator.MatchCommand(expected);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherMixEffectBlock>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    expected.Index = meId;

                    helper.SendAndWaitForChange(stateBefore, () => { sdk.PerformAutoTransition(); });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestInTransition()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(Output, Pool, null, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherMixEffectBlock>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    meBefore.Transition.Position.InTransition = i % 2 != 0;
                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new TransitionPositionGetCommand
                        {
                            Index = meId,
                            RemainingFrames = meBefore.Transition.Position.RemainingFrames,
                            InTransition = i % 2 != 0,
                            HandlePosition = meBefore.Transition.Position.HandlePosition
                        });
                    });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestTransitionFramesRemaining()
        {
            bool tested = false;
            AtemMockServerWrapper.Each(Output, Pool, null, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherMixEffectBlock>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    uint target = Randomiser.RangeInt(250);
                    meBefore.Transition.Position.RemainingFrames = target;
                    helper.SendAndWaitForChange(stateBefore, () => {
                        helper.Server.SendCommands(new TransitionPositionGetCommand
                        {
                            Index = meId,
                            RemainingFrames = target,
                            InTransition = meBefore.Transition.Position.InTransition,
                            HandlePosition = meBefore.Transition.Position.HandlePosition
                        });
                    });
                });
            });
            Assert.True(tested);
        }

        [Fact]
        public void TestHandlePosition()
        {
            bool tested = false;
            var handler = CommandGenerator.CreateAutoCommandHandler<TransitionPositionSetCommand, TransitionPositionGetCommand>("HandlePosition", true);
            AtemMockServerWrapper.Each(Output, Pool, handler, DeviceTestCases.All, helper =>
            {
                EachMixEffect<IBMDSwitcherMixEffectBlock>(helper, (stateBefore, meBefore, sdk, meId, i) =>
                {
                    tested = true;

                    var target = Randomiser.Range(0, 0.9999, 10000);
                    meBefore.Transition.Position.HandlePosition = target;
                    helper.SendAndWaitForChange(stateBefore, () => { sdk.SetTransitionPosition(target); });
                });
            });
            Assert.True(tested);
        }

        // TODO GetPreviewLive

    }
}