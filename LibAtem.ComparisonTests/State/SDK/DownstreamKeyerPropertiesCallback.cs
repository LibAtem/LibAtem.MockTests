using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.DownstreamKey;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class DownstreamKeyerPropertiesCallback : IBMDSwitcherDownstreamKeyCallback, INotify<_BMDSwitcherDownstreamKeyEventType>
    {
        private readonly DownstreamKeyerState _state;
        private readonly DownstreamKeyId _id;
        private readonly IBMDSwitcherDownstreamKey _props;
        private readonly Action<CommandQueueKey> _onChange;

        public DownstreamKeyerPropertiesCallback(DownstreamKeyerState state, DownstreamKeyId id, IBMDSwitcherDownstreamKey props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _id = id;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherDownstreamKeyEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeInputCutChanged:
                    _props.GetInputCut(out long cutInput);
                    _state.Sources.CutSource = (VideoSource)cutInput;
                    _onChange(new CommandQueueKey(new DownstreamKeySourceGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeInputFillChanged:
                    _props.GetInputFill(out long input);
                    _state.Sources.FillSource = (VideoSource)input;
                    _onChange(new CommandQueueKey(new DownstreamKeySourceGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeTieChanged:
                    _props.GetTie(out int tie);
                    _state.Properties.Tie = tie != 0;
                    _onChange(new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeRateChanged:
                    _props.GetRate(out uint frames);
                    _state.Properties.Rate = frames;
                    _onChange(new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeOnAirChanged:
                    _props.GetOnAir(out int onAir);
                    _state.State.OnAir = onAir != 0;
                    _onChange(new CommandQueueKey(new DownstreamKeyStateGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeIsTransitioningChanged:
                    _props.IsTransitioning(out int isTransitioning);
                    _state.State.InTransition = isTransitioning != 0;
                    _onChange(new CommandQueueKey(new DownstreamKeyStateGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeIsAutoTransitioningChanged:
                    _props.IsAutoTransitioning(out int isAuto);
                    _state.State.IsAuto = isAuto != 0;
                    _onChange(new CommandQueueKey(new DownstreamKeyStateGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeFramesRemainingChanged:
                    _props.GetFramesRemaining(out uint framesRemaining);
                    _state.State.RemainingFrames = framesRemaining;
                    _onChange(new CommandQueueKey(new DownstreamKeyStateGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypePreMultipliedChanged:
                    _props.GetPreMultiplied(out int preMultiplied);
                    _state.Properties.PreMultipliedKey = preMultiplied != 0;
                    _onChange(new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeClipChanged:
                    _props.GetClip(out double clip);
                    _state.Properties.Clip = clip * 100;
                    _onChange(new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeGainChanged:
                    _props.GetGain(out double gain);
                    _state.Properties.Gain = gain * 100;
                    _onChange(new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeInverseChanged:
                    _props.GetInverse(out int inverse);
                    _state.Properties.Invert = inverse != 0;
                    _onChange(new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeMaskedChanged:
                    _props.GetMasked(out int masked);
                    _state.Properties.MaskEnabled = masked != 0;
                    _onChange(new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeMaskTopChanged:
                    _props.GetMaskTop(out double top);
                    _state.Properties.MaskTop = top;
                    _onChange(new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeMaskBottomChanged:
                    _props.GetMaskBottom(out double bottom);
                    _state.Properties.MaskBottom = bottom;
                    _onChange(new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeMaskLeftChanged:
                    _props.GetMaskLeft(out double left);
                    _state.Properties.MaskLeft = left;
                    _onChange(new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeMaskRightChanged:
                    _props.GetMaskRight(out double right);
                    _state.Properties.MaskRight = right;
                    _onChange(new CommandQueueKey(new DownstreamKeyPropertiesGetCommand() { Index = _id }));
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeIsTransitionTowardsOnAirChanged:
                    _props.IsTransitionTowardsOnAir(out int isTowardsAir);
                    _state.State.IsTowardsOnAir = isTowardsAir != 0;
                    _onChange(new CommandQueueKey(new DownstreamKeyStateGetV8Command() { Index = _id }));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}