using System;
using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class ColorCallback : SdkCallbackBaseNotify<IBMDSwitcherInputColor, _BMDSwitcherInputColorEventType>, IBMDSwitcherInputColorCallback
    {
        private readonly ColorState _state;

        public ColorCallback(ColorState state, IBMDSwitcherInputColor color, Action<string> onChange) : base(color, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherInputColorEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherInputColorEventType.bmdSwitcherInputColorEventTypeHueChanged:
                    Props.GetHue(out double hue);
                    _state.Hue = hue;
                    break;
                case _BMDSwitcherInputColorEventType.bmdSwitcherInputColorEventTypeSaturationChanged:
                    Props.GetSaturation(out double saturation);
                    _state.Saturation = saturation * 100;
                    break;
                case _BMDSwitcherInputColorEventType.bmdSwitcherInputColorEventTypeLumaChanged:
                    Props.GetLuma(out double luma);
                    _state.Luma = luma * 100;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }
}