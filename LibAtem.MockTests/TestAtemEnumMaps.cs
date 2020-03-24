using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.SdkStateBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LibAtem.Util;
using NAudio.MediaFoundation;
using Xunit;

namespace LibAtem.MockTests
{
    internal static class EnumMap
    {
        public static void EnsureIsComplete<T1, T2>(IReadOnlyDictionary<T1, T2> map, bool? unmatchedZero = null)
        where T1 : System.IConvertible
        {
            ProtocolVersion currentVersion = DeviceTestCases.Version;

            List<T1> vals = Enum.GetValues(typeof(T1)).OfType<T1>().ToList();
            if (unmatchedZero.GetValueOrDefault(false))
                vals = vals.Where(v => Convert.ToInt32(v) != 0).ToList();
            
            List<T1> validVals = vals.Where(v =>
            {
                SinceAttribute attr = v.GetPossibleAttribute<T1, SinceAttribute>();
                return attr == null ||  currentVersion >= attr.Version;
            }).ToList();

            // Check that no values are defined which should not
            List<T1> badVals = vals.Except(validVals).ToList();
            List<T1> definedBadVals = badVals.Where(map.ContainsKey).ToList();
            Assert.Empty(definedBadVals);

            List<T1> missing = validVals.Where(v => !map.ContainsKey(v)).ToList();
            Assert.Empty(missing);

            // Expect map and values to have the same number
            Assert.Equal(validVals.Count, map.Count);
            Assert.Equal(Enum.GetValues(typeof(T2)).Length, map.Count);

            // Expect all the map values to be unique
            Assert.Equal(validVals.Count, map.Select(v => v.Value).Distinct().Count());
        }

        public static void EnsureIsMatching<T1, T2>()
        {
            int vals = Enum.GetValues(typeof(T1)).OfType<T1>().Select(e => Convert.ToInt32(e)).Sum(a => a);
            int vals2 = Enum.GetValues(typeof(T2)).OfType<T2>().Select(e => Convert.ToInt32(e)).Sum(a => a);

            // We assume they are valid if their sums are equal. 
            // This only works for flags. Other types need the conversion map and EnsureIsComplete
            Assert.Equal(vals, vals2);
        }
    }

    public class TestAtemEnumMaps
    {
        [Fact]
        public void EnsureBorderBevelMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.BorderBevelMap);

        [Fact]
        public void EnsureDVEStyleMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.DVEStyleMap);

        [Fact]
        public void EnsureMixEffectKeyTypeMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.MixEffectKeyTypeMap);

        [Fact]
        public void EnsurePatternMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.PatternMap);

        [Fact]
        public void EnsureStingerSourceMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.StingerSourceMap);

        [Fact]
        public void EnsureTransitionStyleMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.TransitionStyleMap);

        [Fact]
        public void EnsureTransitionLayerIsMapped() => EnumMap.EnsureIsMatching<TransitionLayer, _BMDSwitcherTransitionSelection>();

        [Fact]
        public void EnsureVideoModesMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.VideoModesMap);

        [Fact]
        public void EnsureSDDownconvertModesMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.SDDownconvertModesMap);

        [Fact]
        public void EnsureSerialModeMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.SerialModeMap);

        [Fact]
        public void EnsureMultiViewLayoutMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.MultiViewLayoutMap, true);

        [Fact]
        public void EnsureInternalPortMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.InternalPortTypeMap);

        [Fact]
        public void EnsureExternalPortMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.ExternalPortTypeMap);

        [Fact]
        public void EnsureSuperSourceArtOptionMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.SuperSourceArtOptionMap);

        [Fact]
        public void EnsureMediaPlayerSourceMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.MediaPlayerSourceMap);

        [Fact]
        public void EnsureAudioMixOptionMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.AudioMixOptionMap);

        [Fact]
        public void EnsureAudioSourceTypeMapIsComplete() => EnumMap.EnsureIsComplete(AtemEnumMaps.AudioSourceTypeMap);

        [Fact]
        public void EnsureFairlightEqualizerShape() => EnumMap.EnsureIsComplete(AtemEnumMaps.FairlightEqualizerBandShapeMap);

        [Fact]
        public void EnsureFairlightEqualizerBandRange() => EnumMap.EnsureIsComplete(AtemEnumMaps.FairlightEqualizerFrequencyRangeMap);

        [Fact]
        public void EnsureFairlightAudioInputType() => EnumMap.EnsureIsComplete(AtemEnumMaps.FairlightInputTypeMap);

        [Fact]
        public void EnsureFairlightAudioInputConfiguration() => EnumMap.EnsureIsComplete(AtemEnumMaps.FairlightInputConfigurationMap);

        [Fact]
        public void EnsureFairlightAudioMixOptionMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.FairlightAudioMixOptionMap);

        [Fact]
        public void EnsureFairlightAudioSourceTypeMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.FairlightAudioSourceTypeMap);

        [Fact]
        public void EnsureFairlightAnalogInputLevelMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.FairlightAnalogInputLevelMap);
    }
}
