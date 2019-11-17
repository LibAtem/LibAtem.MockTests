using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class ColorCallback : IBMDSwitcherInputColorCallback, INotify<_BMDSwitcherInputColorEventType>
    {
        private readonly ColorState _state;
        private readonly ColorGeneratorId _id;
        private readonly IBMDSwitcherInputColor _color;
        private readonly Action<CommandQueueKey> _onChange;

        public ColorCallback(ColorState state, ColorGeneratorId id, IBMDSwitcherInputColor color, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _id = id;
            _color = color;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherInputColorEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherInputColorEventType.bmdSwitcherInputColorEventTypeHueChanged:
                    _color.GetHue(out double hue);
                    _state.Hue = hue;
                    break;
                case _BMDSwitcherInputColorEventType.bmdSwitcherInputColorEventTypeSaturationChanged:
                    _color.GetSaturation(out double saturation);
                    _state.Saturation = saturation * 100;
                    break;
                case _BMDSwitcherInputColorEventType.bmdSwitcherInputColorEventTypeLumaChanged:
                    _color.GetLuma(out double luma);
                    _state.Luma = luma * 100;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange(new CommandQueueKey(new ColorGeneratorGetCommand() { Index = _id }));
        }
    }
}