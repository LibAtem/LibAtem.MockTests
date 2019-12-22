using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class SerialPortPropertiesCallback : SdkCallbackBaseNotify<IBMDSwitcherSerialPort, _BMDSwitcherSerialPortEventType>, IBMDSwitcherSerialPortCallback
    {
        private readonly SettingsState _state;

        public SerialPortPropertiesCallback(SettingsState state, IBMDSwitcherSerialPort props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherSerialPortEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherSerialPortEventType.bmdSwitcherSerialPortEventTypeFunctionChanged:
                    Props.GetFunction(out _BMDSwitcherSerialPortFunction function);
                    _state.SerialMode = AtemEnumMaps.SerialModeMap.FindByValue(function);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }
}