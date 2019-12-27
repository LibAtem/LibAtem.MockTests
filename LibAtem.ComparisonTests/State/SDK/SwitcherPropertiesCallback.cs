using System;
using BMDSwitcherAPI;
using LibAtem.State;
using LibAtem.State.Builder;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class SwitcherPropertiesCallback : SdkCallbackBaseNotify<IBMDSwitcher, _BMDSwitcherEventType>, IBMDSwitcherCallback
    {
        private readonly AtemState _state;

        public SwitcherPropertiesCallback(AtemState state, IBMDSwitcher props, Action<string> onChange, AtemStateBuilderSettings updateSettings) : base(props, onChange)
        {
            _state = state;

        }

        public override void Notify(_BMDSwitcherEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherEventType.bmdSwitcherEventTypeVideoModeChanged:
                    break;
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
                    OnChange("Info.LastTimecode");
                    break;
                case _BMDSwitcherEventType.bmdSwitcherEventTypeTimeCodeLockedChanged:
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