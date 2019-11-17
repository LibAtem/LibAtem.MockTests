using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Audio;
using LibAtem.State;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class AudioMixerCallback : IBMDSwitcherAudioMixerCallback, INotify<_BMDSwitcherAudioMixerEventType>
    {
        private readonly AudioState.ProgramOutState _state;
        private readonly IBMDSwitcherAudioMixer _props;
        private readonly Action<CommandQueueKey> _onChange;

        public AudioMixerCallback(AudioState.ProgramOutState state, IBMDSwitcherAudioMixer props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherAudioMixerEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherAudioMixerEventType.bmdSwitcherAudioMixerEventTypeProgramOutGainChanged:
                    _props.GetProgramOutGain(out double gain);
                    _state.Gain = gain;
                    break;
                case _BMDSwitcherAudioMixerEventType.bmdSwitcherAudioMixerEventTypeProgramOutBalanceChanged:
                    _props.GetProgramOutBalance(out double balance);
                    _state.Balance = balance * 50;
                    break;
                case _BMDSwitcherAudioMixerEventType.bmdSwitcherAudioMixerEventTypeProgramOutFollowFadeToBlackChanged:
                    _props.GetProgramOutFollowFadeToBlack(out int follow);
                    _state.FollowFadeToBlack = follow != 0;
                    break;
                case _BMDSwitcherAudioMixerEventType.bmdSwitcherAudioMixerEventTypeAudioFollowVideoCrossfadeTransitionChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange(new CommandQueueKey(new AudioMixerMasterGetCommand()));
        }

        public void ProgramOutLevelNotification(double left, double right, double peakLeft, double peakRight)
        {
            _state.LevelLeft = left;
            _state.LevelRight = right;
            _state.PeakLeft = peakLeft;
            _state.PeakRight = peakRight;
        }
    }
}