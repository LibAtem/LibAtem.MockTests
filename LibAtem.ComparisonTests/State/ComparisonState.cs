using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using LibAtem.Common;

namespace LibAtem.ComparisonTests.State
{
    public sealed class ToleranceAttribute : Attribute
    {
        public double Tolerance { get; }

        public ToleranceAttribute(double tolerance)
        {
            Tolerance = tolerance;
        }
    }

    public enum LockOwner
    {
        None,
        This,
        Other,
    }

    [Serializable]
    public class ComparisonState
    {
        public Dictionary<MixEffectBlockId, ComparisonMixEffectState> MixEffects { get; set; } = new Dictionary<MixEffectBlockId, ComparisonMixEffectState>();
        public Dictionary<AuxiliaryId, ComparisonAuxiliaryState> Auxiliaries { get; set; } = new Dictionary<AuxiliaryId, ComparisonAuxiliaryState>();
        public Dictionary<VideoSource, ComparisonInputState> Inputs { get; set; } = new Dictionary<VideoSource, ComparisonInputState>();
        public Dictionary<ColorGeneratorId, ComparisonColorState> Colors { get; set; } = new Dictionary<ColorGeneratorId, ComparisonColorState>();
        public Dictionary<DownstreamKeyId, ComparisonDownstreamKeyerState> DownstreamKeyers { get; set; } = new Dictionary<DownstreamKeyId, ComparisonDownstreamKeyerState>();
        public Dictionary<MediaPlayerId, ComparisonMediaPlayerState> MediaPlayers { get; set; } = new Dictionary<MediaPlayerId, ComparisonMediaPlayerState>();

        public ComparisonMediaPoolState MediaPool { get; set; } = new ComparisonMediaPoolState();
        public ComparisonMacroState Macros { get; set; } = new ComparisonMacroState();
        public ComparisonSuperSourceState SuperSource { get; set; } = new ComparisonSuperSourceState();
        public ComparisonSettingsState Settings { get; set; } = new ComparisonSettingsState();
        public ComparisonAudioState Audio { get; set; } = new ComparisonAudioState();

        public ComparisonState Clone()
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Position = 0;

                return (ComparisonState)formatter.Deserialize(ms);
            }
        }
    }

    [Serializable]
    public class ComparisonMediaPoolStillState
    {
        public bool IsUsed { get; set; }
        public byte[] Hash { get; set; }
        public string Name { get; set; }
    }

    [Serializable]
    public class ComparisonMediaPoolFrameState
    {
        public bool IsUsed { get; set; }
        public byte[] Hash { get; set; }
    }

    [Serializable]
    public class ComparisonMediaPoolClipState
    {
        public bool IsUsed { get; set; }
        public string Name { get; set; }

        public Dictionary<uint, ComparisonMediaPoolFrameState> Frames { get; set; } = new Dictionary<uint, ComparisonMediaPoolFrameState>();
    }

    [Serializable]
    public class ComparisonMediaPoolState
    {
        public Dictionary<uint, ComparisonMediaPoolStillState> Stills { get; set; } = new Dictionary<uint, ComparisonMediaPoolStillState>();
        public Dictionary<uint, ComparisonMediaPoolClipState> Clips { get; set; } = new Dictionary<uint, ComparisonMediaPoolClipState>();
    }

    [Serializable]
    public class ComparisonMediaPlayerState
    {
        public MediaPlayerSource SourceType { get; set; }
        public uint SourceIndex { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsLooped { get; set; }
        public bool AtBeginning { get; set; }
        public uint ClipFrame { get; set; }
    }

    [Serializable]
    public class ComparisonMacroItemState
    {
        public bool IsUsed { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public enum MacroRunStatus
    {
        Idle,
        Running,
        UserWait,
    }

    [Serializable]
    public class ComparisonMacroState
    {
        public Dictionary<uint, ComparisonMacroItemState> Pool { get; set; } = new Dictionary<uint, ComparisonMacroItemState>();

        public bool IsRecording { get; set; }
        public uint RecordIndex { get; set; }
        
        public MacroRunStatus RunStatus { get; set; }
        public uint RunIndex { get; set; }
        public bool Loop { get; set; }
    }

    [Serializable]
    public class ComparisonInputState
    {
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public bool AreNamesDefault { get; set; }
        public bool ProgramTally { get; set; }
        public bool PreviewTally { get; set; }
        public ExternalPortType AvailableExternalPortTypes { get; set; }
        public ExternalPortType CurrentExternalPortType { get; set; }
    }

    [Serializable]
    public class ComparisonDownstreamKeyerState
    {
        public VideoSource FillSource { get; set; }
        public VideoSource CutSource { get; set; }

        public bool Tie { get; set; }
        public uint Rate { get; set; }

        public bool PreMultipliedKey { get; set; }
        [Tolerance(0.01)]
        public double Clip { get; set; }
        [Tolerance(0.01)]
        public double Gain { get; set; }
        public bool Invert { get; set; }

        public bool MaskEnabled { get; set; }
        [Tolerance(0.01)]
        public double MaskTop { get; set; }
        [Tolerance(0.01)]
        public double MaskBottom { get; set; }
        [Tolerance(0.01)]
        public double MaskLeft { get; set; }
        [Tolerance(0.01)]
        public double MaskRight { get; set; }

        public bool OnAir { get; set; }
        public bool InTransition { get; set; }
        public bool IsAuto { get; set; }
        public uint RemainingFrames { get; set; }
    }

    [Serializable]
    public class ComparisonSuperSourceState
    {
        public Dictionary<SuperSourceBoxId, ComparisonSuperSourceBoxState> Boxes { get; set; } = new Dictionary<SuperSourceBoxId, ComparisonSuperSourceBoxState>();

        public VideoSource ArtFillInput { get; set; }
        public VideoSource ArtKeyInput { get; set; }
        public SuperSourceArtOption ArtOption { get; set; }
        public bool ArtPreMultiplied { get; set; }
        [Tolerance(0.01)]
        public double ArtClip { get; set; }
        [Tolerance(0.01)]
        public double ArtGain { get; set; }
        public bool ArtInvertKey { get; set; }

        public bool BorderEnabled { get; set; }
        public BorderBevel BorderBevel { get; set; }
        [Tolerance(0.01)]
        public double BorderWidthOut { get; set; }
        [Tolerance(0.01)]
        public double BorderWidthIn { get; set; }
        public uint BorderSoftnessOut { get; set; }
        public uint BorderSoftnessIn { get; set; }
        public uint BorderBevelSoftness { get; set; }
        public uint BorderBevelPosition { get; set; }
        [Tolerance(0.01)]
        public double BorderHue { get; set; }
        [Tolerance(0.01)]
        public double BorderSaturation { get; set; }
        [Tolerance(0.01)]
        public double BorderLuma { get; set; }
        [Tolerance(0.01)]
        public double BorderLightSourceDirection { get; set; }
        [Tolerance(0.01)]
        public double BorderLightSourceAltitude { get; set; }
    }


    [Serializable]
    public class ComparisonSuperSourceBoxState
    {
        public bool Enabled { get; set; }
        public VideoSource InputSource { get; set; }
        [Tolerance(0.01)]
        public double PositionX { get; set; }
        [Tolerance(0.01)]
        public double PositionY { get; set; }
        [Tolerance(0.01)]
        public double Size { get; set; }

        public bool Cropped { get; set; }
        [Tolerance(0.01)]
        public double CropTop { get; set; }
        [Tolerance(0.01)]
        public double CropBottom { get; set; }
        [Tolerance(0.01)]
        public double CropLeft { get; set; }
        [Tolerance(0.01)]
        public double CropRight { get; set; }
    }

    [Serializable]
    public class ComparisonSettingsState
    {
        public Dictionary<uint, ComparisonSettingsMultiViewState> MultiViews { get; set; } = new Dictionary<uint, ComparisonSettingsMultiViewState>();

        public VideoMode VideoMode { get; set; }
        public SerialMode SerialMode { get; set; }
    }

    [Serializable]
    public class ComparisonAudioState
    {
        [Tolerance(0.5)]
        public double ProgramOutGain { get; set; }
        [Tolerance(0.01)]
        public double ProgramOutBalance { get; set; }
        public bool ProgramOutFollowFadeToBlack { get; set; }

        // TODO level peaks

        public Dictionary<long, ComparisonAudioInputState> Inputs { get; set; } = new Dictionary<long, ComparisonAudioInputState>();
        public Dictionary<uint, ComparisonAudioMonitorOutputState> Monitors { get; set; } = new Dictionary<uint, ComparisonAudioMonitorOutputState>();
    }

    [Serializable]
    public class ComparisonAudioMonitorOutputState
    {
        public bool Enabled { get; set; }
        [Tolerance(0.5)]
        public double Gain { get; set; }

        public bool Mute { get; set; }

        public bool Solo { get; set; }
        public AudioSource SoloInput { get; set; }

        public bool Dim { get; set; }
    }

    [Serializable]
    public class ComparisonAudioInputState
    {
        public ExternalPortType ExternalPortType { get; set; }
        public AudioMixOption MixOption { get; set; }
        [Tolerance(0.5)]
        public double Gain { get; set; }
        [Tolerance(0.01)]
        public double Balance { get; set; }
        public bool IsMixedIn { get; set; } // TODO - disable this field except for when needed?
    }

    [Serializable]
    public class ComparisonSettingsMultiViewState
    {
        public MultiViewLayout Layout { get; set; }
        public bool ProgramPreviewSwapped { get; set; }
        public bool SafeAreaEnabled { get; set; }
        [Tolerance(1)]
        public double VuMeterOpacity { get; set; }

        public List<ComparisonSettingsMultiViewWindowState> Windows { get; set; } = Enumerable.Range(0, (int)Constants.MultiViewWindowCount).Select(c => new ComparisonSettingsMultiViewWindowState()).ToList();
    }

    [Serializable]
    public class ComparisonSettingsMultiViewWindowState
    {
        public bool VuMeter { get; set; }
        public bool SupportsVuMeter { get; set; }
        public VideoSource Source { get; set; }
    }

    [Serializable]
    public class ComparisonMixEffectTransitionState
    {
        public ComparisonMixEffectTransitionMixState Mix { get; set; } = new ComparisonMixEffectTransitionMixState();
        public ComparisonMixEffectTransitionDipState Dip { get; set; } = new ComparisonMixEffectTransitionDipState();
        public ComparisonMixEffectTransitionWipeState Wipe { get; set; } = new ComparisonMixEffectTransitionWipeState();
        public ComparisonMixEffectTransitionStingerState Stinger { get; set; } = new ComparisonMixEffectTransitionStingerState();
        public ComparisonMixEffectTransitionDVEState DVE { get; set; } = new ComparisonMixEffectTransitionDVEState();

        public TStyle Style { get; set; }
        public TStyle NextStyle { get; set; }
        public TransitionLayer Selection { get; set; }
        public TransitionLayer NextSelection { get; set; }
    }

    [Serializable]
    public class ComparisonMixEffectTransitionMixState
    {
        public uint Rate { get; set; }
    }

    [Serializable]
    public class ComparisonMixEffectTransitionDipState
    {
        public VideoSource Input { get; set; }
        public uint Rate { get; set; }
    }

    [Serializable]
    public class ComparisonMixEffectTransitionWipeState
    {
        public uint Rate { get; set; }
        public Pattern Pattern { get; set; }
        [Tolerance(0.01)]
        public double BorderWidth { get; set; }
        public VideoSource BorderInput { get; set; }
        [Tolerance(0.01)]
        public double Symmetry { get; set; }
        [Tolerance(0.01)]
        public double BorderSoftness { get; set; }
        [Tolerance(0.01)]
        public double XPosition { get; set; }
        [Tolerance(0.01)]
        public double YPosition { get; set; }
        public bool ReverseDirection { get; set; }
        public bool FlipFlop { get; set; }
    }

    [Serializable]
    public class ComparisonMixEffectTransitionStingerState
    {
        public StingerSource Source { get; set; }
        public bool PreMultipliedKey { get; set; }

        [Tolerance(0.01)]
        public double Clip { get; set; }
        [Tolerance(0.01)]
        public double Gain { get; set; }
        public bool Invert { get; set; }

        public uint Preroll { get; set; }
        public uint ClipDuration { get; set; }
        public uint TriggerPoint { get; set; }
        public uint MixRate { get; set; }
    }

    [Serializable]
    public class ComparisonMixEffectTransitionDVEState
    {
        public uint Rate { get; set; }
        public uint LogoRate { get; set; }
        public DVEEffect Style { get; set; }

        public VideoSource FillSource { get; set; }
        public VideoSource KeySource { get; set; }

        public bool EnableKey { get; set; }
        public bool PreMultiplied { get; set; }
        [Tolerance(0.01)]
        public double Clip { get; set; }
        [Tolerance(0.01)]
        public double Gain { get; set; }
        public bool InvertKey { get; set; }
        public bool Reverse { get; set; }
        public bool FlipFlop { get; set; }
    }

    [Serializable]
    public class ComparisonMixEffectKeyerState
    {
        public ComparisonMixEffectKeyerLumaState Luma { get; set; } = new ComparisonMixEffectKeyerLumaState();
        public ComparisonMixEffectKeyerChromaState Chroma { get; set; } = new ComparisonMixEffectKeyerChromaState();
        public ComparisonMixEffectKeyerPatternState Pattern { get; set; } = new ComparisonMixEffectKeyerPatternState();
        public ComparisonMixEffectKeyerDVEState DVE { get; set; } = new ComparisonMixEffectKeyerDVEState();
        public ComparisonMixEffectKeyerFlyState Fly { get; set; } = new ComparisonMixEffectKeyerFlyState();

        public bool OnAir { get; set; }

        public MixEffectKeyType Type { get; set; }
        public bool FlyEnabled { get; set; }
        public VideoSource FillSource { get; set; }
        public VideoSource CutSource { get; set; }

        public bool MaskEnabled { get; set; }
        [Tolerance(0.01)]
        public double MaskTop { get; set; }
        [Tolerance(0.01)]
        public double MaskBottom { get; set; }
        [Tolerance(0.01)]
        public double MaskLeft { get; set; }
        [Tolerance(0.01)]
        public double MaskRight { get; set; }
    }

    [Serializable]
    public class ComparisonMixEffectKeyerLumaState
    {
        public bool PreMultiplied { get; set; }

        [Tolerance(0.01)]
        public double Clip { get; set; }
        [Tolerance(0.01)]
        public double Gain { get; set; }

        public bool Invert { get; set; }
    }

    [Serializable]
    public class ComparisonMixEffectKeyerChromaState
    {
        [Tolerance(0.01)]
        public double Hue { get; set; }
        [Tolerance(0.01)]
        public double Gain { get; set; }
        [Tolerance(0.01)]
        public double YSuppress { get; set; }
        [Tolerance(0.01)]
        public double Lift { get; set; }
        public bool Narrow { get; set; }
    }

    [Serializable]
    public class ComparisonMixEffectKeyerPatternState
    {
        public Pattern Style { get; set; }
        [Tolerance(0.01)]
        public double Size { get; set; }
        [Tolerance(0.01)]
        public double Symmetry { get; set; }
        [Tolerance(0.01)]
        public double Softness { get; set; }
        [Tolerance(0.01)]
        public double XPosition { get; set; }
        [Tolerance(0.01)]
        public double YPosition { get; set; }
        public bool Inverse { get; set; }
    }

    [Serializable]
    public class ComparisonMixEffectKeyerDVEState
    {
        public bool BorderEnabled { get; set; }
        public bool BorderShadow { get; set; }
        public BorderBevel BorderBevel { get; set; }
        public double OuterWidth { get; set; }
        public double InnerWidth { get; set; }
        public uint OuterSoftness { get; set; }
        public uint InnerSoftness { get; set; }
        public uint BevelSoftness { get; set; }
        public uint BevelPosition { get; set; }

        public uint BorderOpacity { get; set; }
        [Tolerance(0.01)]
        public double BorderHue { get; set; }
        [Tolerance(0.01)]
        public double BorderSaturation { get; set; }
        [Tolerance(0.01)]
        public double BorderLuma { get; set; }

        [Tolerance(0.01)]
        public double LightSourceDirection { get; set; }
        public uint LightSourceAltitude { get; set; }

        public bool MaskEnabled { get; set; }
        [Tolerance(0.01)]
        public double MaskTop { get; set; }
        [Tolerance(0.01)]
        public double MaskBottom { get; set; }
        [Tolerance(0.01)]
        public double MaskLeft { get; set; }
        [Tolerance(0.01)]
        public double MaskRight { get; set; }
    }


    [Serializable]
    public class ComparisonMixEffectKeyerFlyState
    {
        [Tolerance(0.01)]
        public double SizeX { get; set; }
        [Tolerance(0.01)]
        public double SizeY { get; set; }
        [Tolerance(0.01)]
        public double PositionX { get; set; }
        [Tolerance(0.01)]
        public double PositionY { get; set; }
        [Tolerance(0.01)]
        public double Rotation { get; set; }

        public uint Rate { get; set; }

        // TODO - keyframes
    }

    [Serializable]
    public class ComparisonMixEffectState
    {
        public Dictionary<UpstreamKeyId, ComparisonMixEffectKeyerState> Keyers { get; set; } = new Dictionary<UpstreamKeyId, ComparisonMixEffectKeyerState>();

        public ComparisonMixEffectTransitionState Transition { get; set; } = new ComparisonMixEffectTransitionState();

        public VideoSource Program { get; set; }
        public VideoSource Preview { get; set; }

    }

    [Serializable]
    public class ComparisonAuxiliaryState
    {
        public VideoSource Source { get; set; }
    }

    [Serializable]
    public class ComparisonColorState
    {
        [Tolerance(0.01)]
        public double Hue { get; set; }
        [Tolerance(0.01)]
        public double Saturation { get; set; }
        [Tolerance(0.01)]
        public double Luma { get; set; }
    }
}
