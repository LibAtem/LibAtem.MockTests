using System.Collections.Generic;
using System.Linq;
using BMDSwitcherAPI;
using LibAtem.Common;
using LibAtem.State;
using Xunit;

namespace LibAtem.MockTests.SdkState
{
    public static class TalkbackStateBuilder
    {
        public static void Build(AtemState state, IBMDSwitcher switcher)
        {
            var iterator = AtemSDKConverter.CastSdk<IBMDSwitcherTalkbackIterator>(switcher.CreateIterator);
            var talkback = AtemSDKConverter.IterateList<IBMDSwitcherTalkback, SettingsState.TalkbackState>(iterator.Next,
                (props, id0) =>
                {
                    props.GetId(out _BMDSwitcherTalkbackId channelId);
                    var id = AtemEnumMaps.TalkbackChannelMap.FindByValue(channelId);
                    Assert.Equal((uint) id, id0);

                    props.GetMuteSDI(out int muteSDI);

                    var audioInputIds = state.Settings.Inputs
                        .Where(i => i.Value.Properties.InternalPortType == InternalPortType.External &&
                                    i.Value.Properties.AvailableExternalPortTypes.Contains(VideoPortType.SDI))
                        .Select(i => (long) i.Key).ToList();

                    var res = new SettingsState.TalkbackState
                    {
                        MuteSDI = muteSDI != 0,
                    };

                    foreach (long inputId in audioInputIds)
                    {
                        props.CurrentInputSupportsMuteSDI(inputId, out int supportsMuteInputSdi);
                        int muteInputSdi = 0;
                        int canMuteinputSdi = 0;
                        if (supportsMuteInputSdi != 0)
                        {
                            props.InputCanMuteSDI(inputId, out canMuteinputSdi);
                            if (canMuteinputSdi != 0)
                            {
                                props.GetInputMuteSDI(inputId, out muteInputSdi);
                            }
                        }

                        res.Inputs[(VideoSource) inputId] = new SettingsState.TalkbackInputState
                        {
                            MuteSDI = muteInputSdi != 0,
                            InputCanMuteSDI = canMuteinputSdi != 0,
                            CurrentInputSupportsMuteSDI = supportsMuteInputSdi != 0,
                        };
                    }


                    return res;
                });

            state.Settings.Talkback = talkback;
        }
    }
}