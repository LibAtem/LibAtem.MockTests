﻿using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class SwitcherPropertiesCallback : IBMDSwitcherCallback, INotify<_BMDSwitcherEventType>
    {
        private readonly AtemState _state;
        private readonly IBMDSwitcher _props;
        private readonly Action<string> _onChange;

        public SwitcherPropertiesCallback(AtemState state, IBMDSwitcher props, Action<string> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherEventType.bmdSwitcherEventTypeVideoModeChanged:
                    _props.GetVideoMode(out _BMDSwitcherVideoMode videoMode);
                    _state.Settings.VideoMode = AtemEnumMaps.VideoModesMap.FindByValue(videoMode);
                    _onChange("Settings.VideoMode");
                    break;
                // TODO - the rest
                case _BMDSwitcherEventType.bmdSwitcherEventTypeMethodForDownConvertedSDChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeDownConvertedHDVideoModeChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeMultiViewVideoModeChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypePowerStatusChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeDisconnected:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventType3GSDIOutputLevelChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeTimeCodeChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeTimeCodeLockedChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeSuperSourceCascadeChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }

        public void Notify(_BMDSwitcherEventType eventType, _BMDSwitcherVideoMode coreVideoMode)
        {
            Notify(eventType);
        }
    }
}