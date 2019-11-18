using System;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using LibAtem.Util;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MultiViewPropertiesCallback : IBMDSwitcherMultiViewCallback, INotify<_BMDSwitcherMultiViewEventType>
    {
        private readonly MultiViewerState _state;
        private readonly IBMDSwitcherMultiView _props;
        private readonly Action<string> _onChange;

        public MultiViewPropertiesCallback(MultiViewerState state, IBMDSwitcherMultiView props, Action<string> onChange)
        {
            _state = state;
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
                    break;
            }
        }

        public void Notify(_BMDSwitcherMultiViewEventType eventType, int window)
        {
            switch (eventType)
            {
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeLayoutChanged:
                    _props.GetLayout(out _BMDSwitcherMultiViewLayout layout);
                    _state.Properties.Layout = AtemEnumMaps.MultiViewLayoutMap.FindByValue(layout);
                    _onChange("Properties");
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeWindowChanged:
                    _props.GetWindowInput((uint)window, out long input);
                    _state.Windows[window].Source = (VideoSource)input;
                    _onChange($"Windows.{window}");
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeCurrentInputSupportsVuMeterChanged:
                    _props.SupportsVuMeters(out int supportsVu);
                    if (supportsVu != 0)
                        _props.CurrentInputSupportsVuMeter((uint)window, out supportsVu);
                    _state.Windows[window].SupportsVuMeter = supportsVu != 0;
                    _onChange($"Windows.{window}");
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterEnabledChanged:
                    //_props.GetVuMeterEnabled((uint)window, out int vuEnabled);
                    //_state.Windows[window].VuMeter = vuEnabled != 0;
                    _onChange($"Windows.{window}");
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterOpacityChanged:
                    //_props.GetVuMeterOpacity(out double opacity);
                    //_state.VuMeterOpacity = opacity * 100;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeSafeAreaEnabledChanged:
                    //_props.GetSafeAreaEnabled(out int enabled);
                    //_state.SafeAreaEnabled = enabled != 0;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeProgramPreviewSwappedChanged:
                    _props.GetProgramPreviewSwapped(out int swapped);
                    _state.Properties.ProgramPreviewSwapped = swapped != 0;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeCurrentInputSupportsSafeAreaChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}