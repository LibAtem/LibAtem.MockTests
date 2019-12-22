using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class FairlightDynamicsAudioMixerCallback : SdkCallbackBaseNotify<IBMDSwitcherFairlightAudioDynamicsProcessor, _BMDSwitcherFairlightAudioDynamicsProcessorEventType>, IBMDSwitcherFairlightAudioDynamicsProcessorCallback
    {
        private readonly FairlightAudioState.DynamicsState _state;

        public FairlightDynamicsAudioMixerCallback(FairlightAudioState.DynamicsState state, IBMDSwitcherFairlightAudioDynamicsProcessor props, Action<string> onChange, bool hasExpander) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();

            // Limiter
            var limiterProps = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioLimiter>(props.GetProcessor);
            state.Limiter = new FairlightAudioState.LimiterState();
            Children.Add(new FairlightLimiterDynamicsAudioMixerCallback(state.Limiter, limiterProps, AppendChange("Limiter")));

            // Compressor
            var compressorProps = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioCompressor>(props.GetProcessor);
            state.Compressor = new FairlightAudioState.CompressorState();
            Children.Add(new FairlightCompressorDynamicsAudioMixerCallback(state.Compressor, compressorProps, AppendChange("Compressor")));

            if (hasExpander)
            {
                // Expander
                var expanderProps = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioExpander>(props.GetProcessor);

            }
        }

        public override void Notify(_BMDSwitcherFairlightAudioDynamicsProcessorEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioDynamicsProcessorEventType.bmdSwitcherFairlightAudioDynamicsProcessorEventTypeMakeupGainChanged:
                    Props.GetMakeupGain(out double gain);
                    _state.MakeUpGain = gain;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
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