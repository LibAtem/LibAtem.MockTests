using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MixEffectTransitionPropertiesCallback : SdkCallbackBaseNotify<IBMDSwitcherTransitionParameters, _BMDSwitcherTransitionParametersEventType>, IBMDSwitcherTransitionParametersCallback
    {
        private readonly MixEffectState.TransitionState _state;

        public MixEffectTransitionPropertiesCallback(MixEffectState.TransitionState state, IBMDSwitcherTransitionParameters props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();

            if (props is IBMDSwitcherTransitionMixParameters mix)
            {
                _state.Mix = new MixEffectState.TransitionMixState();
                Children.Add(new MixEffectTransitionMixCallback(_state.Mix, mix, AppendChange("Mix")));
            }

            if (props is IBMDSwitcherTransitionDipParameters dip)
            {
                _state.Dip = new MixEffectState.TransitionDipState();
                Children.Add(new MixEffectTransitionDipCallback(_state.Dip, dip, AppendChange("Dip")));
            }

            if (props is IBMDSwitcherTransitionWipeParameters wipe)
            {
                _state.Wipe = new MixEffectState.TransitionWipeState();
                Children.Add(new MixEffectTransitionWipeCallback(_state.Wipe, wipe, AppendChange("Wipe")));
            }

            if (props is IBMDSwitcherTransitionStingerParameters stinger)
            {
                _state.Stinger = new MixEffectState.TransitionStingerState();
                Children.Add(new MixEffectTransitionStingerCallback(_state.Stinger, stinger,  AppendChange("Stinger")));
            }

            if (props is IBMDSwitcherTransitionDVEParameters dve)
            {
                _state.DVE = new MixEffectState.TransitionDVEState();
                Children.Add(new MixEffectTransitionDVECallback(_state.DVE, dve, AppendChange("DVE")));
            }
        }

        public override void Notify(_BMDSwitcherTransitionParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeTransitionStyleChanged:
                    Props.GetTransitionStyle(out _BMDSwitcherTransitionStyle style);
                    _state.Properties.Style = AtemEnumMaps.TransitionStyleMap.FindByValue(style);
                    break;
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeNextTransitionStyleChanged:
                    Props.GetNextTransitionStyle(out _BMDSwitcherTransitionStyle nextStyle);
                    _state.Properties.NextStyle = AtemEnumMaps.TransitionStyleMap.FindByValue(nextStyle);
                    break;
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeTransitionSelectionChanged:
                    Props.GetTransitionSelection(out _BMDSwitcherTransitionSelection selection);
                    _state.Properties.Selection = (TransitionLayer)selection;
                    break;
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeNextTransitionSelectionChanged:
                    // TODO - this doesnt appear to be fired, but putting the code below works
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
            Props.GetNextTransitionSelection(out _BMDSwitcherTransitionSelection nextSelection);
            _state.Properties.NextSelection = (TransitionLayer)nextSelection;

            OnChange("Properties");
        }
    }
    public sealed class MixEffectTransitionMixCallback : SdkCallbackBaseNotify<IBMDSwitcherTransitionMixParameters, _BMDSwitcherTransitionMixParametersEventType>, IBMDSwitcherTransitionMixParametersCallback
    {
        private readonly MixEffectState.TransitionMixState _state;

        public MixEffectTransitionMixCallback(MixEffectState.TransitionMixState state, IBMDSwitcherTransitionMixParameters props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherTransitionMixParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionMixParametersEventType.bmdSwitcherTransitionMixParametersEventTypeRateChanged:
                    Props.GetRate(out uint rate);
                    _state.Rate = rate;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }

    public sealed class MixEffectTransitionDipCallback : SdkCallbackBaseNotify<IBMDSwitcherTransitionDipParameters, _BMDSwitcherTransitionDipParametersEventType>, IBMDSwitcherTransitionDipParametersCallback
    {
        private readonly MixEffectState.TransitionDipState _state;

        public MixEffectTransitionDipCallback(MixEffectState.TransitionDipState state, IBMDSwitcherTransitionDipParameters props, Action<string> onChange) : base (props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherTransitionDipParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionDipParametersEventType.bmdSwitcherTransitionDipParametersEventTypeRateChanged:
                    Props.GetRate(out uint rate);
                    _state.Rate = rate;
                    break;
                case _BMDSwitcherTransitionDipParametersEventType.bmdSwitcherTransitionDipParametersEventTypeInputDipChanged:
                    Props.GetInputDip(out long input);
                    _state.Input = (VideoSource) input;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }

    public sealed class MixEffectTransitionWipeCallback : SdkCallbackBaseNotify<IBMDSwitcherTransitionWipeParameters, _BMDSwitcherTransitionWipeParametersEventType>, IBMDSwitcherTransitionWipeParametersCallback
    {
        private readonly MixEffectState.TransitionWipeState _state;

        public MixEffectTransitionWipeCallback(MixEffectState.TransitionWipeState state, IBMDSwitcherTransitionWipeParameters props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherTransitionWipeParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeRateChanged:
                    Props.GetRate(out uint rate);
                    _state.Rate = rate;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypePatternChanged:
                    Props.GetPattern(out _BMDSwitcherPatternStyle pattern);
                    _state.Pattern = AtemEnumMaps.PatternMap.FindByValue(pattern);
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeBorderSizeChanged:
                    Props.GetBorderSize(out double size);
                    _state.BorderWidth = size * 100;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeInputBorderChanged:
                    Props.GetInputBorder(out long input);
                    _state.BorderInput = (VideoSource) input;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeSymmetryChanged:
                    Props.GetSymmetry(out double symmetry);
                    _state.Symmetry = symmetry * 100;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeSoftnessChanged:
                    Props.GetSoftness(out double soft);
                    _state.BorderSoftness = soft * 100;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeHorizontalOffsetChanged:
                    Props.GetHorizontalOffset(out double xPos);
                    _state.XPosition = xPos;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeVerticalOffsetChanged:
                    Props.GetVerticalOffset(out double yPos);
                    _state.YPosition = yPos;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeReverseChanged:
                    Props.GetReverse(out int reverse);
                    _state.ReverseDirection = reverse != 0;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeFlipFlopChanged:
                    Props.GetFlipFlop(out int flipflop);
                    _state.FlipFlop = flipflop != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }

    public sealed class MixEffectTransitionStingerCallback : SdkCallbackBaseNotify<IBMDSwitcherTransitionStingerParameters, _BMDSwitcherTransitionStingerParametersEventType>, IBMDSwitcherTransitionStingerParametersCallback
    {
        private readonly MixEffectState.TransitionStingerState _state;

        public MixEffectTransitionStingerCallback(MixEffectState.TransitionStingerState state, IBMDSwitcherTransitionStingerParameters props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherTransitionStingerParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeSourceChanged:
                    Props.GetSource(out _BMDSwitcherStingerTransitionSource src);
                    _state.Source = AtemEnumMaps.StingerSourceMap.FindByValue(src);
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypePreMultipliedChanged:
                    Props.GetPreMultiplied(out int preMultiplied);
                    _state.PreMultipliedKey = preMultiplied != 0;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeClipChanged:
                    Props.GetClip(out double clip);
                    _state.Clip = clip * 100;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeGainChanged:
                    Props.GetGain(out double gain);
                    _state.Gain = gain * 100;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeInverseChanged:
                    Props.GetInverse(out int inverse);
                    _state.Invert = inverse != 0;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypePrerollChanged:
                    Props.GetPreroll(out uint preroll);
                    _state.Preroll = preroll;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeClipDurationChanged:
                    Props.GetClipDuration(out uint duration);
                    _state.ClipDuration = duration;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeTriggerPointChanged:
                    Props.GetTriggerPoint(out uint trigger);
                    _state.TriggerPoint = trigger;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeMixRateChanged:
                    Props.GetMixRate(out uint mixrate);
                    _state.MixRate = mixrate;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }

    public sealed class MixEffectTransitionDVECallback : SdkCallbackBaseNotify<IBMDSwitcherTransitionDVEParameters, _BMDSwitcherTransitionDVEParametersEventType>, IBMDSwitcherTransitionDVEParametersCallback
    {
        private readonly MixEffectState.TransitionDVEState _state;

        public MixEffectTransitionDVECallback(MixEffectState.TransitionDVEState state, IBMDSwitcherTransitionDVEParameters props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherTransitionDVEParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeRateChanged:
                    Props.GetRate(out uint rate);
                    _state.Rate = rate;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeLogoRateChanged:
                    Props.GetLogoRate(out uint logoRate);
                    _state.LogoRate = logoRate;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeReverseChanged:
                    Props.GetReverse(out int reverse);
                    _state.Reverse = reverse != 0;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeFlipFlopChanged:
                    Props.GetFlipFlop(out int flipflop);
                    _state.FlipFlop = flipflop != 0;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeStyleChanged:
                    Props.GetStyle(out _BMDSwitcherDVETransitionStyle style);
                    _state.Style = AtemEnumMaps.DVEStyleMap.FindByValue(style);
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeInputFillChanged:
                    Props.GetInputFill(out long input);
                    _state.FillSource = (VideoSource)input;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeInputCutChanged:
                    Props.GetInputCut(out long inputCut);
                    _state.KeySource = (VideoSource)inputCut;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeEnableKeyChanged:
                    Props.GetEnableKey(out int enable);
                    _state.EnableKey = enable != 0;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypePreMultipliedChanged:
                    Props.GetPreMultiplied(out int preMultiplied);
                    _state.PreMultiplied = preMultiplied != 0;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeClipChanged:
                    Props.GetClip(out double clip);
                    _state.Clip = clip * 100;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeGainChanged:
                    Props.GetGain(out double gain);
                    _state.Gain = gain * 100;
                    break;
                case _BMDSwitcherTransitionDVEParametersEventType.bmdSwitcherTransitionDVEParametersEventTypeInverseChanged:
                    Props.GetInverse(out int inverse);
                    _state.InvertKey = inverse != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }
}