using System;
using BMDSwitcherAPI;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class ColorCallback : IBMDSwitcherInputColorCallback, INotify<_BMDSwitcherInputColorEventType>
    {
        private readonly ComparisonColorState _state;
        private readonly IBMDSwitcherInputColor _color;

        public ColorCallback(ComparisonColorState state, IBMDSwitcherInputColor color)
        {
            _state = state;
            _color = color;
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
        }
    }
}