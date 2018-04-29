using System;
using System.Collections.Generic;
using LibAtem.Commands;
using LibAtem.Commands.DeviceProfile;
using LibAtem.Commands.MixEffects;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Common;

namespace LibAtem.ComparisonTests.State
{
    public static class ComparisonStateBuilder
    {
        private static readonly IReadOnlyDictionary<Type, Action<ComparisonState, ICommand>> updaters;

        static ComparisonStateBuilder()
        {
            updaters = new Dictionary<Type, Action<ComparisonState, ICommand>>()
            {
                {typeof(TopologyCommand), UpdateTopology},
                {typeof(MixEffectBlockConfigCommand), UpdateMixEffectTopology},
                {typeof(AuxSourceGetCommand), UpdateAux},
                {typeof(ColorGeneratorGetCommand), UpdateColor},
                {typeof(ProgramInputGetCommand), UpdateProgramInput},
                {typeof(PreviewInputGetCommand), UpdatePreviewInput},
                {typeof(MixEffectKeyChromaGetCommand), UpdateMixEffectKeyerChroma},
                {typeof(MixEffectKeyDVEGetCommand), UpdateMixEffectKeyerDVE},
                {typeof(TransitionDipGetCommand), UpdateMixEffectTransitionDip},
                {typeof(TransitionDVEGetCommand), UpdateMixEffectTransitionDVE},
            };
        }

        public static void Update(ComparisonState state, IReadOnlyList<ICommand> commands)
        {
            foreach (ICommand cmd in commands)
                Update(state, cmd);
        }

        private static void Update(ComparisonState state, ICommand cmd)
        {
            if (updaters.TryGetValue(cmd.GetType(), out var updater))
                updater(state, cmd);
            else
                Console.WriteLine("ComparisonState: Missing LibAtem handling of " + cmd.GetType().Name);
        }

        private static void UpdateTopology(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (TopologyCommand)rawCmd;
            
            state.Auxiliaries = new Dictionary<AuxiliaryId, ComparisonAuxiliaryState>();
            for (int i = 0; i < cmd.Auxiliaries; i++)
                state.Auxiliaries[(AuxiliaryId) i] = new ComparisonAuxiliaryState();

            state.Colors = new Dictionary<ColorGeneratorId, ComparisonColorState>();
            for (int i = 0; i < cmd.ColorGenerators; i++)
                state.Colors[(ColorGeneratorId)i] = new ComparisonColorState();

            state.MixEffects = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>();
            for (int i = 0; i < cmd.MixEffectBlocks; i++)
                state.MixEffects[(MixEffectBlockId)i] = new ComparisonMixEffectState();

            // TODO others
        }

        private static void UpdateMixEffectTopology(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (MixEffectBlockConfigCommand)rawCmd;

            var me = state.MixEffects[cmd.Index];
            for (int i = 0; i < cmd.KeyCount; i++)
                me.Keyers[(UpstreamKeyId)i] = new ComparisonMixEffectKeyerState();
        }

        private static void UpdateAux(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (AuxSourceGetCommand)rawCmd;
            state.Auxiliaries[cmd.Id].Source = cmd.Source;
        }

        private static void UpdateColor(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (ColorGeneratorGetCommand)rawCmd;
            var col = state.Colors[cmd.Index];
            col.Hue = cmd.Hue;
            col.Saturation = cmd.Saturation;
            col.Luma = cmd.Luma;
        }

        private static void UpdateProgramInput(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (ProgramInputGetCommand)rawCmd;
            state.MixEffects[cmd.Index].Program = cmd.Source;
        }
        private static void UpdatePreviewInput(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (PreviewInputGetCommand)rawCmd;
            state.MixEffects[cmd.Index].Preview = cmd.Source;
        }
        private static void UpdateMixEffectKeyerChroma(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (MixEffectKeyChromaGetCommand)rawCmd;
            var props = state.MixEffects[cmd.MixEffectIndex].Keyers[cmd.KeyerIndex].Chroma;

            props.Hue = cmd.Hue;
            props.Gain = cmd.Gain;
            props.YSuppress = cmd.YSuppress;
            props.Lift = cmd.Lift;
            props.Narrow = cmd.Narrow;
        }
        private static void UpdateMixEffectKeyerDVE(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (MixEffectKeyDVEGetCommand)rawCmd;
            var props = state.MixEffects[cmd.MixEffectIndex].Keyers[cmd.KeyerIndex].DVE;
            var props2 = state.MixEffects[cmd.MixEffectIndex].Keyers[cmd.KeyerIndex].Fly;

            props2.SizeX = cmd.SizeX;
            props2.SizeY = cmd.SizeY;
            props2.PositionX = cmd.PositionX;
            props2.PositionY = cmd.PositionY;
            props2.Rotation = cmd.Rotation;

            props.BorderEnabled = cmd.BorderEnabled;
            props.BorderShadow = cmd.BorderShadow;
            props.BorderBevel = cmd.BorderBevel;
            props.OuterWidth = cmd.OuterWidth;
            props.InnerWidth = cmd.InnerWidth;
            props.OuterSoftness = cmd.OuterSoftness;
            props.InnerSoftness = cmd.InnerSoftness;
            props.BevelSoftness = cmd.BevelSoftness;
            props.BevelPosition = cmd.BevelPosition;

            props.BorderOpacity = cmd.BorderOpacity;
            props.BorderHue = cmd.BorderHue;
            props.BorderSaturation = cmd.BorderSaturation;
            props.BorderLuma = cmd.BorderLuma;

            props.LightSourceDirection = cmd.LightSourceDirection;
            props.LightSourceAltitude = cmd.LightSourceAltitude;

            props.MaskEnabled = cmd.MaskEnabled;
            props.MaskTop = cmd.MaskTop;
            props.MaskBottom = cmd.MaskBottom;
            props.MaskLeft = cmd.MaskLeft;
            props.MaskRight = cmd.MaskRight;

            props2.Rate = cmd.Rate;
        }

        private static void UpdateMixEffectTransitionDip(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (TransitionDipGetCommand)rawCmd;
            var props = state.MixEffects[cmd.Index].Transition.Dip;

            props.Input = cmd.Input;
            props.Rate = cmd.Rate;
        }
        private static void UpdateMixEffectTransitionDVE(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (TransitionDVEGetCommand)rawCmd;
            var props = state.MixEffects[cmd.Index].Transition.DVE;

            props.Rate = cmd.Rate;
            props.LogoRate = cmd.LogoRate;
            props.Style = cmd.Style;

            props.FillSource = cmd.FillSource;
            props.KeySource = cmd.KeySource;

            props.EnableKey = cmd.EnableKey;
            props.PreMultiplied = cmd.PreMultiplied;
            props.Clip = cmd.Clip;
            props.Gain = cmd.Gain;
            props.InvertKey = cmd.InvertKey;
            props.Reverse = cmd.Reverse;
            props.FlipFlop = cmd.FlipFlop;
        }
    }
}