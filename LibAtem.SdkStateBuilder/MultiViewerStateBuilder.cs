using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using System.Collections.Generic;
using System.Linq;

namespace LibAtem.SdkStateBuilder
{
    public static class MultiViewerStateBuilder
    {
        public static IReadOnlyList<MultiViewerState> Build(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherMultiViewIterator>(switcher.CreateIterator);
            return AtemSDKConverter.IterateList<IBMDSwitcherMultiView, MultiViewerState>(iterator.Next,
                (mv, id) => BuildOne(mv));
        }

        private static MultiViewerState BuildOne(IBMDSwitcherMultiView props)
        {
            var state = new MultiViewerState();

            props.SupportsVuMeters(out int supportsVu);
            state.SupportsVuMeters = supportsVu != 0;
            props.SupportsProgramPreviewSwap(out int supportsSwap);
            state.SupportsProgramPreviewSwapped = supportsSwap != 0;
            props.SupportsQuadrantLayout(out int supportsQuadrants);
            state.SupportsQuadrantLayout = supportsQuadrants != 0;
            props.CanToggleSafeAreaEnabled(out int supportsToggleSafeArea);
            state.SupportsToggleSafeArea = supportsToggleSafeArea != 0;

            props.GetLayout(out _BMDSwitcherMultiViewLayout layout);
            state.Properties.Layout = (MultiViewLayoutV8)layout;
            props.GetProgramPreviewSwapped(out int swapped);
            state.Properties.ProgramPreviewSwapped = swapped != 0;
            
            if (state.SupportsVuMeters)
            {
                props.GetVuMeterOpacity(out double opacity);
                state.VuMeterOpacity = opacity * 100;
            }

            props.GetWindowCount(out uint count);
            state.Windows = Enumerable.Range(0, (int)count).Select(window =>
            {
                props.GetWindowInput((uint)window, out long input);
                props.GetSafeAreaEnabled((uint) window, out int enabled);
                //_props.CurrentInputSupportsSafeArea((uint) window, out int supportsSafeArea);

                var st = new MultiViewerState.WindowState
                {
                    Source = (VideoSource)input,
                    // SupportsSafeArea = supportsSafeArea != 0,
                    SafeAreaEnabled = enabled != 0,
                };

                if (state.SupportsVuMeters)
                {
                    props.CurrentInputSupportsVuMeter((uint)window, out int windowSupportsVu);
                    st.SupportsVuMeter = windowSupportsVu != 0;
                    props.GetVuMeterEnabled((uint)window, out int vuEnabled);
                    st.VuMeter = vuEnabled != 0;
                }

                return st;
            }).ToList();

            return state;
        }
    }
}