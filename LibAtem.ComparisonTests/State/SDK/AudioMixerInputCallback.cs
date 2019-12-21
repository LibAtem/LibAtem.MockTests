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
        private readonly Action<string> _onChange;

        public AudioMixerInputCallback(AudioState.InputState state, IBMDSwitcherAudioInput props, Action<string> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;

            props.GetType(out _BMDSwitcherAudioInputType type);
            _state.Properties.SourceType = AtemEnumMaps.AudioSourceTypeMap.FindByValue(type);
        }
        
        public void Notify(_BMDSwitcherAudioInputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeCurrentExternalPortTypeChanged:
                    _props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType type);
                    _state.Properties.PortType = ((ExternalPortTypeFlags)type).ToAudioPortType();
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeMixOptionChanged:
                    _props.GetMixOption(out _BMDSwitcherAudioMixOption mixOption);
                    _state.Properties.MixOption = AtemEnumMaps.AudioMixOptionMap.FindByValue(mixOption);
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeGainChanged:
                    _props.GetGain(out double gain);
                    _state.Properties.Gain = gain;
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeBalanceChanged:
                    _props.GetBalance(out double balance);
                    _state.Properties.Balance = balance * 50;
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeIsMixedInChanged:
                    /*
                    _props.IsMixedIn(out int mixedIn);
                    _state.IsMixedIn = mixedIn != 0;
                    _onChange("IsMixedIn");
                    */
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange("Properties");
        }

        public void LevelNotification(double left, double right, double peakLeft, double peakRight)
        {
            _state.Levels.LevelLeft = left;
            _state.Levels.LevelRight = right;
            _state.Levels.PeakLeft = peakLeft;
            _state.Levels.PeakRight = peakRight;
            _onChange("Level");
        }
    }
}