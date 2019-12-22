using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class DownstreamKeyerPropertiesCallback : SdkCallbackBaseNotify<IBMDSwitcherDownstreamKey, _BMDSwitcherDownstreamKeyEventType>, IBMDSwitcherDownstreamKeyCallback
    {
        private readonly DownstreamKeyerState _state;

        public DownstreamKeyerPropertiesCallback(DownstreamKeyerState state, IBMDSwitcherDownstreamKey props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherDownstreamKeyEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeInputCutChanged:
                    Props.GetInputCut(out long cutInput);
                    _state.Sources.CutSource = (VideoSource)cutInput;
                    OnChange("Sources");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeInputFillChanged:
                    Props.GetInputFill(out long input);
                    _state.Sources.FillSource = (VideoSource)input;
                    OnChange("Sources");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeTieChanged:
                    Props.GetTie(out int tie);
                    _state.Properties.Tie = tie != 0;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeRateChanged:
                    Props.GetRate(out uint frames);
                    _state.Properties.Rate = frames;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeOnAirChanged:
                    Props.GetOnAir(out int onAir);
                    _state.State.OnAir = onAir != 0;
                    OnChange("State");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeIsTransitioningChanged:
                    Props.IsTransitioning(out int isTransitioning);
                    _state.State.InTransition = isTransitioning != 0;
                    OnChange("State");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeIsAutoTransitioningChanged:
                    Props.IsAutoTransitioning(out int isAuto);
                    _state.State.IsAuto = isAuto != 0;
                    OnChange("State");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeFramesRemainingChanged:
                    Props.GetFramesRemaining(out uint framesRemaining);
                    _state.State.RemainingFrames = framesRemaining;
                    OnChange("State");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypePreMultipliedChanged:
                    Props.GetPreMultiplied(out int preMultiplied);
                    _state.Properties.PreMultipliedKey = preMultiplied != 0;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeClipChanged:
                    Props.GetClip(out double clip);
                    _state.Properties.Clip = clip * 100;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeGainChanged:
                    Props.GetGain(out double gain);
                    _state.Properties.Gain = gain * 100;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeInverseChanged:
                    Props.GetInverse(out int inverse);
                    _state.Properties.Invert = inverse != 0;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeMaskedChanged:
                    Props.GetMasked(out int masked);
                    _state.Properties.MaskEnabled = masked != 0;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeMaskTopChanged:
                    Props.GetMaskTop(out double top);
                    _state.Properties.MaskTop = top;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeMaskBottomChanged:
                    Props.GetMaskBottom(out double bottom);
                    _state.Properties.MaskBottom = bottom;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeMaskLeftChanged:
                    Props.GetMaskLeft(out double left);
                    _state.Properties.MaskLeft = left;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeMaskRightChanged:
                    Props.GetMaskRight(out double right);
                    _state.Properties.MaskRight = right;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeIsTransitionTowardsOnAirChanged:
                    Props.IsTransitionTowardsOnAir(out int isTowardsAir);
                    _state.State.IsTowardsOnAir = isTowardsAir != 0;
                    OnChange("State");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}