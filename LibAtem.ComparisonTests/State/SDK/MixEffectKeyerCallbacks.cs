using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class MixEffectKeyerCallback : SdkCallbackBaseNotify<IBMDSwitcherKey, _BMDSwitcherKeyEventType>, IBMDSwitcherKeyCallback
    {
        private readonly MixEffectState.KeyerState _state;

        public MixEffectKeyerCallback(MixEffectState.KeyerState state, IBMDSwitcherKey props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged(_BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeCanBeDVEKeyChanged);

            if (props is IBMDSwitcherKeyLumaParameters luma)
            {
                state.Luma = new MixEffectState.KeyerLumaState();
                Children.Add(new MixEffectKeyerLumaCallback(state.Luma, luma, AppendChange("Luma")));
            }

            if (props is IBMDSwitcherKeyChromaParameters chroma)
            {
                state.Chroma = new MixEffectState.KeyerChromaState();
                Children.Add(new MixEffectKeyerChromaCallback(state.Chroma, chroma, AppendChange("Chroma")));
            }

            if (props is IBMDSwitcherKeyAdvancedChromaParameters advancedChroma)
            {
                state.AdvancedChroma = new MixEffectState.KeyerAdvancedChromaState();
                Children.Add(new MixEffectKeyerAdvancedChromaCallback(state.AdvancedChroma, advancedChroma, AppendChange("AdvancedChroma")));
            }

            if (props is IBMDSwitcherKeyPatternParameters pattern)
            {
                state.Pattern = new MixEffectState.KeyerPatternState();
                Children.Add(new MixEffectKeyerPatternCallback(state.Pattern, pattern, AppendChange("Pattern")));
            }

            if (props is IBMDSwitcherKeyDVEParameters dve)
            {
                state.DVE = new MixEffectState.KeyerDVEState();
                Children.Add(new MixEffectKeyerDVECallback(state.DVE, dve, AppendChange("DVE")));
            }

            if (props is IBMDSwitcherKeyFlyParameters fly)
            {
                var ignore = new[]
                {
                    _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsAtKeyFramesChanged,
                    _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeCanFlyChanged,
                    _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeFlyChanged,
                    _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsKeyFrameStoredChanged,
                    _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsRunningChanged
                };
                Children.Add(new MixEffectKeyerFlyCallback(state.DVE, fly, AppendChange("DVE")));

                fly.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA, out IBMDSwitcherKeyFlyKeyFrameParameters keyframeA);
                fly.GetKeyFrameParameters(_BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB, out IBMDSwitcherKeyFlyKeyFrameParameters keyframeB);

                state.FlyFrames = new[]
                {
                    new MixEffectState.KeyerFlyFrameState(),
                    new MixEffectState.KeyerFlyFrameState()
                };

                Children.Add(new MixEffectKeyerFlyKeyFrameCallback(state.FlyFrames[0], keyframeA, AppendChange("FlyFrames.0")));
                Children.Add(new MixEffectKeyerFlyKeyFrameCallback(state.FlyFrames[1], keyframeB, AppendChange("FlyFrames.1")));
            }
        }

        public override void Notify(_BMDSwitcherKeyEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeTypeChanged:
                    Props.GetType(out _BMDSwitcherKeyType type);
                    _state.Properties.Mode = AtemEnumMaps.MixEffectKeyTypeMap.FindByValue(type);
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeInputCutChanged:
                    Props.GetInputCut(out long inputCut);
                    _state.Properties.CutSource = (VideoSource)inputCut;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeInputFillChanged:
                    Props.GetInputFill(out long input);
                    _state.Properties.FillSource = (VideoSource)input;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeOnAirChanged:
                    Props.GetOnAir(out int onAir);
                    _state.OnAir = onAir != 0;
                    OnChange("OnAir");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskedChanged:
                    Props.GetMasked(out int masked);
                    _state.Properties.MaskEnabled = masked != 0;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskTopChanged:
                    Props.GetMaskTop(out double top);
                    _state.Properties.MaskTop = top;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskBottomChanged:
                    Props.GetMaskBottom(out double bottom);
                    _state.Properties.MaskBottom = bottom;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskLeftChanged:
                    Props.GetMaskLeft(out double left);
                    _state.Properties.MaskLeft = left;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskRightChanged:
                    Props.GetMaskRight(out double right);
                    _state.Properties.MaskRight = right;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeCanBeDVEKeyChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }

    public sealed class MixEffectKeyerLumaCallback : SdkCallbackBaseNotify<IBMDSwitcherKeyLumaParameters, _BMDSwitcherKeyLumaParametersEventType>, IBMDSwitcherKeyLumaParametersCallback
    {
        private readonly MixEffectState.KeyerLumaState _state;

        public MixEffectKeyerLumaCallback(MixEffectState.KeyerLumaState state, IBMDSwitcherKeyLumaParameters props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherKeyLumaParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyLumaParametersEventType.bmdSwitcherKeyLumaParametersEventTypePreMultipliedChanged:
                    Props.GetPreMultiplied(out int preMultiplied);
                    _state.PreMultiplied = preMultiplied != 0;
                    break;
                case _BMDSwitcherKeyLumaParametersEventType.bmdSwitcherKeyLumaParametersEventTypeClipChanged:
                    Props.GetClip(out double clip);
                    _state.Clip = clip * 100;
                    break;
                case _BMDSwitcherKeyLumaParametersEventType.bmdSwitcherKeyLumaParametersEventTypeGainChanged:
                    Props.GetGain(out double gain);
                    _state.Gain = gain * 100;
                    break;
                case _BMDSwitcherKeyLumaParametersEventType.bmdSwitcherKeyLumaParametersEventTypeInverseChanged:
                    Props.GetInverse(out int inverse);
                    _state.Invert = inverse != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }

    public sealed class MixEffectKeyerChromaCallback : SdkCallbackBaseNotify<IBMDSwitcherKeyChromaParameters, _BMDSwitcherKeyChromaParametersEventType>, IBMDSwitcherKeyChromaParametersCallback
    {
        private readonly MixEffectState.KeyerChromaState _state;

        public MixEffectKeyerChromaCallback(MixEffectState.KeyerChromaState state, IBMDSwitcherKeyChromaParameters props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherKeyChromaParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeHueChanged:
                    Props.GetHue(out double hue);
                    _state.Hue = hue;
                    break;
                case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeGainChanged:
                    Props.GetGain(out double gain);
                    _state.Gain = gain * 100;
                    break;
                case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeYSuppressChanged:
                    Props.GetYSuppress(out double ySuppress);
                    _state.YSuppress = ySuppress * 100;
                    break;
                case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeLiftChanged:
                    Props.GetLift(out double lift);
                    _state.Lift = lift * 100;
                    break;
                case _BMDSwitcherKeyChromaParametersEventType.bmdSwitcherKeyChromaParametersEventTypeNarrowChanged:
                    Props.GetNarrow(out int narrow);
                    _state.Narrow = narrow != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }

    public sealed class MixEffectKeyerAdvancedChromaCallback : SdkCallbackBaseNotify<IBMDSwitcherKeyAdvancedChromaParameters, _BMDSwitcherKeyAdvancedChromaParametersEventType>, IBMDSwitcherKeyAdvancedChromaParametersCallback
    {
        private readonly MixEffectState.KeyerAdvancedChromaState _state;

        public MixEffectKeyerAdvancedChromaCallback(MixEffectState.KeyerAdvancedChromaState state, IBMDSwitcherKeyAdvancedChromaParameters props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherKeyAdvancedChromaParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeForegroundLevelChanged:
                    Props.GetForegroundLevel(out double foreground);
                    _state.Properties.ForegroundLevel = foreground * 100;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeBackgroundLevelChanged:
                    Props.GetBackgroundLevel(out double background);
                    _state.Properties.BackgroundLevel = background * 100;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeKeyEdgeChanged:
                    Props.GetKeyEdge(out double keyEdge);
                    _state.Properties.KeyEdge = keyEdge * 100;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeSpillSuppressChanged:
                    Props.GetSpillSuppress(out double spillSuppress);
                    _state.Properties.SpillSuppression = spillSuppress * 100;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeFlareSuppressChanged:
                    Props.GetFlareSuppress(out double flareSuppress);
                    _state.Properties.FlareSuppression = flareSuppress * 100;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeBrightnessChanged:
                    Props.GetBrightness(out double brightness);
                    _state.Properties.Brightness = brightness * 100;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeContrastChanged:
                    Props.GetContrast(out double contrast);
                    _state.Properties.Contrast = contrast * 100;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeSaturationChanged:
                    Props.GetSaturation(out double saturation);
                    _state.Properties.Saturation = saturation * 100;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeRedChanged:
                    Props.GetRed(out double red);
                    _state.Properties.Red = red * 100;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeGreenChanged:
                    Props.GetGreen(out double green);
                    _state.Properties.Green = green * 100;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeBlueChanged:
                    Props.GetBlue(out double blue);
                    _state.Properties.Blue = blue * 100;
                    OnChange("Properties");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeSamplingModeEnabledChanged:
                    Props.GetSamplingModeEnabled(out int sampleEnabled);
                    _state.Sample.EnableCursor = sampleEnabled != 0;
                    OnChange("Sample");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypePreviewEnabledChanged:
                    Props.GetPreviewEnabled(out int previewEnabled);
                    _state.Sample.Preview = previewEnabled != 0;
                    OnChange("Sample");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeCursorXPositionChanged:
                    Props.GetCursorXPosition(out double xPos);
                    _state.Sample.CursorX = xPos;
                    OnChange("Sample");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeCursorYPositionChanged:
                    Props.GetCursorYPosition(out double yPos);
                    _state.Sample.CursorY = yPos;
                    OnChange("Sample");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeCursorSizeChanged:
                    Props.GetCursorSize(out double size);
                    _state.Sample.CursorSize = size * 100;
                    OnChange("Sample");
                    break;
                case _BMDSwitcherKeyAdvancedChromaParametersEventType.bmdSwitcherKeyAdvancedChromaParametersEventTypeSampledColorChanged:
                    Props.GetSampledColor(out double y, out double cb, out double cr);
                    _state.Sample.SampledY = y;
                    _state.Sample.SampledCb = cb;
                    _state.Sample.SampledCr = cr;
                    OnChange("Sample");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }

    public sealed class MixEffectKeyerPatternCallback : SdkCallbackBaseNotify<IBMDSwitcherKeyPatternParameters, _BMDSwitcherKeyPatternParametersEventType>, IBMDSwitcherKeyPatternParametersCallback
    {
        private readonly MixEffectState.KeyerPatternState _state;

        public MixEffectKeyerPatternCallback(MixEffectState.KeyerPatternState state, IBMDSwitcherKeyPatternParameters props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherKeyPatternParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypePatternChanged:
                    Props.GetPattern(out _BMDSwitcherPatternStyle pattern);
                    _state.Pattern = AtemEnumMaps.PatternMap.FindByValue(pattern);
                    break;
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypeSizeChanged:
                    Props.GetSize(out double size);
                    _state.Size = size * 100;
                    break;
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypeSymmetryChanged:
                    Props.GetSymmetry(out double symmetry);
                    _state.Symmetry = symmetry * 100;
                    break;
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypeSoftnessChanged:
                    Props.GetSoftness(out double softness);
                    _state.Softness = softness * 100;
                    break;
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypeHorizontalOffsetChanged:
                    Props.GetHorizontalOffset(out double xPos);
                    _state.XPosition = xPos;
                    break;
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypeVerticalOffsetChanged:
                    Props.GetVerticalOffset(out double yPos);
                    _state.YPosition = yPos;
                    break;
                case _BMDSwitcherKeyPatternParametersEventType.bmdSwitcherKeyPatternParametersEventTypeInverseChanged:
                    Props.GetInverse(out int inverse);
                    _state.Inverse = inverse != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }

    public sealed class MixEffectKeyerDVECallback : SdkCallbackBaseNotify<IBMDSwitcherKeyDVEParameters, _BMDSwitcherKeyDVEParametersEventType>, IBMDSwitcherKeyDVEParametersCallback
    {
        private readonly MixEffectState.KeyerDVEState _state;

        public MixEffectKeyerDVECallback(MixEffectState.KeyerDVEState state, IBMDSwitcherKeyDVEParameters props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherKeyDVEParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeShadowChanged:
                    Props.GetShadow(out int shadow);
                    _state.BorderShadowEnabled = shadow != 0;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeLightSourceDirectionChanged:
                    Props.GetLightSourceDirection(out double deg);
                    _state.LightSourceDirection = deg;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeLightSourceAltitudeChanged:
                    Props.GetLightSourceAltitude(out double alt);
                    _state.LightSourceAltitude = (uint)(alt * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderEnabledChanged:
                    Props.GetBorderEnabled(out int on);
                    _state.BorderEnabled = on != 0;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderBevelChanged:
                    Props.GetBorderBevel(out _BMDSwitcherBorderBevelOption bevel);
                    _state.BorderBevel = AtemEnumMaps.BorderBevelMap.FindByValue(bevel);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderWidthInChanged:
                    Props.GetBorderWidthIn(out double widthIn);
                    _state.BorderInnerWidth = widthIn;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderWidthOutChanged:
                    Props.GetBorderWidthOut(out double widthOut);
                    _state.BorderOuterWidth = widthOut;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderSoftnessInChanged:
                    Props.GetBorderSoftnessIn(out double softIn);
                    _state.BorderInnerSoftness = (uint)(softIn * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderSoftnessOutChanged:
                    Props.GetBorderSoftnessOut(out double softOut);
                    _state.BorderOuterSoftness = (uint)(softOut * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderBevelSoftnessChanged:
                    Props.GetBorderBevelSoftness(out double bevelSoft);
                    _state.BorderBevelSoftness = (uint)(bevelSoft * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderBevelPositionChanged:
                    Props.GetBorderBevelPosition(out double bevelPosition);
                    _state.BorderBevelPosition = (uint)(bevelPosition * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderOpacityChanged:
                    Props.GetBorderOpacity(out double opacity);
                    _state.BorderOpacity = (uint)(opacity * 100);
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderHueChanged:
                    Props.GetBorderHue(out double hue);
                    _state.BorderHue = hue;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderSaturationChanged:
                    Props.GetBorderSaturation(out double sat);
                    _state.BorderSaturation = sat * 100;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeBorderLumaChanged:
                    Props.GetBorderLuma(out double luma);
                    _state.BorderLuma = luma * 100;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskedChanged:
                    Props.GetMasked(out int enabled);
                    _state.MaskEnabled = enabled != 0;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskTopChanged:
                    Props.GetMaskTop(out double top);
                    _state.MaskTop = top;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskBottomChanged:
                    Props.GetMaskBottom(out double bottom);
                    _state.MaskBottom = bottom;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskLeftChanged:
                    Props.GetMaskLeft(out double left);
                    _state.MaskLeft = left;
                    break;
                case _BMDSwitcherKeyDVEParametersEventType.bmdSwitcherKeyDVEParametersEventTypeMaskRightChanged:
                    Props.GetMaskRight(out double right);
                    _state.MaskRight = right;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }

    public sealed class MixEffectKeyerFlyCallback : SdkCallbackBaseNotify<IBMDSwitcherKeyFlyParameters, _BMDSwitcherKeyFlyParametersEventType>, IBMDSwitcherKeyFlyParametersCallback
    {
        private readonly MixEffectState.KeyerDVEState _state;

        public MixEffectKeyerFlyCallback(MixEffectState.KeyerDVEState state, IBMDSwitcherKeyFlyParameters props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;

            TriggerAllChanged(
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsAtKeyFramesChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeCanFlyChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeFlyChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsKeyFrameStoredChanged,
                _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsRunningChanged);
        }

        public override void Notify(_BMDSwitcherKeyFlyParametersEventType eventType)
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
                    Props.GetRate(out uint rate);
                    _state.Rate = rate;
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeSizeXChanged:
                    Props.GetSizeX(out double sizeX);
                    _state.SizeX = sizeX;
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeSizeYChanged:
                    Props.GetSizeY(out double sizeY);
                    _state.SizeY = sizeY;
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypePositionXChanged:
                    Props.GetPositionX(out double positionX);
                    _state.PositionX = positionX;
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypePositionYChanged:
                    Props.GetPositionY(out double positionY);
                    _state.PositionY = positionY;
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeRotationChanged:
                    Props.GetRotation(out double rotation);
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

            OnChange(null);
        }
    }

    public sealed class MixEffectKeyerFlyKeyFrameCallback : SdkCallbackBaseNotify<IBMDSwitcherKeyFlyKeyFrameParameters, _BMDSwitcherKeyFlyKeyFrameParametersEventType>, IBMDSwitcherKeyFlyKeyFrameParametersCallback
    {
        private readonly MixEffectState.KeyerFlyFrameState _state;

        public MixEffectKeyerFlyKeyFrameCallback(MixEffectState.KeyerFlyFrameState state, IBMDSwitcherKeyFlyKeyFrameParameters props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherKeyFlyKeyFrameParametersEventType eventType)
        {
            // TODO - these have no eventType enum value...
            Props.GetBorderOpacity(out double opacity);
            _state.BorderOpacity = (uint)(opacity * 100);

            // TODO MaskEnabled?
            Props.GetMaskTop(out double maskTop);
            _state.MaskTop = maskTop;
            Props.GetMaskBottom(out double maskBottom);
            _state.MaskBottom = maskBottom;
            Props.GetMaskLeft(out double maskLeft);
            _state.MaskLeft = maskLeft;
            Props.GetMaskRight(out double maskRight);
            _state.MaskRight = maskRight;

            switch (eventType)
            {
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeSizeXChanged:
                    Props.GetSizeX(out double sizeX);
                    _state.SizeX = sizeX;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeSizeYChanged:
                    Props.GetSizeY(out double sizeY);
                    _state.SizeY = sizeY;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypePositionXChanged:
                    Props.GetPositionX(out double positionX);
                    _state.PositionX = positionX;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypePositionYChanged:
                    Props.GetPositionY(out double positionY);
                    _state.PositionY = positionY;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeRotationChanged:
                    Props.GetRotation(out double rotation);
                    _state.Rotation = rotation;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderWidthOutChanged:
                    Props.GetBorderWidthOut(out double widthOut);
                    _state.OuterWidth = widthOut;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderWidthInChanged:
                    Props.GetBorderWidthIn(out double widthIn);
                    _state.InnerWidth = widthIn;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderSoftnessOutChanged:
                    Props.GetBorderSoftnessOut(out double borderSoftnessOut);
                    _state.OuterSoftness = (uint)(borderSoftnessOut * 100);
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderSoftnessInChanged:
                    Props.GetBorderSoftnessIn(out double borderSoftnessIn);
                    _state.InnerSoftness = (uint)(borderSoftnessIn * 100);
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderBevelSoftnessChanged:
                    Props.GetBorderBevelSoftness(out double borderBevelSoftness);
                    _state.BevelSoftness = (uint)(borderBevelSoftness * 100);
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderBevelPositionChanged:
                    Props.GetBorderBevelPosition(out double borderBevelPosition);
                    _state.BevelPosition = (uint)(borderBevelPosition * 100);
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderHueChanged:
                    Props.GetBorderHue(out double hue);
                    _state.BorderHue = hue;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderSaturationChanged:
                    Props.GetBorderSaturation(out double sat);
                    _state.BorderSaturation = sat * 100;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderLumaChanged:
                    Props.GetBorderLuma(out double luma);
                    _state.BorderLuma = luma * 100;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderLightSourceDirectionChanged:
                    Props.GetBorderLightSourceDirection(out double deg);
                    _state.LightSourceDirection = deg;
                    break;
                case _BMDSwitcherKeyFlyKeyFrameParametersEventType.bmdSwitcherKeyFlyKeyFrameParametersEventTypeBorderLightSourceAltitudeChanged:
                    Props.GetBorderLightSourceAltitude(out double alt);
                    _state.LightSourceAltitude = (uint)(alt * 100);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }
}