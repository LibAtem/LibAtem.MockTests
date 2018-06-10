using System;
using BMDSwitcherAPI;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class AudioMixerInputCallback : IBMDSwitcherAudioInputCallback, INotify<_BMDSwitcherAudioInputEventType>
    {
        private readonly ComparisonAudioInputState _state;
        private readonly IBMDSwitcherAudioInput _props;

        public AudioMixerInputCallback(ComparisonAudioInputState state, IBMDSwitcherAudioInput props)
        {
            _state = state;
            _props = props;
        }
        
        public void Notify(_BMDSwitcherAudioInputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeCurrentExternalPortTypeChanged:
                    _props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType type);
                    _state.ExternalPortType = AtemEnumMaps.ExternalPortTypeMap.FindByValue(type);
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeMixOptionChanged:
                    _props.GetMixOption(out _BMDSwitcherAudioMixOption mixOption);
                    _state.MixOption = AtemEnumMaps.AudioMixOptionMap.FindByValue(mixOption);
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeGainChanged:
                    _props.GetGain(out double gain);
                    _state.Gain = gain;
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeBalanceChanged:
                    _props.GetBalance(out double balance);
                    _state.Balance = balance * 50;
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeIsMixedInChanged:
                    _props.IsMixedIn(out int mixedIn);
                    _state.IsMixedIn = mixedIn != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }

        public void LevelNotification(double left, double right, double peakLeft, double peakRight)
        {
            // TODO
        }
    }
}