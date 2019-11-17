using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class MixEffectTransitionPropertiesCallback : IBMDSwitcherTransitionParametersCallback, INotify<_BMDSwitcherTransitionParametersEventType>
    {
        private readonly ComparisonMixEffectTransitionState _state;
        private readonly MixEffectBlockId _id;
        private readonly IBMDSwitcherTransitionParameters _props;
        private readonly Action<CommandQueueKey> _onChange;

        public MixEffectTransitionPropertiesCallback(ComparisonMixEffectTransitionState state, MixEffectBlockId id, IBMDSwitcherTransitionParameters props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _id = id;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherTransitionParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeTransitionStyleChanged:
                    _props.GetTransitionStyle(out _BMDSwitcherTransitionStyle style);
                    _state.Style = AtemEnumMaps.TransitionStyleMap.FindByValue(style);
                    break;
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeNextTransitionStyleChanged:
                    _props.GetNextTransitionStyle(out _BMDSwitcherTransitionStyle nextStyle);
                    _state.NextStyle = AtemEnumMaps.TransitionStyleMap.FindByValue(nextStyle);
                    break;
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeTransitionSelectionChanged:
                    _props.GetTransitionSelection(out _BMDSwitcherTransitionSelection selection);
                    _state.Selection = (TransitionLayer)selection;
                    break;
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeNextTransitionSelectionChanged:
                    // TODO - this doesnt appear to be fired, but putting the code below works
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
            _props.GetNextTransitionSelection(out _BMDSwitcherTransitionSelection nextSelection);
            _state.NextSelection = (TransitionLayer)nextSelection;

            _onChange(new CommandQueueKey(new TransitionPropertiesGetCommand() { Index = _id }));
        }
    }
    public sealed class MixEffectTransitionMixCallback : IBMDSwitcherTransitionMixParametersCallback, INotify<_BMDSwitcherTransitionMixParametersEventType>
    {
        private readonly ComparisonMixEffectTransitionMixState _state;
        private readonly MixEffectBlockId _id;
        private readonly IBMDSwitcherTransitionMixParameters _props;
        private readonly Action<CommandQueueKey> _onChange;

        public MixEffectTransitionMixCallback(ComparisonMixEffectTransitionMixState state, MixEffectBlockId id, IBMDSwitcherTransitionMixParameters props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _id = id;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherTransitionMixParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionMixParametersEventType.bmdSwitcherTransitionMixParametersEventTypeRateChanged:
                    _props.GetRate(out uint rate);
                    _state.Rate = rate;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange(new CommandQueueKey(new TransitionMixGetCommand() { Index = _id }));
        }
    }

    public sealed class MixEffectTransitionDipCallback : IBMDSwitcherTransitionDipParametersCallback, INotify<_BMDSwitcherTransitionDipParametersEventType>
    {
        private readonly ComparisonMixEffectTransitionDipState _state;
        private readonly MixEffectBlockId _id;
        private readonly IBMDSwitcherTransitionDipParameters _props;
        private readonly Action<CommandQueueKey> _onChange;

        public MixEffectTransitionDipCallback(ComparisonMixEffectTransitionDipState state, MixEffectBlockId id, IBMDSwitcherTransitionDipParameters props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _id = id;
            _props = props;
            _onChange = onChange;
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
                    _state.Input = (VideoSource) input;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange(new CommandQueueKey(new TransitionDipGetCommand() { Index = _id }));
        }
    }

    public sealed class MixEffectTransitionWipeCallback : IBMDSwitcherTransitionWipeParametersCallback, INotify<_BMDSwitcherTransitionWipeParametersEventType>
    {
        private readonly ComparisonMixEffectTransitionWipeState _state;
        private readonly MixEffectBlockId _id;
        private readonly IBMDSwitcherTransitionWipeParameters _props;
        private readonly Action<CommandQueueKey> _onChange;

        public MixEffectTransitionWipeCallback(ComparisonMixEffectTransitionWipeState state, MixEffectBlockId id, IBMDSwitcherTransitionWipeParameters props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _id = id;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherTransitionWipeParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeRateChanged:
                    _props.GetRate(out uint rate);
                    _state.Rate = rate;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypePatternChanged:
                    _props.GetPattern(out _BMDSwitcherPatternStyle pattern);
                    _state.Pattern = AtemEnumMaps.PatternMap.FindByValue(pattern);
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeBorderSizeChanged:
                    _props.GetBorderSize(out double size);
                    _state.BorderWidth = size * 100;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeInputBorderChanged:
                    _props.GetInputBorder(out long input);
                    _state.BorderInput = (VideoSource) input;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeSymmetryChanged:
                    _props.GetSymmetry(out double symmetry);
                    _state.Symmetry = symmetry * 100;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeSoftnessChanged:
                    _props.GetSoftness(out double soft);
                    _state.BorderSoftness = soft * 100;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeHorizontalOffsetChanged:
                    _props.GetHorizontalOffset(out double xPos);
                    _state.XPosition = xPos;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeVerticalOffsetChanged:
                    _props.GetVerticalOffset(out double yPos);
                    _state.YPosition = yPos;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeReverseChanged:
                    _props.GetReverse(out int reverse);
                    _state.ReverseDirection = reverse != 0;
                    break;
                case _BMDSwitcherTransitionWipeParametersEventType.bmdSwitcherTransitionWipeParametersEventTypeFlipFlopChanged:
                    _props.GetFlipFlop(out int flipflop);
                    _state.FlipFlop = flipflop != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange(new CommandQueueKey(new TransitionWipeGetCommand() { Index = _id }));
        }
    }

    public sealed class MixEffectTransitionStingerCallback : IBMDSwitcherTransitionStingerParametersCallback, INotify<_BMDSwitcherTransitionStingerParametersEventType>
    {
        private readonly ComparisonMixEffectTransitionStingerState _state;
        private readonly MixEffectBlockId _id;
        private readonly IBMDSwitcherTransitionStingerParameters _props;
        private readonly Action<CommandQueueKey> _onChange;

        public MixEffectTransitionStingerCallback(ComparisonMixEffectTransitionStingerState state, MixEffectBlockId id, IBMDSwitcherTransitionStingerParameters props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _id = id;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherTransitionStingerParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeSourceChanged:
                    _props.GetSource(out _BMDSwitcherStingerTransitionSource src);
                    _state.Source = AtemEnumMaps.StingerSourceMap.FindByValue(src);
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypePreMultipliedChanged:
                    _props.GetPreMultiplied(out int preMultiplied);
                    _state.PreMultipliedKey = preMultiplied != 0;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeClipChanged:
                    _props.GetClip(out double clip);
                    _state.Clip = clip * 100;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeGainChanged:
                    _props.GetGain(out double gain);
                    _state.Gain = gain * 100;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeInverseChanged:
                    _props.GetInverse(out int inverse);
                    _state.Invert = inverse != 0;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypePrerollChanged:
                    _props.GetPreroll(out uint preroll);
                    _state.Preroll = preroll;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeClipDurationChanged:
                    _props.GetClipDuration(out uint duration);
                    _state.ClipDuration = duration;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeTriggerPointChanged:
                    _props.GetTriggerPoint(out uint trigger);
                    _state.TriggerPoint = trigger;
                    break;
                case _BMDSwitcherTransitionStingerParametersEventType.bmdSwitcherTransitionStingerParametersEventTypeMixRateChanged:
                    _props.GetMixRate(out uint mixrate);
                    _state.MixRate = mixrate;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange(new CommandQueueKey(new TransitionStingerGetCommand() { Index = _id }));
        }
    }

    public sealed class MixEffectTransitionDVECallback : IBMDSwitcherTransitionDVEParametersCallback, INotify<_BMDSwitcherTransitionDVEParametersEventType>
    {
        private readonly ComparisonMixEffectTransitionDVEState _state;
        private readonly MixEffectBlockId _id;
        private readonly IBMDSwitcherTransitionDVEParameters _props;
        private readonly Action<CommandQueueKey> _onChange;

        public MixEffectTransitionDVECallback(ComparisonMixEffectTransitionDVEState state, MixEffectBlockId id, IBMDSwitcherTransitionDVEParameters props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _id = id;
            _props = props;
            _onChange = onChange;
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

            _onChange(new CommandQueueKey(new TransitionDVEGetCommand() { Index = _id }));
        }
    }
}