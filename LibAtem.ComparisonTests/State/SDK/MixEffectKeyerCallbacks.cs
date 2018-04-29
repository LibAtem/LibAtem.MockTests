using System;
using BMDSwitcherAPI;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MixEffectKeyerChromaCallback : IBMDSwitcherKeyChromaParametersCallback, INotify<_BMDSwitcherKeyChromaParametersEventType>
    {
        private readonly ComparisonMixEffectKeyerChromaState _state;
        private readonly IBMDSwitcherKeyChromaParameters _props;

        public MixEffectKeyerChromaCallback(ComparisonMixEffectKeyerChromaState state, IBMDSwitcherKeyChromaParameters props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherKeyChromaParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeHueChanged:
                    _props.GetHue(out double hue);
                    _state.Hue = hue;
                    break;
                case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeGainChanged:
                    _props.GetGain(out double gain);
                    _state.Gain = gain * 100;
                    break;
                case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeYSuppressChanged:
                    _props.GetYSuppress(out double ySuppress);
                    _state.YSuppress = ySuppress * 100;
                    break;
                case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeLiftChanged:
                    _props.GetLift(out double lift);
                    _state.Lift = lift * 100;
                    break;
                case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeNarrowChanged:
                    _props.GetNarrow(out int narrow);
                    _state.Narrow = narrow != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }

    public sealed class MixEffectKeyerDVECallback : IBMDSwitcherKeyDVEParametersCallback, INotify<_BMDSwitcherKeyDVEParametersEventType>
    {
        private readonly ComparisonMixEffectKeyerDVEState _state;
        private readonly IBMDSwitcherKeyDVEParameters _props;

        public MixEffectKeyerDVECallback(ComparisonMixEffectKeyerDVEState state, IBMDSwitcherKeyDVEParameters props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherKeyDVEParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeShadowChanged:
                    _props.GetShadow(out int shadow);
                    _state.BorderShadow = shadow != 0;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeLightSourceDirectionChanged:
                    _props.GetLightSourceDirection(out double deg);
                    _state.LightSourceDirection = deg;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeLightSourceAltitudeChanged:
                    _props.GetLightSourceAltitude(out double alt);
                    _state.LightSourceAltitude = (uint)(alt * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderEnabledChanged:
                    _props.GetBorderEnabled(out int on);
                    _state.BorderEnabled = on != 0;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderBevelChanged:
                    _props.GetBorderBevel(out _BMDSwitcherBorderBevelOption bevel);
                    _state.BorderBevel = AtemEnumMaps.BorderBevelMap.FindByValue(bevel);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderWidthInChanged:
                    _props.GetBorderWidthIn(out double widthIn);
                    _state.InnerWidth = widthIn;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderWidthOutChanged:
                    _props.GetBorderWidthOut(out double widthOut);
                    _state.OuterWidth = widthOut;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderSoftnessInChanged:
                    _props.GetBorderSoftnessIn(out double softIn);
                    _state.InnerSoftness = (uint)(softIn * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderSoftnessOutChanged:
                    _props.GetBorderSoftnessOut(out double softOut);
                    _state.OuterSoftness = (uint)(softOut * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderBevelSoftnessChanged:
                    _props.GetBorderBevelSoftness(out double bevelSoft);
                    _state.BevelSoftness = (uint)(bevelSoft * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderBevelPositionChanged:
                    _props.GetBorderBevelPosition(out double bevelPosition);
                    _state.BevelPosition = (uint)(bevelPosition * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderOpacityChanged:
                    _props.GetBorderOpacity(out double opacity);
                    _state.BorderOpacity = (uint)(opacity * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderHueChanged:
                    _props.GetBorderHue(out double hue);
                    _state.BorderHue = hue;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderSaturationChanged:
                    _props.GetBorderSaturation(out double sat);
                    _state.BorderSaturation = sat * 100;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderLumaChanged:
                    _props.GetBorderLuma(out double luma);
                    _state.BorderLuma = luma * 100;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskedChanged:
                    _props.GetMasked(out int enabled);
                    _state.MaskEnabled = enabled != 0;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskTopChanged:
                    _props.GetMaskTop(out double top);
                    _state.MaskTop = top;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskBottomChanged:
                    _props.GetMaskBottom(out double bottom);
                    _state.MaskBottom = bottom;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskLeftChanged:
                    _props.GetMaskLeft(out double left);
                    _state.MaskLeft = left;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskRightChanged:
                    _props.GetMaskRight(out double right);
                    _state.MaskRight = right;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }

    public sealed class MixEffectKeyerFlyCallback : IBMDSwitcherKeyFlyParametersCallback, INotify<_BMDSwitcherKeyFlyParametersEventType>
    {
        private readonly ComparisonMixEffectKeyerFlyState _state;
        private readonly IBMDSwitcherKeyFlyParameters _props;

        public MixEffectKeyerFlyCallback(ComparisonMixEffectKeyerFlyState state, IBMDSwitcherKeyFlyParameters props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherKeyFlyParametersEventType eventType)
        {
            Notify(eventType, _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameFull);
        }

        public void Notify(_BMDSwitcherKeyFlyParametersEventType eventType, _BMDSwitcherFlyKeyFrame keyFrame)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeFlyChanged:
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeCanFlyChanged:
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeRateChanged:
                    _props.GetRate(out uint rate);
                    _state.Rate = rate;
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeSizeXChanged:
                    _props.GetSizeX(out double sizeX);
                    _state.SizeX = sizeX;
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeSizeYChanged:
                    _props.GetSizeY(out double sizeY);
                    _state.SizeY = sizeY;
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypePositionXChanged:
                    _props.GetPositionX(out double positionX);
                    _state.PositionX = positionX;
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypePositionYChanged:
                    _props.GetPositionY(out double positionY);
                    _state.PositionY = positionY;
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeRotationChanged:
                    _props.GetRotation(out double rotation);
                    _state.Rotation = rotation;
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsKeyFrameStoredChanged:
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsAtKeyFramesChanged:
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsRunningChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}