﻿using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class AudioMixerMonitorOutputCallback : IBMDSwitcherAudioMonitorOutputCallback, INotify<_BMDSwitcherAudioMonitorOutputEventType>
    {
        private readonly AudioState.MonitorOutputState _state;
        private readonly IBMDSwitcherAudioMonitorOutput _props;
        private readonly Action _onChange;

        public AudioMixerMonitorOutputCallback(AudioState.MonitorOutputState state, IBMDSwitcherAudioMonitorOutput props, Action onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherAudioMonitorOutputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeMonitorEnableChanged:
                    _props.GetMonitorEnable(out int enable);
                    _state.Enabled = enable != 0;
                    break;
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeGainChanged:
                    _props.GetGain(out double gain);
                    _state.Gain = gain;
                    break;
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeMuteChanged:
                    _props.GetMute(out int mute);
                    _state.Mute = mute != 0;
                    break;
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeSoloChanged:
                    _props.GetSolo(out int solo);
                    _state.Solo = solo != 0;
                    break;
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeSoloInputChanged:
                    _props.GetSoloInput(out long soloInput);
                    _state.SoloSource = (AudioSource)soloInput;
                    break;
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeDimChanged:
                    _props.GetDim(out int dim);
                    _state.Dim = dim != 0;
                    break;
                case _BMDSwitcherAudioMonitorOutputEventType.bmdSwitcherAudioMonitorOutputEventTypeDimLevelChanged:
                    /*
                    _props.GetDimLevel(out double dimLevel);
                    _state.DimLevel = dimLevel;
                    */
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange();
        }

        public void LevelNotification(double left, double right, double peakLeft, double peakRight)
        {
            // TODO
        }
    }
}