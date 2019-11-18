using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class AudioMixerInputCallback : IBMDSwitcherAudioInputCallback, INotify<_BMDSwitcherAudioInputEventType>
    {
        private readonly AudioState.InputState _state;
        private readonly IBMDSwitcherAudioInput _props;
        private readonly Action _onChange;

        public AudioMixerInputCallback(AudioState.InputState state, IBMDSwitcherAudioInput props, Action onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }
        
        public void Notify(_BMDSwitcherAudioInputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeCurrentExternalPortTypeChanged:
                    _props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType type);
                    _state.ExternalPortType = (ExternalPortTypeFlags)type;
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

            _onChange();
        }

        public void LevelNotification(double left, double right, double peakLeft, double peakRight)
        {
            _state.LevelLeft = left;
            _state.LevelRight = right;
            _state.PeakLeft = peakLeft;
            _state.PeakRight = peakRight;
        }
    }
}