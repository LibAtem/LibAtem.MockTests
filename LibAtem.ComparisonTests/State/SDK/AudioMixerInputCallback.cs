using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class AudioMixerInputCallback : SdkCallbackBaseNotify<IBMDSwitcherAudioInput, _BMDSwitcherAudioInputEventType>, IBMDSwitcherAudioInputCallback
    {
        private readonly AudioState.InputState _state;

        public AudioMixerInputCallback(AudioState.InputState state, IBMDSwitcherAudioInput props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;

            props.GetType(out _BMDSwitcherAudioInputType type);
            _state.Properties.SourceType = AtemEnumMaps.AudioSourceTypeMap.FindByValue(type);

            TriggerAllChanged();
        }
        
        public override void Notify(_BMDSwitcherAudioInputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeCurrentExternalPortTypeChanged:
                    Props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType type);
                    _state.Properties.PortType = ((ExternalPortTypeFlags)type).ToAudioPortType();
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeMixOptionChanged:
                    Props.GetMixOption(out _BMDSwitcherAudioMixOption mixOption);
                    _state.Properties.MixOption = AtemEnumMaps.AudioMixOptionMap.FindByValue(mixOption);
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeGainChanged:
                    Props.GetGain(out double gain);
                    _state.Properties.Gain = gain;
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeBalanceChanged:
                    Props.GetBalance(out double balance);
                    _state.Properties.Balance = balance * 50;
                    break;
                case _BMDSwitcherAudioInputEventType.bmdSwitcherAudioInputEventTypeIsMixedInChanged:
                    /*
                    _props.IsMixedIn(out int mixedIn);
                    _state.IsMixedIn = mixedIn != 0;
                    _onChange("IsMixedIn");
                    */
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange("Properties");
        }

        public void LevelNotification(double left, double right, double peakLeft, double peakRight)
        {
            _state.Levels.LevelLeft = left;
            _state.Levels.LevelRight = right;
            _state.Levels.PeakLeft = peakLeft;
            _state.Levels.PeakRight = peakRight;
            OnChange("Level");
        }
    }
}