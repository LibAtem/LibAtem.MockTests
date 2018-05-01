using System;
using System.Collections.Generic;
using LibAtem.Commands;
using LibAtem.Commands.DeviceProfile;
using LibAtem.Commands.DownstreamKey;
using LibAtem.Commands.MixEffects;
using LibAtem.Commands.MixEffects.Key;
using LibAtem.Commands.MixEffects.Transition;
using LibAtem.Commands.Settings;
using LibAtem.Commands.Settings.Multiview;
using LibAtem.Commands.SuperSource;
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
                {typeof(MixEffectKeyOnAirGetCommand), UpdateMixEffectKeyerOnAir},
                {typeof(MixEffectKeyPropertiesGetCommand), UpdateMixEffectKeyerProperties},
                {typeof(MixEffectKeyLumaGetCommand), UpdateMixEffectKeyerLuma},
                {typeof(MixEffectKeyChromaGetCommand), UpdateMixEffectKeyerChroma},
                {typeof(MixEffectKeyPatternGetCommand), UpdateMixEffectKeyerPattern},
                {typeof(MixEffectKeyDVEGetCommand), UpdateMixEffectKeyerDVE},
                {typeof(TransitionPropertiesGetCommand), UpdateMixEffectTransitionProperties},
                {typeof(TransitionMixGetCommand), UpdateMixEffectTransitionMix},
                {typeof(TransitionDipGetCommand), UpdateMixEffectTransitionDip},
                {typeof(TransitionWipeGetCommand), UpdateMixEffectTransitionWipe},
                {typeof(TransitionStingerGetCommand), UpdateMixEffectTransitionStinger},
                {typeof(TransitionDVEGetCommand), UpdateMixEffectTransitionDVE},
                {typeof(SerialPortModeCommand), UpdateSettingsSerialMode},
                {typeof(VideoModeGetCommand), UpdateSettingsVideoMode},
                {typeof(MultiviewerConfigCommand), UpdateSettingsMultiviewerConfig},
                {typeof(MultiviewPropertiesGetCommand), UpdateSettingsMultiviewerProperties},
                {typeof(MultiviewWindowInputGetCommand), UpdateSettingsMultiviewerWindowInputProperties},
                {typeof(SuperSourcePropertiesGetCommand), UpdateSuperSourceProperties},
                {typeof(SuperSourceBoxGetCommand), UpdateSuperSourceBoxProperties},
                {typeof(DownstreamKeyPropertiesGetCommand), UpdateDownstreamKeyerProperties},
                {typeof(DownstreamKeySourceGetCommand), UpdateDownstreamKeyerSource},
                {typeof(DownstreamKeyStateGetCommand), UpdateDownstreamKeyerState},
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

        private static void UpdateMixEffectKeyerOnAir(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (MixEffectKeyOnAirGetCommand)rawCmd;
            state.MixEffects[cmd.MixEffectIndex].Keyers[cmd.KeyerIndex].OnAir = cmd.OnAir;
        }
        private static void UpdateMixEffectKeyerProperties(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (MixEffectKeyPropertiesGetCommand)rawCmd;
            var props = state.MixEffects[cmd.MixEffectIndex].Keyers[cmd.KeyerIndex];

            props.Type = cmd.Mode;
            props.FlyEnabled = cmd.FlyEnabled;
            props.FillSource = cmd.FillSource;
            props.CutSource = cmd.CutSource;
            props.MaskEnabled = cmd.MaskEnabled;
            props.MaskTop = cmd.MaskTop;
            props.MaskBottom = cmd.MaskBottom;
            props.MaskLeft = cmd.MaskLeft;
            props.MaskRight = cmd.MaskRight;
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
        private static void UpdateMixEffectKeyerLuma(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (MixEffectKeyLumaGetCommand)rawCmd;
            var props = state.MixEffects[cmd.MixEffectIndex].Keyers[cmd.KeyerIndex].Luma;

            props.PreMultiplied = cmd.PreMultiplied;
            props.Gain = cmd.Gain;
            props.Clip = cmd.Clip;
            props.Invert = cmd.Invert;
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
        private static void UpdateMixEffectKeyerPattern(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (MixEffectKeyPatternGetCommand)rawCmd;
            var props = state.MixEffects[cmd.MixEffectIndex].Keyers[cmd.KeyerIndex].Pattern;

            props.Style = cmd.Style;
            props.Size = cmd.Size;
            props.Symmetry = cmd.Symmetry;
            props.Softness = cmd.Softness;
            props.XPosition = cmd.XPosition;
            props.YPosition = cmd.YPosition;
            props.Inverse = cmd.Inverse;
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

        private static void UpdateMixEffectTransitionProperties(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (TransitionPropertiesGetCommand)rawCmd;
            var props = state.MixEffects[cmd.Index].Transition;

            props.Style = cmd.Style;
            props.NextStyle = cmd.NextStyle;
            props.Selection = cmd.Selection;
            props.NextSelection = cmd.NextSelection;
        }
        private static void UpdateMixEffectTransitionMix(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (TransitionMixGetCommand)rawCmd;
            var props = state.MixEffects[cmd.Index].Transition.Mix;

            props.Rate = cmd.Rate;
        }
        private static void UpdateMixEffectTransitionDip(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (TransitionDipGetCommand)rawCmd;
            var props = state.MixEffects[cmd.Index].Transition.Dip;

            props.Input = cmd.Input;
            props.Rate = cmd.Rate;
        }
        private static void UpdateMixEffectTransitionWipe(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (TransitionWipeGetCommand)rawCmd;
            var props = state.MixEffects[cmd.Index].Transition.Wipe;

            props.Rate = cmd.Rate;
            props.Pattern = cmd.Pattern;
            props.BorderWidth = cmd.BorderWidth;
            props.BorderInput = cmd.BorderInput;
            props.Symmetry = cmd.Symmetry;
            props.BorderSoftness = cmd.BorderSoftness;
            props.XPosition = cmd.XPosition;
            props.YPosition = cmd.YPosition;
            props.ReverseDirection = cmd.ReverseDirection;
            props.FlipFlop = cmd.FlipFlop;
        }
        private static void UpdateMixEffectTransitionStinger(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (TransitionStingerGetCommand)rawCmd;
            var props = state.MixEffects[cmd.Index].Transition.Stinger;

            props.Source = cmd.Source;
            props.PreMultipliedKey = cmd.PreMultipliedKey;
            props.Clip = cmd.Clip;
            props.Gain = cmd.Gain;
            props.Invert = cmd.Invert;
            props.Preroll = cmd.Preroll;
            props.ClipDuration = cmd.ClipDuration;
            props.TriggerPoint = cmd.TriggerPoint;
            props.MixRate = cmd.MixRate;
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

        private static void UpdateSettingsSerialMode(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (SerialPortModeCommand) rawCmd;
            state.Settings.SerialMode = cmd.SerialMode;
        }
        private static void UpdateSettingsVideoMode(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (VideoModeGetCommand)rawCmd;
            state.Settings.VideoMode = cmd.VideoMode;
        }
        private static void UpdateSettingsMultiviewerConfig(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (MultiviewerConfigCommand)rawCmd;
            state.Settings.MultiViews = new Dictionary<uint, ComparisonSettingsMultiViewState>();
            for (uint i = 0; i < cmd.Count; i++)
                state.Settings.MultiViews[i] = new ComparisonSettingsMultiViewState();

            // TODO - remainder
        }
        private static void UpdateSettingsMultiviewerProperties(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (MultiviewPropertiesGetCommand)rawCmd;
            var props = state.Settings.MultiViews[cmd.MultiviewIndex];

            props.Layout = cmd.Layout;
            props.SafeAreaEnabled = cmd.SafeAreaEnabled;
            props.ProgramPreviewSwapped = cmd.ProgramPreviewSwapped;
        }

        private static void UpdateSettingsMultiviewerWindowInputProperties(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (MultiviewWindowInputGetCommand) rawCmd;
            var props = state.Settings.MultiViews[cmd.MultiviewIndex];

            props.Windows[(int) cmd.WindowIndex].Source = cmd.Source;
        }

        private static void UpdateSuperSourceProperties(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (SuperSourcePropertiesGetCommand)rawCmd;
            var props = state.SuperSource;

            props.ArtFillInput = cmd.ArtFillInput;
            props.ArtKeyInput = cmd.ArtKeyInput;
            props.ArtOption = cmd.ArtOption;
            props.ArtPreMultiplied = cmd.ArtPreMultiplied;
            props.ArtClip = cmd.ArtClip;
            props.ArtGain = cmd.ArtGain;
            props.ArtInvertKey = cmd.ArtInvertKey;

            props.BorderEnabled = cmd.BorderEnabled;
            props.BorderBevel = cmd.BorderBevel;
            props.BorderWidthOut = cmd.BorderWidthOut;
            props.BorderWidthIn = cmd.BorderWidthIn;
            props.BorderSoftnessOut = cmd.BorderSoftnessOut;
            props.BorderSoftnessIn = cmd.BorderSoftnessIn;
            props.BorderBevelSoftness = cmd.BorderBevelSoftness;
            props.BorderBevelPosition = cmd.BorderBevelPosition;
            props.BorderHue = cmd.BorderHue;
            props.BorderSaturation = cmd.BorderSaturation;
            props.BorderLuma = cmd.BorderLuma;
            props.BorderLightSourceDirection = cmd.BorderLightSourceDirection;
            props.BorderLightSourceAltitude = cmd.BorderLightSourceAltitude;
        }
        private static void UpdateSuperSourceBoxProperties(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (SuperSourceBoxGetCommand)rawCmd;
            var props = state.SuperSource.Boxes[cmd.Index] = new ComparisonSuperSourceBoxState();

            props.Enabled = cmd.Enabled;
            props.InputSource = cmd.InputSource;
            props.PositionX = cmd.PositionX;
            props.PositionY = cmd.PositionY;
            props.Size = cmd.Size;
            props.Cropped = cmd.Cropped;
            props.CropTop = cmd.CropTop;
            props.CropBottom = cmd.CropBottom;
            props.CropLeft = cmd.CropLeft;
            props.CropRight = cmd.CropRight;
        }

        private static void UpdateDownstreamKeyerProperties(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (DownstreamKeyPropertiesGetCommand) rawCmd;
            var props = state.DownstreamKeyers[cmd.Index];

            props.Tie = cmd.Tie;
            props.Rate = cmd.Rate;
            props.PreMultipliedKey = cmd.PreMultipliedKey;
            props.Clip = cmd.Clip;
            props.Gain = cmd.Gain;
            props.Invert = cmd.Invert;

            props.MaskEnabled = cmd.MaskEnabled;
            props.MaskTop = cmd.MaskTop;
            props.MaskBottom = cmd.MaskBottom;
            props.MaskLeft = cmd.MaskLeft;
            props.MaskRight = cmd.MaskRight;
        }
        private static void UpdateDownstreamKeyerSource(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (DownstreamKeySourceGetCommand)rawCmd;
            if (!state.DownstreamKeyers.ContainsKey(cmd.Index))
                state.DownstreamKeyers[cmd.Index] = new ComparisonDownstreamKeyerState();

            var props = state.DownstreamKeyers[cmd.Index];

            props.CutSource = cmd.CutSource;
            props.FillSource = cmd.FillSource;
        }
        private static void UpdateDownstreamKeyerState(ComparisonState state, ICommand rawCmd)
        {
            var cmd = (DownstreamKeyStateGetCommand)rawCmd;
            var props = state.DownstreamKeyers[cmd.Index];

            props.OnAir = cmd.OnAir;
            props.InTransition = cmd.InTransition;
            props.IsAuto = cmd.IsAuto;
            props.RemainingFrames = cmd.RemainingFrames;
        }
    }
}