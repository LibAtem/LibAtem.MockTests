using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class FairlightLimiterDynamicsAudioMixerCallback : IBMDSwitcherFairlightAudioLimiterCallback, INotify<_BMDSwitcherFairlightAudioLimiterEventType>
    {
        private readonly FairlightAudioState.LimiterState _state;
        private readonly IBMDSwitcherFairlightAudioLimiter _props;
        private readonly Action<string> _onChange;

        public FairlightLimiterDynamicsAudioMixerCallback(FairlightAudioState.LimiterState state, IBMDSwitcherFairlightAudioLimiter props, Action<string> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherFairlightAudioLimiterEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioLimiterEventType.bmdSwitcherFairlightAudioLimiterEventTypeEnabledChanged:
                    _props.GetEnabled(out int enabled);
                    _state.LimiterEnabled = enabled != 0;
                    _onChange("LimiterEnabled");
                    break;
                case _BMDSwitcherFairlightAudioLimiterEventType.bmdSwitcherFairlightAudioLimiterEventTypeThresholdChanged:
                    _props.GetThreshold(out double threshold);
                    _state.Threshold = threshold;
                    _onChange("Threshold");
                    break;
                case _BMDSwitcherFairlightAudioLimiterEventType.bmdSwitcherFairlightAudioLimiterEventTypeAttackChanged:
                    _props.GetAttack(out double attack);
                    _state.Attack = attack;
                    _onChange("Attack");
                    break;
                case _BMDSwitcherFairlightAudioLimiterEventType.bmdSwitcherFairlightAudioLimiterEventTypeHoldChanged:
                    _props.GetHold(out double hold);
                    _state.Hold = hold;
                    _onChange("Hold");
                    break;
                case _BMDSwitcherFairlightAudioLimiterEventType.bmdSwitcherFairlightAudioLimiterEventTypeReleaseChanged:
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
            //throw new NotImplementedException();
        }
    }
}