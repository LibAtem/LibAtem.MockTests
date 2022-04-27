using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using System;

namespace LibAtem.MockTests.SdkState
{
    public static class SuperSourceStateBuilder
    {
        public static SuperSourceState Build(IBMDSwitcherInputSuperSource props)
        {
            var state = new SuperSourceState();

            props.GetInputFill(out long input);
            state.Properties.ArtFillSource = (VideoSource)input;
            props.GetInputCut(out long cutInput);
            state.Properties.ArtCutSource = (VideoSource)cutInput;
            props.GetArtOption(out _BMDSwitcherSuperSourceArtOption option);
            state.Properties.ArtOption = AtemEnumMaps.SuperSourceArtOptionMap.FindByValue(option);
            props.GetPreMultiplied(out int preMultiplied);
            state.Properties.ArtPreMultiplied = preMultiplied != 0;
            props.GetClip(out double clip);
            state.Properties.ArtClip = clip * 100;
            props.GetGain(out double gain);
            state.Properties.ArtGain = gain * 100;
            props.GetInverse(out int inverse);
            state.Properties.ArtInvertKey = inverse != 0;

            if (props is IBMDSwitcherSuperSourceBorder borderProps)
            {
                state.Border = new SuperSourceState.BorderState();
                BuildBorder(state.Border, borderProps);
            }

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherSuperSourceBoxIterator>(props.CreateIterator);
            state.Boxes = AtemSDKConverter.IterateList<IBMDSwitcherSuperSourceBox, SuperSourceState.BoxState>(
                iterator.Next, (box, boxId) => BuildBox(box));

            return state;
        }

        private static void BuildBorder(SuperSourceState.BorderState state, IBMDSwitcherSuperSourceBorder props)
        {
            props.GetBorderEnabled(out int enabled);
            state.Enabled = enabled != 0;
            props.GetBorderBevel(out _BMDSwitcherBorderBevelOption bevelOption);
            state.Bevel = AtemEnumMaps.BorderBevelMap.FindByValue(bevelOption);
            props.GetBorderWidthOut(out double widthOut);
            state.OuterWidth = widthOut;
            props.GetBorderWidthIn(out double widthIn);
            state.InnerWidth = widthIn;
            props.GetBorderSoftnessOut(out double softnessOut);
            state.OuterSoftness = (uint)Math.Round(softnessOut * 100);
            props.GetBorderSoftnessIn(out double softnessIn);
            state.InnerSoftness = (uint)Math.Round(softnessIn * 100);
            props.GetBorderBevelSoftness(out double bevelSoftness);
            state.BevelSoftness = (uint)Math.Round(bevelSoftness * 100);
            props.GetBorderBevelPosition(out double bevelPosition);
            state.BevelPosition = (uint)Math.Round(bevelPosition * 100);
            props.GetBorderHue(out double hue);
            state.Hue = hue;
            props.GetBorderSaturation(out double sat);
            state.Saturation = sat * 100;
            props.GetBorderLuma(out double luma);
            state.Luma = luma * 100;
            props.GetBorderLightSourceDirection(out double deg);
            state.LightSourceDirection = deg;
            props.GetBorderLightSourceAltitude(out double alt);
            state.LightSourceAltitude = alt * 100;
        }

        private static SuperSourceState.BoxState BuildBox(IBMDSwitcherSuperSourceBox props)
        {
            var state = new SuperSourceState.BoxState();

            props.GetEnabled(out int enabled);
            state.Enabled = enabled != 0;
            props.GetInputSource(out long input);
            state.Source = (VideoSource)input;
            props.GetPositionX(out double xPos);
            state.PositionX = xPos;
            props.GetPositionY(out double yPos);
            state.PositionY = yPos;
            props.GetSize(out double size);
            state.Size = size;
            props.GetCropped(out int cropped);
            state.Cropped = cropped != 0;
            props.GetCropTop(out double top);
            state.CropTop = top;
            props.GetCropBottom(out double bottom);
            state.CropBottom = bottom;
            props.GetCropLeft(out double left);
            state.CropLeft = left;
            props.GetCropRight(out double right);
            state.CropRight = right;

            return state;
        }

    }
}