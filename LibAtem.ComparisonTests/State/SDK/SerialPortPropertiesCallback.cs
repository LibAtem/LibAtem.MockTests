using System;
using BMDSwitcherAPI;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class SerialPortPropertiesCallback : IBMDSwitcherSerialPortCallback, INotify<_BMDSwitcherSerialPortEventType>
    {
        private readonly ComparisonSettingsState _state;
        private readonly IBMDSwitcherSerialPort _props;

        public SerialPortPropertiesCallback(ComparisonSettingsState state, IBMDSwitcherSerialPort props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherSerialPortEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherSerialPortEventType.bmdSwitcherSerialPortEventTypeFunctionChanged:
                    _props.GetFunction(out _BMDSwitcherSerialPortFunction function);
                    _state.SerialMode = AtemEnumMaps.SerialModeMap.FindByValue(function);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}