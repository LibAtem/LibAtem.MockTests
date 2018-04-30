using System;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.Util;

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
    public sealed class MultiViewPropertiesCallback : IBMDSwitcherMultiViewCallback, INotify<_BMDSwitcherMultiViewEventType>
    {
        private readonly ComparisonSettingsMultiViewState _state;
        private readonly IBMDSwitcherMultiView _props;

        public MultiViewPropertiesCallback(ComparisonSettingsMultiViewState state, IBMDSwitcherMultiView props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherMultiViewEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeWindowChanged:
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterEnabledChanged:
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterOpacityChanged:
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeCurrentInputSupportsVuMeterChanged:
                    Enumerable.Range(0, (int)Constants.MultiViewWindowCount).ForEach(i => Notify(eventType, i));
                    break;
                default:
                    Notify(eventType, 0);
                    break;
            }
        }

        public void Notify(_BMDSwitcherMultiViewEventType eventType, int window)
        {
            switch (eventType)
            {
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeLayoutChanged:
                    _props.GetLayout(out _BMDSwitcherMultiViewLayout layout);
                    _state.Layout = AtemEnumMaps.MultiViewLayoutMap.FindByValue(layout);
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeWindowChanged:
                    _props.GetWindowInput((uint) window, out long input);
                    _state.Windows[window].Source = (VideoSource) input;
                    break;
                // TODO remainder
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeCurrentInputSupportsVuMeterChanged:
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterEnabledChanged:
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterOpacityChanged:
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeSafeAreaEnabledChanged:
                    _props.GetSafeAreaEnabled(out int enabled);
                    _state.SafeAreaEnabled = enabled != 0;
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeProgramPreviewSwappedChanged:
                    _props.GetProgramPreviewSwapped(out int swapped);
                    _state.ProgramPreviewSwapped = swapped != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}