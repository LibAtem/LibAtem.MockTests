using System;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.State;
using LibAtem.Util;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MacroPoolCallback : SdkCallbackBase<IBMDSwitcherMacroPool>, IBMDSwitcherMacroPoolCallback
    {
        private readonly MacroState _state;

        public MacroPoolCallback(MacroState state, IBMDSwitcherMacroPool props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;

            props.GetMaxCount(out uint count);
            state.Pool = Enumerable.Repeat(0, (int)count).Select(i => new MacroState.ItemState()).ToList();
            for (uint i = 0; i < count; i++)
            {
                Enum.GetValues(typeof(_BMDSwitcherMacroPoolEventType)).OfType<_BMDSwitcherMacroPoolEventType>().ForEach(e => Notify(e, i, null));
            }
        }

        public void Notify(_BMDSwitcherMacroPoolEventType eventType, uint index, IBMDSwitcherTransferMacro macroTransfer)
        {
            switch (eventType)
            {
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeValidChanged:
                    Props.IsValid(index, out int valid);
                    _state.Pool[(int)index].IsUsed = valid != 0;
                    break;
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeHasUnsupportedOpsChanged:
                    break;
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeNameChanged:
                    Props.GetName(index, out string name);
                    _state.Pool[(int)index].Name = name;
                    break;
                case _BMDSwitcherMacroPoolEventType.bmdSwitcherMacroPoolEventTypeDescriptionChanged:
                    Props.GetDescription(index, out string description);
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

            OnChange($"{index:D}");
        }
    }
}