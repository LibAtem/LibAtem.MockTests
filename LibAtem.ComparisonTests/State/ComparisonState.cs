using System;
using System.Collections.Generic;
using System.IO;
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

    [Serializable]
    public class ComparisonState
    {
        public Dictionary<MixEffectBlockId, ComparisonMixEffectState> MixEffects { get; set; }
        public Dictionary<AuxiliaryId, ComparisonAuxiliaryState> Auxiliaries { get; set; }
        public Dictionary<ColorGeneratorId, ComparisonColorState> Colors { get; set; }

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
    public class ComparisonMixEffectTransitionState
    {
        public ComparisonMixEffectTransitionMixState Mix { get; set; } = new ComparisonMixEffectTransitionMixState();
        public ComparisonMixEffectTransitionDipState Dip { get; set; } = new ComparisonMixEffectTransitionDipState();
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
