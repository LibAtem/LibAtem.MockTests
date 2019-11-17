using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class MixEffectPropertiesCallback : IBMDSwitcherMixEffectBlockCallback, INotify<_BMDSwitcherMixEffectBlockEventType>
    {
        private readonly MixEffectState _state;
        private readonly MixEffectBlockId _meId;
        private readonly IBMDSwitcherMixEffectBlock _props;
        private readonly Action<CommandQueueKey> _onChange;

        public MixEffectPropertiesCallback(MixEffectState state, MixEffectBlockId meId, IBMDSwitcherMixEffectBlock props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _meId = meId;
            _props = props;
            _onChange = onChange;
        }

        public void PropertyChanged(_BMDSwitcherMixEffectBlockEventType propertyId)
        {
            Notify(propertyId);
        }

        public void Notify(_BMDSwitcherMixEffectBlockEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeProgramInputChanged:
                    _props.GetProgramInput(out long program);
                    _state.Sources.Program = (VideoSource) program;
                    _onChange(new CommandQueueKey(new ProgramInputGetCommand() { Index = _meId }));
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypePreviewInputChanged:
                    _props.GetPreviewInput(out long preview);
                    _state.Sources.Preview = (VideoSource) preview;
                    _onChange(new CommandQueueKey(new PreviewInputGetCommand() { Index = _meId }));
                    break;
                // TODO - remainder
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeTransitionPositionChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeTransitionFramesRemainingChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeInTransitionChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeFadeToBlackFramesRemainingChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeInFadeToBlackChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypePreviewLiveChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypePreviewTransitionChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeInputAvailabilityMaskChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeFadeToBlackRateChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeFadeToBlackFullyBlackChanged:
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeFadeToBlackInTransitionChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}