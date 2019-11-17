using System;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.Settings.Multiview;
using LibAtem.Common;
using LibAtem.State;
using LibAtem.Util;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class MultiViewPropertiesCallback : IBMDSwitcherMultiViewCallback, INotify<_BMDSwitcherMultiViewEventType>
    {
        private readonly MultiViewerState _state;
        private readonly uint _id;
        private readonly IBMDSwitcherMultiView _props;
        private readonly Action<CommandQueueKey> _onChange;

        public MultiViewPropertiesCallback(MultiViewerState state, uint id, IBMDSwitcherMultiView props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _id = id;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherMultiViewEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeWindowChanged:
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterEnabledChanged:
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterOpacityChanged:
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeCurrentInputSupportsVuMeterChanged:
                    Enumerable.Range(0, _state.Windows.Count).ForEach(i => Notify(eventType, i));
                    break;
                default:
                    Notify(eventType, 0);
                    _onChange(new CommandQueueKey(new MultiviewPropertiesGetCommand() { MultiviewIndex = _id }));
                    break;
            }

            _onChange(new CommandQueueKey(new MultiviewPropertiesGetCommand()));
        }

        public void Notify(_BMDSwitcherMultiViewEventType eventType, int window)
        {
            switch (eventType)
            {
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeLayoutChanged:
                    _props.GetLayout(out _BMDSwitcherMultiViewLayout layout);
                    _state.Properties.Layout = AtemEnumMaps.MultiViewLayoutMap.FindByValue(layout);
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeWindowChanged:
                    _props.GetWindowInput((uint)window, out long input);
                    _state.Windows[window].Source = (VideoSource)input;
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeCurrentInputSupportsVuMeterChanged:
                    _props.SupportsVuMeters(out int supportsVu);
                    if (supportsVu != 0)
                        _props.CurrentInputSupportsVuMeter((uint)window, out supportsVu);
                    _state.Windows[window].SupportsVuMeter = supportsVu != 0;
                    _onChange(new CommandQueueKey(new MultiviewWindowVuMeterGetCommand() { MultiviewIndex = _id, WindowIndex = (uint)window }));
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterEnabledChanged:
                    //_props.GetVuMeterEnabled((uint)window, out int vuEnabled);
                    //_state.Windows[window].VuMeter = vuEnabled != 0;
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterOpacityChanged:
                    //_props.GetVuMeterOpacity(out double opacity);
                    //_state.VuMeterOpacity = opacity * 100;
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeSafeAreaEnabledChanged:
                    //_props.GetSafeAreaEnabled(out int enabled);
                    //_state.SafeAreaEnabled = enabled != 0;
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeProgramPreviewSwappedChanged:
                    _props.GetProgramPreviewSwapped(out int swapped);
                    _state.Properties.ProgramPreviewSwapped = swapped != 0;
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeCurrentInputSupportsSafeAreaChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange(new CommandQueueKey(new MultiviewPropertiesGetCommand()));
        }
    }
}