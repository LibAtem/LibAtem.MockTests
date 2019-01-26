using System;
using BMDSwitcherAPI;
using LibAtem.Common;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class SuperSourceCallback : IBMDSwitcherInputSuperSourceCallback, INotify<_BMDSwitcherInputSuperSourceEventType>
    {
        private readonly ComparisonSuperSourceState _state;
        private readonly IBMDSwitcherInputSuperSource _props;

        public SuperSourceCallback(ComparisonSuperSourceState state, IBMDSwitcherInputSuperSource props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherInputSuperSourceEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeInputFillChanged:
                    _props.GetInputFill(out long input);
                    _state.ArtFillInput = (VideoSource) input;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeInputCutChanged:
                    _props.GetInputCut(out long cutInput);
                    _state.ArtKeyInput = (VideoSource) cutInput;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeArtOptionChanged:
                    _props.GetArtOption(out _BMDSwitcherSuperSourceArtOption option);
                    _state.ArtOption = AtemEnumMaps.SuperSourceArtOptionMap.FindByValue(option);
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypePreMultipliedChanged:
                    _props.GetPreMultiplied(out int preMultiplied);
                    _state.ArtPreMultiplied = preMultiplied != 0;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeClipChanged:
                    _props.GetClip(out double clip);
                    _state.ArtClip = clip * 100;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeGainChanged:
                    _props.GetGain(out double gain);
                    _state.ArtGain = gain * 100;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeInverseChanged:
                    int inverse = 0;
                    _props.GetInverse(ref inverse);
                    _state.ArtInvertKey = inverse != 0;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeBorderEnabledChanged:
                    _props.GetBorderEnabled(out int enabled);
                    _state.BorderEnabled = enabled != 0;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeBorderBevelChanged:
                    _props.GetBorderBevel(out _BMDSwitcherBorderBevelOption bevelOption);
                    _state.BorderBevel = AtemEnumMaps.BorderBevelMap.FindByValue(bevelOption);
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeBorderWidthOutChanged:
                    _props.GetBorderWidthOut(out double widthOut);
                    _state.BorderWidthOut = widthOut;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeBorderWidthInChanged:
                    _props.GetBorderWidthIn(out double widthIn);
                    _state.BorderWidthIn = widthIn;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeBorderSoftnessOutChanged:
                    _props.GetBorderSoftnessOut(out double softnessOut);
                    _state.BorderSoftnessOut = (uint) (softnessOut * 100);
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeBorderSoftnessInChanged:
                    _props.GetBorderSoftnessIn(out double softnessIn);
                    _state.BorderSoftnessIn = (uint) (softnessIn * 100);
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeBorderBevelSoftnessChanged:
                    _props.GetBorderBevelSoftness(out double bevelSoftness);
                    _state.BorderBevelSoftness = (uint) (bevelSoftness * 100);
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeBorderBevelPositionChanged:
                    _props.GetBorderBevelPosition(out double bevelPosition);
                    _state.BorderBevelPosition = (uint) (bevelPosition * 100);
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeBorderHueChanged:
                    _props.GetBorderHue(out double hue);
                    _state.BorderHue = hue;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeBorderSaturationChanged:
                    _props.GetBorderSaturation(out double sat);
                    _state.BorderSaturation = sat * 100;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeBorderLumaChanged:
                    _props.GetBorderLuma(out double luma);
                    _state.BorderLuma = luma * 100;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeBorderLightSourceDirectionChanged:
                    _props.GetBorderLightSourceDirection(out double deg);
                    _state.BorderLightSourceDirection = deg;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeBorderLightSourceAltitudeChanged:
                    _props.GetBorderLightSourceAltitude(out double alt);
                    _state.BorderLightSourceAltitude = alt * 100;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }

    public sealed class SuperSourceBoxCallback : IBMDSwitcherSuperSourceBoxCallback, INotify<_BMDSwitcherSuperSourceBoxEventType>
    {
        private readonly ComparisonSuperSourceBoxState _state;
        private readonly IBMDSwitcherSuperSourceBox _props;

        public SuperSourceBoxCallback(ComparisonSuperSourceBoxState state, IBMDSwitcherSuperSourceBox props)
        {
            _state = state;
            _props = props;
        }

        public void Notify(_BMDSwitcherSuperSourceBoxEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeEnabledChanged:
                    _props.GetEnabled(out int enabled);
                    _state.Enabled = enabled != 0;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeInputSourceChanged:
                    _props.GetInputSource(out long input);
                    _state.InputSource = (VideoSource) input;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypePositionXChanged:
                    _props.GetPositionX(out double xPos);
                    _state.PositionX = xPos;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypePositionYChanged:
                    _props.GetPositionY(out double yPos);
                    _state.PositionY = yPos;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeSizeChanged:
                    _props.GetSize(out double size);
                    _state.Size = size;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeCroppedChanged:
                    _props.GetCropped(out int cropped);
                    _state.Cropped = cropped != 0;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeCropTopChanged:
                    _props.GetCropTop(out double top);
                    _state.CropTop = top;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeCropBottomChanged:
                    _props.GetCropBottom(out double bottom);
                    _state.CropBottom = bottom;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeCropLeftChanged:
                    _props.GetCropLeft(out double left);
                    _state.CropLeft = left;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeCropRightChanged:
                    _props.GetCropRight(out double right);
                    _state.CropRight = right;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
}