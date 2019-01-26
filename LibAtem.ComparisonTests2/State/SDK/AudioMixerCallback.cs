using System;
using BMDSwitcherAPI;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class AudioMixerCallback : IBMDSwitcherAudioMixerCallback, INotify<_BMDSwitcherAudioMixerEventType>
    {
        private readonly ComparisonAudioState _state;
        private readonly IBMDSwitcherAudioMixer _props;

        public AudioMixerCallback(ComparisonAudioState state, IBMDSwitcherAudioMixer props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherAudioMixerEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherAudioMixerEventType.bmdSwitcherAudioMixerEventTypeProgramOutGainChanged:
                    _props.GetProgramOutGain(out double gain);
                    _state.ProgramOutGain = gain;
                    break;
                case _BMDSwitcherAudioMixerEventType.bmdSwitcherAudioMixerEventTypeProgramOutBalanceChanged:
                    _props.GetProgramOutBalance(out double balance);
                    _state.ProgramOutBalance = balance * 50;
                    break;
                case _BMDSwitcherAudioMixerEventType.bmdSwitcherAudioMixerEventTypeProgramOutFollowFadeToBlackChanged:
                    _props.GetProgramOutFollowFadeToBlack(out int follow);
                    _state.ProgramOutFollowFadeToBlack = follow != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }

        public void ProgramOutLevelNotification(double left, double right, double peakLeft, double peakRight)
        {
            _state.ProgramLeft = left;
            _state.ProgramRight = right;
            _state.ProgramPeakLeft = peakLeft;
            _state.ProgramPeakRight = peakRight;
        }
    }
}