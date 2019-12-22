using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
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
}