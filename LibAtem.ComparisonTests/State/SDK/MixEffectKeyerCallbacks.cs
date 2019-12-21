using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MixEffectKeyerCallback : IBMDSwitcherKeyCallback, INotify<_BMDSwitcherKeyEventType>
    {
        private readonly MixEffectState.KeyerState _state;
        private readonly IBMDSwitcherKey _props;
        private readonly Action<string> _onChange;

        public MixEffectKeyerCallback(MixEffectState.KeyerState state, IBMDSwitcherKey props, Action<string> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherKeyEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeTypeChanged:
                    _props.GetType(out _BMDSwitcherKeyType type);
                    _state.Properties.Mode = AtemEnumMaps.MixEffectKeyTypeMap.FindByValue(type);
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeInputCutChanged:
                    _props.GetInputCut(out long inputCut);
                    _state.Properties.CutSource = (VideoSource)inputCut;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeInputFillChanged:
                    _props.GetInputFill(out long input);
                    _state.Properties.FillSource = (VideoSource)input;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeOnAirChanged:
                    _props.GetOnAir(out int onAir);
                    _state.OnAir = onAir != 0;
                    _onChange("OnAir");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskedChanged:
                    _props.GetMasked(out int masked);
                    _state.Properties.MaskEnabled = masked != 0;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskTopChanged:
                    _props.GetMaskTop(out double top);
                    _state.Properties.MaskTop = top;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskBottomChanged:
                    _props.GetMaskBottom(out double bottom);
                    _state.Properties.MaskBottom = bottom;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskLeftChanged:
                    _props.GetMaskLeft(out double left);
                    _state.Properties.MaskLeft = left;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskRightChanged:
                    _props.GetMaskRight(out double right);
                    _state.Properties.MaskRight = right;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeCanBeDVEKeyChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }

    public sealed class MixEffectKeyerLumaCallback : IBMDSwitcherKeyLumaParametersCallback, INotify<_BMDSwitcherKeyLumaParametersEventType>
    {
        private readonly MixEffectState.KeyerLumaState _state;
        private readonly IBMDSwitcherKeyLumaParameters _props;
        private readonly Action _onChange;

        public MixEffectKeyerLumaCallback(MixEffectState.KeyerLumaState state, IBMDSwitcherKeyLumaParameters props, Action onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherKeyLumaParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyLumaParametersEventType.bmdSwitcherKeyLumaParametersEventTypePreMultipliedChanged:
                    _props.GetPreMultiplied(out int preMultiplied);
                    _state.PreMultiplied = preMultiplied != 0;
                    break;
                case _BMDSwitcherKeyLumaParametersEventType.bmdSwitcherKeyLumaParametersEventTypeClipChanged:
                    _props.GetClip(out double clip);
                    _state.Clip = clip * 100;
                    break;
                case _BMDSwitcherKeyLumaParametersEventType.bmdSwitcherKeyLumaParametersEventTypeGainChanged:
                    _props.GetGain(out double gain);
                    _state.Gain = gain * 100;
                    break;
                case _BMDSwitcherKeyLumaParametersEventType.bmdSwitcherKeyLumaParametersEventTypeInverseChanged:
                    _props.GetInverse(out int inverse);
                    _state.Invert = inverse != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange();
        }
    }

    public sealed class MixEffectKeyerChromaCallback : IBMDSwitcherKeyChromaParametersCallback, INotify<_BMDSwitcherKeyChromaParametersEventType>
    {
        private readonly MixEffectState.KeyerChromaState _state;
        private readonly IBMDSwitcherKeyChromaParameters _props;
        private readonly Action _onChange;

        public MixEffectKeyerChromaCallback(MixEffectState.KeyerChromaState state, IBMDSwitcherKeyChromaParameters props, Action onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
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

            _onChange();
        }
    }

    public sealed class MixEffectKeyerAdvancedChromaCallback : IBMDSwitcherKeyAdvancedChromaParametersCallback, INotify<_BMDSwitcherKeyAdvancedChromaParametersEventType>
    {
        private readonly MixEffectState.KeyerAdvancedChromaState _state;
        private readonly IBMDSwitcherKeyAdvancedChromaParameters _props;
        private readonly Action<string> _onChange;

        public MixEffectKeyerAdvancedChromaCallback(MixEffectState.KeyerAdvancedChromaState state, IBMDSwitcherKeyAdvancedChromaParameters props, Action<string> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherKeyAdvancedChromaParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeForegroundLevelChanged:
                    _props.GetForegroundLevel(out double foreground);
                    _state.Properties.ForegroundLevel = foreground * 100;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeBackgroundLevelChanged:
                    _props.GetBackgroundLevel(out double background);
                    _state.Properties.BackgroundLevel = background * 100;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeKeyEdgeChanged:
                    _props.GetKeyEdge(out double keyEdge);
                    _state.Properties.KeyEdge = keyEdge * 100;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeSpillSuppressChanged:
                    _props.GetSpillSuppress(out double spillSuppress);
                    _state.Properties.SpillSuppression = spillSuppress * 100;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeFlareSuppressChanged:
                    _props.GetFlareSuppress(out double flareSuppress);
                    _state.Properties.FlareSuppression = flareSuppress * 100;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeBrightnessChanged:
                    _props.GetBrightness(out double brightness);
                    _state.Properties.Brightness = brightness * 100;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeContrastChanged:
                    _props.GetContrast(out double contrast);
                    _state.Properties.Contrast = contrast * 100;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeSaturationChanged:
                    _props.GetSaturation(out double saturation);
                    _state.Properties.Saturation = saturation * 100;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeRedChanged:
                    _props.GetRed(out double red);
                    _state.Properties.Red = red * 100;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeGreenChanged:
                    _props.GetGreen(out double green);
                    _state.Properties.Green = green * 100;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeBlueChanged:
                    _props.GetBlue(out double blue);
                    _state.Properties.Blue = blue * 100;
                    _onChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeSamplingModeEnabledChanged:
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypePreviewEnabledChanged:
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeCursorXPositionChanged:
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeCursorYPositionChanged:
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeCursorSizeChanged:
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeSampledColorChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }

    public sealed class MixEffectKeyerPatternCallback : IBMDSwitcherKeyPatternParametersCallback, INotify<_BMDSwitcherKeyPatternParametersEventType>
    {
        private readonly MixEffectState.KeyerPatternState _state;
        private readonly IBMDSwitcherKeyPatternParameters _props;
        private readonly Action _onChange;

        public MixEffectKeyerPatternCallback(MixEffectState.KeyerPatternState state, IBMDSwitcherKeyPatternParameters props, Action onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherKeyPatternParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypePatternChanged:
                    _props.GetPattern(out _BMDSwitcherPatternStyle pattern);
                    _state.Style = AtemEnumMaps.PatternMap.FindByValue(pattern);
                    break;
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypeSizeChanged:
                    _props.GetSize(out double size);
                    _state.Size = size * 100;
                    break;
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypeSymmetryChanged:
                    _props.GetSymmetry(out double symmetry);
                    _state.Symmetry = symmetry * 100;
                    break;
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypeSoftnessChanged:
                    _props.GetSoftness(out double softness);
                    _state.Softness = softness * 100;
                    break;
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypeHorizontalOffsetChanged:
                    _props.GetHorizontalOffset(out double xPos);
                    _state.XPosition = xPos;
                    break;
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypeVerticalOffsetChanged:
                    _props.GetVerticalOffset(out double yPos);
                    _state.YPosition = yPos;
                    break;
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypeInverseChanged:
                    _props.GetInverse(out int inverse);
                    _state.Inverse = inverse != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange();
        }
    }

    public sealed class MixEffectKeyerDVECallback : IBMDSwitcherKeyDVEParametersCallback, INotify<_BMDSwitcherKeyDVEParametersEventType>
    {
        private readonly MixEffectState.KeyerDVEState _state;
        private readonly IBMDSwitcherKeyDVEParameters _props;
        private readonly Action _onChange;

        public MixEffectKeyerDVECallback(MixEffectState.KeyerDVEState state, IBMDSwitcherKeyDVEParameters props, Action onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherKeyDVEParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeShadowChanged:
                    _props.GetShadow(out int shadow);
                    _state.BorderShadowEnabled = shadow != 0;
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
                    _state.BorderInnerWidth = widthIn;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderWidthOutChanged:
                    _props.GetBorderWidthOut(out double widthOut);
                    _state.BorderOuterWidth = widthOut;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderSoftnessInChanged:
                    _props.GetBorderSoftnessIn(out double softIn);
                    _state.BorderInnerSoftness = (uint)(softIn * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderSoftnessOutChanged:
                    _props.GetBorderSoftnessOut(out double softOut);
                    _state.BorderOuterSoftness = (uint)(softOut * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderBevelSoftnessChanged:
                    _props.GetBorderBevelSoftness(out double bevelSoft);
                    _state.BorderBevelSoftness = (uint)(bevelSoft * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderBevelPositionChanged:
                    _props.GetBorderBevelPosition(out double bevelPosition);
                    _state.BorderBevelPosition = (uint)(bevelPosition * 100);
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

            _onChange();
        }
    }

    public sealed class MixEffectKeyerFlyCallback : IBMDSwitcherKeyFlyParametersCallback, INotify<_BMDSwitcherKeyFlyParametersEventType>
    {
        private readonly MixEffectState.KeyerDVEState _state;
        private readonly IBMDSwitcherKeyFlyParameters _props;
        private readonly Action _onChange;

        public MixEffectKeyerFlyCallback(MixEffectState.KeyerDVEState state, IBMDSwitcherKeyFlyParameters props, Action onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
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

            _onChange();
        }
    }

    public sealed class MixEffectKeyerFlyKeyFrameCallback : IBMDSwitcherKeyFlyKeyFrameParametersCallback, INotify<_BMDSwitcherKeyFlyKeyFrameParametersEventType>
    {
        private readonly MixEffectState.KeyerFlyFrameState _state;
        private readonly IBMDSwitcherKeyFlyKeyFrameParameters _props;
        private readonly Action _onChange;

        public MixEffectKeyerFlyKeyFrameCallback(MixEffectState.KeyerFlyFrameState state, IBMDSwitcherKeyFlyKeyFrameParameters props, Action onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherKeyFlyKeyFrameParametersEventType eventType)
        {
            // TODO - these have no eventType enum value...
            _props.GetBorderOpacity(out double opacity);
            _state.BorderOpacity = (uint)(opacity * 100);

            // TODO MaskEnabled?
            _props.GetMaskTop(out double maskTop);
            _state.MaskTop = maskTop;
            _props.GetMaskBottom(out double maskBottom);
            _state.MaskBottom = maskBottom;
            _props.GetMaskLeft(out double maskLeft);
            _state.MaskLeft = maskLeft;
            _props.GetMaskRight(out double maskRight);
            _state.MaskRight = maskRight;

            switch (eventType)
            {
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeSizeXChanged:
                    _props.GetSizeX(out double sizeX);
                    _state.SizeX = sizeX;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeSizeYChanged:
                    _props.GetSizeY(out double sizeY);
                    _state.SizeY = sizeY;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypePositionXChanged:
                    _props.GetPositionX(out double positionX);
                    _state.PositionX = positionX;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypePositionYChanged:
                    _props.GetPositionY(out double positionY);
                    _state.PositionY = positionY;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeRotationChanged:
                    _props.GetRotation(out double rotation);
                    _state.Rotation = rotation;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderWidthOutChanged:
                    _props.GetBorderWidthOut(out double widthOut);
                    _state.OuterWidth = widthOut;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderWidthInChanged:
                    _props.GetBorderWidthIn(out double widthIn);
                    _state.InnerWidth = widthIn;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderSoftnessOutChanged:
                    _props.GetBorderSoftnessOut(out double borderSoftnessOut);
                    _state.OuterSoftness = (uint)(borderSoftnessOut * 100);
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderSoftnessInChanged:
                    _props.GetBorderSoftnessIn(out double borderSoftnessIn);
                    _state.InnerSoftness = (uint)(borderSoftnessIn * 100);
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderBevelSoftnessChanged:
                    _props.GetBorderBevelSoftness(out double borderBevelSoftness);
                    _state.BevelSoftness = (uint)(borderBevelSoftness * 100);
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderBevelPositionChanged:
                    _props.GetBorderBevelPosition(out double borderBevelPosition);
                    _state.BevelPosition = (uint)(borderBevelPosition * 100);
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderHueChanged:
                    _props.GetBorderHue(out double hue);
                    _state.BorderHue = hue;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderSaturationChanged:
                    _props.GetBorderSaturation(out double sat);
                    _state.BorderSaturation = sat * 100;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderLumaChanged:
                    _props.GetBorderLuma(out double luma);
                    _state.BorderLuma = luma * 100;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderLightSourceDirectionChanged:
                    _props.GetBorderLightSourceDirection(out double deg);
                    _state.LightSourceDirection = deg;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderLightSourceAltitudeChanged:
                    _props.GetBorderLightSourceAltitude(out double alt);
                    _state.LightSourceAltitude = (uint)(alt * 100);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange();
        }
    }
}