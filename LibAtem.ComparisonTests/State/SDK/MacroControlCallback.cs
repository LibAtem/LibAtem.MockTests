using System;
using BMDSwitcherAPI;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MacroControlCallback : IBMDSwitcherMacroControlCallback, INotify<_BMDSwitcherMacroControlEventType>
    {
        private readonly ComparisonMacroState _state;
        private readonly IBMDSwitcherMacroControl _props;

        public MacroControlCallback(ComparisonMacroState state, IBMDSwitcherMacroControl props)
        {
            _state = state;
            _props = props;
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
                            _state.RunStatus = MacroRunStatus.Idle;
                            break;
                        case _BMDSwitcherMacroRunStatus.bmdSwitcherMacroRunStatusRunning:
                            _state.RunStatus = MacroRunStatus.Running;
                            break;
                        case _BMDSwitcherMacroRunStatus.bmdSwitcherMacroRunStatusWaitingForUser:
                            _state.RunStatus = MacroRunStatus.UserWait;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(status), status, null);
                    }

                    _state.Loop = loop != 0;
                    _state.RunIndex = index;
                    break;
                case _BMDSwitcherMacroControlEventType.bmdSwitcherMacroControlEventTypeRecordStatusChanged:
                    _props.GetRecordStatus(out _BMDSwitcherMacroRecordStatus recStatus, out uint recIndex);
                    _state.IsRecording = recStatus == _BMDSwitcherMacroRecordStatus.bmdSwitcherMacroRecordStatusRecording;
                    _state.RecordIndex = recIndex;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}