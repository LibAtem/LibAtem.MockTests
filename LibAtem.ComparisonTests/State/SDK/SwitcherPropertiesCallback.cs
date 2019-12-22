using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class SwitcherPropertiesCallback : SdkCallbackBaseNotify<IBMDSwitcher, _BMDSwitcherEventType>, IBMDSwitcherCallback
    {
        private readonly AtemState _state;

        public SwitcherPropertiesCallback(AtemState state, IBMDSwitcher props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();

            props.GetProductName(out string productName);
            state.Info.ProductName = productName;
        }

        public override void Notify(_BMDSwitcherEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherEventType.bmdSwitcherEventTypeVideoModeChanged:
                    Props.GetVideoMode(out _BMDSwitcherVideoMode videoMode);
                    _state.Settings.VideoMode = AtemEnumMaps.VideoModesMap.FindByValue(videoMode);
                    OnChange("Settings.VideoMode");
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
                    Props.GetTimeCode(out byte hours, out byte minutes, out byte seconds, out byte frames, out int dropFrame);
                    _state.Info.LastTimecode = new Timecode
                    {
                        Hour = hours,
                        Minute = minutes,
                        Second = seconds,
                        Frame = frames,
                        DropFrame = dropFrame != 0
                    };
                    OnChange("Info.LastTimecode");
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeTimeCodeLockedChanged:
                    Props.GetTimeCodeLocked(out int locked);
                    _state.Info.TimecodeLocked = locked != 0;
                    OnChange("Info.TimecodeLocked");
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeSuperSourceCascadeChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeAutoVideoModeChanged:
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeAutoVideoModeDetectedChanged:
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