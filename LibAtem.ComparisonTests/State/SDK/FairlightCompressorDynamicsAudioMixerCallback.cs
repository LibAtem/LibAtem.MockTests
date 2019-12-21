using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class FairlightCompressorDynamicsAudioMixerCallback : IBMDSwitcherFairlightAudioCompressorCallback, INotify<_BMDSwitcherFairlightAudioCompressorEventType>
    {
        private readonly FairlightAudioState.CompressorState _state;
        private readonly IBMDSwitcherFairlightAudioCompressor _props;
        private readonly Action<string> _onChange;

        public FairlightCompressorDynamicsAudioMixerCallback(FairlightAudioState.CompressorState state, IBMDSwitcherFairlightAudioCompressor props, Action<string> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherFairlightAudioCompressorEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioCompressorEventType.bmdSwitcherFairlightAudioCompressorEventTypeEnabledChanged:
                    _props.GetEnabled(out int enabled);
                    _state.CompressorEnabled = enabled != 0;
                    _onChange("CompressorEnabled");
                    break;
                case _BMDSwitcherFairlightAudioCompressorEventType.bmdSwitcherFairlightAudioCompressorEventTypeThresholdChanged:
                    _props.GetThreshold(out double threshold);
                    _state.Threshold = threshold;
                    _onChange("Threshold");
                    break;
                case _BMDSwitcherFairlightAudioCompressorEventType.bmdSwitcherFairlightAudioCompressorEventTypeRatioChanged:
                    _props.GetRatio(out double ratio);
                    _state.Ratio = ratio;
                    _onChange("Ratio");
                    break;
                case _BMDSwitcherFairlightAudioCompressorEventType.bmdSwitcherFairlightAudioCompressorEventTypeAttackChanged:
                    _props.GetAttack(out double attack);
                    _state.Attack = attack;
                    _onChange("Attack");
                    break;
                case _BMDSwitcherFairlightAudioCompressorEventType.bmdSwitcherFairlightAudioCompressorEventTypeHoldChanged:
                    _props.GetHold(out double hold);
                    _state.Hold = hold;
                    _onChange("Hold");
                    break;
                case _BMDSwitcherFairlightAudioCompressorEventType.bmdSwitcherFairlightAudioCompressorEventTypeReleaseChanged:
                    _props.GetRelease(out double release);
                    _state.Release = release;
                    _onChange("Release");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }

        public void GainReductionLevelNotification(uint numLevels, ref double levels)
        {
            // throw new NotImplementedException();
        }
    }
}