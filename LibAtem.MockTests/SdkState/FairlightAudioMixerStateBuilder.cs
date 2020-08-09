using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using System;
using System.Collections.Generic;

namespace LibAtem.MockTests.SdkState
{
    public static class FairlightAudioMixerStateBuilder
    {
        public static FairlightAudioState Build(IBMDSwitcherFairlightAudioMixer props)
        {
            var state = new FairlightAudioState();

            props.GetMasterOutFaderGain(out double faderGain);
            state.ProgramOut.Gain = faderGain;
            props.GetMasterOutFollowFadeToBlack(out int followFTB);
            state.ProgramOut.FollowFadeToBlack = followFTB != 0;
            props.GetAudioFollowVideoCrossfadeTransition(out int followTransition);
            state.ProgramOut.AudioFollowVideoCrossfadeTransitionEnabled = followTransition != 0;

            // Effects
            var dynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(props.GetMasterOutEffect);
            dynamics.GetMakeupGain(out double makeupGain);
            state.ProgramOut.Dynamics.MakeUpGain = makeupGain;

            var compressor = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioCompressor>(dynamics.GetProcessor);
            FairlightAudioBuilderCommon.ApplyCompressor(compressor, state.ProgramOut.Dynamics.Compressor = new FairlightAudioState.CompressorState());
            var limiter = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioLimiter>(dynamics.GetProcessor);
            FairlightAudioBuilderCommon.ApplyLimiter(limiter, state.ProgramOut.Dynamics.Limiter = new FairlightAudioState.LimiterState());
            // MasterOut appears to never have an expander
            //var expander = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioExpander>(dynamics.GetProcessor);
            //FairlightAudioBuilderCommon.ApplyExpander(expander, state.ProgramOut.Dynamics.Expander = new FairlightAudioState.ExpanderState());

            // Equalizer
            var equalizer = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioEqualizer>(props.GetMasterOutEffect);
            FairlightAudioBuilderCommon.ApplyEqualizer(equalizer, state.ProgramOut.Equalizer);

            // Inputs
            state.Tally = new Dictionary<Tuple<AudioSource, long>, bool>();
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioInputIterator>(props.CreateIterator);
            for (iterator.Next(out IBMDSwitcherFairlightAudioInput input); input != null; iterator.Next(out input))
            {
                input.GetId(out long id);
                state.Inputs[id] = FairlightAudioInputStateBuilder.Build(input, (AudioSource)id, state.Tally);
            }

            var monIter = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioHeadphoneOutputIterator>(props.CreateIterator);
            state.Monitors = AtemSDKConverter.IterateList<IBMDSwitcherFairlightAudioHeadphoneOutput, FairlightAudioState.MonitorOutputState>(monIter.Next, (mon, id) => BuildMonitor(mon));

            return state;
        }

        private static FairlightAudioState.MonitorOutputState BuildMonitor(IBMDSwitcherFairlightAudioHeadphoneOutput props)
        {
            var state = new FairlightAudioState.MonitorOutputState();

            props.GetGain(out double gain);
            state.Gain = gain;
            props.GetInputMasterOutGain(out double pgmGain);
            state.InputMasterGain = pgmGain;
            props.GetInputTalkbackGain(out double tbGain);
            state.InputTalkbackGain = tbGain;
            props.GetInputSidetoneGain(out double sidetoneGain);
            state.InputSidetoneGain = sidetoneGain;

            return state;
        }
    }
}