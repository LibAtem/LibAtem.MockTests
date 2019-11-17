using System;
using BMDSwitcherAPI;
using LibAtem.Commands;
using LibAtem.Commands.SuperSource;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests2.State.SDK
{
    public sealed class SuperSourceCallback : IBMDSwitcherInputSuperSourceCallback, INotify<_BMDSwitcherInputSuperSourceEventType>
    {
        private readonly SuperSourceState.PropertiesState _state;
        private readonly IBMDSwitcherInputSuperSource _props;
        private readonly Action<CommandQueueKey> _onChange;

        public SuperSourceCallback(SuperSourceState.PropertiesState state, IBMDSwitcherInputSuperSource props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
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
                    _props.GetInverse(out int inverse);
                    _state.ArtInvertKey = inverse != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange(new CommandQueueKey(new SuperSourcePropertiesGetCommand()));
        }
    }

    public sealed class SuperSourceBorderCallback : IBMDSwitcherSuperSourceBorderCallback, INotify<_BMDSwitcherSuperSourceBorderEventType>
    {
        private readonly SuperSourceState.BorderState _state;
        private readonly IBMDSwitcherSuperSourceBorder _props;
        private readonly Action<CommandQueueKey> _onChange;

        public SuperSourceBorderCallback(SuperSourceState.BorderState state, IBMDSwitcherSuperSourceBorder props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _props = props;
            _onChange = onChange;
        }

        public void Notify(_BMDSwitcherSuperSourceBorderEventType eventType)
        {
            switch (eventType)
            {
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeEnabledChanged:
                _props.GetBorderEnabled(out int enabled);
                _state.Enabled = enabled != 0;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeBevelChanged:
                _props.GetBorderBevel(out _BMDSwitcherBorderBevelOption bevelOption);
                _state.Bevel = AtemEnumMaps.BorderBevelMap.FindByValue(bevelOption);
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeWidthOutChanged:
                _props.GetBorderWidthOut(out double widthOut);
                _state.OuterWidth = widthOut;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeWidthInChanged:
                _props.GetBorderWidthIn(out double widthIn);
                _state.InnerWidth = widthIn;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeSoftnessOutChanged:
                _props.GetBorderSoftnessOut(out double softnessOut);
                _state.OuterSoftness = (uint) (softnessOut * 100);
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeSoftnessInChanged:
                _props.GetBorderSoftnessIn(out double softnessIn);
                _state.InnerSoftness = (uint) (softnessIn * 100);
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeBevelSoftnessChanged:
                _props.GetBorderBevelSoftness(out double bevelSoftness);
                _state.BevelSoftness = (uint) (bevelSoftness * 100);
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeBevelPositionChanged:
                _props.GetBorderBevelPosition(out double bevelPosition);
                _state.BevelPosition = (uint) (bevelPosition * 100);
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeHueChanged:
                _props.GetBorderHue(out double hue);
                _state.Hue = hue;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeSaturationChanged:
                _props.GetBorderSaturation(out double sat);
                _state.Saturation = sat * 100;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeLumaChanged:
                _props.GetBorderLuma(out double luma);
                _state.Luma = luma * 100;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeLightSourceDirectionChanged:
                _props.GetBorderLightSourceDirection(out double deg);
                _state.LightSourceDirection = deg;
                break;
            case _BMDSwitcherSuperSourceBorderEventType.bmdSwitcherSuperSourceBorderEventTypeLightSourceAltitudeChanged:
                _props.GetBorderLightSourceAltitude(out double alt);
                _state.LightSourceAltitude = alt * 100;
                break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            _onChange(new CommandQueueKey(new SuperSourcePropertiesGetCommand()));
        }
    }

    public sealed class SuperSourceBoxCallback : IBMDSwitcherSuperSourceBoxCallback, INotify<_BMDSwitcherSuperSourceBoxEventType>
    {
        private readonly SuperSourceState.BoxState _state;
        private readonly SuperSourceId _ssrcIndex;
        private readonly SuperSourceBoxId _boxIndex;
        private readonly IBMDSwitcherSuperSourceBox _props;
        private readonly Action<CommandQueueKey> _onChange;

        public SuperSourceBoxCallback(SuperSourceState.BoxState state, SuperSourceId ssrcIndex, SuperSourceBoxId boxIndex, IBMDSwitcherSuperSourceBox props, Action<CommandQueueKey> onChange)
        {
            _state = state;
            _ssrcIndex = ssrcIndex;
            _boxIndex = boxIndex;
            _props = props;
            _onChange = onChange;
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

            _onChange(new CommandQueueKey(new SuperSourceBoxGetV8Command() { SSrcId = _ssrcIndex, BoxIndex = _boxIndex }));
        }
    }
}