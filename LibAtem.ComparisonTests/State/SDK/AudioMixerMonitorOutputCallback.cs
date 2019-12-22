using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class AudioMixerMonitorOutputCallback : SdkCallbackBaseNotify<IBMDSwitcherAudioMonitorOutput, _BMDSwitcherAudioMonitorOutputEventType>, IBMDSwitcherAudioMonitorOutputCallback
    {
        private readonly AudioState.MonitorOutputState _state;

        public AudioMixerMonitorOutputCallback(AudioState.MonitorOutputState state, IBMDSwitcherAudioMonitorOutput props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherAudioMonitorOutputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeMonitorEnableChanged:
                    Props.GetMonitorEnable(out int enable);
                    _state.Enabled = enable != 0;
                    break;
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeGainChanged:
                    Props.GetGain(out double gain);
                    _state.Gain = gain;
                    break;
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeMuteChanged:
                    Props.GetMute(out int mute);
                    _state.Mute = mute != 0;
                    break;
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeSoloChanged:
                    Props.GetSolo(out int solo);
                    _state.Solo = solo != 0;
                    break;
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeSoloInputChanged:
                    Props.GetSoloInput(out long soloInput);
                    _state.SoloSource = (AudioSource)soloInput;
                    break;
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeDimChanged:
                    Props.GetDim(out int dim);
                    _state.Dim = dim != 0;
                    break;
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeDimLevelChanged:
                    /*
                    Props.GetDimLevel(out double dimLevel);
                    _state.DimLevel = dimLevel;
                    */
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }

        public void LevelNotification(double left, double right, double peakLeft, double peakRight)
        {
            // TODO
        }
    }
}