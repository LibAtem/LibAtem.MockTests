using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using System.Linq;

namespace LibAtem.MockTests.SdkState
{
    public static class MultiViewerStateBuilder
    {
        public static void Build(IBMDSwitcher switcher, AtemState state)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMultiViewIterator>(switcher.CreateIterator);
            state.Settings.MultiViewers = AtemSDKConverter.IterateList<IBMDSwitcherMultiView, MultiViewerState>(
                iterator.Next,
                (mv, id) =>
                {
                    var res = BuildOne(mv);

                    if (state.Info.MultiViewers == null)
                        state.Info.MultiViewers = res.Item1;

                    return res.Item2;
                });
        }

        private static Tuple<InfoState.MultiViewInfoState, MultiViewerState> BuildOne(IBMDSwitcherMultiView props)
        {
            var state = new MultiViewerState();
            var info = new InfoState.MultiViewInfoState();

            props.SupportsVuMeters(out int supportsVu);
            info.SupportsVuMeters = supportsVu != 0;
            props.SupportsProgramPreviewSwap(out int supportsSwap);
            info.SupportsProgramPreviewSwapped = supportsSwap != 0;
            props.SupportsQuadrantLayout(out int supportsQuadrants);
            info.SupportsQuadrantLayout = supportsQuadrants != 0;
            props.CanToggleSafeAreaEnabled(out int supportsToggleSafeArea);
            info.SupportsToggleSafeArea = supportsToggleSafeArea != 0;
            props.CanRouteInputs(out int canRoute);
            info.CanRouteInputs = canRoute != 0;
#if !ATEM_v8_1
            props.CanChangeLayout(out int canChangeLayout);
            info.CanChangeLayout = canChangeLayout != 0;
            props.CanAdjustVuMeterOpacity(out int canChangeVuOpacity);
            info.CanChangeVuMeterOpacity = canChangeVuOpacity != 0;

            props.CanChangeOverlayProperties(out int canChangeOverlayProperties);
            info.SupportsOverlayProperties = canChangeOverlayProperties != 0;

            if (info.SupportsOverlayProperties)
            {
                props.GetBorderColor(out double red, out double green, out double blue, out double alpha);
                state.BorderColor = new MultiViewerState.BorderColorState() { Red = red, Green = green, Blue = blue, Alpha = alpha };
            }
#endif

            props.GetLayout(out _BMDSwitcherMultiViewLayout layout);
            state.Properties.Layout = (MultiViewLayoutV8)layout;
            props.GetProgramPreviewSwapped(out int swapped);
            state.Properties.ProgramPreviewSwapped = swapped != 0;
            
            if (info.SupportsVuMeters)
            {
                props.GetVuMeterOpacity(out double opacity);
                state.VuMeterOpacity = opacity * 100;
            }

            props.GetWindowCount(out uint count);
            state.Windows = Enumerable.Range(0, (int)count).Select(window =>
            {
                props.GetWindowInput((uint)window, out long input);
                props.GetSafeAreaEnabled((uint) window, out int enabled);

                var st = new MultiViewerState.WindowState
                {
                    Source = (VideoSource)input,
                    SafeAreaEnabled = enabled != 0,
                };

                if (info.SupportsToggleSafeArea)
                {
                    props.CurrentInputSupportsSafeArea((uint)window, out int supportsSafeArea);
                    st.SupportsSafeArea = supportsSafeArea != 0;
                }

                if (info.SupportsVuMeters)
                {
                    props.CurrentInputSupportsVuMeter((uint)window, out int windowSupportsVu);
                    st.SupportsVuMeter = windowSupportsVu != 0;
                    props.GetVuMeterEnabled((uint)window, out int vuEnabled);
                    st.VuMeterEnabled = vuEnabled != 0;
                }

                if (info.SupportsOverlayProperties)
                {
                    //  props.CurrentInputSupportsLabelOverlay((uint)window, out int windowSupportsLabel);
                    // st.SupportsLabelVisible = windowSupportsLabel != 0;
                    props.GetLabelVisible((uint)window, out int labelVisible);
                    st.LabelVisible = labelVisible != 0;
                    props.GetBorderVisible((uint)window, out int borderVisible);
                    st.BorderVisible = borderVisible != 0;
                }

                return st;
            }).ToList();

            return Tuple.Create(info, state);
        }
    }
}