using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class FairlightDynamicsAudioMixerCallback : IBMDSwitcherFairlightAudioDynamicsProcessorCallback, INotify<_BMDSwitcherFairlightAudioDynamicsProcessorEventType>
    {
        private readonly FairlightAudioState.DynamicsState _state;
        private readonly IBMDSwitcherFairlightAudioDynamicsProcessor _props;
        private readonly Action _onChange;

        public FairlightDynamicsAudioMixerCallback(FairlightAudioState.DynamicsState state, IBMDSwitcherFairlightAudioDynamicsProcessor props, Action onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherFairlightAudioDynamicsProcessorEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioDynamicsProcessorEventType.bmdSwitcherFairlightAudioDynamicsProcessorEventTypeMakeupGainChanged:
                    _props.GetMakeupGain(out double gain);
                    _state.MakeUpGain = gain;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange();
        }

        public void InputLevelNotification(uint numLevels, ref double levels, uint numPeakLevels, ref double peakLevels)
        {
            // throw new NotImplementedException();
        }

        public void OutputLevelNotification(uint numLevels, ref double levels, uint numPeakLevels, ref double peakLevels)
        {
            // throw new NotImplementedException();
        }
    }
}