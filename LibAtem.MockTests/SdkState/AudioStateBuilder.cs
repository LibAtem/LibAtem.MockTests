using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.MockTests.SdkState
{
    public static class AudioStateBuilder
    {
        public static AudioState Build(IBMDSwitcherAudioMixer props)
        {
            var state = new AudioState();

            props.GetProgramOutGain(out double gain);
            state.ProgramOut.Gain = gain;
            props.GetProgramOutBalance(out double balance);
            state.ProgramOut.Balance = balance * 50;
            props.GetProgramOutFollowFadeToBlack(out int follow);
            state.ProgramOut.FollowFadeToBlack = follow != 0;
            props.GetAudioFollowVideoCrossfadeTransition(out int followTransition);
            state.ProgramOut.AudioFollowVideoCrossfadeTransitionEnabled = followTransition != 0;

            state.Tally = new Dictionary<AudioSource, bool>();

            var inputIt = AtemSDKConverter.CastSdk<IBMDSwitcherAudioInputIterator>(props.CreateIterator);
            AtemSDKConverter.Iterate<IBMDSwitcherAudioInput>(inputIt.Next, (port, i) =>
            {
                port.GetAudioInputId(out long inputId);
                state.Inputs[inputId] = BuildInput(port);

                port.IsMixedIn(out int isMixedIn);
                state.Tally[(AudioSource) inputId] = isMixedIn != 0;
            });

            var monIt = AtemSDKConverter.CastSdk<IBMDSwitcherAudioMonitorOutputIterator>(props.CreateIterator);
            state.MonitorOutputs =
                AtemSDKConverter.IterateList<IBMDSwitcherAudioMonitorOutput, AudioState.MonitorOutputState>(
                    monIt.Next,
                    (mon, id) => BuildMonitor(mon));

            var headphoneIt = AtemSDKConverter.CastSdk<IBMDSwitcherAudioHeadphoneOutputIterator>(props.CreateIterator);
            state.HeadphoneOutputs =
                AtemSDKConverter.IterateList<IBMDSwitcherAudioHeadphoneOutput, AudioState.HeadphoneOutputState>(
                    headphoneIt.Next,
                    (hp, id) => BuildHeadphone(hp));

            return state;
        }

        private static AudioState.InputState BuildInput(IBMDSwitcherAudioInput props)
        {
            var state = new AudioState.InputState();

            props.GetType(out _BMDSwitcherAudioInputType type);
            state.Properties.SourceType = AtemEnumMaps.AudioSourceTypeMap.FindByValue(type);

            props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType externalType);
            state.Properties.PortType = AtemEnumMaps.AudioPortTypeMap.FindByValue(externalType);
            props.GetMixOption(out _BMDSwitcherAudioMixOption mixOption);
            state.Properties.MixOption = AtemEnumMaps.AudioMixOptionMap.FindByValue(mixOption);
            props.GetGain(out double gain);
            state.Properties.Gain = gain;
            props.GetBalance(out double balance);
            state.Properties.Balance = balance * 50;

            if (props is IBMDSwitcherAudioInputXLR xlrProps)
            {
                xlrProps.HasRCAToXLR(out int supportsXlr);
                if (supportsXlr != 0)
                {
                    xlrProps.GetRCAToXLREnabled(out int xlrEnabled);
                    state.Analog = new AudioState.InputState.AnalogState
                    {
                        RcaToXlr = xlrEnabled != 0
                    };
                }
            }

            return state;
        }

        private static AudioState.MonitorOutputState BuildMonitor(IBMDSwitcherAudioMonitorOutput props)
        {
            var state = new AudioState.MonitorOutputState();

            props.GetMonitorEnable(out int enable);
            state.Enabled = enable != 0;
            props.GetGain(out double gain);
            state.Gain = gain;
            props.GetMute(out int mute);
            state.Mute = mute != 0;
            props.GetSolo(out int solo);
            state.Solo = solo != 0;
            props.GetSoloInput(out long soloInput);
            state.SoloSource = (AudioSource)soloInput;
            props.GetDim(out int dim);
            state.Dim = dim != 0;
            props.GetDimLevel(out double dimLevel);
            state.DimLevel = (uint) (dimLevel * 100);

            return state;
        }

        private static AudioState.HeadphoneOutputState BuildHeadphone(IBMDSwitcherAudioHeadphoneOutput props)
        {
            var state = new AudioState.HeadphoneOutputState();

            props.GetGain(out double gain);
            props.GetInputProgramOutGain(out double programGain);
            props.GetInputSidetoneGain(out double sidetoneGain);
            props.GetInputTalkbackGain(out double talkbackGain);

            state.Gain = gain;
            state.ProgramOutGain = programGain;
            state.SidetoneGain = sidetoneGain;
            state.TalkbackGain = talkbackGain;

            return state;
        }
    }
}