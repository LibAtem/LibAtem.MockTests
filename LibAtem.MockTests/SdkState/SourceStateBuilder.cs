using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.Serialization;
using LibAtem.State;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace LibAtem.MockTests.SdkState
{
    public static class SourceStateBuilder
    {
        public static void Build(AtemState state, IBMDSwitcher switcher)
        {
            var auxes = new List<AuxState>();
            var cols = new List<ColorState>();
            var ssrcs = new List<SuperSourceState>();

            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherInputIterator>(switcher.CreateIterator);
            AtemSDKConverter.Iterate<IBMDSwitcherInput>(iterator.Next, (input, i) =>
            {
                input.GetInputId(out long id);
                var src = (VideoSource)id;

                state.Settings.Inputs[src] = BuildOne(input);

                if (input is IBMDSwitcherInputAux aux) {
                    auxes.Add(AuxInput(aux));

                    if (input is IBMDSwitcherDisplayClock dc)
                    {
                        if (src != VideoSource.Auxilary1) throw new Exception("Got IBMDSwitcherDisplayClock for unexpected aux");

                        state.DisplayClock = DisplayClock(dc);
                    }
                }

                if (input is IBMDSwitcherInputColor col)
                    cols.Add(ColorInput(col));

                if (input is IBMDSwitcherInputSuperSource ssrc)
                    ssrcs.Add(SuperSourceStateBuilder.Build(ssrc));
            });

            state.Auxiliaries = auxes;
            state.ColorGenerators = cols;
            state.SuperSources = ssrcs;
        }

        private static InputState BuildOne(IBMDSwitcherInput props)
        {
            var state = new InputState();

            props.GetShortName(out string name);
            state.Properties.ShortName = name;
            props.GetLongName(out string longName);
            state.Properties.LongName = longName;
            props.AreNamesDefault(out int isDefault);
            state.Properties.AreNamesDefault = isDefault != 0;
            props.IsProgramTallied(out int progTally);
            state.Tally.ProgramTally = progTally != 0;
            props.IsPreviewTallied(out int prevTally);
            state.Tally.PreviewTally = prevTally != 0;
            props.GetAvailableExternalPortTypes(out _BMDSwitcherExternalPortType types);
            state.Properties.AvailableExternalPortTypes = AtemEnumMaps.VideoPortTypeMap.FindFlagsComponentsByValue(types);
            props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType value);
            state.Properties.CurrentExternalPortType = AtemEnumMaps.VideoPortTypeMap.FindByValue(value);
            props.GetPortType(out _BMDSwitcherPortType internalType);
            state.Properties.InternalPortType = AtemEnumMaps.InternalPortTypeMap.FindByValue(internalType);

            props.GetInputAvailability(out _BMDSwitcherInputAvailability rawAvailability);
            var translatedAvailability = AtemEnumMaps.TranslateSourceAvailability(rawAvailability);
            state.Properties.MeAvailability = translatedAvailability.Item2;
            state.Properties.SourceAvailability = translatedAvailability.Item1;

            return state;
        }

        private static AuxState AuxInput(IBMDSwitcherInputAux props)
        {
            var state = new AuxState();

            props.GetInputSource(out long source);
            state.Source = (VideoSource)source;

            return state;
        }

        private static ColorState ColorInput(IBMDSwitcherInputColor props)
        {
            var state = new ColorState();

            props.GetHue(out double hue);
            state.Hue = hue;
            props.GetSaturation(out double saturation);
            state.Saturation = saturation * 100;
            props.GetLuma(out double luma);
            state.Luma = luma * 100;

            return state;
        }

        private static DisplayClockState DisplayClock(IBMDSwitcherDisplayClock props)
        {
            var state = new DisplayClockState();

            props.GetEnabled(out int enabled);
            state.Properties.Enabled = enabled != 0;
            props.GetOpacity(out ushort opacity);
            state.Properties.Opacity = opacity;
            props.GetSize(out ushort size);
            state.Properties.Size = size;
            props.GetPositionX(out double positionX);
            state.Properties.PositionX = positionX;
            props.GetPositionY(out double positionY);
            state.Properties.PositionY = positionY;
            props.GetAutoHide(out int autoHide);
            state.Properties.AutoHide = autoHide != 0;
            props.GetStartFrom(out byte startHours, out byte startMinutes, out byte startSeconds, out byte startFrames);
            state.Properties.StartFrom = new HyperDeckTime()
            {
                Hour = startHours,
                Minute = startMinutes,
                Second = startSeconds,
                Frame = startFrames,
            };
            props.GetClockMode(out _BMDSwitcherDisplayClockMode clockMode);
            state.Properties.ClockMode = AtemEnumMaps.DisplayClockModeMap.FindByValue(clockMode);
            props.GetClockState(out _BMDSwitcherDisplayClockState clockState);
            state.Properties.ClockState = AtemEnumMaps.DisplayClockStateMap.FindByValue(clockState);
            props.GetClockTime(out byte clockHours, out byte clockMinutes, out byte clockSeconds, out byte clockFrames);
            state.CurrentTime= new HyperDeckTime()
            {
                Hour = clockHours,
                Minute = clockMinutes,
                Second = clockSeconds,
                Frame = clockFrames,
            };

            return state;
        }
    }
}