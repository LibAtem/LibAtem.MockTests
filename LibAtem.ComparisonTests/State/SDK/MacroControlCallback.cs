﻿using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MacroControlCallback : IBMDSwitcherMacroControlCallback, INotify<_BMDSwitcherMacroControlEventType>
    {
        private readonly MacroState _state;
        private readonly IBMDSwitcherMacroControl _props;
        private readonly Action<string> _onChange;

        public MacroControlCallback(MacroState state, IBMDSwitcherMacroControl props, Action<string> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherMacroControlEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherMacroControlEventType.bmdSwitcherMacroControlEventTypeRunStatusChanged:
                    _props.GetRunStatus(out _BMDSwitcherMacroRunStatus status, out int loop, out uint index);

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
                    _onChange("RunStatus");
                    break;
                case _BMDSwitcherMacroControlEventType.bmdSwitcherMacroControlEventTypeRecordStatusChanged:
                    _props.GetRecordStatus(out _BMDSwitcherMacroRecordStatus recStatus, out uint recIndex);
                    _state.RecordStatus.IsRecording = recStatus == _BMDSwitcherMacroRecordStatus.bmdSwitcherMacroRecordStatusRecording;
                    _state.RecordStatus.RecordIndex = recIndex;
                    _onChange("RecordStatus");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}