using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.SdkStateBuilder
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
    }
}