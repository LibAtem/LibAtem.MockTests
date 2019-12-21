using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class FairlightAudioMixerCallback : IBMDSwitcherFairlightAudioMixerCallback, INotify<_BMDSwitcherFairlightAudioMixerEventType>
    {
        private readonly FairlightAudioState.ProgramOutState _state;
        private readonly IBMDSwitcherFairlightAudioMixer _props;
        private readonly Action _onChange;

        public FairlightAudioMixerCallback(FairlightAudioState.ProgramOutState state, IBMDSwitcherFairlightAudioMixer props, Action onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherFairlightAudioMixerEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioMixerEventType.bmdSwitcherFairlightAudioMixerEventTypeMasterOutFaderGainChanged:
                    _props.GetMasterOutFaderGain(out double gain);
                    _state.Gain = gain;
                    _onChange();
                    break;
                case _BMDSwitcherFairlightAudioMixerEventType.bmdSwitcherFairlightAudioMixerEventTypeMasterOutFollowFadeToBlackChanged:
                    _props.GetMasterOutFollowFadeToBlack(out int follow);
                    _state.FollowFadeToBlack = follow != 0;
                    _onChange();
                    break;
                case _BMDSwitcherFairlightAudioMixerEventType.bmdSwitcherFairlightAudioMixerEventTypeAudioFollowVideoCrossfadeTransitionChanged:
                    // TODO
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange();
        }

        public void MasterOutLevelNotification(uint numLevels, ref double levels, uint numPeakLevels, ref double peakLevels)
        {
            //throw new NotImplementedException();
        }

    }
}