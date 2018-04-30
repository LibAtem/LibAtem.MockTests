using System;
using BMDSwitcherAPI;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class SwitcherPropertiesCallback : IBMDSwitcherCallback, INotify<_BMDSwitcherEventType>
    {
        private readonly ComparisonState _state;
        private readonly IBMDSwitcher _props;

        public SwitcherPropertiesCallback(ComparisonState state, IBMDSwitcher props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherEventType.bmdSwitcherEventTypeVideoModeChanged:
                    _props.GetVideoMode(out _BMDSwitcherVideoMode videoMode);
                    _state.Settings.VideoMode = AtemEnumMaps.VideoModesMap.FindByValue(videoMode);
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