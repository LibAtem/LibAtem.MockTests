using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Macro;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MacroPoolCallback : IBMDSwitcherMacroPoolCallback
    {
        private readonly MacroState _state;
        private readonly IBMDSwitcherMacroPool _props;
        private readonly Action<CommandQueueKey> _onChange;

        public MacroPoolCallback(MacroState state, IBMDSwitcherMacroPool props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherMacroPoolEventType eventType, uint index, IBMDSwitcherTransferMacro macroTransfer)
        {
            switch (eventType)
            {
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeValidChanged:
                    _props.IsValid(index, out int valid);
                    _state.Pool[(int)index].IsUsed = valid != 0;
                    break;
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeHasUnsupportedOpsChanged:
                    break;
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeNameChanged:
                    _props.GetName(index, out string name);
                    _state.Pool[(int)index].Name = name;
                    break;
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeDescriptionChanged:
                    _props.GetDescription(index, out string description);
                    _state.Pool[(int)index].Description = description;
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

            _onChange(new CommandQueueKey(new MacroPropertiesGetCommand() { Index = index }));
        }
    }
}