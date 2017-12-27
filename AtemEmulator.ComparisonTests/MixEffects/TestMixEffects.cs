using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BMDSwitcherAPI;
using LibAtem.Commands.MixEffects;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Commands.Settings;
using LibAtem.Common;
using Xunit;
using Xunit.Abstractions;

namespace AtemEmulator.ComparisonTests.MixEffects
{
    [Collection("Client")]
    public class TestMixEffects
    {
        private readonly ITestOutputHelper _output;
        private readonly AtemClientWrapper _client;

        public TestMixEffects(ITestOutputHelper output, AtemClientWrapper client)
        {
            _client = client;
            _output = output;
        }

        [Fact]
        public void TestMixEffectProperties()
        {
            using (var helper = new AtemComparisonHelper(_client))
            {
                Guid itId = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
                helper.SdkSwitcher.CreateIterator(ref itId, out var itPtr);
                IBMDSwitcherMixEffectBlockIterator iterator = (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(itPtr);

                List<IBMDSwitcherMixEffectBlock> sdkMeBlocks = new List<IBMDSwitcherMixEffectBlock>();
                for (iterator.Next(out IBMDSwitcherMixEffectBlock meBlock); meBlock != null; iterator.Next(out meBlock))
                    sdkMeBlocks.Add(meBlock);

                Assert.Equal(sdkMeBlocks.Count, helper.Profile.MixEffectBlocks.Count);

                var failures = new List<string>();

                for (int i = 0; i < sdkMeBlocks.Count; i++)
                {
                    IBMDSwitcherMixEffectBlock sdkProps = sdkMeBlocks[i];
                    MixEffectBlockId blockId = (MixEffectBlockId) i;

                    var previewInput = helper.FindWithMatching(new PreviewInputGetCommand {Index = blockId});
                    sdkProps.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput, out long previewId);
                    if (previewInput == null || (long)previewInput.Source != previewId)
                        failures.Add(string.Format("{0}: Preview source mismatch: {1}, {2}", blockId, previewId, previewInput?.Source));

                    var programInput = helper.FindWithMatching(new ProgramInputGetCommand { Index = blockId });
                    sdkProps.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, out long programId);
                    if (programInput == null || (long)programInput.Source != programId)
                        failures.Add(string.Format("{0}: Program source mismatch: {1}, {2}", blockId, programId, programInput?.Source));

                    failures.AddRange(CheckFadeToBlackProps(helper, sdkProps, blockId));

                    // TODO - run ftb, wait a few frames, then check props again.
                    // will need to ensure ftb is setup appropriately first

                    failures.AddRange(CheckTransitionProps(helper, sdkProps, blockId));

                    // TODO - run transition, wait a few frames, then check props again.
                    // will need to ensure transition is setup appropriately first (mix with good time)

                    // TODO - this appears to be wrong in this, but it is passing the raw deserialized value correctly.
                    // var sourceProps = helper.FindWithMatching(new InputPropertiesGetCommand {Id = GetSourceIdForMe(blockId)});
                    // sdkProps.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdInputAvailabilityMask, out long srcAvailability);
                    // if (sourceProps == null || (long)sourceProps.SourceAvailability != srcAvailability)
                    //     failures.Add(string.Format("{0}: Input Availability mismatch: {1}, {2}", blockId, (SourceAvailability)srcAvailability, sourceProps?.SourceAvailability));

                    // TODO - what is this value? is it not the same as InTransition?
                    // bmdSwitcherMixEffectBlockPropertyIdPreviewLive = 1886809206,
                }

                failures.ForEach(f => _output.WriteLine(f));
                Assert.Equal(new List<string>(), failures);
            }
        }

        private static VideoSource GetSourceIdForMe(MixEffectBlockId id)
        {
            switch (id)
            {
                case MixEffectBlockId.One:
                    return VideoSource.ME1Prog;
                case MixEffectBlockId.Two:
                    return VideoSource.ME2Prog;
                default:
                    throw new Exception("Unhandled MixEffectBlockId");
            }
        }

        private static IEnumerable<string> CheckTransitionProps(AtemComparisonHelper helper, IBMDSwitcherMixEffectBlock sdkProps, MixEffectBlockId blockId)
        {
            var transPvwProps = helper.FindWithMatching(new TransitionPreviewGetCommand { Index = blockId });
            sdkProps.GetFlag(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewTransition, out int previewTrans);
            if (transPvwProps == null || transPvwProps.PreviewTransition != (previewTrans != 0))
                yield return string.Format("{0}: Preiew transition: {1}, {2}", blockId, previewTrans != 0, transPvwProps?.PreviewTransition);
            
            var transProps = helper.FindWithMatching(new TransitionPositionGetCommand { Index = blockId });
            if (transProps == null)
            {
                yield return string.Format("{0}: Transition missing state props", blockId);
                yield break;
            }

            sdkProps.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdTransitionFramesRemaining, out long framesRemaining);
            if (transProps.RemainingFrames != framesRemaining)
                yield return string.Format("{0}: Transition frames remaining mismatch: {1}, {2}", blockId, framesRemaining, transProps.RemainingFrames);

            sdkProps.GetFloat(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdTransitionPosition, out double transPosition);
            if (Math.Abs(transProps.HandlePosition - transPosition) > 0.01)
                yield return string.Format("{0}: Transition position mismatch: {1}, {2}", blockId, transPosition, transProps.HandlePosition);
        }

        private static IEnumerable<string> CheckFadeToBlackProps(AtemComparisonHelper helper, IBMDSwitcherMixEffectBlock sdkProps, MixEffectBlockId blockId)
        {
            var ftbProps = helper.FindWithMatching(new FadeToBlackPropertiesGetCommand {Index = blockId});
            sdkProps.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdFadeToBlackRate, out long ftbRate);
            if (ftbProps == null || ftbProps.Rate != ftbRate)
                yield return string.Format("{0}: FTB Rate mismatch: {1}, {2}", blockId, ftbRate, ftbProps?.Rate);

            var ftbState = helper.FindWithMatching(new FadeToBlackStateCommand {Index = blockId});
            if (ftbState == null)
            {
                yield return string.Format("{0}: FTB missing state props", blockId);
                yield break;
            }

            sdkProps.GetFlag(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdFadeToBlackFullyBlack, out int isFullyBlack);
            if (ftbState.IsFullyBlack != (isFullyBlack != 0))
                yield return string.Format("{0}: FTB fully black: {1}, {2}", blockId, isFullyBlack != 0, ftbState.IsFullyBlack);

            sdkProps.GetFlag(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdInFadeToBlack, out int inTransition);
            if (ftbState.InTransition != (inTransition != 0))
                yield return string.Format("{0}: FTB in transition: {1}, {2}", blockId, inTransition != 0, ftbState.InTransition);

            sdkProps.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdFadeToBlackFramesRemaining, out long framesRemaining);
            if (ftbState.RemainingFrames != framesRemaining)
                yield return string.Format("{0}: FTB frames remaining: {1}, {2}", blockId, framesRemaining, ftbState.RemainingFrames);
        }
    }
}