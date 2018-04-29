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
    public class ComparisonMixEffectState
    {
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
