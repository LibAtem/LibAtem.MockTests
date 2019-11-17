using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;
using LibAtem.ComparisonTests.State;
using LibAtem.ComparisonTests.Util;
using LibAtem.DeviceProfile;
using LibAtem.State;
using Xunit;
using Xunit.Abstractions;

namespace LibAtem.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestMixEffects : MixEffectsTestBase
    {
        public TestMixEffects(ITestOutputHelper output, AtemClientWrapper client) : base(output, client)
        {
        }

        private sealed class ProgramInputTestDefinition : TestDefinitionBase<ProgramInputSetCommand, VideoSource>
        {
            private readonly MixEffectBlockId _meId;
            private readonly IBMDSwitcherMixEffectBlock _sdk;

            public ProgramInputTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherMixEffectBlock> me) : base(helper)
            {
                _meId = me.Item1;
                _sdk = me.Item2;

                _sdk.GetInputAvailabilityMask(out _BMDSwitcherInputAvailability availabilityMask);
                Assert.Equal((long)SourceAvailability.Auxiliary, (long)availabilityMask);
            }

            public override void Prepare() => _sdk.SetProgramInput((long)VideoSource.ColorBars);

            public override void SetupCommand(ProgramInputSetCommand cmd)
            {
                cmd.Index = _meId;
            }

            public override string PropertyName => "Source";
            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                {
                    state.MixEffects[(int)_meId].Sources.Program = v;
                    AtemStateUtil.UpdateVideoTally(state);
                }
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    yield return new CommandQueueKey(new ProgramInputGetCommand() { Index = _meId });
            }

            public override VideoSource[] GoodValues => Enum.GetValues(typeof(VideoSource)).OfType<VideoSource>().Where(i => i.IsAvailable(_helper.Profile) && i.IsAvailable(_meId)).ToArray();
        }
        [Fact]
        public void TestMixEffectProgram()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                var mes = GetMixEffects<IBMDSwitcherMixEffectBlock>();
                Assert.NotEmpty(mes);
                Assert.Equal(mes.Count, (int)helper.Profile.MixEffectBlocks);

                mes.ForEach(me => new ProgramInputTestDefinition(helper, me).Run());
            }
        }
        private sealed class PreviewInputTestDefinition : TestDefinitionBase<PreviewInputSetCommand, VideoSource>
        {
            private readonly MixEffectBlockId _meId;
            private readonly IBMDSwitcherMixEffectBlock _sdk;

            public PreviewInputTestDefinition(AtemComparisonHelper helper, Tuple<MixEffectBlockId, IBMDSwitcherMixEffectBlock> me) : base(helper)
            {
                _meId = me.Item1;
                _sdk = me.Item2;

                _sdk.GetInputAvailabilityMask(out _BMDSwitcherInputAvailability availabilityMask);
                Assert.Equal((long)SourceAvailability.Auxiliary, (long)availabilityMask);
            }

            public override void Prepare() => _sdk.SetProgramInput((long)VideoSource.ColorBars);

            public override void SetupCommand(PreviewInputSetCommand cmd)
            {
                cmd.Index = _meId;
            }

            public override string PropertyName => "Source";
            public override void UpdateExpectedState(AtemState state, bool goodValue, VideoSource v)
            {
                if (goodValue)
                {
                    state.MixEffects[(int)_meId].Sources.Preview = v;
                    AtemStateUtil.UpdateVideoTally(state);
                }
            }

            public override IEnumerable<CommandQueueKey> ExpectedCommands(bool goodValue, VideoSource v)
            {
                if (goodValue)
                    yield return new CommandQueueKey(new PreviewInputGetCommand() { Index = _meId });
            }

            public override VideoSource[] GoodValues => Enum.GetValues(typeof(VideoSource)).OfType<VideoSource>().Where(i => i.IsAvailable(_helper.Profile) && i.IsAvailable(_meId)).ToArray();
        }
        [Fact]
        public void TestMixEffectPreview()
        {
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                var mes = GetMixEffects<IBMDSwitcherMixEffectBlock>();
                Assert.NotEmpty(mes);
                Assert.Equal(mes.Count, (int)helper.Profile.MixEffectBlocks);

                mes.ForEach(me => new PreviewInputTestDefinition(helper, me).Run());
            }
        }

        // TODO - test LibAtem setters

        [Fact]
        public void TestMixEffectPropertiesOld()
        {
            // TODO - refactor to use ValueComparer
            using (var helper = new AtemComparisonHelper(Client, Output))
            {
                Guid itId = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
                helper.SdkSwitcher.CreateIterator(ref itId, out var itPtr);
                IBMDSwitcherMixEffectBlockIterator iterator = (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(itPtr);

                List<IBMDSwitcherMixEffectBlock> sdkMeBlocks = new List<IBMDSwitcherMixEffectBlock>();
                for (iterator.Next(out IBMDSwitcherMixEffectBlock meBlock); meBlock != null; iterator.Next(out meBlock))
                    sdkMeBlocks.Add(meBlock);

                Assert.Equal(sdkMeBlocks.Count, (int) helper.Profile.MixEffectBlocks);

                var failures = new List<string>();

                for (int i = 0; i < sdkMeBlocks.Count; i++)
                {
                    IBMDSwitcherMixEffectBlock sdkProps = sdkMeBlocks[i];
                    MixEffectBlockId blockId = (MixEffectBlockId) i;

                    failures.AddRange(CheckFadeToBlackProps(helper, sdkProps, blockId));

                    // TODO - run ftb, wait a few frames, then check props again.
                    // will need to ensure ftb is setup appropriately first

                    failures.AddRange(CheckTransitionProps(helper, sdkProps, blockId));

                    // TODO - run transition, wait a few frames, then check props again.
                    // will need to ensure transition is setup appropriately first (mix with good time)


                    // TODO - what is this value? is it not the same as InTransition?
                    // bmdSwitcherMixEffectBlockPropertyIdPreviewLive = 1886809206,
                }

                failures.ForEach(f => Output.WriteLine(f));
                Assert.Equal(new List<string>(), failures);
            }
        }

        private static IEnumerable<string> CheckTransitionProps(AtemComparisonHelper helper, IBMDSwitcherMixEffectBlock sdkProps, MixEffectBlockId blockId)
        {
            var transPvwProps = helper.FindWithMatching(new TransitionPreviewGetCommand { Index = blockId });
            sdkProps.GetPreviewTransition(out int previewTrans);
            if (transPvwProps == null || transPvwProps.PreviewTransition != (previewTrans != 0))
                yield return string.Format("{0}: Preiew transition: {1}, {2}", blockId, previewTrans != 0, transPvwProps?.PreviewTransition);
            
            var transProps = helper.FindWithMatching(new TransitionPositionGetCommand { Index = blockId });
            if (transProps == null)
            {
                yield return string.Format("{0}: Transition missing state props", blockId);
                yield break;
            }

            sdkProps.GetTransitionFramesRemaining(out uint framesRemaining);
            if (transProps.RemainingFrames != framesRemaining)
                yield return string.Format("{0}: Transition frames remaining mismatch: {1}, {2}", blockId, framesRemaining, transProps.RemainingFrames);

            sdkProps.GetTransitionPosition(out double transPosition);
            if (Math.Abs(transProps.HandlePosition - transPosition) > 0.01)
                yield return string.Format("{0}: Transition position mismatch: {1}, {2}", blockId, transPosition, transProps.HandlePosition);
        }

        private static IEnumerable<string> CheckFadeToBlackProps(AtemComparisonHelper helper, IBMDSwitcherMixEffectBlock sdkProps, MixEffectBlockId blockId)
        {
            var ftbProps = helper.FindWithMatching(new FadeToBlackPropertiesGetCommand {Index = blockId});
            sdkProps.GetFadeToBlackRate(out uint ftbRate);
            if (ftbProps == null || ftbProps.Rate != ftbRate)
                yield return string.Format("{0}: FTB Rate mismatch: {1}, {2}", blockId, ftbRate, ftbProps?.Rate);

            var ftbState = helper.FindWithMatching(new FadeToBlackStateCommand {Index = blockId});
            if (ftbState == null)
            {
                yield return string.Format("{0}: FTB missing state props", blockId);
                yield break;
            }

            sdkProps.GetFadeToBlackFullyBlack(out int isFullyBlack);
            if (ftbState.IsFullyBlack != (isFullyBlack != 0))
                yield return string.Format("{0}: FTB fully black: {1}, {2}", blockId, isFullyBlack != 0, ftbState.IsFullyBlack);

            sdkProps.GetInFadeToBlack(out int inTransition);
            if (ftbState.InTransition != (inTransition != 0))
                yield return string.Format("{0}: FTB in transition: {1}, {2}", blockId, inTransition != 0, ftbState.InTransition);

            sdkProps.GetFadeToBlackFramesRemaining(out uint framesRemaining);
            if (ftbState.RemainingFrames != framesRemaining)
                yield return string.Format("{0}: FTB frames remaining: {1}, {2}", blockId, framesRemaining, ftbState.RemainingFrames);
        }
    }
}