using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Settings;
using LibAtem.State;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class SerialPortPropertiesCallback : IBMDSwitcherSerialPortCallback, INotify<_BMDSwitcherSerialPortEventType>
    {
        private readonly SettingsState _state;
        private readonly IBMDSwitcherSerialPort _props;
        private readonly Action<CommandQueueKey> _onChange;

        public SerialPortPropertiesCallback(SettingsState state, IBMDSwitcherSerialPort props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
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

            _onChange(new CommandQueueKey(new SerialPortModeCommand()));
        }
    }
}