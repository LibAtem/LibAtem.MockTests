using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MixEffectPropertiesCallback : SdkCallbackBaseNotify<IBMDSwitcherMixEffectBlock, _BMDSwitcherMixEffectBlockEventType>, IBMDSwitcherMixEffectBlockCallback
    {
        private readonly MixEffectState _state;

        public MixEffectPropertiesCallback(MixEffectState state, IBMDSwitcherMixEffectBlock props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();

            if (props is IBMDSwitcherTransitionParameters trans)
            {
                Children.Add(new MixEffectTransitionPropertiesCallback(state.Transition, trans, AppendChange("Transition")));
            }

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherKeyIterator>(props.CreateIterator);
            state.Keyers = AtemSDKConverter.IterateList<IBMDSwitcherKey, MixEffectState.KeyerState>(iterator.Next,
                (keyer, id) =>
                {
                    var keyerState = new MixEffectState.KeyerState();
                    Children.Add(new MixEffectKeyerCallback(keyerState, keyer, AppendChange($"Keyers.{id:D}")));
                    return keyerState;
                });
        }

        public override void Notify(_BMDSwitcherMixEffectBlockEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeProgramInputChanged:
                    Props.GetProgramInput(out long program);
                    _state.Sources.Program = (VideoSource) program;
                    OnChange("Sources");
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypePreviewInputChanged:
                    Props.GetPreviewInput(out long preview);
                    _state.Sources.Preview = (VideoSource) preview;
                    OnChange("Sources");
                    break;
                // TODO - remainder
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeTransitionPositionChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeTransitionFramesRemainingChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeInTransitionChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeFadeToBlackFramesRemainingChanged:
                    Props.GetFadeToBlackFramesRemaining(out uint frames);
                    _state.FadeToBlack.Status.RemainingFrames = frames;
                    OnChange("FadeToBlack.Status");
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeInFadeToBlackChanged:
                   /* Props.GetInFadeToBlack(out int inFadeToBlack);
                    _state.FadeToBlack.Status.InTransition = inFadeToBlack != 0;
                    OnChange("FadeToBlack.Status");*/
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypePreviewLiveChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypePreviewTransitionChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeInputAvailabilityMaskChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeFadeToBlackRateChanged:
                    Props.GetFadeToBlackRate(out uint rate);
                    _state.FadeToBlack.Properties.Rate = rate;
                    OnChange("FadeToBlack.Properties");
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeFadeToBlackFullyBlackChanged:
                    Props.GetFadeToBlackFullyBlack(out int isFullyBlack);
                    _state.FadeToBlack.Status.IsFullyBlack = isFullyBlack != 0;
                    OnChange("FadeToBlack.Status");
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeFadeToBlackInTransitionChanged:
                    Props.GetFadeToBlackFullyBlack(out int inTransition);
                    _state.FadeToBlack.Status.InTransition = inTransition != 0;
                    OnChange("FadeToBlack.Status");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}