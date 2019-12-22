using System;
using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class FairlightAudioInputCallback : IBMDSwitcherFairlightAudioInputCallback, INotify<_BMDSwitcherFairlightAudioInputEventType>
    {
        private readonly FairlightAudioState.InputState _state;
        private readonly IBMDSwitcherFairlightAudioInput _props;
        private readonly Action<string> _onChange;

        public FairlightAudioInputCallback(FairlightAudioState.InputState state, IBMDSwitcherFairlightAudioInput props, Action<string> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherFairlightAudioInputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioInputEventType.bmdSwitcherFairlightAudioInputEventTypeCurrentExternalPortTypeChanged:
                    _props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType portType);
                    _state.ExternalPortType = AtemEnumMaps.ExternalPortTypeMap.FindByValue(portType);
                    _onChange("ExternalPortType");
                    break;
                case _BMDSwitcherFairlightAudioInputEventType.bmdSwitcherFairlightAudioInputEventTypeConfigurationChanged:
                    _props.GetConfiguration(out _BMDSwitcherFairlightAudioInputConfiguration configuration);
                    _state.ActiveConfiguration = AtemEnumMaps.FairlightAudioInputConfiguration.FindByValue(configuration);
                    
                    // Need to clear the sources, as the number of them will have changed
                    _state.Sources.Clear();
                    _onChange("ActiveConfiguration");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }

    public sealed class FairlightAudioInputSourceCallback : IBMDSwitcherFairlightAudioSourceCallback, INotify<_BMDSwitcherFairlightAudioSourceEventType>
    {
        private readonly FairlightAudioState.InputSourceState _state;
        private readonly IBMDSwitcherFairlightAudioSource _props;
        private readonly Action _onChange;

        public FairlightAudioInputSourceCallback(FairlightAudioState.InputSourceState state, IBMDSwitcherFairlightAudioSource props, Action onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;

            props.GetId(out long id);
            _state.Id = id;
        }

        public void Notify(_BMDSwitcherFairlightAudioSourceEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeIsActiveChanged:
                    _props.IsActive(out int active);
                    _state.IsActive = active != 0;
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeMaxDelayFramesChanged:
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeDelayFramesChanged:
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeInputGainChanged:
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeStereoSimulationIntensityChanged:
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypePanChanged:
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeFaderGainChanged:
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeMixOptionChanged:
                    break;
                case _BMDSwitcherFairlightAudioSourceEventType.bmdSwitcherFairlightAudioSourceEventTypeIsMixedInChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange();
        }

        public void OutputLevelNotification(uint numLevels, ref double levels, uint numPeakLevels, ref double peakLevels)
        {
            //throw new NotImplementedException();
        }
    }
}