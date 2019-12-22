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
    public sealed class FairlightLimiterDynamicsAudioMixerCallback : SdkCallbackBaseNotify<IBMDSwitcherFairlightAudioLimiter, _BMDSwitcherFairlightAudioLimiterEventType>, IBMDSwitcherFairlightAudioLimiterCallback
    {
        private readonly FairlightAudioState.LimiterState _state;

        public FairlightLimiterDynamicsAudioMixerCallback(FairlightAudioState.LimiterState state, IBMDSwitcherFairlightAudioLimiter props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherFairlightAudioLimiterEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioLimiterEventType.bmdSwitcherFairlightAudioLimiterEventTypeEnabledChanged:
                    Props.GetEnabled(out int enabled);
                    _state.LimiterEnabled = enabled != 0;
                    break;
                case _BMDSwitcherFairlightAudioLimiterEventType.bmdSwitcherFairlightAudioLimiterEventTypeThresholdChanged:
                    Props.GetThreshold(out double threshold);
                    _state.Threshold = threshold;
                    break;
                case _BMDSwitcherFairlightAudioLimiterEventType.bmdSwitcherFairlightAudioLimiterEventTypeAttackChanged:
                    Props.GetAttack(out double attack);
                    _state.Attack = attack;
                    break;
                case _BMDSwitcherFairlightAudioLimiterEventType.bmdSwitcherFairlightAudioLimiterEventTypeHoldChanged:
                    Props.GetHold(out double hold);
                    _state.Hold = hold;
                    break;
                case _BMDSwitcherFairlightAudioLimiterEventType.bmdSwitcherFairlightAudioLimiterEventTypeReleaseChanged:
                    Props.GetRelease(out double release);
                    _state.Release = release;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }

        public void GainReductionLevelNotification(uint numLevels, ref double levels)
        {
            //throw new NotImplementedException();
        }
    }
    public sealed class FairlightCompressorDynamicsAudioMixerCallback : SdkCallbackBaseNotify<IBMDSwitcherFairlightAudioCompressor, _BMDSwitcherFairlightAudioCompressorEventType>, IBMDSwitcherFairlightAudioCompressorCallback
    {
        private readonly FairlightAudioState.CompressorState _state;

        public FairlightCompressorDynamicsAudioMixerCallback(FairlightAudioState.CompressorState state, IBMDSwitcherFairlightAudioCompressor props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherFairlightAudioCompressorEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioCompressorEventType.bmdSwitcherFairlightAudioCompressorEventTypeEnabledChanged:
                    Props.GetEnabled(out int enabled);
                    _state.CompressorEnabled = enabled != 0;
                    break;
                case _BMDSwitcherFairlightAudioCompressorEventType.bmdSwitcherFairlightAudioCompressorEventTypeThresholdChanged:
                    Props.GetThreshold(out double threshold);
                    _state.Threshold = threshold;
                    break;
                case _BMDSwitcherFairlightAudioCompressorEventType.bmdSwitcherFairlightAudioCompressorEventTypeRatioChanged:
                    Props.GetRatio(out double ratio);
                    _state.Ratio = ratio;
                    break;
                case _BMDSwitcherFairlightAudioCompressorEventType.bmdSwitcherFairlightAudioCompressorEventTypeAttackChanged:
                    Props.GetAttack(out double attack);
                    _state.Attack = attack;
                    break;
                case _BMDSwitcherFairlightAudioCompressorEventType.bmdSwitcherFairlightAudioCompressorEventTypeHoldChanged:
                    Props.GetHold(out double hold);
                    _state.Hold = hold;
                    break;
                case _BMDSwitcherFairlightAudioCompressorEventType.bmdSwitcherFairlightAudioCompressorEventTypeReleaseChanged:
                    Props.GetRelease(out double release);
                    _state.Release = release;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }

        public void GainReductionLevelNotification(uint numLevels, ref double levels)
        {
            // throw new NotImplementedException();
        }
    }
}