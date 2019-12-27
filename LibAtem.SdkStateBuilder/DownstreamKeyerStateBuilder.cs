using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using System.Collections.Generic;

namespace LibAtem.SdkStateBuilder
{
    public static class DownstreamKeyerStateBuilder
    {
        public static IReadOnlyList<DownstreamKeyerState> Build(IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherDownstreamKeyIterator>(switcher.CreateIterator);
            return AtemSDKConverter.IterateList<IBMDSwitcherDownstreamKey, DownstreamKeyerState>(
                iterator.Next,
                (key, id) => BuildOne(key));
        }

        private static DownstreamKeyerState BuildOne(IBMDSwitcherDownstreamKey props)
        {
            var state = new DownstreamKeyerState();

            props.GetInputCut(out long cutInput);
            state.Sources.CutSource = (VideoSource)cutInput;
            props.GetInputFill(out long input);
            state.Sources.FillSource = (VideoSource)input;

            props.GetTie(out int tie);
            state.Properties.Tie = tie != 0;
            props.GetRate(out uint frames);
            state.Properties.Rate = frames;
            props.GetPreMultiplied(out int preMultiplied);
            state.Properties.PreMultipliedKey = preMultiplied != 0;
            props.GetClip(out double clip);
            state.Properties.Clip = clip * 100;
            props.GetGain(out double gain);
            state.Properties.Gain = gain * 100;
            props.GetInverse(out int inverse);
            state.Properties.Invert = inverse != 0;
            props.GetMasked(out int masked);
            state.Properties.MaskEnabled = masked != 0;
            props.GetMaskTop(out double top);
            state.Properties.MaskTop = top;
            props.GetMaskBottom(out double bottom);
            state.Properties.MaskBottom = bottom;
            props.GetMaskLeft(out double left);
            state.Properties.MaskLeft = left;
            props.GetMaskRight(out double right);
            state.Properties.MaskRight = right;

            props.GetOnAir(out int onAir);
            state.State.OnAir = onAir != 0;
            props.IsTransitioning(out int isTransitioning);
            state.State.InTransition = isTransitioning != 0;
            props.IsAutoTransitioning(out int isAuto);
            state.State.IsAuto = isAuto != 0;
            props.GetFramesRemaining(out uint framesRemaining);
            state.State.RemainingFrames = framesRemaining;
            props.IsTransitionTowardsOnAir(out int isTowardsAir);
            state.State.IsTowardsOnAir = isTowardsAir != 0;

            return state;
        }
    }
}