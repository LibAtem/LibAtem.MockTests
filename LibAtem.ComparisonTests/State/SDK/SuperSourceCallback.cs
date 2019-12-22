using System;
using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public sealed class SuperSourceCallback : SdkCallbackBaseNotify<IBMDSwitcherInputSuperSource, _BMDSwitcherInputSuperSourceEventType>, IBMDSwitcherInputSuperSourceCallback
    {
        private readonly SuperSourceState.PropertiesState _state;

        public SuperSourceCallback(SuperSourceState state, IBMDSwitcherInputSuperSource props, Action<string> onChange) : base(props, onChange)
        {
            _state = state.Properties;
            TriggerAllChanged();

            var borderProps = props as IBMDSwitcherSuperSourceBorder;
            Children.Add(new SuperSourceBorderCallback(state.Border, borderProps, AppendChange("Border")));

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherSuperSourceBoxIterator>(props.CreateIterator);
            state.Boxes = AtemSDKConverter.IterateList<IBMDSwitcherSuperSourceBox, SuperSourceState.BoxState>(
                iterator.Next, (box, boxId) =>
                {
                    var boxState = new SuperSourceState.BoxState();
                    Children.Add(new SuperSourceBoxCallback(boxState, box, AppendChange($"Boxes.{boxId:D}")));
                    return boxState;
                });
        }

        public override void Notify(_BMDSwitcherInputSuperSourceEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeInputFillChanged:
                    Props.GetInputFill(out long input);
                    _state.ArtFillInput = (VideoSource) input;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeInputCutChanged:
                    Props.GetInputCut(out long cutInput);
                    _state.ArtKeyInput = (VideoSource) cutInput;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeArtOptionChanged:
                    Props.GetArtOption(out _BMDSwitcherSuperSourceArtOption option);
                    _state.ArtOption = AtemEnumMaps.SuperSourceArtOptionMap.FindByValue(option);
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypePreMultipliedChanged:
                    Props.GetPreMultiplied(out int preMultiplied);
                    _state.ArtPreMultiplied = preMultiplied != 0;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeClipChanged:
                    Props.GetClip(out double clip);
                    _state.ArtClip = clip * 100;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeGainChanged:
                    Props.GetGain(out double gain);
                    _state.ArtGain = gain * 100;
                    break;
                case _BMDSwitcherInputSuperSourceEventType.bmdSwitcherInputSuperSourceEventTypeInverseChanged:
                    Props.GetInverse(out int inverse);
                    _state.ArtInvertKey = inverse != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange("Properties");
        }
    }

    public sealed class SuperSourceBorderCallback : SdkCallbackBaseNotify<IBMDSwitcherSuperSourceBorder, _BMDSwitcherSuperSourceBorderEventType>, IBMDSwitcherSuperSourceBorderCallback
    {
        private readonly SuperSourceState.BorderState _state;

        public SuperSourceBorderCallback(SuperSourceState.BorderState state, IBMDSwitcherSuperSourceBorder props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherSuperSourceBorderEventType eventType)
        {
            switch (eventType)
            {
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeEnabledChanged:
                Props.GetBorderEnabled(out int enabled);
                _state.Enabled = enabled != 0;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeBevelChanged:
                Props.GetBorderBevel(out _BMDSwitcherBorderBevelOption bevelOption);
                _state.Bevel = AtemEnumMaps.BorderBevelMap.FindByValue(bevelOption);
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeWidthOutChanged:
                Props.GetBorderWidthOut(out double widthOut);
                _state.OuterWidth = widthOut;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeWidthInChanged:
                Props.GetBorderWidthIn(out double widthIn);
                _state.InnerWidth = widthIn;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeSoftnessOutChanged:
                Props.GetBorderSoftnessOut(out double softnessOut);
                _state.OuterSoftness = (uint) (softnessOut * 100);
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeSoftnessInChanged:
                Props.GetBorderSoftnessIn(out double softnessIn);
                _state.InnerSoftness = (uint) (softnessIn * 100);
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeBevelSoftnessChanged:
                Props.GetBorderBevelSoftness(out double bevelSoftness);
                _state.BevelSoftness = (uint) (bevelSoftness * 100);
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeBevelPositionChanged:
                Props.GetBorderBevelPosition(out double bevelPosition);
                _state.BevelPosition = (uint) (bevelPosition * 100);
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeHueChanged:
                Props.GetBorderHue(out double hue);
                _state.Hue = hue;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeSaturationChanged:
                Props.GetBorderSaturation(out double sat);
                _state.Saturation = sat * 100;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeLumaChanged:
                Props.GetBorderLuma(out double luma);
                _state.Luma = luma * 100;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeLightSourceDirectionChanged:
                Props.GetBorderLightSourceDirection(out double deg);
                _state.LightSourceDirection = deg;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeLightSourceAltitudeChanged:
                Props.GetBorderLightSourceAltitude(out double alt);
                _state.LightSourceAltitude = alt * 100;
                break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }

    public sealed class SuperSourceBoxCallback : SdkCallbackBaseNotify<IBMDSwitcherSuperSourceBox, _BMDSwitcherSuperSourceBoxEventType>, IBMDSwitcherSuperSourceBoxCallback
    {
        private readonly SuperSourceState.BoxState _state;

        public SuperSourceBoxCallback(SuperSourceState.BoxState state, IBMDSwitcherSuperSourceBox props, Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherSuperSourceBoxEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeEnabledChanged:
                    Props.GetEnabled(out int enabled);
                    _state.Enabled = enabled != 0;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeInputSourceChanged:
                    Props.GetInputSource(out long input);
                    _state.InputSource = (VideoSource) input;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypePositionXChanged:
                    Props.GetPositionX(out double xPos);
                    _state.PositionX = xPos;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypePositionYChanged:
                    Props.GetPositionY(out double yPos);
                    _state.PositionY = yPos;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeSizeChanged:
                    Props.GetSize(out double size);
                    _state.Size = size;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeCroppedChanged:
                    Props.GetCropped(out int cropped);
                    _state.Cropped = cropped != 0;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeCropTopChanged:
                    Props.GetCropTop(out double top);
                    _state.CropTop = top;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeCropBottomChanged:
                    Props.GetCropBottom(out double bottom);
                    _state.CropBottom = bottom;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeCropLeftChanged:
                    Props.GetCropLeft(out double left);
                    _state.CropLeft = left;
                    break;
                case _BMDSwitcherSuperSourceBoxEventType.bmdSwitcherSuperSourceBoxEventTypeCropRightChanged:
                    Props.GetCropRight(out double right);
                    _state.CropRight = right;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }
}