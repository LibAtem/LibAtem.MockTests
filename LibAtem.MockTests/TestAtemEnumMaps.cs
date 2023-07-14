using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.MockTests.SdkState;
using System;
using System.Collections.Generic;
using System.Linq;
using LibAtem.Util;
using Xunit;

namespace LibAtem.MockTests
{
    internal static class EnumMap
    {
        public static void EnsureIsComplete<T1, T2>(IReadOnlyDictionary<T1, T2> map, bool? unmatchedZero = null, params T2[] skip)
        where T1 : System.IConvertible
        {
            ProtocolVersion currentVersion = DeviceTestCases.Version;

            List<T1> keys = Enum.GetValues(typeof(T1)).OfType<T1>().ToList();
            if (unmatchedZero.GetValueOrDefault(false))
                keys = keys.Where(v => Convert.ToInt32(v) != 0).ToList();
            
            List<T1> validKeys = keys.Where(v =>
            {
                SinceAttribute attr = v.GetPossibleAttribute<T1, SinceAttribute>();
                return attr == null ||  currentVersion >= attr.Version;
            }).ToList();

            // Check that no values are defined which should not
            List<T1> badKeys = keys.Except(validKeys).ToList();
            List<T1> definedBadKeys = badKeys.Where(map.ContainsKey).ToList();
            Assert.Empty(definedBadKeys);

            List<T1> missing = validKeys.Where(v => !map.ContainsKey(v)).ToList();
            Assert.Empty(missing);

            List<T2> validVals = Enum.GetValues(typeof(T2)).OfType<T2>().Except(skip).OrderBy(v => v).ToList();

            // Expect map and values to have the same number
            Assert.Equal(validKeys.Count, map.Count);
            Assert.True(validVals.SequenceEqual(map.Values.OrderBy(v => v).ToList()));

            // Expect all the map values to be unique
            Assert.Equal(validKeys.Count, map.Values.Distinct().Count());
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

        /*
        [Fact]
        public void EnsureVideoAndAudioPortTypeMapsAreComplete()
        {

            EnumMap.EnsureIsComplete(AtemEnumMaps.VideoPortTypeMap);
        }
        */

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

        [Fact]
        public void EnsureSDI3GOutputLevelMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.SDI3GOutputLevelMap);

        [Fact]
        public void EnsureTalkbackChannelMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.TalkbackChannelMap);

        [Fact]
        public void EnsureMixMinusModeMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.MixMinusModeMap);

        [Fact]
        public void EnsureHyperDeckPlayerStateMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.HyperDeckPlayerStateMap,
            null,
            _BMDSwitcherHyperDeckPlayerState.bmdSwitcherHyperDeckStateUnknown);

        [Fact]
        public void EnsureHyperDeckConnectionStatusMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.HyperDeckConnectionStatusMap);

        [Fact]
        public void EnsureHyperDeckStorageStatusMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.HyperDeckStorageStatusMap);

#if !ATEM_v8_1
        [Fact]
        public void EnsureTimeCodeModeMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.TimeCodeModeMap);

        [Fact]
        public void EnsureStreamingStatusMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.StreamingStatusMap);
        [Fact]
        public void EnsureStreamingErrorMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.StreamingErrorMap);
        [Fact]
        public void EnsureRecordingStateMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.RecordingStateMap);
        [Fact]
        public void EnsureRecordingDiskStatusMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.RecordingDiskStatusMap);
        [Fact]
        public void EnsureRecordingErrorMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.RecordingErrorMap);
        [Fact]
        public void EnsureDisplayClockModeMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.DisplayClockModeMap);
        [Fact]
        public void EnsureDisplayClockStateMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.DisplayClockStateMap);
        [Fact]
        public void EnsureAudioChannelPairMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.AudioChannelPairMap);
        [Fact]
        public void EnsureAudioInternalPortTypeMap() => EnumMap.EnsureIsComplete(AtemEnumMaps.AudioInternalPortTypeMap);
#endif
    }
}
