using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using LibAtem.Util;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class HyperDeckPropertiesCallback : SdkCallbackBaseNotify<IBMDSwitcherHyperDeck, _BMDSwitcherHyperDeckEventType>, IBMDSwitcherHyperDeckCallback
    {
        private readonly SettingsState.HyperdeckState _state;

        public HyperDeckPropertiesCallback(SettingsState.HyperdeckState state, IBMDSwitcherHyperDeck props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherHyperDeckEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeConnectionStatusChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeRemoteAccessEnabledChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeStorageMediaStateChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeEstimatedRecordTimeRemainingChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeActiveStorageMediaChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeClipCountChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeSwitcherInputChanged:
                    Props.GetSwitcherInput(out long inputId);
                    _state.Input = (VideoSource) inputId;
                    OnChange("Input");
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeFrameRateChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeInterlacedVideoChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeDropFrameTimeCodeChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypePlayerStateChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeCurrentClipChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeCurrentClipTimeChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeCurrentTimelineTimeChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeShuttleSpeedChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeLoopedPlaybackChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeSingleClipPlaybackChanged:
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeAutoRollOnTakeChanged:
                    Props.GetAutoRollOnTake(out int autoRoll);
                    _state.AutoRoll = autoRoll != 0;
                    OnChange("AutoRoll");
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeAutoRollOnTakeFrameDelayChanged:
                    Props.GetAutoRollOnTakeFrameDelay(out ushort frameDelay);
                    _state.AutoRollFrameDelay = frameDelay;
                    OnChange("AutoRollFrameDelay");
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeNetworkAddressChanged:
                    Props.GetNetworkAddress(out uint address);
                    _state.NetworkAddress = address == 0 ? null : IPUtil.IPToString(BitConverter.GetBytes(address));
                    OnChange("NetworkAddress");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            // _onChange();
        }

        public void NotifyError(_BMDSwitcherHyperDeckErrorType errorType)
        {
            //throw new NotImplementedException();
        }
    }
}