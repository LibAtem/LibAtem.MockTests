using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class AudioMixerInputCallback : IBMDSwitcherAudioInputCallback, INotify<_BMDSwitcherAudioInputEventType>
    {
        private readonly AudioState.InputState _state;
        private readonly AudioSource _id;
        private readonly IBMDSwitcherAudioInput _props;
        private readonly Action<CommandQueueKey> _onChange;

        public AudioMixerInputCallback(AudioState.InputState state, AudioSource id, IBMDSwitcherAudioInput props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _id = id;
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
                    _onChange(new CommandQueueKey(new AudioMixerInputGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeMixOptionChanged:
                    _props.GetMixOption(out _BMDSwitcherAudioMixOption mixOption);
                    _state.MixOption = AtemEnumMaps.AudioMixOptionMap.FindByValue(mixOption);
                    _onChange(new CommandQueueKey(new AudioMixerInputGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeGainChanged:
                    _props.GetGain(out double gain);
                    _state.Gain = gain;
                    _onChange(new CommandQueueKey(new AudioMixerInputGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeBalanceChanged:
                    _props.GetBalance(out double balance);
                    _state.Balance = balance * 50;
                    _onChange(new CommandQueueKey(new AudioMixerInputGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeIsMixedInChanged:
                    _props.IsMixedIn(out int mixedIn);
                    _state.IsMixedIn = mixedIn != 0;
                    _onChange(new CommandQueueKey(new AudioMixerTallyCommand()));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
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