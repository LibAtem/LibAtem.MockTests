using System;
using System.ComponentModel;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using LibAtem.Util;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class HyperDeckPropertiesCallback : IBMDSwitcherHyperDeckCallback, INotify<_BMDSwitcherHyperDeckEventType>
    {
        private readonly SettingsState.HyperdeckState _state;
        private readonly IBMDSwitcherHyperDeck _props;
        private readonly Action<string> _onChange;

        public HyperDeckPropertiesCallback(SettingsState.HyperdeckState state, IBMDSwitcherHyperDeck props, Action<string> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherHyperDeckEventType eventType)
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
                    _props.GetSwitcherInput(out long inputId);
                    _state.Input = (VideoSource) inputId;
                    _onChange("Input");
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
                    _props.GetAutoRollOnTake(out int autoRoll);
                    _state.AutoRoll = autoRoll != 0;
                    _onChange("AutoRoll");
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeAutoRollOnTakeFrameDelayChanged:
                    _props.GetAutoRollOnTakeFrameDelay(out ushort frameDelay);
                    _state.AutoRollFrameDelay = frameDelay;
                    _onChange("AutoRollFrameDelay");
                    break;
                case _BMDSwitcherHyperDeckEventType.bmdSwitcherHyperDeckEventTypeNetworkAddressChanged:
                    _props.GetNetworkAddress(out uint address);
                    _state.NetworkAddress = address == 0 ? null : IPUtil.IPToString(BitConverter.GetBytes(address));
                    _onChange("NetworkAddress");
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