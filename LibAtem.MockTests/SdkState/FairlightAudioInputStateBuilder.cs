﻿using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using System;
using System.Collections.Generic;

namespace LibAtem.MockTests.SdkState
{
    public static class FairlightAudioInputStateBuilder
    {
        public static FairlightAudioState.InputState Build(IBMDSwitcherFairlightAudioInput props, AudioSource inputId, Dictionary<Tuple<AudioSource, long>, bool> tally)
        {
            var state = new FairlightAudioState.InputState();

            // Input basics
            props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType portType);
            state.ExternalPortType = AtemEnumMaps.AudioPortTypeMap.FindByValue(portType);
            props.GetConfiguration(out _BMDSwitcherFairlightAudioInputConfiguration configuration);
            state.ActiveConfiguration = AtemEnumMaps.FairlightInputConfigurationMap.FindByValue(configuration);
            props.GetSupportedConfigurations(out _BMDSwitcherFairlightAudioInputConfiguration supportedConfigurations);
            state.SupportedConfigurations = (FairlightInputConfiguration) supportedConfigurations;
            props.GetType(out _BMDSwitcherFairlightAudioInputType type);
            state.InputType = AtemEnumMaps.FairlightInputTypeMap.FindByValue(type);

            // Analog
            if (props is IBMDSwitcherFairlightAnalogAudioInput analog)
            {
                analog.GetSupportedInputLevels(out _BMDSwitcherFairlightAudioAnalogInputLevel supportedLevels);
                if (supportedLevels != 0)
                {
                    state.Analog = new FairlightAudioState.AnalogState();
                    analog.GetInputLevel(out _BMDSwitcherFairlightAudioAnalogInputLevel level);
                    state.Analog.InputLevel = AtemEnumMaps.FairlightAnalogInputLevelMap.FindByValue(level);

                    state.Analog.SupportedInputLevel = (FairlightAnalogInputLevel)supportedLevels;
                }
            }

#if ATEM_v8_1
            // XLR
            if (props is IBMDSwitcherFairlightAudioInputXLR xlr)
            {
                xlr.HasRCAToXLR(out int hasRcaToXlr);
                if (hasRcaToXlr != 0)
                {
                    if (state.Analog == null) state.Analog = new FairlightAudioState.AnalogState();

                    state.Analog.SupportedInputLevel =
                        FairlightAnalogInputLevel.ConsumerLine | FairlightAnalogInputLevel.ProLine;
                    xlr.GetRCAToXLREnabled(out int rcaToXlrEnabled);
                    state.Analog.InputLevel = rcaToXlrEnabled != 0
                        ? FairlightAnalogInputLevel.ConsumerLine
                        : FairlightAnalogInputLevel.ProLine;
                }
            }
#else
            // TODO
#endif

            // Sources
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioSourceIterator>(props.CreateIterator);
            AtemSDKConverter.Iterate<IBMDSwitcherFairlightAudioSource>(
                iterator.Next,
                (src, id) =>
                {
                    var val = BuildSource(src, inputId, tally);
                    if (val != null)
                        state.Sources.Add(val);
                });

            return state;
        }

        private static FairlightAudioState.InputSourceState BuildSource(IBMDSwitcherFairlightAudioSource props, AudioSource inputId, Dictionary<Tuple<AudioSource, long>, bool> tally)
        {
            var state = new FairlightAudioState.InputSourceState();

            props.IsActive(out int active);
            if (active == 0)
                return null;
            
            props.GetId(out long id);
            state.SourceId = id;
            props.GetSupportedMixOptions(out _BMDSwitcherFairlightAudioMixOption supportedMixOptions);
            state.SupportedMixOptions = (FairlightAudioMixOption) supportedMixOptions;
            props.GetSourceType(out _BMDSwitcherFairlightAudioSourceType sourceType);
            state.SourceType = AtemEnumMaps.FairlightAudioSourceTypeMap.FindByValue(sourceType);

            props.GetMaxDelayFrames(out ushort maxDelay);
            state.MaxFramesDelay = maxDelay;
            props.GetDelayFrames(out ushort delay);
            state.FramesDelay = delay;

            props.GetInputGain(out double inputGain);
            state.Gain = inputGain;
            props.GetPan(out double pan);
            state.Balance = pan;
            props.GetFaderGain(out double faderGain);
            state.FaderGain = faderGain;
            props.GetMixOption(out _BMDSwitcherFairlightAudioMixOption mixOption);
            state.MixOption = AtemEnumMaps.FairlightAudioMixOptionMap.FindByValue(mixOption);
            props.HasStereoSimulation(out int hasStereoSimulation);
            state.HasStereoSimulation = hasStereoSimulation != 0;
            props.GetStereoSimulationIntensity(out double stereoSimulation);
            state.StereoSimulation = stereoSimulation;

            props.IsMixedIn(out int mixedIn);
            tally[Tuple.Create(inputId, id)] = mixedIn != 0;

            var dynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(props.GetEffect);
            dynamics.GetMakeupGain(out double makeupGain);
            state.Dynamics.MakeUpGain = makeupGain;

            var compressor = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioCompressor>(dynamics.GetProcessor);
            FairlightAudioBuilderCommon.ApplyCompressor(compressor, state.Dynamics.Compressor = new FairlightAudioState.CompressorState());
            var limiter = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioLimiter>(dynamics.GetProcessor);
            FairlightAudioBuilderCommon.ApplyLimiter(limiter, state.Dynamics.Limiter = new FairlightAudioState.LimiterState());
            var expander = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioExpander>(dynamics.GetProcessor);
            FairlightAudioBuilderCommon.ApplyExpander(expander, state.Dynamics.Expander = new FairlightAudioState.ExpanderState());

            var eq = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioEqualizer>(props.GetEffect);
            FairlightAudioBuilderCommon.ApplyEqualizer(eq, state.Equalizer);

            return state;
        }

    }
}