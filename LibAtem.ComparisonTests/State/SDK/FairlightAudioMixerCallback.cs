﻿using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
    public static class FairlightAudioMixerStateBuilder
    {
        public static FairlightAudioState Build(IBMDSwitcherFairlightAudioMixer props)
        {
            var state = new FairlightAudioState();

            // Effects
            var dynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(props.GetMasterOutEffect);
            var compressor = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioCompressor>(dynamics.GetProcessor);
            FairlightAudioInputStateBuilder.ApplyCompressor(compressor, state.ProgramOut.Dynamics.Compressor = new FairlightAudioState.CompressorState());
            var limiter = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioLimiter>(dynamics.GetProcessor);
            FairlightAudioInputStateBuilder.ApplyLimiter(limiter, state.ProgramOut.Dynamics.Limiter = new FairlightAudioState.LimiterState());
            // MasterOut appears to never have an expander
            //var expander = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioExpander>(dynamics.GetProcessor);
            //FairlightAudioInputStateBuilder.ApplyExpander(expander, state.ProgramOut.Dynamics.Expander = new FairlightAudioState.ExpanderState());

            // Equalizer
            var equalizer = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioEqualizer>(props.GetMasterOutEffect);
            equalizer.GetEnabled(out int eqEnabled);
            state.ProgramOut.Equalizer.Enabled = eqEnabled != 0;
            equalizer.GetGain(out double eqGain);
            state.ProgramOut.Equalizer.Gain = eqGain;
            // TODO - bands

            // Inputs
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioInputIterator>(props.CreateIterator);
            for (iterator.Next(out IBMDSwitcherFairlightAudioInput input); input != null; iterator.Next(out input))
            {
                input.GetId(out long id);
                state.Inputs[id] = FairlightAudioInputStateBuilder.Build(input);
            }

            var monIter = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioHeadphoneOutputIterator>(props.CreateIterator);
            state.Monitors = AtemSDKConverter.IterateList<IBMDSwitcherFairlightAudioHeadphoneOutput, FairlightAudioState.MonitorOutputState>(monIter.Next, (mon, id) =>
            {
                return BuildMonitor(mon);
            });

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

    public sealed class FairlightAudioMixerCallback : SdkCallbackBaseNotify<IBMDSwitcherFairlightAudioMixer, _BMDSwitcherFairlightAudioMixerEventType>, IBMDSwitcherFairlightAudioMixerCallback
    {
        private readonly FairlightAudioState.ProgramOutState _state;

        public FairlightAudioMixerCallback(FairlightAudioState state, IBMDSwitcherFairlightAudioMixer props, Action<string> onChange) : base(props, onChange)
        {
            _state = state.ProgramOut;
            TriggerAllChanged();
            
            // ProgramOut Dynamics
            var pgmDynamics = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioDynamicsProcessor>(props.GetMasterOutEffect);
            Children.Add(new FairlightDynamicsAudioMixerCallback(state.ProgramOut.Dynamics, pgmDynamics, AppendChange("ProgramOut.Dynamics"), false));

            // ProgramOut Equalizer
            var pgmEqualizer = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioEqualizer>(props.GetMasterOutEffect);
            Children.Add(new FairlightEqualizerAudioMixerCallback(state.ProgramOut.Equalizer, pgmEqualizer, AppendChange("ProgramOut.Equalizer")));

            // Inputs
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioInputIterator>(props.CreateIterator);
            for (iterator.Next(out IBMDSwitcherFairlightAudioInput input); input != null; iterator.Next(out input))
            {
                input.GetId(out long id);
                input.GetType(out _BMDSwitcherFairlightAudioInputType type);
                input.GetSupportedConfigurations(out _BMDSwitcherFairlightAudioInputConfiguration configs);

                var inputState = state.Inputs[id] = new FairlightAudioState.InputState
                {
                    InputType = AtemEnumMaps.FairlightInputTypeMap.FindByValue(type),
                    SupportedConfigurations = (FairlightInputConfiguration)configs
                };

                Children.Add(new FairlightAudioInputCallback(inputState, input, AppendChange($"Input.{id}")));

                /*
                // Sources
                var sourceIterator = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioSourceIterator>(input.CreateIterator);
                int srcId = 0;
                for (sourceIterator.Next(out IBMDSwitcherFairlightAudioSource src); src != null; sourceIterator.Next(out src))
                {
                    var srcState = new FairlightAudioState.InputSourceState();
                    inputState.Sources.Add(srcState);

                    var srcId2 = srcId;
                    var cb3 = new FairlightAudioInputSourceCallback(srcState, src, () => FireCommandKey($"{inputPath}.Sources.{srcId2}"));
                    SetupCallback<FairlightAudioInputSourceCallback, _BMDSwitcherFairlightAudioSourceEventType>(cb3, src.AddCallback, src.RemoveCallback);

                    // TODO - Effects?

                    srcId++;
                }
                */
            }

            var monIter = AtemSDKConverter.CastSdk<IBMDSwitcherFairlightAudioHeadphoneOutputIterator>(props.CreateIterator);
            state.Monitors = AtemSDKConverter.IterateList<IBMDSwitcherFairlightAudioHeadphoneOutput, FairlightAudioState.MonitorOutputState>(monIter.Next, (mon, id) =>
            {
                var monState = new FairlightAudioState.MonitorOutputState();
                Children.Add(new FairlightAudioMixerMonitorCallback(monState, mon, AppendChange($"Monitors.{id}")));
                return monState;
            });

        }

        public override void Notify(_BMDSwitcherFairlightAudioMixerEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherFairlightAudioMixerEventType.bmdSwitcherFairlightAudioMixerEventTypeMasterOutFaderGainChanged:
                    Props.GetMasterOutFaderGain(out double gain);
                    _state.Gain = gain;
                    break;
                case _BMDSwitcherFairlightAudioMixerEventType.bmdSwitcherFairlightAudioMixerEventTypeMasterOutFollowFadeToBlackChanged:
                    Props.GetMasterOutFollowFadeToBlack(out int follow);
                    _state.FollowFadeToBlack = follow != 0;
                    break;
                case _BMDSwitcherFairlightAudioMixerEventType.bmdSwitcherFairlightAudioMixerEventTypeAudioFollowVideoCrossfadeTransitionChanged:
                    Props.GetAudioFollowVideoCrossfadeTransition(out int transition);
                    _state.AudioFollowVideoCrossfadeTransitionEnabled = transition != 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange("ProgramOut");
        }

        public void MasterOutLevelNotification(uint numLevels, ref double levels, uint numPeakLevels, ref double peakLevels)
        {
            //throw new NotImplementedException();
        }

    }

    public sealed class FairlightAudioMixerMonitorCallback :
        SdkCallbackBaseNotify<IBMDSwitcherFairlightAudioHeadphoneOutput, _BMDSwitcherFairlightAudioHeadphoneOutputEventType>,
        IBMDSwitcherFairlightAudioHeadphoneOutputCallback
    {
        private readonly FairlightAudioState.MonitorOutputState _state;

        public FairlightAudioMixerMonitorCallback(FairlightAudioState.MonitorOutputState state, IBMDSwitcherFairlightAudioHeadphoneOutput props,
            Action<string> onChange) : base(props, onChange)
        {
            _state = state;
            TriggerAllChanged();
        }

        public override void Notify(_BMDSwitcherFairlightAudioHeadphoneOutputEventType eventType)
        {
            switch (eventType) {
                case _BMDSwitcherFairlightAudioHeadphoneOutputEventType.bmdSwitcherFairlightAudioHeadphoneOutputEventTypeGainChanged:
                    Props.GetGain(out double gain);
                    _state.Gain = gain;
                    break;
                case _BMDSwitcherFairlightAudioHeadphoneOutputEventType.bmdSwitcherFairlightAudioHeadphoneOutputEventTypeInputMasterOutGainChanged:
                    Props.GetInputMasterOutGain(out double pgmGain);
                    _state.InputMasterGain = pgmGain;
                    break;
                case _BMDSwitcherFairlightAudioHeadphoneOutputEventType.bmdSwitcherFairlightAudioHeadphoneOutputEventTypeInputTalkbackGainChanged:
                    Props.GetInputTalkbackGain(out double tbGain);
                    _state.InputTalkbackGain = tbGain;
                    break;
                case _BMDSwitcherFairlightAudioHeadphoneOutputEventType.bmdSwitcherFairlightAudioHeadphoneOutputEventTypeInputSidetoneGainChanged:
                    Props.GetInputSidetoneGain(out double sidetoneGain);
                    _state.InputSidetoneGain = sidetoneGain;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }

            OnChange(null);
        }
    }
}