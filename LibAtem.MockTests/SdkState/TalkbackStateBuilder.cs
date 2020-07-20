using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;

namespace LibAtem.MockTests.SdkState
{
    public static class TalkbackStateBuilder
    {
        public static void Build(AtemState state, IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherTalkbackIterator>(switcher.CreateIterator);
            var talkback = new Dictionary<TalkbackChannel, SettingsState.TalkbackState>();

            iterator.Next(out var tb);


            AtemSDKConverter.Iterate<IBMDSwitcherTalkback>(iterator.Next,
                (props, id0) =>
                {
                    props.GetId(out _BMDSwitcherTalkbackId channelId);
                    props.GetMuteSDI(out int muteSDI);

                    List<long> audioInputIds = state.Audio?.Inputs.Keys.ToList() ??
                                               state.Fairlight?.Inputs.Keys.ToList() ?? new List<long>();

                    //props.InputCanMuteSDI()
                    //props.GetInputMuteSDI();

                    talkback.Add(AtemEnumMaps.TalkbackChannelMap.FindByValue(channelId), new SettingsState.TalkbackState
                    {
                        MuteSDI = muteSDI != 0,
                        //Inputs = 
                    });
                });

            if (talkback.Count > 0)
                state.Settings.Talkback = talkback;
        }
    }
}