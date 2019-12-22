using System;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using LibAtem.Util;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MultiViewPropertiesCallback : SdkCallbackBaseNotify<IBMDSwitcherMultiView, _BMDSwitcherMultiViewEventType>, IBMDSwitcherMultiViewCallback
    {
        private readonly MultiViewerState _state;

        public MultiViewPropertiesCallback(MultiViewerState state, IBMDSwitcherMultiView props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;

            props.SupportsVuMeters(out int supportsVu);
            _state.SupportsVuMeters = supportsVu != 0;
            props.SupportsProgramPreviewSwap(out int supportsSwap);
            _state.SupportsProgramPreviewSwapped = supportsSwap != 0;
            props.SupportsQuadrantLayout(out int supportsQuadrants);
            _state.SupportsQuadrantLayout = supportsQuadrants != 0;

            props.GetWindowCount(out uint count);
            _state.Windows = Enumerable.Repeat(0, (int)count).Select(i => new MultiViewerState.WindowState()).ToList();

            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherMultiViewEventType eventType)
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
                    Props.GetLayout(out _BMDSwitcherMultiViewLayout layout);
                    _state.Properties.Layout = (MultiViewLayoutV8) layout;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeWindowChanged:
                    Props.GetWindowInput((uint)window, out long input);
                    _state.Windows[window].Source = (VideoSource)input;
                    OnChange($"Windows.{window:D}");
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeCurrentInputSupportsVuMeterChanged:
                    if (_state.SupportsVuMeters)
                    {
                        Props.CurrentInputSupportsVuMeter((uint) window, out int supportsVu);
                        _state.Windows[window].SupportsVuMeter = supportsVu != 0;
                        OnChange($"Windows.{window:D}");
                    }
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterEnabledChanged:
                    if (_state.SupportsVuMeters)
                    {
                        Props.GetVuMeterEnabled((uint) window, out int vuEnabled);
                        _state.Windows[window].VuMeter = vuEnabled != 0;
                        OnChange($"Windows.{window:D}");
                    }
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterOpacityChanged:
                    if (_state.SupportsVuMeters)
                    {
                        Props.GetVuMeterOpacity(out double opacity);
                        _state.VuMeterOpacity = opacity * 100;
                        OnChange("Properties");
                    }
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeSafeAreaEnabledChanged:
                    //_props.GetSafeAreaEnabled((uint) window, out int enabled);
                    //_state.Windows[window].SafeAreaEnabled = enabled != 0;
                    //_onChange($"Windows.{window:D}");
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeProgramPreviewSwappedChanged:
                    Props.GetProgramPreviewSwapped(out int swapped);
                    _state.Properties.ProgramPreviewSwapped = swapped != 0;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeCurrentInputSupportsSafeAreaChanged:
                    //_props.CurrentInputSupportsSafeArea((uint) window, out int supportsSafeArea);
                    //_state.Windows[window].SupportsSafeArea = supportsSafeArea != 0;
                    //_onChange($"Windows.{window:D}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}