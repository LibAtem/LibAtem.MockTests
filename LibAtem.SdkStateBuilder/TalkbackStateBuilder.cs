using BMDSwitcherAPI;
using LibAtem.State;

namespace LibAtem.SdkStateBuilder
{
    public static class TalkbackStateBuilder
    {
        public static void Build(AudioState.TalkbackState state, IBMDSwitcherTalkback props)
        {
            props.GetMuteSDI(out int muteSDI);
            state.MuteSDI = muteSDI != 0;

            /*
            props.InputCanMuteSDI(audioInputId, out int supports);
            if (supports == 0) // If hardware doesnt support it, it exceptions
            {
                state.Inputs[audioInputId] = false;
            }

            Assert.True(false, "Not tested");
            props.GetInputMuteSDI(audioInputId, out int muteSDIin);
            state.Inputs[audioInputId] = muteSDIin != 0;
            props.CurrentInputSupportsMuteSDI(audioInputId, out int supportsMuteSDI);
            // TODO - this will be fired when changed port type
            */
        }
    }
}