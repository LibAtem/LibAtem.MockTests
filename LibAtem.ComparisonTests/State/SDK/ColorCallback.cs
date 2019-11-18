using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class ColorCallback : IBMDSwitcherInputColorCallback, INotify<_BMDSwitcherInputColorEventType>
    {
        private readonly ColorState _state;
        private readonly IBMDSwitcherInputColor _color;
        private readonly Action _onChange;

        public ColorCallback(ColorState state, IBMDSwitcherInputColor color, Action onChange)
        {
            _state = state;
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

            _onChange();
        }
    }
}