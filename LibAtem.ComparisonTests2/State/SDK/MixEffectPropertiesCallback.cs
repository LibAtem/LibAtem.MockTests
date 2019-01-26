using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects;
using LibAtem.Common;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class MixEffectPropertiesCallback : IBMDSwitcherMixEffectBlockCallback, INotify<_BMDSwitcherMixEffectBlockPropertyId>
    {
        private readonly ComparisonMixEffectState _state;
        private readonly MixEffectBlockId _meId;
        private readonly IBMDSwitcherMixEffectBlock _props;
        private readonly Action<CommandQueueKey> _onChange;

        public MixEffectPropertiesCallback(ComparisonMixEffectState state, MixEffectBlockId meId, IBMDSwitcherMixEffectBlock props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _meId = meId;
            _props = props;
            _onChange = onChange;
        }

        public void PropertyChanged(_BMDSwitcherMixEffectBlockPropertyId propertyId)
        {
            Notify(propertyId);
        }

        public void Notify(_BMDSwitcherMixEffectBlockPropertyId eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput:
                    _props.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, out long program);
                    _state.Program = (VideoSource) program;
                    _onChange(new CommandQueueKey(new ProgramInputGetCommand() { Index = _meId }));
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput:
                    _props.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput, out long preview);
                    _state.Preview = (VideoSource) preview;
                    _onChange(new CommandQueueKey(new PreviewInputGetCommand() { Index = _meId }));
                    break;
                // TODO - remainder
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdTransitionPosition:
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdTransitionFramesRemaining:
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdInTransition:
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdFadeToBlackFramesRemaining:
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdInFadeToBlack:
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewLive:
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewTransition:
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdInputAvailabilityMask:
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdFadeToBlackRate:
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdFadeToBlackFullyBlack:
                    break;
                case _BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdFadeToBlackInTransition:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}