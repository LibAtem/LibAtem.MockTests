using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MacroControlCallback : SdkCallbackBaseNotify<IBMDSwitcherMacroControl, _BMDSwitcherMacroControlEventType>, IBMDSwitcherMacroControlCallback
    {
        private readonly MacroState _state;

        public MacroControlCallback(MacroState state, IBMDSwitcherMacroControl props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherMacroControlEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherMacroControlEventType.bmdSwitcherMacroControlEventTypeRunStatusChanged:
                    Props.GetRunStatus(out _BMDSwitcherMacroRunStatus status, out int loop, out uint index);

                    switch (status)
                    {
                        case _BMDSwitcherMacroRunStatus.bmdSwitcherMacroRunStatusIdle:
                            _state.RunStatus.RunStatus = MacroState.MacroRunStatus.Idle;
                            break;
                        case _BMDSwitcherMacroRunStatus.bmdSwitcherMacroRunStatusRunning:
                            _state.RunStatus.RunStatus = MacroState.MacroRunStatus.Running;
                            break;
                        case _BMDSwitcherMacroRunStatus.bmdSwitcherMacroRunStatusWaitingForUser:
                            _state.RunStatus.RunStatus = MacroState.MacroRunStatus.UserWait;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(status), status, null);
                    }

                    _state.RunStatus.Loop = loop != 0;
                    _state.RunStatus.RunIndex = index;
                    OnChange("RunStatus");
                    break;
                case _BMDSwitcherMacroControlEventType.bmdSwitcherMacroControlEventTypeRecordStatusChanged:
                    Props.GetRecordStatus(out _BMDSwitcherMacroRecordStatus recStatus, out uint recIndex);
                    _state.RecordStatus.IsRecording = recStatus == _BMDSwitcherMacroRecordStatus.bmdSwitcherMacroRecordStatusRecording;
                    _state.RecordStatus.RecordIndex = recIndex;
                    OnChange("RecordStatus");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}