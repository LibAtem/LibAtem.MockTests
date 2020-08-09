using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.MockTests.SdkState
{
    public static class FairlightAudioBuilderCommon
    {
        public static void ApplyCompressor(IBMDSwitcherFairlightAudioCompressor compressor,
            FairlightAudioState.CompressorState state)
        {
            compressor.GetEnabled(out int enabled);
            state.CompressorEnabled = enabled != 0;
            compressor.GetThreshold(out double threshold);
            state.Threshold = threshold;
            compressor.GetRatio(out double ratio);
            state.Ratio = ratio;
            compressor.GetAttack(out double attack);
            state.Attack = attack;
            compressor.GetRelease(out double release);
            state.Release = release;
            compressor.GetHold(out double hold);
            state.Hold = hold;
        }

        public static void ApplyLimiter(IBMDSwitcherFairlightAudioLimiter limiter,
            FairlightAudioState.LimiterState state)
        {
            limiter.GetEnabled(out int enabled);
            state.LimiterEnabled = enabled != 0;
            limiter.GetThreshold(out double threshold);
            state.Threshold = threshold;
            limiter.GetAttack(out double attack);
            state.Attack = attack;
            limiter.GetRelease(out double release);
            state.Release = release;
            limiter.GetHold(out double hold);
            state.Hold = hold;
        }
        public static void ApplyExpander(IBMDSwitcherFairlightAudioExpander expander,
            FairlightAudioState.ExpanderState state)
        {
            expander.GetEnabled(out int enabled);
            state.ExpanderEnabled = enabled != 0;
            expander.GetGateMode(out int gateMode);
            state.GateEnabled = gateMode != 0;
            expander.GetThreshold(out double threshold);
            state.Threshold = threshold;
            expander.GetRatio(out double ratio);
            state.Ratio = ratio;
            expander.GetRange(out double range);
            state.Range = range;
            expander.GetAttack(out double attack);
            state.Attack = attack;
            expander.GetRelease(out double release);
            state.Release = release;
            expander.GetHold(out double hold);
            state.Hold = hold;
        }

        public static void ApplyEqualizer(IBMDSwitcherFairlightAudioEqualizer eq, FairlightAudioState.EqualizerState state)
        {

            eq.GetEnabled(out int eqEnabled);
            state.Enabled = eqEnabled != 0;
            eq.GetGain(out double eqGain);
            state.Gain = eqGain;

#if !ATEM_v8_1
            var bands = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioEqualizerBandIterator>(eq.CreateIterator);
            state.Bands = AtemSDKConverter.IterateList<IBMDSwitcherFairlightAudioEqualizerBand, FairlightAudioState.EqualizerBandState>(bands.Next, (band, i) =>
            {
                band.GetEnabled(out int enabled);
                band.GetFrequency(out uint freq);
                band.GetFrequencyRange(out _BMDSwitcherFairlightAudioEqualizerBandFrequencyRange freqRange);
                band.GetGain(out double gain);
                band.GetQFactor(out double qfactor);
                band.GetShape(out _BMDSwitcherFairlightAudioEqualizerBandShape shape);
                band.GetSupportedFrequencyRanges(out _BMDSwitcherFairlightAudioEqualizerBandFrequencyRange supportedRanges);
                band.GetSupportedShapes(out _BMDSwitcherFairlightAudioEqualizerBandShape supportedShapes);

                return new FairlightAudioState.EqualizerBandState
                {
                    BandEnabled = enabled != 0,
                    Frequency = freq,
                    FrequencyRange = AtemEnumMaps.FairlightEqualizerFrequencyRangeMap.FindByValue(freqRange),
                    Gain = gain,
                    QFactor = qfactor,
                    Shape = AtemEnumMaps.FairlightEqualizerBandShapeMap.FindByValue(shape),
                    SupportedFrequencyRanges = AtemEnumMaps.FairlightEqualizerFrequencyRangeMap.FindFlagsByValue(supportedRanges),
                    SupportedShapes = AtemEnumMaps.FairlightEqualizerBandShapeMap.FindFlagsByValue(supportedShapes)
                };
            });
#endif
        }
    }
}