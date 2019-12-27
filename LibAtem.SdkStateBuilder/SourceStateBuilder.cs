using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using System.Collections.Generic;

namespace LibAtem.SdkStateBuilder
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

                if (input is IBMDSwitcherInputAux aux)
                    auxes.Add(AuxInput(aux));

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
            int isDefault = 0;
            props.AreNamesDefault(ref isDefault);
            //state.AreNamesDefault = isDefault != 0;
            props.IsProgramTallied(out int progTally);
            state.Tally.ProgramTally = progTally != 0;
            props.IsPreviewTallied(out int prevTally);
            state.Tally.PreviewTally = prevTally != 0;
            props.GetAvailableExternalPortTypes(out _BMDSwitcherExternalPortType types);
            state.Properties.AvailableExternalPortTypes = (ExternalPortTypeFlags)types;
            props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType value);
            state.Properties.CurrentExternalPortType = (ExternalPortTypeFlags)value;

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
    }
}