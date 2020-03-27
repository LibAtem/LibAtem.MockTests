using System.Collections.Generic;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.SdkStateBuilder
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
            state.Monitors =
                AtemSDKConverter.IterateList<IBMDSwitcherAudioMonitorOutput, AudioState.MonitorOutputState>(
                    monIt.Next,
                    (mon, id) => BuildMonitor(mon));

            return state;
        }

        private static AudioState.InputState BuildInput(IBMDSwitcherAudioInput props)
        {
            var state = new AudioState.InputState();

            props.GetType(out _BMDSwitcherAudioInputType type);
            state.Properties.SourceType = AtemEnumMaps.AudioSourceTypeMap.FindByValue(type);

            props.GetCurrentExternalPortType(out _BMDSwitcherExternalPortType externalType);
            state.Properties.PortType = ((ExternalPortTypeFlags)externalType).ToAudioPortType();
            props.GetMixOption(out _BMDSwitcherAudioMixOption mixOption);
            state.Properties.MixOption = AtemEnumMaps.AudioMixOptionMap.FindByValue(mixOption);
            props.GetGain(out double gain);
            state.Properties.Gain = gain;
            props.GetBalance(out double balance);
            state.Properties.Balance = balance * 50;

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
            /*
            props.GetDimLevel(out double dimLevel);
            state.DimLevel = dimLevel;
            */

            return state;
        }
    }
}