using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
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