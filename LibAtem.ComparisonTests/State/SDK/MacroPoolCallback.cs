using System;
using BMDSwitcherAPI;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MacroPoolCallback : IBMDSwitcherMacroPoolCallback
    {
        private readonly ComparisonMacroState _state;
        private readonly IBMDSwitcherMacroPool _props;

        public MacroPoolCallback(ComparisonMacroState state, IBMDSwitcherMacroPool props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherMacroPoolEventType eventType, uint index, IBMDSwitcherTransferMacro macroTransfer)
        {
            switch (eventType)
            {
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeValidChanged:
                    _props.IsValid(index, out int valid);
                    _state.Pool[index].IsUsed = valid != 0;
                    break;
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeHasUnsupportedOpsChanged:
                    break;
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeNameChanged:
                    _props.GetName(index, out string name);
                    _state.Pool[index].Name = name;
                    break;
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeDescriptionChanged:
                    _props.GetDescription(index, out string description);
                    _state.Pool[index].Description = description;
                    break;
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeTransferCompleted:
                    break;
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeTransferCancelled:
                    break;
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeTransferFailed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}