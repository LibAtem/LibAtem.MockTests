using System;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.Util;

namespace LibAtem.ComparisonTests.State.SDK
{
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
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeCurrentInputSupportsVuMeterChanged:
                    _props.CurrentInputSupportsVuMeter((uint)window, out int supportsVu);
                    _state.Windows[window].SupportsVuMeter = supportsVu != 0;
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterEnabledChanged:
                    _props.GetVuMeterEnabled((uint)window, out int vuEnabled);
                    _state.Windows[window].VuMeter = vuEnabled != 0;
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterOpacityChanged:
                    _props.GetVuMeterOpacity(out double opacity);
                    _state.VuMeterOpacity = opacity * 100;
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