using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MixEffectPropertiesCallback : IBMDSwitcherMixEffectBlockCallback, INotify<_BMDSwitcherMixEffectBlockEventType>
    {
        private readonly MixEffectState _state;
        private readonly IBMDSwitcherMixEffectBlock _props;
        private readonly Action<string> _onChange;

        public MixEffectPropertiesCallback(MixEffectState state, IBMDSwitcherMixEffectBlock props, Action<string> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherMixEffectBlockEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeProgramInputChanged:
                    _props.GetProgramInput(out long program);
                    _state.Sources.Program = (VideoSource) program;
                    _onChange("Sources");
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypePreviewInputChanged:
                    _props.GetPreviewInput(out long preview);
                    _state.Sources.Preview = (VideoSource) preview;
                    _onChange("Sources");
                    break;
                // TODO - remainder
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeTransitionPositionChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeTransitionFramesRemainingChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeInTransitionChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeFadeToBlackFramesRemainingChanged:
                    _props.GetFadeToBlackFramesRemaining(out uint frames);
                    _state.FadeToBlack.Status.RemainingFrames = frames;
                    _onChange("FadeToBlack.Status");
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeInFadeToBlackChanged:
                   /* _props.GetInFadeToBlack(out int inFadeToBlack);
                    _state.FadeToBlack.Status.InTransition = inFadeToBlack != 0;
                    _onChange("FadeToBlack.Status");*/
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypePreviewLiveChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypePreviewTransitionChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeInputAvailabilityMaskChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeFadeToBlackRateChanged:
                    _props.GetFadeToBlackRate(out uint rate);
                    _state.FadeToBlack.Properties.Rate = rate;
                    _onChange("FadeToBlack.Properties");
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeFadeToBlackFullyBlackChanged:
                    _props.GetFadeToBlackFullyBlack(out int isFullyBlack);
                    _state.FadeToBlack.Status.IsFullyBlack = isFullyBlack != 0;
                    _onChange("FadeToBlack.Status");
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeFadeToBlackInTransitionChanged:
                    _props.GetFadeToBlackFullyBlack(out int inTransition);
                    _state.FadeToBlack.Status.InTransition = inTransition != 0;
                    _onChange("FadeToBlack.Status");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}