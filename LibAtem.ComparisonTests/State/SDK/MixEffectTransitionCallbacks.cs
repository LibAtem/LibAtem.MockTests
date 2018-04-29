using System;
using BMDSwitcherAPI;
using LibAtem.Common;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MixEffectTransitionDipCallback : IBMDSwitcherTransitionDipParametersCallback, INotify<_BMDSwitcherTransitionDipParametersEventType>
    {
        private readonly ComparisonMixEffectTransitionDipState _state;
        private readonly IBMDSwitcherTransitionDipParameters _props;

        public MixEffectTransitionDipCallback(ComparisonMixEffectTransitionDipState state, IBMDSwitcherTransitionDipParameters props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherTransitionDipParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionDipParametersEventType.bmdSwitcherTransitionDipParametersEventTypeRateChanged:
                    _props.GetRate(out uint rate);
                    _state.Rate = rate;
                    break;
                case _BMDSwitcherTransitionDipParametersEventType.bmdSwitcherTransitionDipParametersEventTypeInputDipChanged:
                    _props.GetInputDip(out long input);
                    _state.Input = (VideoSource)input;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }

    public sealed class MixEffectTransitionDVECallback : IBMDSwitcherTransitionDVEParametersCallback, INotify<_BMDSwitcherTransitionDVEParametersEventType>
    {
        private readonly ComparisonMixEffectTransitionDVEState _state;
        private readonly IBMDSwitcherTransitionDVEParameters _props;

        public MixEffectTransitionDVECallback(ComparisonMixEffectTransitionDVEState state, IBMDSwitcherTransitionDVEParameters props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherTransitionDVEParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeRateChanged:
                    _props.GetRate(out uint rate);
                    _state.Rate = rate;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeLogoRateChanged:
                    _props.GetLogoRate(out uint logoRate);
                    _state.LogoRate = logoRate;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeReverseChanged:
                    _props.GetReverse(out int reverse);
                    _state.Reverse = reverse != 0;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeFlipFlopChanged:
                    _props.GetFlipFlop(out int flipflop);
                    _state.FlipFlop = flipflop != 0;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeStyleChanged:
                    _props.GetStyle(out _BMDSwitcherDVETransitionStyle style);
                    _state.Style = AtemEnumMaps.DVEStyleMap.FindByValue(style);
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeInputFillChanged:
                    _props.GetInputFill(out long input);
                    _state.FillSource = (VideoSource)input;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeInputCutChanged:
                    _props.GetInputCut(out long inputCut);
                    _state.KeySource = (VideoSource)inputCut;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeEnableKeyChanged:
                    _props.GetEnableKey(out int enable);
                    _state.EnableKey = enable != 0;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypePreMultipliedChanged:
                    _props.GetPreMultiplied(out int preMultiplied);
                    _state.PreMultiplied = preMultiplied != 0;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeClipChanged:
                    _props.GetClip(out double clip);
                    _state.Clip = clip * 100;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeGainChanged:
                    _props.GetGain(out double gain);
                    _state.Gain = gain * 100;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeInverseChanged:
                    _props.GetInverse(out int inverse);
                    _state.InvertKey = inverse != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}