using System;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.ComparisonTests.State.SDK
{
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
                    InputType = AtemEnumMaps.FairlightAudioInputType.FindByValue(type),
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
                    // TODO
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
}